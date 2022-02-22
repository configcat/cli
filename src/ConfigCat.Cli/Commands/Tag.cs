using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    internal class Tag
    {
        private readonly ITagClient tagClient;
        private readonly IProductClient productClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IOutput output;

        public Tag(ITagClient tagClient,
            IProductClient productClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt,
            IOutput output)
        {
            this.tagClient = tagClient;
            this.productClient = productClient;
            this.workspaceLoader = workspaceLoader;
            this.prompt = prompt;
            this.output = output;
        }

        public async Task<int> ListAllTagsAsync(string productId, bool json, CancellationToken token)
        {
            var tags = new List<TagModel>();
            if (!productId.IsEmpty())
                tags.AddRange(await this.tagClient.GetTagsAsync(productId, token));
            else
            {
                var products = await this.productClient.GetProductsAsync(token);
                foreach (var product in products)
                    tags.AddRange(await this.tagClient.GetTagsAsync(product.ProductId, token));
            }

            if (json)
            {
                this.output.RenderJson(tags);
                return ExitCodes.Ok;
            }

            var itemsToRender = tags.Select(t => new 
            { 
                Id = t.TagId, 
                Name = t.Name,
                Color = t.Color, 
                Product = $"{t.Product.Name} [{t.Product.ProductId}]" 
            });
            this.output.RenderTable(itemsToRender);
            return ExitCodes.Ok;
        }

        public async Task<int> CreateTagAsync(string productId, string name, string color, CancellationToken token)
        {
            if (productId.IsEmpty())
                productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token);

            if (color.IsEmpty())
                color = await this.prompt.GetStringAsync("Color", token);

            var result = await this.tagClient.CreateTagAsync(productId, name, color, token);
            this.output.Write(result.TagId.ToString());

            return ExitCodes.Ok;
        }

        public async Task<int> DeleteTagAsync(int? tagId, CancellationToken token)
        {
            tagId ??= (await this.workspaceLoader.LoadTagAsync(token)).TagId;

            await this.tagClient.DeleteTagAsync(tagId.Value, token);
            return ExitCodes.Ok;
        }

        public async Task<int> UpdateTagAsync(int? tagId, string name, string color, CancellationToken token)
        {
            var tag = tagId switch
            {
                null => await this.workspaceLoader.LoadTagAsync(token),
                _ => await this.tagClient.GetTagAsync(tagId.Value, token)
            };

            if (tagId is null)
            {
                if (name.IsEmpty())
                    name = await this.prompt.GetStringAsync("Name", token, tag.Name);

                if (color.IsEmpty())
                    color = await this.prompt.GetStringAsync("Color", token, tag.Color);
            }

            if (name.IsEmptyOrEquals(tag.Name) && color.IsEmptyOrEquals(tag.Color))
            {
                this.output.WriteNoChange();
                return ExitCodes.Ok;
            }

            await this.tagClient.UpdateTagAsync(tag.TagId, name, color, token);
            return ExitCodes.Ok;
        }
    }
}

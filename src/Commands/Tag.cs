using ConfigCat.Cli.Api;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Api.Tag;
using ConfigCat.Cli.Configuration;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering.Views;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Tag : ICommandDescriptor
    {
        private readonly ITagClient tagClient;
        private readonly IProductClient productClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;
        private readonly IConfigurationProvider configurationProvider;

        public Tag(ITagClient tagClient, 
            IProductClient productClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt, 
            IExecutionContextAccessor accessor, 
            IConfigurationProvider configurationProvider)
        {
            this.tagClient = tagClient;
            this.productClient = productClient;
            this.workspaceLoader = workspaceLoader;
            this.prompt = prompt;
            this.accessor = accessor;
            this.configurationProvider = configurationProvider;
        }

        public string Name => "tag";

        public string Description => "Manage tags";

        public IEnumerable<string> Aliases => new[] { "t" };

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "ls",
                Description = "List all tags",
                Handler = this.CreateHandler(nameof(Tag.ListAllTagsAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--product-id", "-p" }, "Show only a product's tags"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "create",
                Aliases = new[] { "cr" },
                Description = "Create Tag",
                Handler = this.CreateHandler(nameof(Tag.CreateTagAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--product-id", "-p" }, "ID of the product where the tag must be created"),
                    new Option<string>(new[] { "--name", "-n" }, "The name of the new tag"),
                    new Option<string>(new[] { "--color", "-c" }, "The color of the new tag"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete Tag",
                Handler = this.CreateHandler(nameof(Tag.DeleteTagAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--tag-id", "-i" }, "ID of the tag to delete"),
                    new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                    new Option<string>(new[] { "--color", "-c" }, "The updated color"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Aliases = new[] { "up" },
                Description = "Update Tag",
                Handler = this.CreateHandler(nameof(Tag.UpdateTagAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--tag-id", "-i" }, "ID of the tag to update"),
                    new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                    new Option<string>(new[] { "--color", "-c" }, "The updated color"),
                }
            },
        };

        public async Task<int> ListAllTagsAsync(string productId, CancellationToken token)
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

            var table = new TableView<TagModel>() { Items = tags };
            table.AddColumn(p => p.TagId, "ID");
            table.AddColumn(p => p.Name, "NAME");
            table.AddColumn(p => p.Color, "COLOR");
            table.AddColumn(p => $"{p.Product.Name} [{p.Product.ProductId}]", "PRODUCT");

            this.accessor.ExecutionContext.Output.RenderView(table);

            return Constants.ExitCodes.Ok;
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
            this.accessor.ExecutionContext.Output.Write(result.TagId.ToString());

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteTagAsync(int? tagId, CancellationToken token)
        {
            if (tagId is null)
                tagId = (await this.workspaceLoader.LoadTagAsync(token)).TagId;

            await this.tagClient.DeleteTagAsync(tagId.Value, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateTagAsync(int? tagId, string name, string color, CancellationToken token)
        {
            var tag = tagId is null 
                ? await this.workspaceLoader.LoadTagAsync(token)
                : await this.tagClient.GetTagAsync(tagId.Value, token);

            if (tagId is null)
            { 
                if (name.IsEmpty())
                    name = await this.prompt.GetStringAsync("Name", token, tag.Name);

                if (color.IsEmpty())
                    color = await this.prompt.GetStringAsync("Color", token, tag.Color);
            }

            if (name.IsEmptyOrEquals(tag.Name) && color.IsEmptyOrEquals(tag.Color))
            {
                this.accessor.ExecutionContext.Output.WriteNoChange();
                return Constants.ExitCodes.Ok;
            }

            await this.tagClient.UpdateTagAsync(tag.TagId, name, color, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

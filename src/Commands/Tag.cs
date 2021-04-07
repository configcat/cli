using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Api.Tag;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Tag : ICommandDescriptor
    {
        private readonly ITagClient tagClient;
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public Tag(ITagClient tagClient, IProductClient productClient, IPrompt prompt, IExecutionContextAccessor accessor)
        {
            this.tagClient = tagClient;
            this.productClient = productClient;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "tag";

        public string Description => "Manage tags";

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "ls",
                Description = "List all tags",
                Handler = this.CreateHandler(nameof(Tag.ListAllTagsAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--product-id", "-p" }) { Description = $"Show only a product's tags" },
                }
            },
            new SubCommandDescriptor
            {
                Name = "create",
                Description = "Create Tag",
                Handler = this.CreateHandler(nameof(Tag.CreateTagAsync)),
                Arguments = new[]
                {
                    new Argument<string>("product-id") { Description = $"ID of the product where the tag must be created" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--name", "-n" }) { Description = $"The name of the new tag" },
                    new Option<string>(new[] { "--color", "-c" }) { Description = $"The color of the new tag" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete Tag",
                Handler = this.CreateHandler(nameof(Tag.DeleteTagAsync)),
                Arguments = new[]
                {
                     new Argument<int>("tag-id") { Description = $"ID of the tag to delete" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Description = "Update Tag",
                Handler = this.CreateHandler(nameof(Tag.UpdateTagAsync)),
                Arguments = new[]
                {
                     new Argument<int>("tag-id") { Description = $"ID of the tag to update" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--name", "-n" }) { Description = $"The updated name" },
                    new Option<string>(new[] { "--color", "-c" }) { Description = $"The updated color" },
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

            var console = this.accessor.ExecutionContext.Output.Console;
            var renderer = new ConsoleRenderer(console, resetAfterRender: true);
            table.RenderFitToContent(renderer, console);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> CreateTagAsync(string productId, string name, string color, CancellationToken token)
        {
            if (!token.IsCancellationRequested && name.IsEmpty())
                name = this.prompt.GetString("Tag name");

            if (!token.IsCancellationRequested && color.IsEmpty())
                color = this.prompt.GetString("Tag color");

            var result = await this.tagClient.CreateTagAsync(productId, name, color, token);
            this.accessor.ExecutionContext.Output.Write(result.TagId.ToString());

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteTagAsync(int tagId, CancellationToken token)
        {
            await this.tagClient.DeleteTagAsync(tagId, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateTagAsync(int tagId, string name, string color, CancellationToken token)
        {
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(color))
            {
                this.accessor.ExecutionContext.Output.Write($"No changes detected... ");
                this.accessor.ExecutionContext.Output.WriteYellow("Skipped.");
                return Constants.ExitCodes.Ok;
            }

            await this.tagClient.UpdateTagAsync(tagId, name, color, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

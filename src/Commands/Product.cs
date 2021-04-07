using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Product : ICommandDescriptor
    {
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public Product(IProductClient productClient,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.productClient = productClient;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "product";

        public string Description => "Manage products";

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new [] 
        { 
            new SubCommandDescriptor
            {
                Name = "ls",
                Description = "List all products",
                Handler = this.CreateHandler(nameof(Product.ListAllProductsAsync))
            },
            new SubCommandDescriptor
            {
                Name = "create",
                Description = "Create product",
                Handler = this.CreateHandler(nameof(Product.CreateProductAsync)),
                Arguments = new[]
                {
                    new Argument<string>("organization-id") { Description = $"The organization's ID where the product must be created" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--name", "-n" }) { Description = "Name of the new product" },
                }
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete product",
                Handler = this.CreateHandler(nameof(Product.DeleteProductAsync)),
                Arguments = new[]
                {
                     new Argument<string>("product-id") { Description = "ID of the product to delete" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Description = "Update product",
                Handler = this.CreateHandler(nameof(Product.UpdateProductAsync)),
                Arguments = new[]
                {
                     new Argument<string>("product-id") { Description = "ID of the product to update" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--name", "-n" }) { Description = "The updated name" },
                }
            },
        };

        public async Task<int> ListAllProductsAsync(CancellationToken token)
        {
            var products = await this.productClient.GetProductsAsync(token);

            var table = new TableView<ProductModel>() { Items = products.ToList() };
            table.AddColumn(p => p.ProductId, "ID");
            table.AddColumn(p => p.Name, "NAME");
            table.AddColumn(p => $"{p.Organization.Name} [{p.Organization.OrganizationId}]", "ORGANIZATION");

            var console = this.accessor.ExecutionContext.Output.Console;
            var renderer = new ConsoleRenderer(console, resetAfterRender: true);
            table.RenderFitToContent(renderer, console);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> CreateProductAsync(string organizationId, string name, CancellationToken token)
        {
            if (!token.IsCancellationRequested && name.IsEmpty())
                name = this.prompt.GetString("Product name");

            var result = await this.productClient.CreateProductAsync(organizationId, name, token);
            this.accessor.ExecutionContext.Output.Write(result.ProductId);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteProductAsync(string productId, CancellationToken token)
        {
            await this.productClient.DeleteProductAsync(productId, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateProductAsync(string productId, string name, CancellationToken token)
        {
            if (!token.IsCancellationRequested && name.IsEmpty())
                name = this.prompt.GetString("Product name");

            await this.productClient.UpdateProductAsync(productId, name, token);

            return Constants.ExitCodes.Ok;
        }
    }
}

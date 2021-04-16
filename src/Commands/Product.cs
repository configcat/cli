using ConfigCat.Cli.Api;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Product : ICommandDescriptor
    {
        private readonly IProductClient productClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public Product(IProductClient productClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.productClient = productClient;
            this.workspaceLoader = workspaceLoader;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "product";

        public string Description => "Manage products";

        public IEnumerable<string> Aliases => new[] { "p" };

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
                Aliases = new[] { "cr" },
                Description = "Create product",
                Handler = this.CreateHandler(nameof(Product.CreateProductAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--organization-id", "-o" }, "The organization's ID where the product must be created"),
                    new Option<string>(new[] { "--name", "-n" }, "Name of the new product"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete product",
                Handler = this.CreateHandler(nameof(Product.DeleteProductAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--product-id", "-i" }, "ID of the product to delete"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Aliases = new[] { "up" },
                Description = "Update product",
                Handler = this.CreateHandler(nameof(Product.UpdateProductAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--product-id", "-i" }, "ID of the product to update"),
                    new Option<string>(new[] { "--name", "-n" }, "The updated name"),
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

            this.accessor.ExecutionContext.Output.RenderView(table);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> CreateProductAsync(string organizationId, string name, CancellationToken token)
        {
            if (organizationId.IsEmpty())
                organizationId = (await this.workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token);

            var result = await this.productClient.CreateProductAsync(organizationId, name, token);
            this.accessor.ExecutionContext.Output.Write(result.ProductId);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteProductAsync(string productId, CancellationToken token)
        {
            if (productId.IsEmpty())
                productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

            await this.productClient.DeleteProductAsync(productId, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateProductAsync(string productId, string name, CancellationToken token)
        {
            var product = productId.IsEmpty()
                ? await this.workspaceLoader.LoadProductAsync(token)
                : await this.productClient.GetProductAsync(productId, token);

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token, product.Name);

            if (name.IsEmptyOrEquals(product.Name))
            {
                this.accessor.ExecutionContext.Output.WriteNoChange();
                return Constants.ExitCodes.Ok;
            }

            await this.productClient.UpdateProductAsync(product.ProductId, name, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

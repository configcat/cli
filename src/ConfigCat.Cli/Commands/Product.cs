using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Product
    {
        private readonly IProductClient productClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IOutput output;

        public Product(IProductClient productClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt,
            IOutput output)
        {
            this.productClient = productClient;
            this.workspaceLoader = workspaceLoader;
            this.prompt = prompt;
            this.output = output;
        }

        public async Task<int> ListAllProductsAsync(CancellationToken token)
        {
            var products = await this.productClient.GetProductsAsync(token);

            var table = new TableView<ProductModel>() { Items = products.ToList() };
            table.AddColumn(p => p.ProductId, "ID");
            table.AddColumn(p => p.Name, "NAME");
            table.AddColumn(p => $"{p.Organization.Name} [{p.Organization.OrganizationId}]", "ORGANIZATION");

            this.output.RenderView(table);

            return ExitCodes.Ok;
        }

        public async Task<int> CreateProductAsync(string organizationId, string name, CancellationToken token)
        {
            if (organizationId.IsEmpty())
                organizationId = (await this.workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token);

            var result = await this.productClient.CreateProductAsync(organizationId, name, token);
            this.output.Write(result.ProductId);

            return ExitCodes.Ok;
        }

        public async Task<int> DeleteProductAsync(string productId, CancellationToken token)
        {
            if (productId.IsEmpty())
                productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

            await this.productClient.DeleteProductAsync(productId, token);
            return ExitCodes.Ok;
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
                this.output.WriteNoChange();
                return ExitCodes.Ok;
            }

            await this.productClient.UpdateProductAsync(product.ProductId, name, token);
            return ExitCodes.Ok;
        }
    }
}

using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    internal class Product
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

        public async Task<int> ListAllProductsAsync(bool json, CancellationToken token)
        {
            var products = await this.productClient.GetProductsAsync(token);

            if (json)
            {
                this.output.RenderJson(products);
                return ExitCodes.Ok;
            }

            var itemsToRender = products.Select(p => new
            {
                Id = p.ProductId,
                Name = p.Name,
                Description = p.Description.TrimToFitColumn(),
                Organization = $"{p.Organization.Name} [{p.Organization.OrganizationId}]"
            });
            this.output.RenderTable(itemsToRender);

            return ExitCodes.Ok;
        }

        public async Task<int> CreateProductAsync(string organizationId, string name, string description, CancellationToken token)
        {
            if (organizationId.IsEmpty())
                organizationId = (await this.workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token);

            if (description.IsEmpty())
                description = await this.prompt.GetStringAsync("Description", token);

            var result = await this.productClient.CreateProductAsync(organizationId, name, description, token);
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

        public async Task<int> UpdateProductAsync(string productId, string name, string description, CancellationToken token)
        {
            var product = productId.IsEmpty()
                ? await this.workspaceLoader.LoadProductAsync(token)
                : await this.productClient.GetProductAsync(productId, token);

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token, product.Name);

            if (description.IsEmpty())
                description = await this.prompt.GetStringAsync("Description", token, product.Description);

            if (name.IsEmptyOrEquals(product.Name) && description.IsEmptyOrEquals(product.Description))
            {
                this.output.WriteNoChange();
                return ExitCodes.Ok;
            }

            await this.productClient.UpdateProductAsync(product.ProductId, name, description, token);
            return ExitCodes.Ok;
        }
    }
}
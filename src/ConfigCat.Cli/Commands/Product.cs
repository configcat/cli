using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class Product(
    IProductClient productClient,
    IWorkspaceLoader workspaceLoader,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ListAllProductsAsync(bool json, CancellationToken token)
    {
        var products = await productClient.GetProductsAsync(token);

        if (json)
        {
            output.RenderJson(products);
            return ExitCodes.Ok;
        }

        var itemsToRender = products.Select(p => new
        {
            Id = p.ProductId,
            p.Name,
            Description = p.Description.TrimToFitColumn(),
            Organization = $"{p.Organization.Name} [{p.Organization.OrganizationId}]"
        });
        output.RenderTable(itemsToRender);

        return ExitCodes.Ok;
    }

    public async Task<int> CreateProductAsync(string organizationId, string name, string description, CancellationToken token)
    {
        if (organizationId.IsEmpty())
            organizationId = (await workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token);

        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token);

        var result = await productClient.CreateProductAsync(organizationId, name, description, token);
        output.Write(result.ProductId);

        return ExitCodes.Ok;
    }

    public async Task<int> DeleteProductAsync(string productId, CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await workspaceLoader.LoadProductAsync(token)).ProductId;

        await productClient.DeleteProductAsync(productId, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateProductAsync(string productId, string name, string description, CancellationToken token)
    {
        var product = productId.IsEmpty()
            ? await workspaceLoader.LoadProductAsync(token)
            : await productClient.GetProductAsync(productId, token);

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token, product.Name);

        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token, product.Description);

        if (name.IsEmptyOrEquals(product.Name) && description.IsEmptyOrEquals(product.Description))
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        await productClient.UpdateProductAsync(product.ProductId, name, description, token);
        return ExitCodes.Ok;
    }
}
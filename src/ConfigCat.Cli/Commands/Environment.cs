using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class Environment(
    IEnvironmentClient environmentClient,
    IWorkspaceLoader workspaceLoader,
    IProductClient productClient,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ListAllEnvironmentsAsync(string productId, bool json, CancellationToken token)
    {
        var environments = new List<EnvironmentModel>();
        if (!productId.IsEmpty())
            environments.AddRange(await environmentClient.GetEnvironmentsAsync(productId, token));
        else
        {
            var products = await productClient.GetProductsAsync(token);
            foreach (var product in products)
                environments.AddRange(await environmentClient.GetEnvironmentsAsync(product.ProductId, token));
        }

        if (json)
        {
            output.RenderJson(environments);
            return ExitCodes.Ok;
        }

        var itemsToRender = environments.Select(e => new
        {
            Id = e.EnvironmentId,
            e.Name,
            Description = e.Description.TrimToFitColumn(),
            e.Color,
            Product = $"{e.Product.Name} [{e.Product.ProductId}]"
        });
        output.RenderTable(itemsToRender);
        return ExitCodes.Ok;
    }

    public async Task<int> CreateEnvironmentAsync(string productId, string name, string description, string color, CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await workspaceLoader.LoadProductAsync(token)).ProductId;

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token);

        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token);

        if (color.IsEmpty())
            color = await prompt.GetStringAsync("Color", token);

        var result = await environmentClient.CreateEnvironmentAsync(productId, name, description, color, token);
        output.Write(result.EnvironmentId);

        return ExitCodes.Ok;
    }

    public async Task<int> DeleteEnvironmentAsync(string environmentId, CancellationToken token)
    {
        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token)).EnvironmentId;

        await environmentClient.DeleteEnvironmentAsync(environmentId, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateEnvironmentAsync(string environmentId, string name, string description, string color, CancellationToken token)
    {
        var environment = environmentId.IsEmpty()
            ? await workspaceLoader.LoadEnvironmentAsync(token)
            : await environmentClient.GetEnvironmentAsync(environmentId, token);

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token, environment.Name);

        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token, environment.Description);

        if (color.IsEmpty())
            color = await prompt.GetStringAsync("Color", token, environment.Color);

        if (name.IsEmptyOrEquals(environment.Name) &&
            description.IsEmptyOrEquals(environment.Description) &&
            color.IsEmptyOrEquals(environment.Color))
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        await environmentClient.UpdateEnvironmentAsync(environment.EnvironmentId, name, description, color, token);
        return ExitCodes.Ok;
    }
}
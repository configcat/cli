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

internal class Config(
    IConfigClient configClient,
    IWorkspaceLoader workspaceLoader,
    IProductClient productClient,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ListAllConfigsAsync(string productId, bool json, CancellationToken token)
    {
        var configs = new List<ConfigModel>();
        if (!productId.IsEmpty())
            configs.AddRange(await configClient.GetConfigsAsync(productId, token));
        else
        {
            var products = await productClient.GetProductsAsync(token);
            foreach (var product in products)
                configs.AddRange(await configClient.GetConfigsAsync(product.ProductId, token));
        }

        if (json)
        {
            output.RenderJson(configs);
            return ExitCodes.Ok;
        }

        var itemsToRender = configs.Select(c => new
        {
            Id = c.ConfigId,
            c.Name,
            Description = c.Description.TrimToFitColumn(),
            Product = $"{c.Product.Name} [{c.Product.ProductId}]"
        });
        output.RenderTable(itemsToRender);

        return ExitCodes.Ok;
    }

    public async Task<int> CreateConfigAsync(string productId, string name, string evalVersion, string description, CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await workspaceLoader.LoadProductAsync(token)).ProductId;

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token);

        if (evalVersion.IsEmpty())
            evalVersion = await prompt.ChooseFromListAsync("Choose evaluation version", EvalVersion.Collection.ToList(), t => t, token);
        
        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token);

        var result = await configClient.CreateConfigAsync(productId, name, evalVersion, description, token);
        output.Write(result.ConfigId);
        return ExitCodes.Ok;
    }

    public async Task<int> DeleteConfigAsync(string configId, CancellationToken token)
    {
        if (configId.IsEmpty())
            configId = (await workspaceLoader.LoadConfigAsync(token)).ConfigId;

        await configClient.DeleteConfigAsync(configId, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateConfigAsync(string configId, string name, string description, CancellationToken token)
    {
        var config = configId.IsEmpty()
            ? await workspaceLoader.LoadConfigAsync(token)
            : await configClient.GetConfigAsync(configId, token);

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token, config.Name);

        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token, config.Description);

        if (name.IsEmptyOrEquals(config.Name) && description.IsEmptyOrEquals(config.Description))
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        await configClient.UpdateConfigAsync(config.ConfigId, name, description, token);
        return ExitCodes.Ok;
    }
}
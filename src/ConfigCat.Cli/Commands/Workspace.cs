using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Configuration;
using ConfigCat.Cli.Services.Extensions;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands;

internal class Workspace(
    IConfigurationStorage configurationStorage,
    IOutput output,
    IWorkspaceLoader workspaceLoader,
    IProductClient productClient,
    IConfigClient configClient,
    CliConfig cliConfig)
{
    public async Task<int> SetAsync(string productId, string configId, CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await workspaceLoader.LoadProductAsync(token)).ProductId;

        if (configId.IsEmpty())
            configId = (await workspaceLoader.LoadConfigAsync(productId, token)).ConfigId;

        cliConfig.Workspace = new Models.Configuration.Workspace
        {
            Config = configId,
            Product = productId
        };
        output.Write($"Updating configuration... ");
        await configurationStorage.WriteConfigAsync(cliConfig, token);
        output.WriteSuccess().WriteLine();

        return ExitCodes.Ok;
    }
    
    public async Task<int> UnSetAsync(CancellationToken token)
    {
        cliConfig.Workspace = new Models.Configuration.Workspace();
        output.Write($"Updating configuration... ");
        await configurationStorage.WriteConfigAsync(cliConfig, token);
        output.WriteSuccess().WriteLine();

        return ExitCodes.Ok;
    }
    
    public async Task<int> ShowAsync(CancellationToken token)
    {
        if (cliConfig.Workspace.IsEmpty())
        {
            output.WriteLine("Workspace is empty.");
        }
        else
        {
            var product = await productClient.GetProductAsync(cliConfig.Workspace.Product, token);
            var config = await configClient.GetConfigAsync(cliConfig.Workspace.Config, token);
            output.RenderTable([new
            {
                Product = product.Name,
                Config = config.Name
            }]);
        }

        return ExitCodes.Ok;
    }
}
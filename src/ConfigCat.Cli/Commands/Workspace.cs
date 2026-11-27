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
    CliContext cliContext)
{
    public async Task<int> SetAsync(string productId, string configId, CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await workspaceLoader.LoadProductAsync(token)).ProductId;

        if (configId.IsEmpty())
            configId = (await workspaceLoader.LoadConfigAsync(productId, token)).ConfigId;

        var workspace = new Models.Configuration.Workspace
        {
            Config = configId,
            Product = productId
        };
        cliContext.Workspace = workspace;

        var config = new CliConfig
        {
            Auth = new Auth
            {
                ApiHost = cliContext.ApiHost
            },
            Workspace = workspace
        };
        
        output.Write($"Updating configuration... ");
        await configurationStorage.WriteConfigAsync(config, token);
        output.WriteSuccess().WriteLine();

        return ExitCodes.Ok;
    }
    
    public async Task<int> UnSetAsync(CancellationToken token)
    {
        cliContext.Workspace = new Models.Configuration.Workspace();
        output.Write($"Updating configuration... ");
        await configurationStorage.WriteConfigAsync(new CliConfig
        {
            Auth = new Auth
            {
                ApiHost = cliContext.ApiHost
            },
            Workspace = new Models.Configuration.Workspace()
        }, token);
        output.WriteSuccess().WriteLine();

        return ExitCodes.Ok;
    }
    
    public async Task<int> ShowAsync(CancellationToken token)
    {
        if (cliContext.Workspace.IsEmpty())
        {
            output.WriteLine("Workspace is empty.");
        }
        else
        {
            var product = await productClient.GetProductAsync(cliContext.Workspace.Product, token);
            var config = await configClient.GetConfigAsync(cliContext.Workspace.Config, token);
            output.RenderTable([new
            {
                Product = product.Name,
                Config = config.Name
            }]);
        }

        return ExitCodes.Ok;
    }
}
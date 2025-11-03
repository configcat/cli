using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class SdkKey(
    IProductClient productClient,
    IConfigClient configClient,
    IEnvironmentClient environmentClient,
    ISdkKeyClient sdkKeyClient,
    IOutput output)
{
    public async Task<int> InvokeAsync(string configId, string environmentId, bool json, CancellationToken token)
    {
        if (!configId.IsEmpty() && !environmentId.IsEmpty())
        {
            var sdkKey = await sdkKeyClient.GetSdkKeyAsync(configId, environmentId, token);
            if (json)
            {
                output.RenderJson(sdkKey);
                return ExitCodes.Ok;
            }

            output.Write(sdkKey.Primary);
            return ExitCodes.Ok;
        }
        else
        {
            var items = new List<SdkKeyTableItem>();
            var products = await productClient.GetProductsAsync(token);

            
            foreach (var product in products)
            {
                var configs = await configClient.GetConfigsAsync(product.ProductId, token);
                var environments = await environmentClient.GetEnvironmentsAsync(product.ProductId, token);

                foreach (var config in configs)
                foreach (var environment in environments)
                    items.Add(new SdkKeyTableItem
                    {
                        Config = config,
                        Environment = environment,
                        SdkKey = await sdkKeyClient.GetSdkKeyAsync(config.ConfigId, environment.EnvironmentId, token)
                    });
            }
            
            if (json)
            {
                output.RenderJson(items);
                return ExitCodes.Ok;
            }

            var itemsToRender = items.Select(p => new
            {
                p.SdkKey.Primary,
                p.SdkKey.Secondary,
                Environment = p.Environment.Name,
                Config = p.Config.Name,
                Product = p.Config.Product.Name
            });
            output.RenderTable(itemsToRender);
            return ExitCodes.Ok;
        }
    }

    private class SdkKeyTableItem
    {
        public SdkKeyModel SdkKey { get; set; }

        public ConfigModel Config { get; set; }

        public EnvironmentModel Environment { get; set; }
    }
}
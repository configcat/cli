using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class ListAll
{
    private readonly IProductClient productClient;
    private readonly IConfigClient configClient;
    private readonly IEnvironmentClient environmentClient;
    private readonly IOutput output;

    public ListAll(IProductClient productClient,
        IConfigClient configClient,
        IEnvironmentClient environmentClient,
        IOutput output)
    {
        this.productClient = productClient;
        this.configClient = configClient;
        this.environmentClient = environmentClient;
        this.output = output;
    }

    public async Task<int> InvokeAsync(bool json, CancellationToken token)
    {

        var products = await this.productClient.GetProductsAsync(token);

        if (json)
        {
            var jsonOutput = new List<ProductJsonOutput>();
            foreach (var product in products)
            {
                var configs = await this.configClient.GetConfigsAsync(product.ProductId, token);
                var environments = await this.environmentClient.GetEnvironmentsAsync(product.ProductId, token);
                jsonOutput.Add(new ProductJsonOutput
                {
                    Configs = configs,
                    Environments = environments,
                    Name = product.Name,
                    ProductId = product.ProductId,
                    Organization = product.Organization
                });
            }

            this.output.RenderJson(jsonOutput);
            return ExitCodes.Ok;
        }

        var items = new List<ConfigEnvironment>();
        foreach (var product in products)
        {
            var configs = await this.configClient.GetConfigsAsync(product.ProductId, token);
            var environments = await this.environmentClient.GetEnvironmentsAsync(product.ProductId, token);

            items.AddRange(from config in configs
                from environment in environments
                select new ConfigEnvironment { Config = config, Environment = environment });
        }

        var itemsToRender = items.Select(p => new
        {
            Organization = $"{p.Config.Product.Organization.OrganizationId} ({p.Config.Product.Organization.Name})",
            Product = $"{p.Config.Product.ProductId} ({p.Config.Product.Name})",
            Config = $"{p.Config.ConfigId} ({p.Config.Name})",
            Environment = $"{p.Environment.EnvironmentId} ({p.Environment.Name})",
        });
        this.output.RenderTable(itemsToRender);

        return ExitCodes.Ok;
    }

    private class ConfigEnvironment
    {
        public ConfigModel Config { get; set; }

        public EnvironmentModel Environment { get; set; }
    }

    private class ProductJsonOutput : ProductModel
    {
        public IEnumerable<EnvironmentModel> Environments { get; set; }

        public IEnumerable<ConfigModel> Configs { get; set; }
    }
}
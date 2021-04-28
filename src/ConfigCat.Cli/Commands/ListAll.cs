using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class ListAll
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

        public async Task<int> InvokeAsync(CancellationToken token)
        {
            var items = new List<ConfigEnvironment>();
            var products = await this.productClient.GetProductsAsync(token);

            foreach (var product in products)
            {
                var configs = await this.configClient.GetConfigsAsync(product.ProductId, token);
                var environments = await this.environmentClient.GetEnvironmentsAsync(product.ProductId, token);

                foreach (var config in configs)
                    foreach (var environment in environments)
                        items.Add(new ConfigEnvironment
                        {
                            Config = config,
                            Environment = environment
                        });
            }

            var table = new TableView<ConfigEnvironment>() { Items = items };
            table.AddColumn(p => $"{p.Config.Product.Organization.OrganizationId} ({p.Config.Product.Organization.Name})", "ORGANIZATION");
            table.AddColumn(p => $"{p.Config.Product.ProductId} ({p.Config.Product.Name})", "PRODUCT");
            table.AddColumn(p => $"{p.Config.ConfigId} ({p.Config.Name})", "CONFIG");
            table.AddColumn(p => $"{p.Environment.EnvironmentId} ({p.Environment.Name})", "ENVIRONMENT");

            this.output.RenderView(table);

            return ExitCodes.Ok;
        }

        class ConfigEnvironment
        {
            public ConfigModel Config { get; set; }

            public EnvironmentModel Environment { get; set; }
        }
    }
}

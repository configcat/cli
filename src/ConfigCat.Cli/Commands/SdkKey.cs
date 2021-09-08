using ConfigCat.Cli.Models;
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
    class SdkKey
    {
        private readonly IProductClient productClient;
        private readonly IConfigClient configClient;
        private readonly IEnvironmentClient environmentClient;
        private readonly ISdkKeyClient sdkKeyClient;
        private readonly IOutput output;

        public SdkKey(IProductClient productClient,
            IConfigClient configClient,
            IEnvironmentClient environmentClient,
            ISdkKeyClient sdkKeyClient,
            IOutput output)
        {
            this.productClient = productClient;
            this.configClient = configClient;
            this.environmentClient = environmentClient;
            this.sdkKeyClient = sdkKeyClient;
            this.output = output;
        }

        public async Task<int> InvokeAsync(bool json, CancellationToken token)
        {
            var items = new List<SdkKeyTableItem>();
            var products = await this.productClient.GetProductsAsync(token);
            foreach (var product in products)
            {
                var configs = await this.configClient.GetConfigsAsync(product.ProductId, token);
                var environments = await this.environmentClient.GetEnvironmentsAsync(product.ProductId, token);

                foreach (var config in configs)
                    foreach (var environment in environments)
                        items.Add(new SdkKeyTableItem
                        {
                            Config = config,
                            Environment = environment,
                            SdkKey = await this.sdkKeyClient.GetSdkKeyAsync(config.ConfigId, environment.EnvironmentId, token)
                        });
            }

            if (json)
            {
                this.output.RenderJson(items);
                return ExitCodes.Ok;
            }

            var table = new TableView<SdkKeyTableItem>() { Items = items };
            table.AddColumn(k => k.SdkKey.Primary, "PRIMARY");
            table.AddColumn(k => k.SdkKey.Secondary ?? "-", "SECONDARY");
            table.AddColumn(k => k.Environment.Name, "ENVIRONMENT");
            table.AddColumn(k => k.Config.Name, "CONFIG");
            table.AddColumn(k => k.Config.Product.Name, "PRODUCT");

            this.output.RenderView(table);

            return ExitCodes.Ok;
        }

        class SdkKeyTableItem
        {
            public SdkKeyModel SdkKey { get; set; }

            public ConfigModel Config { get; set; }

            public EnvironmentModel Environment { get; set; }
        }
    }
}

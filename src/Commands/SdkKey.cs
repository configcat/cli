using ConfigCat.Cli.Api.Config;
using ConfigCat.Cli.Api.Environment;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Api.SdkKey;
using ConfigCat.Cli.Utils;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class SdkKey : IExecutableCommand
    {
        private readonly IProductClient productClient;
        private readonly IConfigClient configClient;
        private readonly IEnvironmentClient environmentClient;
        private readonly ISdkKeyClient sdkKeyClient;
        private readonly IExecutionContextAccessor accessor;

        public SdkKey(IProductClient productClient,
            IConfigClient configClient,
            IEnvironmentClient environmentClient,
            ISdkKeyClient sdkKeyClient, 
            IExecutionContextAccessor accessor)
        {
            this.productClient = productClient;
            this.configClient = configClient;
            this.environmentClient = environmentClient;
            this.sdkKeyClient = sdkKeyClient;
            this.accessor = accessor;
        }

        public string Name => "sdk-key";

        public string Description => "List sdk keys";

        public IEnumerable<string> Aliases => new[] { "k" };

        public async Task<int> InvokeAsync(CancellationToken token)
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

            var table = new TableView<SdkKeyTableItem>() { Items = items };
            table.AddColumn(k => k.SdkKey.Primary, "PRIMARY");
            table.AddColumn(k => k.SdkKey.Secondary ?? "-", "SECONDARY");
            table.AddColumn(k => k.Environment.Name, "ENVIRONMENT");
            table.AddColumn(k => k.Config.Name, "CONFIG");
            table.AddColumn(k => k.Config.Product.Name, "PRODUCT");

            this.accessor.ExecutionContext.Output.RenderView(table);

            return Constants.ExitCodes.Ok;
        }

        class SdkKeyTableItem
        {
            public SdkKeyModel SdkKey { get; set; }

            public ConfigModel Config { get; set; }

            public EnvironmentModel Environment { get; set; }
        }
    }
}

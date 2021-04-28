using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.CommandLine.Rendering.Views;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Config
    {
        private readonly IConfigClient configClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IOutput output;

        public Config(IConfigClient configClient,
            IWorkspaceLoader workspaceLoader,
            IProductClient productClient, 
            IPrompt prompt,
            IOutput output)
        {
            this.configClient = configClient;
            this.workspaceLoader = workspaceLoader;
            this.productClient = productClient;
            this.prompt = prompt;
            this.output = output;
        }

        public async Task<int> ListAllConfigsAsync(string productId, CancellationToken token)
        {
            var configs = new List<ConfigModel>();
            if (!productId.IsEmpty())
                configs.AddRange(await this.configClient.GetConfigsAsync(productId, token));
            else
            {
                var products = await this.productClient.GetProductsAsync(token);
                foreach (var product in products)
                    configs.AddRange(await this.configClient.GetConfigsAsync(product.ProductId, token));
            }

            var table = new TableView<ConfigModel>() { Items = configs };
            table.AddColumn(c => c.ConfigId, "ID");
            table.AddColumn(c => c.Name, "NAME");
            table.AddColumn(c => $"{c.Product.Name} [{c.Product.ProductId}]", "PRODUCT");

            this.output.RenderView(table);

            return ExitCodes.Ok;
        }

        public async Task<int> CreateConfigAsync(string productId, string name, CancellationToken token)
        {
            if (productId.IsEmpty())
                productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token);

            var result = await this.configClient.CreateConfigAsync(productId, name, token);
            this.output.Write(result.ConfigId);
            return ExitCodes.Ok;
        }

        public async Task<int> DeleteConfigAsync(string configId, CancellationToken token)
        {
            if (configId.IsEmpty())
                configId = (await this.workspaceLoader.LoadConfigAsync(token)).ConfigId;

            await this.configClient.DeleteConfigAsync(configId, token);
            return ExitCodes.Ok;
        }

        public async Task<int> UpdateConfigAsync(string configId, string name, CancellationToken token)
        {
            var config = configId.IsEmpty() 
                ? await this.workspaceLoader.LoadConfigAsync(token)
                : await this.configClient.GetConfigAsync(configId, token);

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token, config.Name);

            if (name.IsEmptyOrEquals(config.Name))
            {
                this.output.WriteNoChange();
                return ExitCodes.Ok;
            }

            await this.configClient.UpdateConfigAsync(config.ConfigId, name, token);
            return ExitCodes.Ok;
        }
    }
}

using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    internal class Config
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

        public async Task<int> ListAllConfigsAsync(string productId, bool json, CancellationToken token)
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

            if (json)
            {
                this.output.RenderJson(configs);
                return ExitCodes.Ok;
            }

            var itemsToRender = configs.Select(c => new { Id = c.ConfigId, Name = c.Name, Description = c.Description, Product = $"{c.Product.Name} [{c.Product.ProductId}]" });
            this.output.RenderTable(itemsToRender);

            return ExitCodes.Ok;
        }

        public async Task<int> CreateConfigAsync(string productId, string name, string description, CancellationToken token)
        {
            if (productId.IsEmpty())
                productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token);

            if (description.IsEmpty())
                description = await this.prompt.GetStringAsync("Description", token);

            var result = await this.configClient.CreateConfigAsync(productId, name, description, token);
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

        public async Task<int> UpdateConfigAsync(string configId, string name, string description, CancellationToken token)
        {
            var config = configId.IsEmpty() 
                ? await this.workspaceLoader.LoadConfigAsync(token)
                : await this.configClient.GetConfigAsync(configId, token);

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token, config.Name);

            if (description.IsEmpty())
                description = await this.prompt.GetStringAsync("Description", token, config.Description);

            if (name.IsEmptyOrEquals(config.Name) && description.IsEmptyOrEquals(config.Description))
            {
                this.output.WriteNoChange();
                return ExitCodes.Ok;
            }

            await this.configClient.UpdateConfigAsync(config.ConfigId, name, description, token);
            return ExitCodes.Ok;
        }
    }
}

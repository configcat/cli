using ConfigCat.Cli.Api;
using ConfigCat.Cli.Api.Config;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering.Views;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Config : ICommandDescriptor
    {
        private readonly IConfigClient configClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public Config(IConfigClient configClient,
            IWorkspaceLoader workspaceLoader,
            IProductClient productClient, 
            IPrompt prompt, 
            IExecutionContextAccessor accessor)
        {
            this.configClient = configClient;
            this.workspaceLoader = workspaceLoader;
            this.productClient = productClient;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "config";

        public string Description => "Manage configs";

        public IEnumerable<string> Aliases => new[] { "c" };

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "ls",
                Description = "List all configs",
                Handler = this.CreateHandler(nameof(Config.ListAllConfigsAsync)),
                Options = new[]
                {
                    new Option<string>(new string[] { "--product-id", "-p" }, "Show only a product's configs"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "create",
                Aliases = new[] { "cr" },
                Description = "Create config",
                Handler = this.CreateHandler(nameof(Config.CreateConfigAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--product-id", "-p" }, "ID of the product where the config must be created"),
                    new Option<string>(new[] { "--name", "-n" }, "Name of the new config"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete config",
                Handler = this.CreateHandler(nameof(Config.DeleteConfigAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--config-id", "-i" }, "ID of the config to delete"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Aliases = new[] { "up" },
                Description = "Update Config",
                Handler = this.CreateHandler(nameof(Config.UpdateConfigAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--config-id", "-i" }, "ID of the config to update"),
                    new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                }
            },
        };

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

            this.accessor.ExecutionContext.Output.RenderView(table);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> CreateConfigAsync(string productId, string name, CancellationToken token)
        {
            if (productId.IsEmpty())
                productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token);

            var result = await this.configClient.CreateConfigAsync(productId, name, token);
            this.accessor.ExecutionContext.Output.Write(result.ConfigId);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteConfigAsync(string configId, CancellationToken token)
        {
            if (configId.IsEmpty())
                configId = (await this.workspaceLoader.LoadConfigAsync(token)).ConfigId;

            await this.configClient.DeleteConfigAsync(configId, token);
            return Constants.ExitCodes.Ok;
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
                this.accessor.ExecutionContext.Output.WriteNoChange();
                return Constants.ExitCodes.Ok;
            }

            await this.configClient.UpdateConfigAsync(config.ConfigId, name, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

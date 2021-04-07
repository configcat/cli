using ConfigCat.Cli.Api.Config;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Config : ICommandDescriptor
    {
        private readonly IConfigClient configClient;
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public Config(IConfigClient configClient, 
            IProductClient productClient, 
            IPrompt prompt, 
            IExecutionContextAccessor accessor)
        {
            this.configClient = configClient;
            this.productClient = productClient;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "config";

        public string Description => "Manage configs";

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "ls",
                Description = "List all configs",
                Handler = this.CreateHandler(nameof(Config.ListAllConfigsAsync)),
                Options = new[]
                {
                    new Option<string>(new string[] { "--product-id", "-p" }) { Description = "Show only a product's configs" },
                }
            },
            new SubCommandDescriptor
            {
                Name = "create",
                Description = "Create config",
                Handler = this.CreateHandler(nameof(Config.CreateConfigAsync)),
                Arguments = new[]
                {
                    new Argument<string>("product-id") { Description = "ID of the product where the config must be created" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--name", "-n" }) { Description = "Name of the new config" },
                }
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete config",
                Handler = this.CreateHandler(nameof(Config.DeleteConfigAsync)),
                Arguments = new[]
                {
                     new Argument<string>("config-id") { Description = $"ID of the config to delete" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Description = "Update Config",
                Handler = this.CreateHandler(nameof(Config.UpdateConfigAsync)),
                Arguments = new[]
                {
                     new Argument<string>("config-id") { Description = "ID of the config to update" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--name", "-n" }) { Description = "The updated name" },
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

            var console = this.accessor.ExecutionContext.Output.Console;
            var renderer = new ConsoleRenderer(console, resetAfterRender: true);
            table.RenderFitToContent(renderer, console);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> CreateConfigAsync(string productId, string name, CancellationToken token)
        {
            if (!token.IsCancellationRequested && name.IsEmpty())
                name = this.prompt.GetString("Config name");

            var result = await this.configClient.CreateConfigAsync(productId, name, token);
            this.accessor.ExecutionContext.Output.Write(result.ConfigId);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteConfigAsync(string configId, CancellationToken token)
        {
            await this.configClient.DeleteConfigAsync(configId, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateConfigAsync(string configId, string name, CancellationToken token)
        {
            if (!token.IsCancellationRequested && name.IsEmpty())
                name = this.prompt.GetString("Config name");

            await this.configClient.UpdateConfigAsync(configId, name, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

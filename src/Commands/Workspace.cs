using ConfigCat.Cli.Api.Config;
using ConfigCat.Cli.Api.Environment;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Api.Tag;
using ConfigCat.Cli.Configuration;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Workspace : ICommandDescriptor
    {
        private readonly IConfigClient configClient;
        private readonly IProductClient productClient;
        private readonly IEnvironmentClient environmentClient;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;
        private readonly IConfigurationProvider configurationProvider;

        public Workspace(IConfigClient configClient,
            IProductClient productClient,
            IEnvironmentClient environmentClient,
            IPrompt prompt,
            IExecutionContextAccessor accessor,
            IConfigurationProvider configurationProvider)
        {
            this.configClient = configClient;
            this.productClient = productClient;
            this.environmentClient = environmentClient;
            this.prompt = prompt;
            this.accessor = accessor;
            this.configurationProvider = configurationProvider;
        }

        public string Name => "workspace";

        public string Description => "Manage CLI workspaces";

        public IEnumerable<string> Aliases => new[] { "ws" };

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "setup",
                Description = "Setup a new workspace",
                Handler = this.CreateHandler(nameof(Workspace.SetupAsync)),
                Options = new Option[]
                {
                    new Option<string>(new[] { "--config-id", "-c" }, "ID of the config used in the workspace"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "show",
                Aliases = new[] { "sh", "pr", "print" },
                Description = "Show the current workspace",
                Handler = this.CreateHandler(nameof(Workspace.ShowAsync)),
            },
        };

        public async Task<int> SetupAsync(string configId, CancellationToken token)
        {
            if (configId.IsEmpty())
            {
                var products = await this.productClient.GetProductsAsync(token);
                var product = await this.prompt.ChooseFromListAsync("Choose product",
                    products.ToList(), p => $"{p.Name} ({p.Organization.Name})", token);

                var configs = await this.configClient.GetConfigsAsync(product.ProductId, token);
                var config = await this.prompt.ChooseFromListAsync("Choose config", configs.ToList(), c => c.Name, token);
                configId = config.ConfigId;
            }

            this.accessor.ExecutionContext.Config.Workspace = new Configuration.Workspace
            {
                ConfigId = configId
            };

            await this.configurationProvider.SaveConfigAsync(this.accessor.ExecutionContext.Config, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> ShowAsync(CancellationToken token)
        {
            
            return Constants.ExitCodes.Ok;
        }
    }
}

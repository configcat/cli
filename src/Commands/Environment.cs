using ConfigCat.Cli.Api.Environment;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Configuration;
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
    class Environment : ICommandDescriptor
    {
        private readonly IEnvironmentClient environmentClient;
        private readonly IWorkspaceManager workspaceManager;
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public Environment(IEnvironmentClient environmentClient,
            IWorkspaceManager workspaceManager,
            IProductClient productClient,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.environmentClient = environmentClient;
            this.workspaceManager = workspaceManager;
            this.productClient = productClient;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "environment";

        public string Description => "Manage environments";

        public IEnumerable<string> Aliases => new[] { "e" };

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "ls",
                Description = "List all environments",
                Handler = this.CreateHandler(nameof(Environment.ListAllEnvironmentsAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--product-id", "-p" }, "Show only a product's environments"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "create",
                Aliases = new[] { "cr" },
                Description = "Create Environment",
                Handler = this.CreateHandler(nameof(Environment.CreateEnvironmentAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--product-id", "-p" }, "ID of the product where the environment must be created"),
                    new Option<string>(new[] { "--name", "-n" }, "Name of the new environment"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete Environment",
                Handler = this.CreateHandler(nameof(Environment.DeleteEnvironmentAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--environment-id", "-i" }, "ID of the environment to delete"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Aliases = new[] { "up" },
                Description = "Update Environment",
                Handler = this.CreateHandler(nameof(Environment.UpdateEnvironmentAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--environment-id", "-i" }, "ID of the environment to update"),
                    new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                }
            },
        };

        public async Task<int> ListAllEnvironmentsAsync(string productId, CancellationToken token)
        {
            var environments = new List<EnvironmentModel>();
            if (!productId.IsEmpty())
                environments.AddRange(await this.environmentClient.GetEnvironmentsAsync(productId, token));
            else
            {
                var products = await this.productClient.GetProductsAsync(token);
                foreach (var product in products)
                    environments.AddRange(await this.environmentClient.GetEnvironmentsAsync(product.ProductId, token));
            }

            var table = new TableView<EnvironmentModel>() { Items = environments };
            table.AddColumn(e => e.EnvironmentId, "ID");
            table.AddColumn(e => e.Name, "NAME");
            table.AddColumn(e => $"{e.Product.Name} [{e.Product.ProductId}]", "PRODUCT");

            this.accessor.ExecutionContext.Output.RenderView(table);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> CreateEnvironmentAsync(string productId, string name, CancellationToken token)
        {
            if (productId.IsEmpty())
                productId = (await this.workspaceManager.LoadProductAsync(token)).ProductId;

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token);

            var result = await this.environmentClient.CreateEnvironmentAsync(productId, name, token);
            this.accessor.ExecutionContext.Output.Write(result.EnvironmentId);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteEnvironmentAsync(string environmentId, CancellationToken token)
        {
            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceManager.LoadEnvironmentAsync(token)).EnvironmentId;

            await this.environmentClient.DeleteEnvironmentAsync(environmentId, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateEnvironmentAsync(string environmentId, string name, CancellationToken token)
        {
            var environment = environmentId.IsEmpty()
                ? await this.workspaceManager.LoadEnvironmentAsync(token)
                : await this.environmentClient.GetEnvironmentAsync(environmentId, token);

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token, environment.Name);

            if (name.IsEmptyOrEquals(environment.Name))
            {
                this.accessor.ExecutionContext.Output.WriteNoChange();
                return Constants.ExitCodes.Ok;
            }

            await this.environmentClient.UpdateEnvironmentAsync(environmentId, name, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

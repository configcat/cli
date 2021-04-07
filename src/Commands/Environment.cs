using ConfigCat.Cli.Api.Environment;
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
    class Environment : ICommandDescriptor
    {
        private readonly IEnvironmentClient environmentClient;
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public Environment(IEnvironmentClient environmentClient, 
            IProductClient productClient,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.environmentClient = environmentClient;
            this.productClient = productClient;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "environment";

        public string Description => "Manage environments";

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "ls",
                Description = "List all environments",
                Handler = this.CreateHandler(nameof(Environment.ListAllEnvironmentsAsync)),
                Options = new[]
                {
                    new Option<string>(new[] { "--product-id", "-p" }) { Description = "Show only a product's environments" },
                }
            },
            new SubCommandDescriptor
            {
                Name = "create",
                Description = "Create Environment",
                Handler = this.CreateHandler(nameof(Environment.CreateEnvironmentAsync)),
                Arguments = new[]
                {
                    new Argument<string>("product-id") { Description = "ID of the product where the environment must be created" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--name", "-n" }) { Description = "Name of the new environment" },
                }
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete Environment",
                Handler = this.CreateHandler(nameof(Environment.DeleteEnvironmentAsync)),
                Arguments = new[]
                {
                     new Argument<string>("environment-id") { Description = "ID of the environment to delete" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Description = "Update Environment",
                Handler = this.CreateHandler(nameof(Environment.UpdateEnvironmentAsync)),
                Arguments = new[]
                {
                     new Argument<string>("environment-id") { Description = "ID of the environment to update" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--name", "-n" }) { Description = "The updated name" },
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

            var console = this.accessor.ExecutionContext.Output.Console;
            var renderer = new ConsoleRenderer(console, resetAfterRender: true);
            table.RenderFitToContent(renderer, console);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> CreateEnvironmentAsync(string productId, string name, CancellationToken token)
        {
            if (!token.IsCancellationRequested && name.IsEmpty())
                name = this.prompt.GetString("Environment name");

            var result = await this.environmentClient.CreateEnvironmentAsync(productId, name, token);
            this.accessor.ExecutionContext.Output.Write(result.EnvironmentId);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteEnvironmentAsync(string environmentId, CancellationToken token)
        {
            await this.environmentClient.DeleteEnvironmentAsync(environmentId, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateEnvironmentAsync(string environmentId, string name, CancellationToken token)
        {
            if (!token.IsCancellationRequested && name.IsEmpty())
                name = this.prompt.GetString("Environment name");

            await this.environmentClient.UpdateEnvironmentAsync(environmentId, name, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

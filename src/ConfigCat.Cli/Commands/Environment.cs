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
    class Environment
    {
        private readonly IEnvironmentClient environmentClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IOutput output;

        public Environment(IEnvironmentClient environmentClient,
            IWorkspaceLoader workspaceLoader,
            IProductClient productClient,
            IPrompt prompt,
            IOutput output)
        {
            this.environmentClient = environmentClient;
            this.workspaceLoader = workspaceLoader;
            this.productClient = productClient;
            this.prompt = prompt;
            this.output = output;
        }

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

            this.output.RenderView(table);

            return ExitCodes.Ok;
        }

        public async Task<int> CreateEnvironmentAsync(string productId, string name, CancellationToken token)
        {
            if (productId.IsEmpty())
                productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token);

            var result = await this.environmentClient.CreateEnvironmentAsync(productId, name, token);
            this.output.Write(result.EnvironmentId);

            return ExitCodes.Ok;
        }

        public async Task<int> DeleteEnvironmentAsync(string environmentId, CancellationToken token)
        {
            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token)).EnvironmentId;

            await this.environmentClient.DeleteEnvironmentAsync(environmentId, token);
            return ExitCodes.Ok;
        }

        public async Task<int> UpdateEnvironmentAsync(string environmentId, string name, CancellationToken token)
        {
            var environment = environmentId.IsEmpty()
                ? await this.workspaceLoader.LoadEnvironmentAsync(token)
                : await this.environmentClient.GetEnvironmentAsync(environmentId, token);

            if (name.IsEmpty())
                name = await this.prompt.GetStringAsync("Name", token, environment.Name);

            if (name.IsEmptyOrEquals(environment.Name))
            {
                this.output.WriteNoChange();
                return ExitCodes.Ok;
            }

            await this.environmentClient.UpdateEnvironmentAsync(environment.EnvironmentId, name, token);
            return ExitCodes.Ok;
        }
    }
}

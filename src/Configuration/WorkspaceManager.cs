using ConfigCat.Cli.Api.Config;
using ConfigCat.Cli.Api.Environment;
using ConfigCat.Cli.Api.Flag;
using ConfigCat.Cli.Api.Organization;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Api.Tag;
using ConfigCat.Cli.Exceptions;
using ConfigCat.Cli.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Configuration
{
    interface IWorkspaceManager
    {
        Task<OrganizationModel> LoadOrganizationAsync(CancellationToken token);

        Task<ProductModel> LoadProductAsync(CancellationToken token);

        Task<ConfigModel> LoadConfigAsync(CancellationToken token);

        Task<EnvironmentModel> LoadEnvironmentAsync(CancellationToken token, string configId = null);

        Task<TagModel> LoadTagAsync(CancellationToken token);

        Task<FlagModel> LoadFlagAsync(CancellationToken token);

        Task<List<TagModel>> LoadTagsAsync(CancellationToken token, string configId = null, List<TagModel> defaultTags = null);
    }

    class WorkspaceManager : IWorkspaceManager
    {
        private readonly IConfigClient configClient;
        private readonly IOrganizationClient organizationClient;
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IEnvironmentClient environmentClient;
        private readonly ITagClient tagClient;
        private readonly IFlagClient flagClient;
        private readonly IExecutionContextAccessor executionContextAccessor;

        public WorkspaceManager(IConfigClient configClient,
            IOrganizationClient organizationClient,
            IProductClient productClient,
            IEnvironmentClient environmentClient,
            ITagClient tagClient,
            IFlagClient flagClient,
            IPrompt prompt,
            IExecutionContextAccessor executionContextAccessor)
        {
            this.configClient = configClient;
            this.organizationClient = organizationClient;
            this.productClient = productClient;
            this.prompt = prompt;
            this.environmentClient = environmentClient;
            this.tagClient = tagClient;
            this.flagClient = flagClient;
            this.executionContextAccessor = executionContextAccessor;
        }

        public async Task<OrganizationModel> LoadOrganizationAsync(CancellationToken token)
        {
            if (this.executionContextAccessor.ExecutionContext.Config.Workspace is null)
            {
                var organizations = await this.organizationClient.GetOrganizationsAsync(token);
                var selected = await this.prompt.ChooseFromListAsync("Choose organization", organizations.ToList(), o => o.Name, token);
                if (selected == null)
                    this.ThrowHelpException("Organization ID", "--organization-id");

                return selected;
            }

            var output = this.executionContextAccessor.ExecutionContext.Output;
            output.Verbose($"Loading organization from workspace...");
            var config = await this.configClient.GetConfigAsync(this.executionContextAccessor.ExecutionContext.Config.Workspace.ConfigId, token);
            output.Verbose($"Organization '{config.Product.Organization.Name}' loaded");
            return config.Product.Organization;
        }

        public async Task<ProductModel> LoadProductAsync(CancellationToken token)
        {
            if (this.executionContextAccessor.ExecutionContext.Config.Workspace is null)
            {
                var products = await this.productClient.GetProductsAsync(token);
                var selected = await this.prompt.ChooseFromListAsync("Choose product", products.ToList(), p => $"{p.Name} ({p.Organization.Name})", token);
                if (selected == null)
                    this.ThrowHelpException("Product ID", "--product-id");

                return selected;
            }

            var output = this.executionContextAccessor.ExecutionContext.Output;
            output.Verbose($"Loading product from workspace...");
            var config = await this.configClient.GetConfigAsync(this.executionContextAccessor.ExecutionContext.Config.Workspace.ConfigId, token);
            output.Verbose($"Product '{config.Product.Name}' loaded");
            return config.Product;
        }

        public async Task<ConfigModel> LoadConfigAsync(CancellationToken token)
        {
            if (this.executionContextAccessor.ExecutionContext.Config.Workspace is null)
            {
                var products = await this.productClient.GetProductsAsync(token);
                var configs = new List<ConfigModel>();
                foreach (var product in products)
                    configs.AddRange(await this.configClient.GetConfigsAsync(product.ProductId, token));

                var selected = await this.prompt.ChooseFromListAsync("Choose config", configs.ToList(), c => $"{c.Name} ({c.Product.Name})", token);
                if (selected == null)
                    this.ThrowHelpException("Config ID", "--config-id");

                return selected;
            }

            var output = this.executionContextAccessor.ExecutionContext.Output;
            output.Verbose($"Loading config from workspace...");
            var config = await this.configClient.GetConfigAsync(this.executionContextAccessor.ExecutionContext.Config.Workspace.ConfigId, token);
            output.Verbose($"Config '{config.Name}' loaded");
            return config;
        }

        public async Task<EnvironmentModel> LoadEnvironmentAsync(CancellationToken token, string configId = null)
        {
            if (this.executionContextAccessor.ExecutionContext.Config.Workspace is null)
            {
                var environments = new List<EnvironmentModel>();
                if (configId == null)
                {
                    var products = await this.productClient.GetProductsAsync(token);
                    foreach (var product in products)
                        environments.AddRange(await this.environmentClient.GetEnvironmentsAsync(product.ProductId, token));
                }
                else
                {
                    var config = await this.configClient.GetConfigAsync(configId, token);
                    environments = (await this.environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token)).ToList();
                }

                var selected = await this.prompt.ChooseFromListAsync("Choose environment", environments.ToList(), e => e.Name, token);
                if (selected == null)
                    this.ThrowHelpExceptionWithoutDetails("Environment ID", "--environment-id");

                return selected;
            }

            var output = this.executionContextAccessor.ExecutionContext.Output;
            output.Verbose($"Loading environment from workspace...");
            var fromWorkspace = await this.LoadEnvironmentFromWorkspaceAsync(token);
            output.Verbose($"Environment '{fromWorkspace.Name}' loaded");
            return fromWorkspace;
        }

        public async Task<TagModel> LoadTagAsync(CancellationToken token)
        {
            if (this.executionContextAccessor.ExecutionContext.Config.Workspace is null)
            {
                var products = await this.productClient.GetProductsAsync(token);
                var tags = new List<TagModel>();
                foreach (var product in products)
                    tags.AddRange(await this.tagClient.GetTagsAsync(product.ProductId, token));

                var selected = await this.prompt.ChooseFromListAsync("Choose tag", tags.ToList(), t => $"{t.Name} ({t.Product.Name})", token);
                if (selected == null)
                    this.ThrowHelpExceptionWithoutDetails("Tag ID", "--tag-id");

                return selected;
            }
            var output = this.executionContextAccessor.ExecutionContext.Output;
            output.Verbose($"Loading tag from workspace...");
            var fromWorkspace = await this.LoadTagFromWorkspaceAsync(token);
            output.Verbose($"Tag '{fromWorkspace.Name}' loaded");
            return fromWorkspace;
        }

        public async Task<FlagModel> LoadFlagAsync(CancellationToken token)
        {
            if (this.executionContextAccessor.ExecutionContext.Config.Workspace is null)
            {
                var flags = new List<FlagModel>();
                var products = await this.productClient.GetProductsAsync(token);
                foreach (var product in products)
                {
                    var configs = await this.configClient.GetConfigsAsync(product.ProductId, token);
                    foreach (var config in configs)
                        flags.AddRange(await this.flagClient.GetFlagsAsync(config.ConfigId, token));
                }

                var selected = await this.prompt.ChooseFromListAsync("Choose flag", flags.ToList(), f => $"{f.Name} ({f.ConfigName})", token);
                if (selected == null)
                    this.ThrowHelpExceptionWithoutDetails("Flag or setting ID", "--flag-id or --setting-id");

                return selected;
            }

            var output = this.executionContextAccessor.ExecutionContext.Output;
            output.Verbose($"Loading flag from workspace...");
            var fromWorkspace = await this.LoadFlagFromWorkspaceAsync(token);
            output.Verbose($"Flag '{fromWorkspace.Name}' loaded");
            return fromWorkspace;
        }

        public async Task<List<TagModel>> LoadTagsAsync(CancellationToken token, string configId = null, List<TagModel> defaultTags = null)
        {
            if (this.executionContextAccessor.ExecutionContext.Config.Workspace is null)
            {
                var tags = new List<TagModel>();
                if (configId == null)
                {
                    var products = await this.productClient.GetProductsAsync(token);
                    foreach (var product in products)
                        tags.AddRange(await this.tagClient.GetTagsAsync(product.ProductId, token));
                }
                else
                {
                    var config = await this.configClient.GetConfigAsync(configId, token);
                    tags = (await this.tagClient.GetTagsAsync(config.Product.ProductId, token)).ToList();
                }

                var selected = await this.prompt.ChooseMultipleFromListAsync("Choose tags", tags.ToList(), t => t.Name, token, defaultTags);
                if (selected == null)
                    this.ThrowHelpExceptionWithoutDetails("Tag IDs", "--tag-ids");

                return selected;
            }

            var output = this.executionContextAccessor.ExecutionContext.Output;
            output.Verbose($"Loading tags from workspace...");
            var fromWorkspace = await this.LoadTagsFromWorkspaceAsync(token, defaultTags);
            output.Verbose($"{fromWorkspace.Count} tags loaded");
            return fromWorkspace;
        }

        private async Task<EnvironmentModel> LoadEnvironmentFromWorkspaceAsync(CancellationToken token)
        {
            var config = await this.configClient.GetConfigAsync(this.executionContextAccessor.ExecutionContext.Config.Workspace.ConfigId, token);
            var environments = await this.environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token);
            var selected = await this.prompt.ChooseFromListAsync("Choose environment", environments.ToList(), e => e.Name, token);
            if (selected == null)
                this.ThrowHelpExceptionWithoutDetails("Environment ID", "--environment-id");

            return selected;
        }

        private async Task<TagModel> LoadTagFromWorkspaceAsync(CancellationToken token)
        {
            var config = await this.configClient.GetConfigAsync(this.executionContextAccessor.ExecutionContext.Config.Workspace.ConfigId, token);
            var tags = await this.tagClient.GetTagsAsync(config.Product.ProductId, token);
            var selected = await this.prompt.ChooseFromListAsync("Choose tag", tags.ToList(), t => $"{t.Name} ({t.Product.Name})", token);
            if (selected == null)
                this.ThrowHelpExceptionWithoutDetails("Tag ID", "--tag-id");

            return selected;
        }

        private async Task<FlagModel> LoadFlagFromWorkspaceAsync(CancellationToken token)
        {
            var config = await this.configClient.GetConfigAsync(this.executionContextAccessor.ExecutionContext.Config.Workspace.ConfigId, token);
            var flags = await this.flagClient.GetFlagsAsync(config.ConfigId, token);
            var selected = await this.prompt.ChooseFromListAsync("Choose flag", flags.ToList(), f => $"{f.Name} ({f.Key})", token);
            if (selected == null)
                this.ThrowHelpExceptionWithoutDetails("Flag or setting ID", "--flag-id or --setting-id");

            return selected;
        }

        private async Task<List<TagModel>> LoadTagsFromWorkspaceAsync(CancellationToken token, List<TagModel> defaultTags)
        {
            var config = await this.configClient.GetConfigAsync(this.executionContextAccessor.ExecutionContext.Config.Workspace.ConfigId, token);
            var tags = await this.tagClient.GetTagsAsync(config.Product.ProductId, token);
            var selected = await this.prompt.ChooseMultipleFromListAsync("Choose tags", tags.ToList(), t => t.Name, token, defaultTags);
            if (selected == null)
                this.ThrowHelpExceptionWithoutDetails("Tag IDs", "--tag-ids");

            return selected;
        }

        private void ThrowHelpException(string label, string argument) =>
            throw new ShowHelpException($"{label} is not set. Use the {argument} option or set up a workspace with the 'configcat ws setup' command.");

        private void ThrowHelpExceptionWithoutDetails(string label, string argument) =>
            throw new ShowHelpException($"{label} is not set. Use the {argument} option.");
    }
}

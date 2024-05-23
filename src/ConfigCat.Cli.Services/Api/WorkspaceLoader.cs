using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Configuration;

namespace ConfigCat.Cli.Services.Api;

public interface IWorkspaceLoader
{
    Task<OrganizationModel> LoadOrganizationAsync(CancellationToken token);

    Task<ProductModel> LoadProductAsync(CancellationToken token);

    Task<ConfigModel> LoadConfigAsync(CancellationToken token);

    Task<ConfigModel> LoadConfigAsync(string productId, CancellationToken token);

    Task<SegmentModel> LoadSegmentAsync(CancellationToken token);

    Task<WebhookModel> LoadWebhookAsync(CancellationToken token);

    Task<EnvironmentModel> LoadEnvironmentAsync(CancellationToken token, string configId = null);

    Task<TagModel> LoadTagAsync(CancellationToken token);

    Task<FlagModel> LoadFlagAsync(CancellationToken token);

    Task<PermissionGroupModel> LoadPermissionGroupAsync(CancellationToken token);

    Task<bool> NeedsReasonAsync(string environmentId, CancellationToken token);

    Task<List<TagModel>> LoadTagsAsync(CancellationToken token, string configId = null, List<TagModel> defaultTags = null, bool optional = false);
}

public class WorkspaceLoader(
    IConfigClient configClient,
    IOrganizationClient organizationClient,
    IProductClient productClient,
    IEnvironmentClient environmentClient,
    ISegmentClient segmentClient,
    ITagClient tagClient,
    IFlagClient flagClient,
    IWebhookClient webhookClient,
    IPermissionGroupClient permissionGroupClient,
    IPrompt prompt,
    IOutput output,
    CliConfig cliConfig)
    : IWorkspaceLoader
{
    public async Task<OrganizationModel> LoadOrganizationAsync(CancellationToken token)
    {
        var organizations = await organizationClient.GetOrganizationsAsync(token);
        var selected = await prompt.ChooseFromListAsync("Choose organization", organizations.ToList(), o => o.Name, token);
        if (selected == null)
            throw CreateHelpException("--organization-id");

        return selected;
    }

    public async Task<ProductModel> LoadProductAsync(CancellationToken token)
    {
        var products = await PreloadProducts(token);

        if (products.Count == 1) return products[0];
        
        var selected = await prompt.ChooseFromListAsync("Choose product", products.ToList(), p => $"{p.Name} ({p.Organization.Name})", token);
        if (selected == null)
            throw CreateHelpException("--product-id");

        return selected;
    }

    public async Task<ConfigModel> LoadConfigAsync(CancellationToken token)
    {
        var products = await PreloadProducts(token);

        var configs = new List<ConfigModel>();
        if (products.Count == 1)
        {
            configs = await PreloadConfigs(products[0].ProductId, token);
            if (configs.Count == 1) return configs[0];
        }
        else
        {
            foreach (var product in products)
                configs.AddRange(await configClient.GetConfigsAsync(product.ProductId, token));
        }

        if (configs.Count == 0)
            throw CreateInformalException("config", "config create");

        var selected = await prompt.ChooseFromListAsync("Choose config", configs.ToList(), c => $"{c.Name} ({c.Product.Name})", token);
        if (selected == null)
            throw CreateHelpException("--config-id");

        return selected;
    }
    
    public async Task<ConfigModel> LoadConfigAsync(string productId, CancellationToken token)
    {
        var configs = (await configClient.GetConfigsAsync(productId, token)).ToList();
        if (configs.Count == 0)
            throw CreateInformalException("config", "config create");

        var selected = await prompt.ChooseFromListAsync("Choose config", configs.ToList(), c => $"{c.Name} ({c.Product.Name})", token);
        if (selected == null)
            throw CreateHelpException("--config-id");

        return selected;
    }

    public async Task<PermissionGroupModel> LoadPermissionGroupAsync(CancellationToken token)
    {
        var products = await PreloadProducts(token);
        var permissionGroups = new List<PermissionGroupModel>();
        foreach (var product in products)
            permissionGroups.AddRange(await permissionGroupClient.GetPermissionGroupsAsync(product.ProductId, token));

        if (permissionGroups.Count == 0)
            throw CreateInformalException("permission-group", "permission-group create");

        var selected = await prompt.ChooseFromListAsync("Choose permission group",
            permissionGroups.ToList(), c => $"{c.Name} ({c.Product.Name})", token);
        if (selected == null)
            throw CreateHelpException("--permission-group-id");

        return selected;
    }

    public async Task<bool> NeedsReasonAsync(string environmentId, CancellationToken token)
    {
        var environment = await environmentClient.GetEnvironmentAsync(environmentId, token);
        var preferences = await productClient.GetProductPreferencesAsync(environment.Product.ProductId, token);
        return preferences.ReasonRequired ||
            preferences.ReasonRequiredEnvironments.Any(e => e.EnvironmentId == environmentId && e.ReasonRequired);
    }

    public async Task<SegmentModel> LoadSegmentAsync(CancellationToken token)
    {
        var products = await PreloadProducts(token);
        var segments = new List<SegmentModel>();
        foreach (var product in products)
            segments.AddRange(await segmentClient.GetSegmentsAsync(product.ProductId, token));

        if (segments.Count == 0)
            throw CreateInformalException("segment", "segment create");

        var selected = await prompt.ChooseFromListAsync("Choose segment", segments.ToList(), s => $"{s.Name} ({s.Product.Name})", token);
        if (selected == null)
            throw CreateHelpException("--segment-id");

        return await segmentClient.GetSegmentAsync(selected.SegmentId, token);
    }
    
    public async Task<WebhookModel> LoadWebhookAsync(CancellationToken token)
    {
        var products = await PreloadProducts(token);
        var webhooks = new List<WebhookModel>();
        foreach (var product in products)
            webhooks.AddRange(await webhookClient.GetWebhooksAsync(product.ProductId, token));

        if (webhooks.Count == 0)
            throw CreateInformalException("webhook", "webhook create");

        var selected = await prompt.ChooseFromListAsync("Choose webhook", 
            webhooks.ToList(), 
            w => $"{w.HttpMethod.ToUpper()} {w.Url.TrimToLength(30)} ({w.Config.Name} / {w.Environment.Name})", token);
        if (selected == null)
            throw CreateHelpException("--webhook-id");

        return await webhookClient.GetWebhookAsync(selected.WebhookId, token);
    }

    public async Task<EnvironmentModel> LoadEnvironmentAsync(CancellationToken token, string configId = null)
    {
        var environments = new List<EnvironmentModel>();
        if (configId == null)
        {
            var products = await PreloadProducts(token);
            foreach (var product in products)
                environments.AddRange(await environmentClient.GetEnvironmentsAsync(product.ProductId, token));
        }
        else
        {
            var config = await configClient.GetConfigAsync(configId, token);
            environments = (await environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token)).ToList();
        }

        if (environments.Count == 0)
            throw CreateInformalException("environment", "environment create");

        var selected = await prompt.ChooseFromListAsync("Choose environment", environments.ToList(), e => $"{e.Name} ({e.Product.Name})", token);
        if (selected == null)
            throw CreateHelpException("--environment-id");

        return selected;
    }

    public async Task<TagModel> LoadTagAsync(CancellationToken token)
    {
        var products = await PreloadProducts(token);
        var tags = new List<TagModel>();
        foreach (var product in products)
            tags.AddRange(await tagClient.GetTagsAsync(product.ProductId, token));

        if (tags.Count == 0)
            throw CreateInformalException("tag", "tag create");

        var selected = await prompt.ChooseFromListAsync("Choose tag", tags.ToList(), t => $"{t.Name} ({t.Product.Name})", token);
        if (selected == null)
            throw CreateHelpException("--tag-id");

        return selected;
    }

    public async Task<FlagModel> LoadFlagAsync(CancellationToken token)
    {
        var flags = new List<FlagModel>();
        var products = await PreloadProducts(token);
        foreach (var product in products)
        {
            var configs = await PreloadConfigs(product.ProductId, token);
            foreach (var config in configs)
                flags.AddRange(await flagClient.GetFlagsAsync(config.ConfigId, token));
        }

        if (flags.Count == 0)
            throw CreateInformalException("flag", "flag create");

        var selected = await prompt.ChooseFromListAsync("Choose flag", flags.ToList(), f => $"{f.Name} ({f.ConfigName})", token);
        if (selected == null)
            throw CreateHelpException("--flag-id / --setting-id");

        return selected;
    }

    public async Task<List<TagModel>> LoadTagsAsync(CancellationToken token, string configId = null, List<TagModel> defaultTags = null, bool optional = false)
    {
        var tags = new List<TagModel>();
        if (configId == null)
        {
            var products = await PreloadProducts(token);
            foreach (var product in products)
                tags.AddRange(await tagClient.GetTagsAsync(product.ProductId, token));
        }
        else
        {
            var config = await configClient.GetConfigAsync(configId, token);
            tags = (await tagClient.GetTagsAsync(config.Product.ProductId, token)).ToList();
        }

        if (tags.Count == 0)
        {
            if (optional)
                return [];

            throw CreateInformalException("tag", "tag create");
        }

        var selected = await prompt.ChooseMultipleFromListAsync("Choose tags", tags.ToList(), t => t.Name, token, defaultTags);
        if (selected == null)
            throw CreateHelpException("--tag-ids");

        return selected;
    }

    private async Task<List<ConfigModel>> PreloadConfigs(string productId, CancellationToken token)
    {
        var configs = (await configClient.GetConfigsAsync(productId, token)).ToList();

        if (cliConfig.Workspace?.Config.IsEmpty() ?? true) return configs;
        var config = configs.FirstOrDefault(c => c.ConfigId == cliConfig.Workspace.Config);
        if (config is null) return configs;
        output.Verbose($"Using config '{config.Name}' from workspace.");
        return [config];

    }
    
    private async Task<List<ProductModel>> PreloadProducts(CancellationToken token)
    {
        var products = (await productClient.GetProductsAsync(token)).ToList();

        if (products.Count == 0)
            throw CreateInformalException("product", "product create");

        if (cliConfig.Workspace?.Product.IsEmpty() ?? true) return products;
        var product = products.FirstOrDefault(p => p.ProductId == cliConfig.Workspace.Product);
        if (product is null) return products;
        output.Verbose($"Using product '{product.Name}' from workspace.");
        return [product];

    }

    private static ShowHelpException CreateHelpException(string argument) =>
        new ShowHelpException($"Required option `{argument}` is missing.");

    private static Exception CreateInformalException(string resource, string argument) =>
        new Exception($"No available {resource} found, to create one, use the `configcat {argument}` command.");
}
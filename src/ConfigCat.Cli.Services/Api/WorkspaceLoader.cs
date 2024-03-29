using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Api;

public interface IWorkspaceLoader
{
    Task<OrganizationModel> LoadOrganizationAsync(CancellationToken token);

    Task<ProductModel> LoadProductAsync(CancellationToken token);

    Task<ConfigModel> LoadConfigAsync(CancellationToken token);

    Task<SegmentModel> LoadSegmentAsync(CancellationToken token);

    Task<EnvironmentModel> LoadEnvironmentAsync(CancellationToken token, string configId = null);

    Task<TagModel> LoadTagAsync(CancellationToken token);

    Task<FlagModel> LoadFlagAsync(CancellationToken token);

    Task<PermissionGroupModel> LoadPermissionGroupAsync(CancellationToken token);

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
    IPermissionGroupClient permissionGroupClient,
    IPrompt prompt)
    : IWorkspaceLoader
{
    public async Task<OrganizationModel> LoadOrganizationAsync(CancellationToken token)
    {
        var organizations = await organizationClient.GetOrganizationsAsync(token);
        var selected = await prompt.ChooseFromListAsync("Choose organization", organizations.ToList(), o => o.Name, token);
        if (selected == null)
            this.ThrowHelpException("--organization-id");

        return selected;
    }

    public async Task<ProductModel> LoadProductAsync(CancellationToken token)
    {
        var products = await productClient.GetProductsAsync(token);

        if (!products.Any())
            this.ThrowInformalException("product", "product create");

        var selected = await prompt.ChooseFromListAsync("Choose product", products.ToList(), p => $"{p.Name} ({p.Organization.Name})", token);
        if (selected == null)
            this.ThrowHelpException("--product-id");

        return selected;
    }

    public async Task<ConfigModel> LoadConfigAsync(CancellationToken token)
    {
        var products = await productClient.GetProductsAsync(token);
        var configs = new List<ConfigModel>();
        foreach (var product in products)
            configs.AddRange(await configClient.GetConfigsAsync(product.ProductId, token));

        if (!configs.Any())
            this.ThrowInformalException("config", "config create");

        var selected = await prompt.ChooseFromListAsync("Choose config", configs.ToList(), c => $"{c.Name} ({c.Product.Name})", token);
        if (selected == null)
            this.ThrowHelpException("--config-id");

        return selected;
    }

    public async Task<PermissionGroupModel> LoadPermissionGroupAsync(CancellationToken token)
    {
        var products = await productClient.GetProductsAsync(token);
        var permissionGroups = new List<PermissionGroupModel>();
        foreach (var product in products)
            permissionGroups.AddRange(await permissionGroupClient.GetPermissionGroupsAsync(product.ProductId, token));

        if (!permissionGroups.Any())
            this.ThrowInformalException("permission-group", "permission-group create");

        var selected = await prompt.ChooseFromListAsync("Choose permission group",
            permissionGroups.ToList(), c => $"{c.Name} ({c.Product.Name})", token);
        if (selected == null)
            this.ThrowHelpException("--permission-group-id");

        return selected;
    }

    public async Task<SegmentModel> LoadSegmentAsync(CancellationToken token)
    {
        var products = await productClient.GetProductsAsync(token);
        var segments = new List<SegmentModel>();
        foreach (var product in products)
            segments.AddRange(await segmentClient.GetSegmentsAsync(product.ProductId, token));

        if (!segments.Any())
            this.ThrowInformalException("segment", "segment create");

        var selected = await prompt.ChooseFromListAsync("Choose segment", segments.ToList(), s => $"{s.Name} ({s.Product.Name})", token);
        if (selected == null)
            this.ThrowHelpException("--segment-id");

        return await segmentClient.GetSegmentAsync(selected.SegmentId, token);
    }

    public async Task<EnvironmentModel> LoadEnvironmentAsync(CancellationToken token, string configId = null)
    {
        var environments = new List<EnvironmentModel>();
        if (configId == null)
        {
            var products = await productClient.GetProductsAsync(token);
            foreach (var product in products)
                environments.AddRange(await environmentClient.GetEnvironmentsAsync(product.ProductId, token));
        }
        else
        {
            var config = await configClient.GetConfigAsync(configId, token);
            environments = (await environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token)).ToList();
        }

        if (!environments.Any())
            this.ThrowInformalException("environment", "environment create");

        var selected = await prompt.ChooseFromListAsync("Choose environment", environments.ToList(), e => $"{e.Name} ({e.Product.Name})", token);
        if (selected == null)
            this.ThrowHelpException("--environment-id");

        return selected;
    }

    public async Task<TagModel> LoadTagAsync(CancellationToken token)
    {
        var products = await productClient.GetProductsAsync(token);
        var tags = new List<TagModel>();
        foreach (var product in products)
            tags.AddRange(await tagClient.GetTagsAsync(product.ProductId, token));

        if (!tags.Any())
            this.ThrowInformalException("tag", "tag create");

        var selected = await prompt.ChooseFromListAsync("Choose tag", tags.ToList(), t => $"{t.Name} ({t.Product.Name})", token);
        if (selected == null)
            this.ThrowHelpException("--tag-id");

        return selected;
    }

    public async Task<FlagModel> LoadFlagAsync(CancellationToken token)
    {
        var flags = new List<FlagModel>();
        var products = await productClient.GetProductsAsync(token);
        foreach (var product in products)
        {
            var configs = await configClient.GetConfigsAsync(product.ProductId, token);
            foreach (var config in configs)
                flags.AddRange(await flagClient.GetFlagsAsync(config.ConfigId, token));
        }

        if (!flags.Any())
            this.ThrowInformalException("flag", "flag create");

        var selected = await prompt.ChooseFromListAsync("Choose flag", flags.ToList(), f => $"{f.Name} ({f.ConfigName})", token);
        if (selected == null)
            this.ThrowHelpException("--flag-id / --setting-id");

        return selected;
    }

    public async Task<List<TagModel>> LoadTagsAsync(CancellationToken token, string configId = null, List<TagModel> defaultTags = null, bool optional = false)
    {
        var tags = new List<TagModel>();
        if (configId == null)
        {
            var products = await productClient.GetProductsAsync(token);
            foreach (var product in products)
                tags.AddRange(await tagClient.GetTagsAsync(product.ProductId, token));
        }
        else
        {
            var config = await configClient.GetConfigAsync(configId, token);
            tags = (await tagClient.GetTagsAsync(config.Product.ProductId, token)).ToList();
        }

        if (!tags.Any())
        {
            if (optional)
                return [];

            this.ThrowInformalException("tag", "tag create");
        }

        var selected = await prompt.ChooseMultipleFromListAsync("Choose tags", tags.ToList(), t => t.Name, token, defaultTags);
        if (selected == null)
            this.ThrowHelpException("--tag-ids");

        return selected;
    }

    private void ThrowHelpException(string argument) =>
        throw new ShowHelpException($"Required option `{argument}` is missing.");

    private void ThrowInformalException(string resource, string argument) =>
        throw new Exception($"No available {resource} found, to create one, use the `configcat {argument}` command.");
}
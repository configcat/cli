using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;

namespace ConfigCat.Cli.Commands;

internal class Product(
    IProductClient productClient,
    IWorkspaceLoader workspaceLoader,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ListAllProductsAsync(bool json, CancellationToken token)
    {
        var products = await productClient.GetProductsAsync(token);

        if (json)
        {
            output.RenderJson(products);
            return ExitCodes.Ok;
        }

        var itemsToRender = products.Select(p => new
        {
            Id = p.ProductId,
            p.Name,
            Description = p.Description.TrimToFitColumn(),
            Organization = $"{p.Organization.Name} [{p.Organization.OrganizationId}]"
        });
        output.RenderTable(itemsToRender);

        return ExitCodes.Ok;
    }

    public async Task<int> CreateProductAsync(string organizationId, string name, string description, CancellationToken token)
    {
        if (organizationId.IsEmpty())
            organizationId = (await workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token);

        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token);

        var result = await productClient.CreateProductAsync(organizationId, name, description, token);
        output.Write(result.ProductId);

        return ExitCodes.Ok;
    }

    public async Task<int> DeleteProductAsync(string productId, CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await workspaceLoader.LoadProductAsync(token)).ProductId;

        await productClient.DeleteProductAsync(productId, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateProductAsync(string productId, string name, string description, CancellationToken token)
    {
        var product = productId.IsEmpty()
            ? await workspaceLoader.LoadProductAsync(token)
            : await productClient.GetProductAsync(productId, token);

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token, product.Name);

        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token, product.Description);

        if (name.IsEmptyOrEquals(product.Name) && description.IsEmptyOrEquals(product.Description))
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        await productClient.UpdateProductAsync(product.ProductId, name, description, token);
        return ExitCodes.Ok;
    }
    
    public async Task<int> ShowProductPreferencesAsync(string productId, bool json, CancellationToken token)
    {
        var product = productId.IsEmpty()
            ? await workspaceLoader.LoadProductAsync(token)
            : await productClient.GetProductAsync(productId, token);

        var preferences = await productClient.GetProductPreferencesAsync(product.ProductId, token);
        if (json)
        {
            output.RenderJson(preferences);
            return ExitCodes.Ok;
        }

        output.WriteDarkGray("Reason required: ").Write(preferences.ReasonRequired.ToString()).WriteLine();
        output.WriteDarkGray("Key generation mode: ").Write(preferences.KeyGenerationMode).WriteLine();
        output.WriteDarkGray("Show variation ID: ").Write(preferences.ShowVariationId.ToString()).WriteLine();
        output.WriteDarkGray("Mandatory setting hint: ").Write(preferences.MandatorySettingHint.ToString()).WriteLine();
        output.WriteDarkGray("Per-environment required reason: ").WriteLine();
        output.RenderTable(preferences.ReasonRequiredEnvironments.Select(e => new
        {
            Evnrionment = e.EnvironmentName,
            Required = e.ReasonRequired,
        }));
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> UpdateProductPreferencesAsync(string productId, bool? reasonRequired, string keyGenMode, bool? showVariationId, bool? mandatorySettingHint, CancellationToken token)
    {
        var product = productId.IsEmpty()
            ? await workspaceLoader.LoadProductAsync(token)
            : await productClient.GetProductAsync(productId, token);

        var preferences = await productClient.GetProductPreferencesAsync(product.ProductId, token);

        if (!reasonRequired.HasValue && keyGenMode.IsEmpty() && !showVariationId.HasValue &&
            !mandatorySettingHint.HasValue)
        {
            reasonRequired = await prompt.ChooseFromListAsync("Reason required", ["yes", "no"], a => a, token, preferences.ReasonRequired ? "yes" : "no") == "yes";
            showVariationId = await prompt.ChooseFromListAsync("Show Variation ID", ["yes", "no"], a => a, token, preferences.ShowVariationId ? "yes" : "no") == "yes";
            mandatorySettingHint = await prompt.ChooseFromListAsync("Mandatory Setting hints", ["yes", "no"], a => a, token, preferences.MandatorySettingHint ? "yes" : "no") == "yes";
            keyGenMode = await prompt.ChooseFromListAsync("Key generation mode", KeyGenerationModes.Collection.ToList(), a => a,
                token, preferences.KeyGenerationMode);
        }
        
        if (reasonRequired.HasValue)
            preferences.ReasonRequired = reasonRequired.Value;
        if (!keyGenMode.IsEmpty())
            preferences.KeyGenerationMode = keyGenMode;
        if (showVariationId.HasValue)
            preferences.ShowVariationId = showVariationId.Value;
        if (mandatorySettingHint.HasValue)
            preferences.MandatorySettingHint = mandatorySettingHint.Value;

        await productClient.UpdateProductPreferencesAsync(product.ProductId, preferences, token);
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> UpdateEnvSpecProductPreferencesAsync(string productId, ReasonRequiredEnvironmentModel[] environments, CancellationToken token)
    {
        var product = productId.IsEmpty()
            ? await workspaceLoader.LoadProductAsync(token)
            : await productClient.GetProductAsync(productId, token);

        var preferences = await productClient.GetProductPreferencesAsync(product.ProductId, token);

        if (environments.IsEmpty())
        {
            var envModels = preferences.ReasonRequiredEnvironments.Select(ev => new
            {
                ev.EnvironmentId,
                ev.ReasonRequired,
                ev.EnvironmentName,
            }).ToList();
            
            var selected = await prompt.ChooseMultipleFromListAsync("Environments where reason is required", envModels, e => e.EnvironmentName, token, envModels.Where(e => e.ReasonRequired).ToList());
            environments = preferences.ReasonRequiredEnvironments.Select(e =>
            {
                e.ReasonRequired = selected.Any(s => s.EnvironmentId == e.EnvironmentId);
                return e;
            }).ToArray();
        }

        preferences.ReasonRequiredEnvironments = environments;
        
        await productClient.UpdateProductPreferencesAsync(product.ProductId, preferences, token);
        
        return ExitCodes.Ok;
    }
}
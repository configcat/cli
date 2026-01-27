using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands.Flags.V2;

internal class Variation(
    IFlagClient flagClient,
    IVariationClient variationClient,
    IWorkspaceLoader workspaceLoader,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ListAsync(int? flagId, bool json, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);
        
        if (json)
        {
            output.RenderJson(flag.PredefinedVariations);
            return ExitCodes.Ok;
        }
        
        var itemsToRender = flag.PredefinedVariations.Select(f => new
        {
            Id = f.PredefinedVariationId,
            f.Name,
            Hint = f.Hint.TrimToFitColumn(),
            f.Value,
        });
        output.RenderTable(itemsToRender);

        return ExitCodes.Ok;
    }
    
    public async Task<int> CreateAsync(int? flagId, string name, string hint, string flagValue, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);
        
        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token);

        if (hint.IsEmpty())
            hint = await prompt.GetStringAsync("Hint", token);

        if (flagValue.IsEmpty())
            flagValue = await prompt.GetStringAsync($"Value", token);
        
        var parsed = flagValue.ToFlagValue(flag.SettingType);

        var updated = flag.PredefinedVariations.ToList();
        updated.Add(new VariationModel
        {
            Name = name,
            Hint = hint,
            Value = parsed
        });
        
        await variationClient.UpdateVariationsAsync(flag.SettingId, updated, token);

        return ExitCodes.Ok;
    }
    
    public async Task<int> UpdateAsync(int? flagId, string predefinedVariationId, string name, string hint, string flagValue, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);
        
        VariationModel selected;
        if (predefinedVariationId.IsEmpty())
        {
            selected = await prompt.ChooseFromListAsync("Choose variation", flag.PredefinedVariations, e => e.Name ?? e.Value.ToString(), token);
        }
        else
        {
            selected = flag.PredefinedVariations.FirstOrDefault(e => e.PredefinedVariationId == predefinedVariationId);
        }
        
        if (selected == null)
            throw new ShowHelpException($"Required option --predefined-variation-id is missing.");
        
        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token, selected?.Name);

        if (hint.IsEmpty())
            hint = await prompt.GetStringAsync("Hint", token, selected?.Hint);
        
        if (flagValue.IsEmpty())
            flagValue = await prompt.GetStringAsync($"Value", token, selected?.Value.ToString());

        var parsed = flagValue.ToFlagValue(flag.SettingType);

        selected.Name = name;
        selected.Hint = hint;
        selected.Value = parsed;
        
        await variationClient.UpdateVariationsAsync(flag.SettingId, flag.PredefinedVariations, token);

        return ExitCodes.Ok;
    }
    
    public async Task<int> DeleteAsync(int? flagId, string predefinedVariationId, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);
        
        VariationModel selected;
        if (predefinedVariationId.IsEmpty())
        {
            selected = await prompt.ChooseFromListAsync("Choose variation", flag.PredefinedVariations, e => e.Name ?? e.Value.ToString(), token);
        }
        else
        {
            selected = flag.PredefinedVariations.FirstOrDefault(e => e.PredefinedVariationId == predefinedVariationId);
        }
        if (selected == null)
            throw new ShowHelpException($"Required option --predefined-variation-id is missing.");
        
        var updated = flag.PredefinedVariations.Where(e => e.PredefinedVariationId != selected.PredefinedVariationId).ToList();
        await variationClient.UpdateVariationsAsync(flag.SettingId, updated, token);

        return ExitCodes.Ok;
    }
}
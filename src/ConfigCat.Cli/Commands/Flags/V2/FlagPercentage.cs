using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Options;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Extensions;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands.Flags.V2;

internal class FlagPercentage(IPrompt prompt, IOutput output, IWorkspaceLoader workspaceLoader, 
    IFlagClient flagClient, IFlagValueV2Client flagValueClient)
{
    public async Task<int> UpdatePercentageRulesAsync(int? flagId, 
        string environmentId, 
        string reason,
        UpdatePercentageModel[] percentageOptions, 
        CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await workspaceLoader.LoadFlagAsync(token),
            _ => await flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        var value = await flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);
        var percentageRule = value.TargetingRules.FirstOrDefault(tr =>
            !tr.PercentageOptions.IsEmpty() && tr.Conditions.IsEmpty() && tr.Value.IsEmpty());

        if (percentageRule is null)
        {
            percentageRule = new TargetingRuleModel();
            value.TargetingRules.Add(percentageRule);
        }
        
        var result = await GetPercentageOptions(percentageOptions, flag, token);
        
        if (value.Setting.SettingType == SettingTypes.Boolean && result.Count != 2)
            throw new ShowHelpException($"Boolean type can only have 2 percentage rules");

        percentageRule.PercentageOptions = result;
        await flagValueClient.ReplaceValueAsync(flag.SettingId, environmentId, reason, value, token);

        return ExitCodes.Ok;
    }

    public async Task<int> DeletePercentageRulesAsync(int? flagId, string environmentId, string reason, CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await workspaceLoader.LoadFlagAsync(token),
            _ => await flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        var value = await flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);
        var percentageRule = value.TargetingRules.FirstOrDefault(tr =>
            !tr.PercentageOptions.IsEmpty() && tr.Conditions.IsEmpty() && tr.Value.IsEmpty());

        if (percentageRule is null)
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }
        
        value.TargetingRules.Remove(percentageRule);
        await flagValueClient.ReplaceValueAsync(flag.SettingId, environmentId, reason, value, token);

        return ExitCodes.Ok;
    }
    
    public async Task<int> UpdatePercentageAttributeAsync(int? flagId, string environmentId, string attributeName, string reason, CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await workspaceLoader.LoadFlagAsync(token),
            _ => await flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        if (attributeName.IsEmpty())
            attributeName = await prompt.GetStringAsync("Percentage attribute", token, "Identifier");
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Replace($"/percentageEvaluationAttribute", attributeName);
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, reason, jsonPatchDocument.Operations, token);

        return ExitCodes.Ok;
    }
    
    private async Task<List<PercentageOptionModel>> GetPercentageOptions(UpdatePercentageModel[] percentageOptions, 
        FlagModel flag, CancellationToken token)
    {
        if (!percentageOptions.IsEmpty())
            return percentageOptions.Select(po => new PercentageOptionModel
            {
                Percentage = po.Percentage,
                Value = po.Value.ToFlagValue(flag.SettingType)
            }).ToList();
        
        var items = await prompt.GetRepeatedValuesAsync("Set percentage options", token, ["Percentage", "Value"]);
        if (items is null) throw new ShowHelpException($"Percentage options are required");
        return items.Select(i =>
        {
            if (!int.TryParse(i[0], out var percentage) || percentage < 0) throw new ShowHelpException($"Percentage value '{i[0]}' is invalid");
            return new PercentageOptionModel
            {
                Percentage = percentage,
                Value = i[1].ToFlagValue(flag.SettingType),
            };
        }).ToList();
    }
}
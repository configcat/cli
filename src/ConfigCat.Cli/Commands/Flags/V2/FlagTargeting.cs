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

internal class FlagTargeting(IPrompt prompt, IFlagClient flagClient, ISegmentClient segmentClient,
    IWorkspaceLoader workspaceLoader, IFlagValueV2Client flagValueClient)
{
    public async Task<int> AddUserTargetingRuleAsync(int? flagId,
        string environmentId,
        string attribute,
        string comparator,
        ComparisonValueModel comparisonValue,
        string servedValue,
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
        
        if (attribute.IsEmpty())
            attribute = await prompt.GetStringAsync("Comparison attribute", token, "Identifier");

        if (comparator.IsEmpty())
            comparator = (await prompt.ChooseFromListAsync("Choose comparator", Constants.UserComparatorTypes.ToList(), c => $"{c.Key} [{c.Value}]", token)).Key;

        if (!Constants.UserComparatorTypes.Keys.Contains(comparator, StringComparer.OrdinalIgnoreCase))
            throw new ShowHelpException($"Comparator must be one of the following: {string.Join('|', Constants.UserComparatorTypes)}");

        var condition = new UserConditionModel { Comparator = comparator, ComparisonAttribute = attribute };

        if (comparisonValue.IsEmpty())
            condition.ComparisonValue = await ParseComparisonValue(comparator, token);
        
        var rule = new TargetingRuleModel { Conditions = [new ConditionModel { UserCondition = condition }] };
        await SetRuleThenPart(servedValue, percentageOptions, token, rule, flag);

        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/targetingRules/-", rule);
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }

    public async Task<int> AddSegmentTargetingRuleAsync(int? flagId,
        string environmentId,
        string comparator,
        string segmentId,
        string servedValue,
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
        
        if (comparator.IsEmpty())
            comparator = (await prompt.ChooseFromListAsync("Choose comparator", Constants.SegmentComparatorTypes.ToList(), c => $"{c.Key} [{c.Value}]", token)).Key;

        if (!Constants.SegmentComparatorTypes.Keys.Contains(comparator, StringComparer.OrdinalIgnoreCase))
            throw new ShowHelpException($"Comparator must be one of the following: {string.Join('|', Constants.SegmentComparatorTypes)}");
        
        var condition = new SegmentConditionModel { Comparator = comparator };
        if (!segmentId.IsEmpty())
        {
            condition.SegmentId = segmentId;
        }
        else
        {
            var segment = await workspaceLoader.LoadSegmentAsync(token);
            condition.SegmentId = segment.SegmentId;
        }
        
        var rule = new TargetingRuleModel { Conditions = [new ConditionModel { SegmentCondition = condition }] };
        await SetRuleThenPart(servedValue, percentageOptions, token, rule, flag);
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/targetingRules/-", rule);
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> AddPrerequisiteTargetingRuleAsync(int? flagId,
        string environmentId,
        string comparator,
        int? prerequisiteId,
        string prerequisiteValue,
        string servedValue,
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
        
        if (comparator.IsEmpty())
            comparator = (await prompt.ChooseFromListAsync("Choose comparator", Constants.PrerequisiteComparatorTypes.ToList(), c => $"{c.Key} [{c.Value}]", token)).Key;

        if (!Constants.PrerequisiteComparatorTypes.Keys.Contains(comparator, StringComparer.OrdinalIgnoreCase))
            throw new ShowHelpException($"Comparator must be one of the following: {string.Join('|', Constants.PrerequisiteComparatorTypes)}");
        
        var flags = (await flagClient.GetFlagsAsync(flag.ConfigId, token)).ToList();
        var condition = new PrerequisiteFlagConditionModel { Comparator = comparator };
        if (prerequisiteId is not null)
        {
            var prerequisite = flags.FirstOrDefault(f => f.SettingId == prerequisiteId);
            if (prerequisite is null) throw new ShowHelpException($"Flag with ID '{prerequisiteId}' not found");
            condition.PrerequisiteSettingId = prerequisite.SettingId;
        }
        else
        {
            var filtered = flags.Where(f => f.SettingId != flag.SettingId).ToList();
            if (filtered.Count == 0) throw new ShowHelpException("No other flags can be selected as prerequisite");
            var selected = await prompt.ChooseFromListAsync("Choose prerequisite", filtered, f => $"{f.Name} ({f.ConfigName})", token);
            if (selected is null) throw new ShowHelpException("Prerequisite flag is required");
            condition.PrerequisiteSettingId = selected.SettingId;
        }

        var prerequisiteFlag = flags.First(f => f.SettingId == condition.PrerequisiteSettingId);
        if (!prerequisiteValue.IsEmpty())
        {
            condition.PrerequisiteComparisonValue = prerequisiteValue.ToFlagValue(prerequisiteFlag.SettingType);
        }
        else
        {
            var val = await prompt.GetStringAsync("Prerequisite flag value", token);
            if (val is null) throw new ShowHelpException($"Prerequisite flag value is required");
            condition.PrerequisiteComparisonValue = val.ToFlagValue(prerequisiteFlag.SettingType);
        }
        
        var rule = new TargetingRuleModel { Conditions = [new ConditionModel { PrerequisiteFlagCondition = condition }] };
        await SetRuleThenPart(servedValue, percentageOptions, token, rule, flag);
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/targetingRules/-", rule);
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }

    public async Task<ComparisonValueModel> ParseComparisonValue(string comparator, CancellationToken token)
    {
        if (comparator.IsListComparator())
        {
            var items = await prompt.GetRepeatedValuesAsync("Set values for comparison value list", token, ["Value", "Hint"]);
            if (items is null) throw new ShowHelpException($"Comparison value is required");
            return new ComparisonValueModel
            {
                ListValue = items.Select(i => new ComparisonValueListModel
                {
                    Value = i[0],
                    Hint = i[1],
                }).ToList()
            };
        }

        var cv = await prompt.GetStringAsync("Comparison value", token);
        if (cv is null) throw new ShowHelpException($"Comparison value is required");
        if (!comparator.IsNumberComparator()) return new ComparisonValueModel { StringValue = cv };
        if (!double.TryParse(cv, out var parsed)) throw new ShowHelpException($"Comparison value '{cv}' is not a valid number");
        return new ComparisonValueModel { DoubleValue = parsed };
    }
    
    private async Task SetRuleThenPart(string servedValue, UpdatePercentageModel[] percentageOptions,
        CancellationToken token, TargetingRuleModel rule, FlagModel flag)
    {
        if (percentageOptions.IsEmpty() && servedValue.IsEmpty())
        {
            var selected = await prompt.ChooseFromListAsync("Choose the targeting rule's THEN part", ["value", "percentage"], c => c, token);
            if (selected == "value")
            {
                var val = await prompt.GetStringAsync("Served value", token);
                if (val is null) throw new ShowHelpException($"Served value is required");
                rule.Value = val.ToFlagValue(flag.SettingType);
            }
            else
            {
                var items = await prompt.GetRepeatedValuesAsync("Set percentage options", token, ["Percentage", "Value"]);
                if (items is null) throw new ShowHelpException($"Percentage options are required");
                rule.PercentageOptions = items.Select(i =>
                {
                    if (!int.TryParse(i[0], out var percentage)) throw new ShowHelpException($"Percentage value '{i[0]}' is invalid");
                    return new PercentageOptionModel
                    {
                        Percentage = percentage,
                        Value = i[1].ToFlagValue(flag.SettingType),
                    };
                }).ToList();
            }
        } 
        else if (!percentageOptions.IsEmpty())
        {
            rule.PercentageOptions = percentageOptions.Select(po => new PercentageOptionModel
            {
                Percentage = po.Percentage,
                Value = po.Value.ToFlagValue(flag.SettingType)
            }).ToList();
        }
        else
            rule.Value = servedValue.ToFlagValue(flag.SettingType);
    }
}
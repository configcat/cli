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
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands.Flags.V2;

internal class FlagTargeting(IPrompt prompt, IFlagClient flagClient,
    IWorkspaceLoader workspaceLoader, IFlagValueV2Client flagValueClient)
{
    public async Task<int> AddUserTargetingRuleAsync(int? flagId,
        string environmentId,
        string attribute,
        string comparator,
        string[] comparisonValue,
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

        var condition = new UserConditionModel
        {
            Comparator = comparator, ComparisonAttribute = attribute,
            ComparisonValue = await ParseComparisonValue(comparator, comparisonValue, token)
        };

        var rule = new TargetingRuleModel { Conditions = [new ConditionModel { UserCondition = condition }] };
        await SetRuleThenPart(servedValue, percentageOptions, rule, flag, token);

        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/targetingRules/-", rule);
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> AddUserConditionAsync(int? flagId,
        string environmentId,
        int? rulePosition,
        string attribute,
        string comparator,
        string[] comparisonValue,
        CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await workspaceLoader.LoadFlagAsync(token),
            _ => await flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        rulePosition ??= await PromptPosition("Targeting rule's position the condition should be added to", token);
        
        if (attribute.IsEmpty())
            attribute = await prompt.GetStringAsync("Comparison attribute", token, "Identifier");

        if (comparator.IsEmpty())
            comparator = (await prompt.ChooseFromListAsync("Choose comparator", Constants.UserComparatorTypes.ToList(), c => $"{c.Key} [{c.Value}]", token)).Key;

        if (!Constants.UserComparatorTypes.Keys.Contains(comparator, StringComparer.OrdinalIgnoreCase))
            throw new ShowHelpException($"Comparator must be one of the following: {string.Join('|', Constants.UserComparatorTypes)}");

        var condition = new UserConditionModel
        {
            Comparator = comparator, ComparisonAttribute = attribute,
            ComparisonValue = await ParseComparisonValue(comparator, comparisonValue, token)
        };
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/targetingRules/{rulePosition-1}/conditions/-", new ConditionModel { UserCondition = condition });
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
        await SetRuleThenPart(servedValue, percentageOptions, rule, flag, token);
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/targetingRules/-", rule);
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> AddSegmentConditionAsync(int? flagId,
        string environmentId,
        int? rulePosition,
        string comparator,
        string segmentId,
        CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await workspaceLoader.LoadFlagAsync(token),
            _ => await flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;
        
        rulePosition ??= await PromptPosition("Targeting rule's position the condition should be added to", token);
        
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
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/targetingRules/{rulePosition-1}/conditions/-", new ConditionModel { SegmentCondition = condition });
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
        await SetRuleThenPart(servedValue, percentageOptions, rule, flag, token);
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/targetingRules/-", rule);
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> AddPrerequisiteConditionAsync(int? flagId,
        string environmentId,
        int? rulePosition,
        string comparator,
        int? prerequisiteId,
        string prerequisiteValue,
        CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await workspaceLoader.LoadFlagAsync(token),
            _ => await flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;
        
        rulePosition ??= await PromptPosition("Targeting rule's position the condition should be added to", token);
        
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
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/targetingRules/{rulePosition-1}/conditions/-", new ConditionModel { PrerequisiteFlagCondition = condition });
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> DeleteRuleAsync(int? flagId,
        string environmentId,
        int? rulePosition,
        CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await workspaceLoader.LoadFlagAsync(token),
            _ => await flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;
        
        rulePosition ??= await PromptPosition("Targeting rule's position to remove", token);
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Remove($"/targetingRules/{rulePosition-1}");
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> DeleteConditionAsync(int? flagId,
        string environmentId,
        int? rulePosition,
        int? conditionPosition,
        CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await workspaceLoader.LoadFlagAsync(token),
            _ => await flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;
        
        rulePosition ??= await PromptPosition("Targeting rule's position", token);
        conditionPosition ??= await PromptPosition("Condition's position to remove", token);
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Remove($"/targetingRules/{rulePosition-1}/conditions/{conditionPosition-1}");
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> MoveTargetingRuleAsync(int? flagId, string environmentId, int? from, int? to, CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await workspaceLoader.LoadFlagAsync(token),
            _ => await flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        from ??= await PromptPosition("Move from position", token);
        to ??= await PromptPosition("Move to position", token);

        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Move($"/targetingRules/{from-1}", $"/targetingRules/{to-1}");

        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        return ExitCodes.Ok;
    }
    
    public async Task<int> UpdateRuleServedValueAsync(int? flagId,
        string environmentId,
        int? rulePosition,
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

        rulePosition ??= await PromptPosition("Targeting rule's position to update", token);
        
        var value = await flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);
        var rule = value.TargetingRules?.ElementAtOrDefault(rulePosition.Value-1);
        if (rule is null) throw new ShowHelpException($"Targeting rule in position '{rulePosition}' not found");
        
        await SetRuleThenPart(servedValue, percentageOptions, rule, flag, token);
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Replace($"/targetingRules/{rulePosition-1}", rule);
        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        
        return ExitCodes.Ok;
    }

    private async Task<ComparisonValueModel> ParseComparisonValue(string comparator, string[] comparisonValue, CancellationToken token)
    {
        if (!comparisonValue.IsEmpty())
        {
            if (comparator.IsListComparator())
                return new ComparisonValueModel { ListValue = ToListModel(comparisonValue) };

            var compVal = comparisonValue[0];
            if (!comparator.IsNumberComparator()) return new ComparisonValueModel { StringValue = compVal };
            if (!double.TryParse(compVal, out var d)) throw new ShowHelpException($"Comparison value '{compVal}' is not a valid number");
            return new ComparisonValueModel { DoubleValue = d };
        }
        
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

    private List<ComparisonValueListModel> ToListModel(string[] comparisonValues)
    {
        var length = comparisonValues.Length;
        var result = new List<ComparisonValueListModel>();
        for (var i = 0; i < length; i++)
        {
            var expression = comparisonValues[i];
            var indexOfSeparator = expression.IndexOf(':');
            if (indexOfSeparator == -1)
                throw new ShowHelpException($"The expression `{expression}` is invalid. Required format: <value>:<hint>");

            var value = expression[..indexOfSeparator];
            var hint = expression[(indexOfSeparator + 1)..];

            if (value.IsEmpty())
                throw new ShowHelpException($"The <value> part of the expression `{expression}` is invalid.");

            result.Add(new ComparisonValueListModel { Value = value, Hint = hint });
        }
        return result;
    }
    
    private async Task SetRuleThenPart(string servedValue, UpdatePercentageModel[] percentageOptions,
        TargetingRuleModel rule, FlagModel flag, CancellationToken token)
    {
        if (percentageOptions.IsEmpty() && servedValue.IsEmpty())
        {
            var selected = await prompt.ChooseFromListAsync("Choose the targeting rule's THEN part", ["value", "percentage"], c => c, token);
            if (selected == "value")
            {
                var val = await prompt.GetStringAsync("Served value", token);
                if (val is null) throw new ShowHelpException($"Served value is required");
                rule.Value = val.ToFlagValue(flag.SettingType);
                rule.PercentageOptions = null;
            }
            else
            {
                var items = await prompt.GetRepeatedValuesAsync("Set percentage options", token, ["Percentage", "Value"]);
                if (items is null) throw new ShowHelpException($"Percentage options are required");
                rule.PercentageOptions = items.Select(i =>
                {
                    if (!int.TryParse(i[0], out var percentage) || percentage < 0) throw new ShowHelpException($"Percentage value '{i[0]}' is invalid");
                    return new PercentageOptionModel
                    {
                        Percentage = percentage,
                        Value = i[1].ToFlagValue(flag.SettingType),
                    };
                }).ToList();
                rule.Value = null;
            }
        } 
        else if (!percentageOptions.IsEmpty())
        {
            rule.PercentageOptions = percentageOptions.Select(po => new PercentageOptionModel
            {
                Percentage = po.Percentage,
                Value = po.Value.ToFlagValue(flag.SettingType)
            }).ToList();
            rule.Value = null;
        }
        else
        {
            rule.Value = servedValue.ToFlagValue(flag.SettingType);
            rule.PercentageOptions = null;
        }
    }

    private async Task<int> PromptPosition(string label, CancellationToken token)
    {
        var position = await prompt.GetStringAsync(label, token, "1");
        if (!int.TryParse(position, out var parsed) || parsed < 1) throw new ShowHelpException($"Position '{position}' is invalid");
        return parsed;
    }
}
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands.Flags;

internal class FlagTargeting
{
    private readonly IFlagValueClient flagValueClient;
    private readonly IFlagClient flagClient;
    private readonly ISegmentClient segmentClient;
    private readonly IEnvironmentClient environmentClient;
    private readonly IWorkspaceLoader workspaceLoader;
    private readonly IPrompt prompt;

    public FlagTargeting(IFlagValueClient flagValueClient,
        IFlagClient flagClient,
        ISegmentClient segmentClient,
        IEnvironmentClient environmentClient,
        IWorkspaceLoader workspaceLoader,
        IPrompt prompt)
    {
        this.flagValueClient = flagValueClient;
        this.flagClient = flagClient;
        this.segmentClient = segmentClient;
        this.environmentClient = environmentClient;
        this.workspaceLoader = workspaceLoader;
        this.prompt = prompt;
    }

    public async Task<int> AddTargetingRuleAsync(int? flagId, string environmentId, AddTargetinRuleModel addTargetingRuleModel, CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await this.workspaceLoader.LoadFlagAsync(token),
            _ => await this.flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        await this.ValidateAddModel(addTargetingRuleModel, environmentId, token);

        if (!addTargetingRuleModel.FlagValue.TryParseFlagValue(flag.SettingType, out var parsed))
            throw new ShowHelpException($"Flag value '{addTargetingRuleModel.FlagValue}' must respect the type '{flag.SettingType}'.");

        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Add($"/{FlagValueModel.TargetingRuleJsonName}/-", new TargetingModel
        {
            Comparator = addTargetingRuleModel.Comparator,
            ComparisonAttribute = addTargetingRuleModel.Attribute,
            ComparisonValue = addTargetingRuleModel.CompareTo,
            SegmentComparator = addTargetingRuleModel.SegmentComparator,
            SegmentId = addTargetingRuleModel.SegmentId,
            Value = parsed,
        });

        await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateTargetingRuleAsync(int? flagId, string environmentId, int? position, AddTargetinRuleModel addTargetingRuleModel, CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await this.workspaceLoader.LoadFlagAsync(token),
            _ => await this.flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        var (existing, realPosition) = await this.GetRuleAsync("Choose rule to update", flag.SettingId, environmentId, position, token);
        await this.ValidateAddModel(addTargetingRuleModel, environmentId, token, existing);

        if (!addTargetingRuleModel.FlagValue.TryParseFlagValue(flag.SettingType, out var parsed))
            throw new ShowHelpException($"Flag value '{addTargetingRuleModel.FlagValue}' must respect the type '{flag.SettingType}'.");

        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Replace($"/{FlagValueModel.TargetingRuleJsonName}/{realPosition}", new TargetingModel
        {
            Comparator = addTargetingRuleModel.Comparator,
            ComparisonAttribute = addTargetingRuleModel.Attribute,
            ComparisonValue = addTargetingRuleModel.CompareTo,
            SegmentComparator = addTargetingRuleModel.SegmentComparator,
            SegmentId = addTargetingRuleModel.SegmentId,
            Value = parsed,
        });

        await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        return ExitCodes.Ok;
    }

    public async Task<int> DeleteTargetingRuleAsync(int? flagId, string environmentId, int? position, CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await this.workspaceLoader.LoadFlagAsync(token),
            _ => await this.flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        var (_, realPosition) = await this.GetRuleAsync("Choose rule to delete", flag.SettingId, environmentId, position, token);

        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Remove($"/{FlagValueModel.TargetingRuleJsonName}/{realPosition}");

        await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        return ExitCodes.Ok;
    }

    public async Task<int> MoveTargetingRuleAsync(int? flagId, string environmentId, int? from, int? to, CancellationToken token)
    {
        var flag = flagId switch
        {
            null => await this.workspaceLoader.LoadFlagAsync(token),
            _ => await this.flagClient.GetFlagAsync(flagId.Value, token)
        };

        if (environmentId.IsEmpty())
            environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        var (_, realFrom) = await this.GetRuleAsync("Choose rule to re-position", flag.SettingId, environmentId, from, token);
        var (_, realTo) = await this.GetRuleAsync("Choose the position to move", flag.SettingId, environmentId, to, token);

        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Move($"/{FlagValueModel.TargetingRuleJsonName}/{realFrom}", $"/{FlagValueModel.TargetingRuleJsonName}/{realTo}");

        await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        return ExitCodes.Ok;
    }

    private async Task ValidateAddModel(AddTargetinRuleModel addTargetingRuleModel, string environmentId, CancellationToken token, TargetingModel defaultModel = null)
    {
        var isSegment = !addTargetingRuleModel.SegmentId.IsEmpty() || defaultModel?.Segment is not null;
        if (addTargetingRuleModel.SegmentId.IsEmpty() && addTargetingRuleModel.Attribute.IsEmpty() && defaultModel?.Segment is null && defaultModel?.ComparisonAttribute is null)
        {
            var options = new List<string> { "Targeting rule", "Segment rule" };
            var selected = await this.prompt.ChooseFromListAsync("Choose rule type", options, t => t, token);
            isSegment = selected == options[1];
        }

        if (isSegment)
        {
            if (addTargetingRuleModel.SegmentId.IsEmpty())
            {
                var environment = await this.environmentClient.GetEnvironmentAsync(environmentId, token);
                var segments = await this.segmentClient.GetSegmentsAsync(environment.Product.ProductId, token);
                var selectedSegment = await this.prompt.ChooseFromListAsync("Choose segment", segments.ToList(), s => s.Name, token);
                addTargetingRuleModel.SegmentId = selectedSegment.SegmentId;
            }

            if (addTargetingRuleModel.SegmentComparator.IsEmpty())
            {
                var preSelectedKey = defaultModel?.SegmentComparator ?? "isIn";
                var preSelected = Constants.SegmentComparatorTypes.Single(c => c.Key == preSelectedKey);
                var selected = await this.prompt.ChooseFromListAsync("Choose segment comparator", Constants.SegmentComparatorTypes.ToList(), c => $"{c.Key} [{c.Value}]", token, preSelected);
                addTargetingRuleModel.SegmentComparator = selected.Key;
            }

            if (!Constants.SegmentComparatorTypes.Keys.Contains(addTargetingRuleModel.SegmentComparator, StringComparer.OrdinalIgnoreCase))
                throw new ShowHelpException($"Segment comparator must be one of the following: {string.Join('|', Constants.SegmentComparatorTypes)}");
        }
        else
        {
            if (addTargetingRuleModel.Attribute.IsEmpty())
                addTargetingRuleModel.Attribute = await this.prompt.GetStringAsync("Comparison attribute", token, defaultModel?.ComparisonAttribute ?? "Identifier");

            if (addTargetingRuleModel.Comparator.IsEmpty())
            {
                var preSelectedKey = defaultModel?.Comparator ?? "sensitiveIsOneOf";
                var preSelected = Constants.ComparatorTypes.Single(c => c.Key == preSelectedKey);
                var selected = await this.prompt.ChooseFromListAsync("Choose comparator", Constants.ComparatorTypes.ToList(), c => $"{c.Key} [{c.Value}]", token, preSelected);

                addTargetingRuleModel.Comparator = selected.Key;
            }

            if (addTargetingRuleModel.CompareTo.IsEmpty())
                addTargetingRuleModel.CompareTo = await this.prompt.GetStringAsync("Value to compare", token, defaultModel?.ComparisonValue);

            if (!Constants.ComparatorTypes.Keys.Contains(addTargetingRuleModel.Comparator, StringComparer.OrdinalIgnoreCase))
                throw new ShowHelpException($"Comparator must be one of the following: {string.Join('|', Constants.ComparatorTypes)}");
        }

        if (addTargetingRuleModel.FlagValue.IsEmpty())
            addTargetingRuleModel.FlagValue = await this.prompt.GetStringAsync($"Value", token, defaultModel?.Value?.ToString());
    }

    private async Task<(TargetingModel, int)> GetRuleAsync(string label, int settingId, string environmentId, int? positionFromInput, CancellationToken token)
    {
        var value = await this.flagValueClient.GetValueAsync(settingId, environmentId, token);

        if(value.TargetingRules.Count == 0)
            throw new Exception("No rules found in the selected environment.");

        foreach (var rule in value.TargetingRules.Where(rule => !rule.SegmentId.IsEmpty()))
        {
            rule.Segment = await this.segmentClient.GetSegmentAsync(rule.SegmentId, token);
        }

        var existing = positionFromInput switch
        {
            null => await this.prompt.ChooseFromListAsync(label, value.TargetingRules, r => r.Segment switch 
            {
                null => $"When {r.ComparisonAttribute} {Constants.ComparatorTypes.GetValueOrDefault(r.Comparator) ?? r.Comparator.ToUpperInvariant()} {r.ComparisonValue} then {r.Value}",
                _ => $"When {Constants.SegmentComparatorTypes.GetValueOrDefault(r.SegmentComparator) ?? r.SegmentComparator.ToUpperInvariant()} {r.Segment.Name} then {r.Value}"
            }, token),
            _ => value.TargetingRules.ElementAtOrDefault(positionFromInput.Value - 1)
        };

        if (existing is null)
            throw new ShowHelpException($"Rule not found.");

        return (existing, value.TargetingRules.IndexOf(existing));
    }
}

internal class AddTargetinRuleModel
{
    public string Attribute { get; set; }

    public string Comparator { get; set; }

    public string CompareTo { get; set; }

    public string FlagValue { get; set; }

    public string SegmentId { get; set; }

    public string SegmentComparator { get; set; }
}
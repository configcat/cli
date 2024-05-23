using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands.Flags;

class FlagPercentage(
    IFlagValueClient flagValueClient,
    IFlagClient flagClient,
    IWorkspaceLoader workspaceLoader,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> UpdatePercentageRulesAsync(int? flagId, string environmentId, string reason, UpdatePercentageModel[] rules, CancellationToken token)
    {
        if (rules.Length == 0)
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        var value = await flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);

        if (value.Setting.SettingType == SettingTypes.Boolean && rules.Length != 2)
            throw new ShowHelpException($"Boolean type can only have 2 percentage rules.");

        var result = new List<PercentageModel>();
        foreach (var percentageRule in rules)
        {
            if (!percentageRule.Value.TryParseFlagValue(value.Setting.SettingType, out var parsed))
                throw new ShowHelpException($"Flag value '{percentageRule.Value}' must respect the type '{value.Setting.SettingType}'.");

            result.Add(new PercentageModel { Percentage = percentageRule.Percentage, Value = parsed });
        }

        if (value.Setting.SettingType == SettingTypes.Boolean &&
            ((bool)result[0].Value && (bool)result[1].Value ||
             !(bool)result[0].Value && !(bool)result[1].Value))
            throw new ShowHelpException($"Boolean percentage rules cannot have the same value.");

        if (await workspaceLoader.NeedsReasonAsync(environmentId, token) && reason.IsEmpty())
            reason = await prompt.GetStringAsync("Mandatory reason", token);
        
        value.PercentageRules = result;
        await flagValueClient.ReplaceValueAsync(flag.SettingId, environmentId, reason, value, token);

        return ExitCodes.Ok;
    }

    public async Task<int> DeletePercentageRulesAsync(int? flagId, string environmentId, string reason, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        if (await workspaceLoader.NeedsReasonAsync(environmentId, token) && reason.IsEmpty())
            reason = await prompt.GetStringAsync("Mandatory reason", token);
        
        var value = await flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);
        value.PercentageRules = [];
        await flagValueClient.ReplaceValueAsync(flag.SettingId, environmentId, reason, value, token);

        return ExitCodes.Ok;
    }
}
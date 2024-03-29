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
using ConfigCat.Cli.Services.Extensions;

namespace ConfigCat.Cli.Commands.Flags.V2;

internal class FlagValueV2(
    IFlagValueV2Client flagValueClient,
    IFlagClient flagClient,
    IConfigClient configClient,
    IEnvironmentClient environmentClient,
    ISegmentClient segmentClient,
    IWorkspaceLoader workspaceLoader,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ShowValueAsync(int? flagId, bool json, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);

        var config = await configClient.GetConfigAsync(flag.ConfigId, token);
        var environments = await environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token);
        var segments = (await segmentClient.GetSegmentsAsync(config.Product.ProductId, token)).ToArray();
        var flags = (await flagClient.GetFlagsAsync(config.ConfigId, token)).ToArray();

        if (json)
        {
            var valuesInJson = new List<ValueV2InEnvironmentJsonOutput>();
            foreach (var environment in environments)
            {
                var value = await flagValueClient.GetValueAsync(flag.SettingId, environment.EnvironmentId, token);
                valuesInJson.Add(new ValueV2InEnvironmentJsonOutput
                {
                    EnvironmentId = environment.EnvironmentId,
                    EnvironmentName = environment.Name,
                    TargetingRules = value.TargetingRules,
                });
            }

            output.RenderJson(new FlagValueV2JsonOutput
            {
                Setting = flag,
                Values = valuesInJson
            });

            return ExitCodes.Ok;
        }

        var separatorLength = flag.Name.Length + flag.Key.Length + flag.SettingId.ToString().Length + 9;

        output.WriteDarkGray(new string('-', separatorLength));
        output.WriteLine().Write(" ");
        output.WriteColor($" {flag.Name} ", ConsoleColor.White, ConsoleColor.DarkGreen);
        output.Write($" ({flag.Key}) ");
        output.WriteDarkGray($"[{flag.SettingId}]").WriteLine();
        output.WriteDarkGray(new string('-', separatorLength)).WriteLine();

        foreach (var environment in environments)
        {
            var value = await flagValueClient.GetValueAsync(flag.SettingId, environment.EnvironmentId, token);

            output.WriteDarkGray($"| ")
                .Write(environment.Name)
                .WriteDarkGray($" [{environment.EnvironmentId}]");

            if (value.TargetingRules.Count > 0)
            {
                output.WriteLine().WriteDarkGray($"|");
                foreach (var (targeting, ti) in value.TargetingRules.Select((item, index) => (item, index)))
                {
                    output.WriteLine()
                        .WriteDarkGray($"| ")
                        .Write($"{ti + 1}.");
                    if (!targeting.Conditions.IsEmpty())
                    {
                        foreach (var (condition, i) in targeting.Conditions.Select((item, index) => (item, index)))
                        {
                            if (condition.SegmentCondition is not null)
                            {
                                var comparatorName = Constants.SegmentComparatorTypes.GetValueOrDefault(condition.SegmentCondition.Comparator) ?? condition.SegmentCondition.Comparator.ToUpperInvariant();
                                var segment = segments.FirstOrDefault(s => s.SegmentId == condition.SegmentCondition.SegmentId);
                                if (i == 0)
                                {
                                    output.WriteDarkGray($" If ")
                                        .WriteYellow($"{comparatorName} ")
                                        .WriteCyan($"{segment?.Name}");
                                }
                                else
                                {
                                    output.WriteLine()
                                        .WriteDarkGray($"| ")
                                        .WriteDarkGray($"    && ")
                                        .WriteYellow($"{comparatorName} ")
                                        .WriteCyan($"{segment?.Name}");
                                }
                                
                            }
                            else if (condition.UserCondition is not null)
                            {
                                var comparatorName = Constants.UserComparatorTypes.GetValueOrDefault(condition.UserCondition.Comparator) ?? condition.UserCondition.Comparator.ToUpperInvariant();
                                if (i == 0)
                                {
                                    output.WriteDarkGray($" If ")
                                        .WriteCyan($"{condition.UserCondition.ComparisonAttribute} ")
                                        .WriteYellow($"{comparatorName} ")
                                        .WriteCyan($"{condition.UserCondition.ComparisonValue.ToStringValue()}");
                                }
                                else
                                {
                                    output.WriteLine()
                                        .WriteDarkGray($"| ")
                                        .WriteDarkGray($"    && ")
                                        .WriteCyan($"{condition.UserCondition.ComparisonAttribute} ")
                                        .WriteYellow($"{comparatorName} ")
                                        .WriteCyan($"{condition.UserCondition.ComparisonValue.ToStringValue()}");
                                }
                            }   
                            else if (condition.PrerequisiteFlagCondition is not null)
                            {
                                var comparatorName = Constants.PrerequisiteComparatorTypes.GetValueOrDefault(condition.PrerequisiteFlagCondition.Comparator) ?? condition.PrerequisiteFlagCondition.Comparator.ToUpperInvariant();
                                var preReq = flags.FirstOrDefault(f => f.SettingId == condition.PrerequisiteFlagCondition.PrerequisiteSettingId);
                                if (i == 0)
                                {
                                    output.WriteDarkGray($" If ")
                                        .WriteCyan($"{preReq?.Key} ")
                                        .WriteYellow($"{comparatorName} ")
                                        .WriteCyan($"{condition.PrerequisiteFlagCondition.PrerequisiteComparisonValue.ToSingle(preReq?.SettingType)}");
                                }
                                else
                                {
                                    output.WriteLine()
                                        .WriteDarkGray($"| ")
                                        .WriteDarkGray($"    && ")
                                        .WriteCyan($"{preReq?.Key} ")
                                        .WriteYellow($"{comparatorName} ")
                                        .WriteCyan($"{condition.PrerequisiteFlagCondition.PrerequisiteComparisonValue.ToSingle(preReq?.SettingType)}");
                                }
                            }   
                        }
                    }
                    
                    if (targeting.Value is not null)
                    {
                        output.WriteLine().WriteDarkGray($"|    Then: ").WriteMagenta(targeting.Value.ToSingle(flag.SettingType).ToString());
                    } 
                    else if (targeting.PercentageOptions.Count > 0)
                    {
                        if (!targeting.Conditions.IsEmpty())
                        {
                            output.WriteLine().WriteDarkGray($"|    Then:");
                            foreach (var percentage in targeting.PercentageOptions)
                            {
                                output.WriteLine()
                                    .WriteDarkGray("|     ")
                                    .WriteCyan($"{percentage.Percentage}%")
                                    .WriteDarkGray(" -> ")
                                    .WriteMagenta(percentage.Value.ToSingle(flag.SettingType).ToString());
                            }
                        }
                        else
                        {
                            foreach (var (percentage, i) in targeting.PercentageOptions.Select((item, index) => (item, index)))
                            {
                                if (i == 0)
                                {
                                    output.WriteCyan($" {percentage.Percentage}%")
                                        .WriteDarkGray(" -> ")
                                        .WriteMagenta(percentage.Value.ToSingle(flag.SettingType).ToString());
                                }
                                else
                                {
                                    output.WriteLine()
                                        .WriteDarkGray("|    ")
                                        .WriteCyan($"{percentage.Percentage}%")
                                        .WriteDarkGray(" -> ")
                                        .WriteMagenta(percentage.Value.ToSingle(flag.SettingType).ToString());
                                }
                            }
                        }
                    }
                    if (ti < value.TargetingRules.Count - 1)
                    {
                        output.WriteLine().WriteDarkGray($"|");
                    }
                }
            }

            output.WriteLine().WriteDarkGray($"|");
            
            output.WriteLine()
                .WriteDarkGray($"| Default: ")
                .WriteMagenta(value.DefaultValue.ToSingle(flag.SettingType).ToString())
                .WriteLine()
                .WriteDarkGray(new string('-', separatorLength))
                .WriteLine();
        }

        return ExitCodes.Ok;
    }

    public async Task<int> UpdateFlagValueAsync(int? flagId, string environmentId, string flagValue, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        var value = await flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);

        if (flagValue.IsEmpty())
            flagValue = await prompt.GetStringAsync($"Value", token, value.DefaultValue.ToSingle(flag.SettingType).ToString());

        if (!flagValue.TryParseFlagValue(value.Setting.SettingType, out var parsed))
            throw new ShowHelpException($"Flag value '{flagValue}' must respect the type '{value.Setting.SettingType}'.");

        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Replace($"/defaultValue/{flag.SettingType.ToValuePropertyName()}", parsed);

        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
        return ExitCodes.Ok;
    }
}

internal class FlagValueV2JsonOutput
{
    public FlagModel Setting { get; set; }

    public IEnumerable<ValueV2InEnvironmentJsonOutput> Values { get; set; }
}

internal class ValueV2InEnvironmentJsonOutput : FlagValueV2Model
{
    public string EnvironmentId { get; set; }

    public string EnvironmentName { get; set; }
}
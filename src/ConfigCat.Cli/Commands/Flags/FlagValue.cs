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

internal class FlagValue(
    IFlagValueClient flagValueClient,
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

        if (json)
        {
            var valuesInJson = new List<ValueInEnvironmentJsonOutput>();
            foreach (var environment in environments)
            {
                var value = await flagValueClient.GetValueAsync(flag.SettingId, environment.EnvironmentId, token);
                valuesInJson.Add(new ValueInEnvironmentJsonOutput
                {
                    EnvironmentId = environment.EnvironmentId,
                    EnvironmentName = environment.Name,
                    PercentageRules = value.PercentageRules,
                    TargetingRules = value.TargetingRules,
                    Value = value.Value
                });
            }

            output.RenderJson(new FlagValueJsonOutput
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
                foreach (var rule in value.TargetingRules.Where(rule => !rule.SegmentId.IsEmpty()))
                {
                    rule.Segment = await segmentClient.GetSegmentAsync(rule.SegmentId, token);
                }

                output.WriteLine().WriteDarkGray($"|");
                foreach (var targeting in value.TargetingRules)
                {
                    if (targeting.Segment is not null)
                    {
                        var comparatorName = Constants.SegmentComparatorTypes.GetValueOrDefault(targeting.SegmentComparator) ?? targeting.SegmentComparator.ToUpperInvariant();
                        output.WriteLine()
                            .WriteDarkGray($"| ")
                            .Write($"{value.TargetingRules.IndexOf(targeting) + 1}.")
                            .WriteDarkGray($" When ")
                            .WriteYellow($"{comparatorName} ")
                            .WriteCyan($"{targeting.Segment.Name} ")
                            .WriteDarkGray("then ")
                            .WriteMagenta(targeting.Value.ToString());
                    }
                    else
                    {
                        var comparatorName = Constants.ComparatorTypes.GetValueOrDefault(targeting.Comparator) ?? targeting.Comparator.ToUpperInvariant();
                        output.WriteLine()
                            .WriteDarkGray($"| ")
                            .Write($"{value.TargetingRules.IndexOf(targeting) + 1}.")
                            .WriteDarkGray($" When ")
                            .WriteCyan($"{targeting.ComparisonAttribute} ")
                            .WriteYellow($"{comparatorName} ")
                            .WriteCyan($"{targeting.ComparisonValue} ")
                            .WriteDarkGray("then ")
                            .WriteMagenta(targeting.Value.ToString());
                    }
                }
            }

            if (value.PercentageRules.Count > 0)
            {
                output.WriteLine().WriteDarkGray($"|");
                foreach (var percentage in value.PercentageRules)
                {
                    output.WriteLine()
                        .WriteDarkGray("| ")
                        .WriteCyan($"{percentage.Percentage}%")
                        .WriteDarkGray(" -> ")
                        .WriteMagenta(percentage.Value.ToString());
                }
                output.WriteLine().WriteDarkGray($"|");
            }
            else
            {
                output.WriteLine().WriteDarkGray($"|");
            }

            output.WriteLine()
                .WriteDarkGray($"| Default: ")
                .WriteMagenta(value.Value.ToString())
                .WriteLine()
                .WriteDarkGray(new string('-', separatorLength))
                .WriteLine();
        }

        return ExitCodes.Ok;
    }

    public async Task<int> UpdateFlagValueAsync(int? flagId, string environmentId, string flagValue, string reason, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);

        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

        var value = await flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);

        if (flagValue.IsEmpty())
            flagValue = await prompt.GetStringAsync($"Value", token, value.Value.ToString());

        if (!flagValue.TryParseFlagValue(value.Setting.SettingType, out var parsed))
            throw new ShowHelpException($"Flag value '{flagValue}' must respect the type '{value.Setting.SettingType}'.");

        if (await workspaceLoader.NeedsReasonAsync(environmentId, token) && reason.IsEmpty())
            reason = await prompt.GetStringAsync("Mandatory reason", token);
        
        var jsonPatchDocument = new JsonPatchDocument();
        jsonPatchDocument.Replace($"/value", parsed);

        await flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, reason, jsonPatchDocument.Operations, token);
        return ExitCodes.Ok;
    }
}

internal class FlagValueJsonOutput
{
    public FlagModel Setting { get; set; }

    public IEnumerable<ValueInEnvironmentJsonOutput> Values { get; set; }
}

internal class ValueInEnvironmentJsonOutput
{
    public string EnvironmentId { get; set; }

    public string EnvironmentName { get; set; }

    public List<PercentageModel> PercentageRules { get; set; }

    public List<TargetingModel> TargetingRules { get; set; }

    public object Value { get; set; }
}
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

namespace ConfigCat.Cli.Commands.Flags
{
    internal class FlagValue
    {
        private readonly IFlagValueClient flagValueClient;
        private readonly IFlagClient flagClient;
        private readonly IConfigClient configClient;
        private readonly IEnvironmentClient environmentClient;
        private readonly ISegmentClient segmentClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IOutput output;

        public FlagValue(IFlagValueClient flagValueClient,
            IFlagClient flagClient,
            IConfigClient configClient,
            IEnvironmentClient environmentClient,
            ISegmentClient segmentClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt,
            IOutput output)
        {
            this.flagValueClient = flagValueClient;
            this.flagClient = flagClient;
            this.configClient = configClient;
            this.environmentClient = environmentClient;
            this.segmentClient = segmentClient;
            this.workspaceLoader = workspaceLoader;
            this.prompt = prompt;
            this.output = output;
        }

        public async Task<int> ShowValueAsync(int? flagId, bool json, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            var config = await this.configClient.GetConfigAsync(flag.ConfigId, token);
            var environments = await this.environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token);

            if (json)
            {
                var valuesInJson = new List<ValueInEnvironmentJsonOutput>();
                foreach (var environment in environments)
                {
                    var value = await this.flagValueClient.GetValueAsync(flag.SettingId, environment.EnvironmentId, token);
                    valuesInJson.Add(new ValueInEnvironmentJsonOutput
                    {
                        EnvironmentId = environment.EnvironmentId,
                        EnvironmentName = environment.Name,
                        PercentageRules = value.PercentageRules,
                        TargetingRules = value.TargetingRules,
                        Value = value.Value
                    });
                }

                this.output.RenderJson(new FlagValueJsonOutput
                {
                    Setting = flag,
                    Values = valuesInJson
                });

                return ExitCodes.Ok;
            }

            var separatorLength = flag.Name.Length + flag.Key.Length + flag.SettingId.ToString().Length + 9;

            this.output.WriteDarkGray(new string('-', separatorLength));
            this.output.WriteLine().Write(" ");
            this.output.WriteColor($" {flag.Name} ", ConsoleColor.White, ConsoleColor.DarkGreen);
            this.output.Write($" ({flag.Key}) ");
            this.output.WriteDarkGray($"[{flag.SettingId}]").WriteLine();
            this.output.WriteDarkGray(new string('-', separatorLength)).WriteLine();

            foreach (var environment in environments)
            {
                var value = await this.flagValueClient.GetValueAsync(flag.SettingId, environment.EnvironmentId, token);

                this.output.WriteDarkGray($"| ")
                    .Write(environment.Name)
                    .WriteDarkGray($" [{environment.EnvironmentId}]");

                if (value.TargetingRules.Count > 0)
                {
                    foreach (var rule in value.TargetingRules.Where(rule => !rule.SegmentId.IsEmpty()))
                    {
                        rule.Segment = await this.segmentClient.GetSegmentAsync(rule.SegmentId, token);
                    }

                    this.output.WriteLine().WriteDarkGray($"|");
                    foreach (var targeting in value.TargetingRules)
                    {
                        if (targeting.Segment is not null)
                        {
                            var comparatorName = Constants.SegmentComparatorTypes.GetValueOrDefault(targeting.SegmentComparator) ?? targeting.SegmentComparator.ToUpperInvariant();
                            this.output.WriteLine()
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
                            this.output.WriteLine()
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
                    this.output.WriteLine().WriteDarkGray($"|");
                    foreach (var percentage in value.PercentageRules)
                    {
                        this.output.WriteLine()
                            .WriteDarkGray("| ")
                            .WriteCyan($"{percentage.Percentage}%")
                            .WriteDarkGray(" -> ")
                            .WriteMagenta(percentage.Value.ToString());
                    }
                    this.output.WriteLine().WriteDarkGray($"|");
                }
                else
                {
                    this.output.WriteLine().WriteDarkGray($"|");
                }

                this.output.WriteLine()
                    .WriteDarkGray($"| Default: ")
                    .WriteMagenta(value.Value.ToString())
                    .WriteLine()
                    .WriteDarkGray(new string('-', separatorLength))
                    .WriteLine();
            }

            return ExitCodes.Ok;
        }

        public async Task<int> UpdateFlagValueAsync(int? flagId, string environmentId, string flagValue, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var value = await this.flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);

            if (flagValue.IsEmpty())
                flagValue = await this.prompt.GetStringAsync($"Value", token, value.Value.ToString());

            if (!flagValue.TryParseFlagValue(value.Setting.SettingType, out var parsed))
                throw new ShowHelpException($"Flag value '{flagValue}' must respect the type '{value.Setting.SettingType}'.");

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Replace($"/value", parsed);

            await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
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
}
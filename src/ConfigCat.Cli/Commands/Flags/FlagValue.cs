using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class FlagValue
    {
        private readonly IFlagValueClient flagValueClient;
        private readonly IFlagClient flagClient;
        private readonly IConfigClient configClient;
        private readonly IEnvironmentClient environmentClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IOutput output;

        public FlagValue(IFlagValueClient flagValueClient,
            IFlagClient flagClient,
            IConfigClient configClient,
            IEnvironmentClient environmentClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt,
            IOutput output)
        {
            this.flagValueClient = flagValueClient;
            this.flagClient = flagClient;
            this.configClient = configClient;
            this.environmentClient = environmentClient;
            this.workspaceLoader = workspaceLoader;
            this.prompt = prompt;
            this.output = output;
        }

        public async Task<int> ListAllAsync(int? flagId, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            var config = await this.configClient.GetConfigAsync(flag.ConfigId, token);
            var environments = await this.environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token);
            var separatorLength = flag.Name.Length + flag.Key.Length + flag.SettingId.ToString().Length + 9;

            this.output.WriteColored(new string('-', separatorLength), ForegroundColorSpan.DarkGray());
            this.output.WriteLine();
            this.output.Write(" ");
            this.output.WriteColoredWithBackground($" {flag.Name} ", ForegroundColorSpan.Rgb(255, 255, 255), BackgroundColorSpan.Green());
            this.output.WriteStandout($" ({flag.Key}) ");
            this.output.WriteColored($"[{flag.SettingId}]", ForegroundColorSpan.DarkGray());
            this.output.WriteLine();
            this.output.WriteColored(new string('-', separatorLength), ForegroundColorSpan.DarkGray());
            this.output.WriteLine();

            foreach (var environment in environments)
            {
                var value = await this.flagValueClient.GetValueAsync(flag.SettingId, environment.EnvironmentId, token);

                this.output.WriteColored($"| ", ForegroundColorSpan.DarkGray());
                this.output.WriteUnderline(environment.Name);
                this.output.WriteColored($" [{environment.EnvironmentId}]", ForegroundColorSpan.DarkGray());

                if (value.TargetingRules.Count > 0)
                {
                    this.output.WriteLine();
                    this.output.WriteColored($"|", ForegroundColorSpan.DarkGray());
                    foreach (var targeting in value.TargetingRules)
                    {
                        var comparatorName = Constants.ComparatorTypes.GetValueOrDefault(targeting.Comparator) ?? targeting.Comparator.ToUpperInvariant();
                        this.output.WriteLine();
                        this.output.WriteColored($"| ", ForegroundColorSpan.DarkGray());
                        this.output.Write($"{value.TargetingRules.IndexOf(targeting) + 1}.");
                        this.output.WriteColored($" When ", ForegroundColorSpan.DarkGray());
                        this.output.WriteColored($"{targeting.ComparisonAttribute} ", ForegroundColorSpan.LightCyan());
                        this.output.WriteColored($"{comparatorName} ", ForegroundColorSpan.LightYellow());
                        this.output.WriteColored($"{targeting.ComparisonValue} ", ForegroundColorSpan.LightCyan());
                        this.output.WriteColored("then ", ForegroundColorSpan.DarkGray());
                        this.output.WriteColored(targeting.Value.ToString(), ForegroundColorSpan.LightMagenta());
                    }
                }

                if (value.PercentageRules.Count > 0)
                {
                    this.output.WriteLine();
                    this.output.WriteColored($"|", ForegroundColorSpan.DarkGray());
                    foreach (var percentage in value.PercentageRules)
                    {
                        this.output.WriteLine();
                        this.output.WriteColored("| ", ForegroundColorSpan.DarkGray());
                        this.output.WriteColored($"{percentage.Percentage}%", ForegroundColorSpan.LightCyan());
                        this.output.WriteColored(" -> ", ForegroundColorSpan.DarkGray());
                        this.output.WriteColored(percentage.Value.ToString(), ForegroundColorSpan.LightMagenta());
                    }
                    this.output.WriteLine();
                    this.output.WriteColored($"|", ForegroundColorSpan.DarkGray());
                }
                else
                {
                    this.output.WriteLine();
                    this.output.WriteColored($"|", ForegroundColorSpan.DarkGray());
                }

                this.output.WriteLine();
                this.output.WriteColored($"| Default: ", ForegroundColorSpan.DarkGray());
                this.output.WriteColored(value.Value.ToString(), ForegroundColorSpan.LightMagenta());
                this.output.WriteLine();
                this.output.WriteColored(new string('-', separatorLength), ForegroundColorSpan.DarkGray());
                this.output.WriteLine();
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
}

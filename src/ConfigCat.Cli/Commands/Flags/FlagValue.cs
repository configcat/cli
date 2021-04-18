using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class FlagValue : ICommandDescriptor
    {
        private readonly IFlagValueClient flagValueClient;
        private readonly IFlagClient flagClient;
        private readonly IConfigClient configClient;
        private readonly IProductClient productClient;
        private readonly IEnvironmentClient environmentClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public FlagValue(IFlagValueClient flagValueClient,
            IFlagClient flagClient,
            IConfigClient configClient,
            IProductClient productClient,
            IEnvironmentClient environmentClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.flagValueClient = flagValueClient;
            this.flagClient = flagClient;
            this.configClient = configClient;
            this.productClient = productClient;
            this.environmentClient = environmentClient;
            this.workspaceLoader = workspaceLoader;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "value";

        public string Description => "Show, and update flag values in different environments";

        public IEnumerable<string> Aliases => new[] { "v" };

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "show",
                Description = "Show flag values, targeting, and percentage rules for each environment",
                Handler = this.CreateHandler(nameof(FlagValue.ListAllAsync)),
                Aliases = new[] { "sh", "pr", "print" },
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting") 
                    { 
                        Name = "flag-id"
                    },
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Description = "Update the flag's value",
                Handler = this.CreateHandler(nameof(FlagValue.UpdateFlagValueAsync)),
                Aliases = new[] { "up" },
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the update must be applied"),
                    new Option<string>(new[] { "--flag-value", "-f" }, "The value to serve, it must respect the setting type"),
                },
            },
        };

        public async Task<int> ListAllAsync(int? flagId, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            var output = this.accessor.ExecutionContext.Output;
            var config = await this.configClient.GetConfigAsync(flag.ConfigId, token);
            var environments = await this.environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token);
            var separatorLength = flag.Name.Length + flag.Key.Length + flag.SettingId.ToString().Length + 9;

            output.WriteColored(new string('-', separatorLength), ForegroundColorSpan.DarkGray());
            output.WriteLine();
            output.Write(" ");
            output.WriteColoredWithBackground($" {flag.Name} ", ForegroundColorSpan.Rgb(255, 255, 255), BackgroundColorSpan.Green());
            output.WriteStandout($" ({flag.Key}) ");
            output.WriteColored($"[{flag.SettingId}]", ForegroundColorSpan.DarkGray());
            output.WriteLine();
            output.WriteColored(new string('-', separatorLength), ForegroundColorSpan.DarkGray());
            output.WriteLine();

            foreach (var environment in environments)
            {
                var value = await this.flagValueClient.GetValueAsync(flag.SettingId, environment.EnvironmentId, token);

                output.WriteColored($"| ", ForegroundColorSpan.DarkGray());
                output.WriteUnderline(environment.Name);
                output.WriteColored($" [{environment.EnvironmentId}]", ForegroundColorSpan.DarkGray());

                if (value.TargetingRules.Count > 0)
                {
                    output.WriteLine();
                    output.WriteColored($"|", ForegroundColorSpan.DarkGray());
                    foreach (var targeting in value.TargetingRules)
                    {
                        var comparatorName = Constants.ComparatorTypes.GetValueOrDefault(targeting.Comparator) ?? targeting.Comparator.ToUpperInvariant();
                        output.WriteLine();
                        output.WriteColored($"| ", ForegroundColorSpan.DarkGray());
                        output.Write($"{value.TargetingRules.IndexOf(targeting) + 1}.");
                        output.WriteColored($" When ", ForegroundColorSpan.DarkGray());
                        output.WriteColored($"{targeting.ComparisonAttribute} ", ForegroundColorSpan.LightCyan());
                        output.WriteColored($"{comparatorName} ", ForegroundColorSpan.LightYellow());
                        output.WriteColored($"{targeting.ComparisonValue} ", ForegroundColorSpan.LightCyan());
                        output.WriteColored("then ", ForegroundColorSpan.DarkGray());
                        output.WriteColored(targeting.Value.ToString(), ForegroundColorSpan.LightMagenta());
                    }
                }

                if (value.PercentageRules.Count > 0)
                {
                    output.WriteLine();
                    output.WriteColored($"|", ForegroundColorSpan.DarkGray());
                    foreach (var percentage in value.PercentageRules)
                    {
                        output.WriteLine();
                        output.WriteColored("| ", ForegroundColorSpan.DarkGray());
                        output.WriteColored($"{percentage.Percentage}%", ForegroundColorSpan.LightCyan());
                        output.WriteColored(" -> ", ForegroundColorSpan.DarkGray());
                        output.WriteColored(percentage.Value.ToString(), ForegroundColorSpan.LightMagenta());
                    }
                    output.WriteLine();
                    output.WriteColored($"|", ForegroundColorSpan.DarkGray());
                }
                else
                {
                    output.WriteLine();
                    output.WriteColored($"|", ForegroundColorSpan.DarkGray());
                }

                output.WriteLine();
                output.WriteColored($"| Default: ", ForegroundColorSpan.DarkGray());
                output.WriteColored(value.Value.ToString(), ForegroundColorSpan.LightMagenta());
                output.WriteLine();
                output.WriteColored(new string('-', separatorLength), ForegroundColorSpan.DarkGray());
                output.WriteLine();
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

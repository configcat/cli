using ConfigCat.Cli.Api.Config;
using ConfigCat.Cli.Api.Environment;
using ConfigCat.Cli.Api.Flag;
using ConfigCat.Cli.Api.Flag.Value;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Utils;
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
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public FlagValue(IFlagValueClient flagValueClient,
            IFlagClient flagClient,
            IConfigClient configClient,
            IProductClient productClient,
            IEnvironmentClient environmentClient,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.flagValueClient = flagValueClient;
            this.flagClient = flagClient;
            this.configClient = configClient;
            this.productClient = productClient;
            this.environmentClient = environmentClient;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "value";

        public string Description => "Show, and update flag values in different environments";

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "print",
                Description = "Print flag values, targeting, and percentage rules for each environment",
                Handler = this.CreateHandler(nameof(FlagValue.ListAllAsync)),
                Arguments = new []
                {
                    new Argument<int>("flag-id") { Description = "The ID of the flag" }
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Description = "Update the flag's value",
                Handler = this.CreateHandler(nameof(FlagValue.UpdateFlagValueAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("flag-id") { Description = "ID of the flag" },
                    new Argument<string>("environment-id") { Description = "ID of the environment where the update must be applied" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--flag-value", "-f" }) { Description = "The value to serve, it must respect the setting type" },
                },
            },
        };

        public async Task<int> ListAllAsync(int flagId, CancellationToken token)
        {
            var output = this.accessor.ExecutionContext.Output;
            var flag = await this.flagClient.GetFlagAsync(flagId, token);
            var config = await this.configClient.GetConfigAsync(flag.ConfigId, token);
            var environments = await this.environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token);
            var separatorLength = flag.Name.Length + flag.Key.Length + flag.SettingId.ToString().Length + 9;

            output.WriteColored(new string('-', separatorLength), ForegroundColorSpan.DarkGray());
            output.WriteLine();
            output.Write(" ");
            output.WriteColoredWithBackground($" {flag.Name} ", ForegroundColorSpan.Rgb(255,255,255), BackgroundColorSpan.Green());
            output.WriteStandout($" ({flag.Key}) ");
            output.WriteColored($"[{flag.SettingId}]", ForegroundColorSpan.DarkGray());
            output.WriteLine();
            output.WriteColored(new string('-', separatorLength), ForegroundColorSpan.DarkGray());
            output.WriteLine();

            foreach (var environment in environments)
            {
                var value = await this.flagValueClient.GetValueAsync(flagId, environment.EnvironmentId, token);

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

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateFlagValueAsync(int flagId, string environmentId, string flagValue, CancellationToken token)
        {
            var value = await this.flagValueClient.GetValueAsync(flagId, environmentId, token);

            if (!token.IsCancellationRequested && flagValue.IsEmpty())
                flagValue = this.prompt.GetString($"Value", value.Value.ToString());

            if (!flagValue.TryParseFlagValue(value.Setting.SettingType, out var parsed))
            {
                this.accessor.ExecutionContext.Output.WriteError($"Flag value '{flagValue}' must respect the type '{value.Setting.SettingType}'");
                return Constants.ExitCodes.Error;
            }

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Replace($"/value", parsed);

            await this.flagValueClient.UpdateValueAsync(flagId, environmentId, jsonPatchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

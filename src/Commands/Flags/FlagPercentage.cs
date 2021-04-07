using ConfigCat.Cli.Api.Flag.Value;
using ConfigCat.Cli.Options;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class FlagPercentage : ICommandDescriptor
    {
        private readonly IFlagValueClient flagValueClient;
        private readonly IExecutionContextAccessor accessor;

        public FlagPercentage(IFlagValueClient flagValueClient,
            IExecutionContextAccessor accessor)
        {
            this.flagValueClient = flagValueClient;
            this.accessor = accessor;
        }

        public string Name => "percentage";

        public string Description => "Manage percentage rules";

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "update",
                Description = "Update percentage rules",
                Handler = this.CreateHandler(nameof(FlagPercentage.UpdatePercentageRulesAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("flag-id") { Description = "ID of the flag" },
                    new Argument<string>("environment-id") { Description = "ID of the environment where the update must be applied" },
                    new PercentageRuleArgument()
                },
            },
            new SubCommandDescriptor
            {
                Name = "clear",
                Description = "Delete all percentage rules",
                Handler = this.CreateHandler(nameof(FlagPercentage.DeletePercentageRulesAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("flag-id") { Description = "ID of the flag" },
                    new Argument<string>("environment-id") { Description = "ID of the environment from where the rules must be deleted" },
                },
            },
        };

        public async Task<int> UpdatePercentageRulesAsync(int flagId, string environmentId, UpdatePercentageModel[] rules, CancellationToken token)
        {
            if (rules.Length == 0)
            {
                this.accessor.ExecutionContext.Output.Write($"No changes detected... ");
                this.accessor.ExecutionContext.Output.WriteYellow("Skipped.");
                return Constants.ExitCodes.Ok;
            }

            var value = await this.flagValueClient.GetValueAsync(flagId, environmentId, token);

            if (value.Setting.SettingType == Constants.SettingTypes.Boolean && rules.Length != 2)
            {
                this.accessor.ExecutionContext.Output.WriteError($"Boolean type can only have 2 percentage rules.");
                return Constants.ExitCodes.Error;
            }

            var result = new List<PercentageModel>();
            foreach (var percentageRule in rules)
            {
                if (!percentageRule.Value.TryParseFlagValue(value.Setting.SettingType, out var parsed))
                {
                    this.accessor.ExecutionContext.Output.WriteError($"Flag value '{percentageRule.Value}' must respect the type '{value.Setting.SettingType}'");
                    return Constants.ExitCodes.Error;
                }

                result.Add(new PercentageModel { Percentage = percentageRule.Percentage, Value = parsed });
            }

            if (value.Setting.SettingType == Constants.SettingTypes.Boolean &&
                ((bool)result[0].Value && (bool)result[1].Value ||
                !(bool)result[0].Value && !(bool)result[1].Value))
            {
                this.accessor.ExecutionContext.Output.WriteError($"Boolean percentage rules cannot have the same value.");
                return Constants.ExitCodes.Error;
            }

            value.PercentageRules = result;
            await this.flagValueClient.ReplaceValueAsync(flagId, environmentId, value, token);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeletePercentageRulesAsync(int flagId, string environmentId, CancellationToken token)
        {
            var value = await this.flagValueClient.GetValueAsync(flagId, environmentId, token);
            value.PercentageRules = new List<PercentageModel>();
            await this.flagValueClient.ReplaceValueAsync(flagId, environmentId, value, token);

            return Constants.ExitCodes.Ok;
        }
    }
}

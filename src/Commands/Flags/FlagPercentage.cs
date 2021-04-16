using ConfigCat.Cli.Api;
using ConfigCat.Cli.Api.Flag;
using ConfigCat.Cli.Api.Flag.Value;
using ConfigCat.Cli.Exceptions;
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
        private readonly IFlagClient flagClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IExecutionContextAccessor accessor;

        public FlagPercentage(IFlagValueClient flagValueClient,
            IFlagClient flagClient,
            IWorkspaceLoader workspaceLoader,
            IExecutionContextAccessor accessor)
        {
            this.flagValueClient = flagValueClient;
            this.flagClient = flagClient;
            this.workspaceLoader = workspaceLoader;
            this.accessor = accessor;
        }

        public string Name => "percentage";

        public string Description => "Manage percentage rules";

        public IEnumerable<string> Aliases => new[] { "%" };

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "update",
                Aliases = new[] { "up" },
                Description = "Update percentage rules",
                Handler = this.CreateHandler(nameof(FlagPercentage.UpdatePercentageRulesAsync)),
                Arguments = new Argument[]
                {
                    new PercentageRuleArgument()
                },
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the update must be applied"),
                }
            },
            new SubCommandDescriptor
            {
                Name = "clear",
                Aliases = new[] { "clr" },
                Description = "Delete all percentage rules",
                Handler = this.CreateHandler(nameof(FlagPercentage.DeletePercentageRulesAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment from where the rules must be deleted"),
                }
            },
        };

        public async Task<int> UpdatePercentageRulesAsync(int? flagId, string environmentId, UpdatePercentageModel[] rules, CancellationToken token)
        {
            if (rules.Length == 0)
            {
                this.accessor.ExecutionContext.Output.WriteNoChange();
                return Constants.ExitCodes.Ok;
            }

            var flag = flagId is null 
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var value = await this.flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);

            if (value.Setting.SettingType == Constants.SettingTypes.Boolean && rules.Length != 2)
                throw new ShowHelpException($"Boolean type can only have 2 percentage rules.");

            var result = new List<PercentageModel>();
            foreach (var percentageRule in rules)
            {
                if (!percentageRule.Value.TryParseFlagValue(value.Setting.SettingType, out var parsed))
                    throw new ShowHelpException($"Flag value '{percentageRule.Value}' must respect the type '{value.Setting.SettingType}'.");

                result.Add(new PercentageModel { Percentage = percentageRule.Percentage, Value = parsed });
            }

            if (value.Setting.SettingType == Constants.SettingTypes.Boolean &&
                ((bool)result[0].Value && (bool)result[1].Value ||
                !(bool)result[0].Value && !(bool)result[1].Value))
                throw new ShowHelpException($"Boolean percentage rules cannot have the same value.");

            value.PercentageRules = result;
            await this.flagValueClient.ReplaceValueAsync(flag.SettingId, environmentId, value, token);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeletePercentageRulesAsync(int? flagId, string environmentId, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var value = await this.flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);
            value.PercentageRules = new List<PercentageModel>();
            await this.flagValueClient.ReplaceValueAsync(flag.SettingId, environmentId, value, token);

            return Constants.ExitCodes.Ok;
        }
    }
}

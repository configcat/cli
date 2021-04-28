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

namespace ConfigCat.Cli.Commands
{
    class FlagPercentage
    {
        private readonly IFlagValueClient flagValueClient;
        private readonly IFlagClient flagClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IOutput output;

        public FlagPercentage(IFlagValueClient flagValueClient,
            IFlagClient flagClient,
            IWorkspaceLoader workspaceLoader,
            IOutput output)
        {
            this.flagValueClient = flagValueClient;
            this.flagClient = flagClient;
            this.workspaceLoader = workspaceLoader;
            this.output = output;
        }

        public async Task<int> UpdatePercentageRulesAsync(int? flagId, string environmentId, UpdatePercentageModel[] rules, CancellationToken token)
        {
            if (rules.Length == 0)
            {
                this.output.WriteNoChange();
                return ExitCodes.Ok;
            }

            var flag = flagId is null 
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var value = await this.flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);

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

            value.PercentageRules = result;
            await this.flagValueClient.ReplaceValueAsync(flag.SettingId, environmentId, value, token);

            return ExitCodes.Ok;
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

            return ExitCodes.Ok;
        }
    }
}

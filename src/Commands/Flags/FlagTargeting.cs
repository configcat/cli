using ConfigCat.Cli.Api.Flag;
using ConfigCat.Cli.Api.Flag.Value;
using ConfigCat.Cli.Configuration;
using ConfigCat.Cli.Exceptions;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class FlagTargeting : ICommandDescriptor
    {
        private readonly IFlagValueClient flagValueClient;
        private readonly IFlagClient flagClient;
        private readonly IWorkspaceManager workspaceManager;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public FlagTargeting(IFlagValueClient flagValueClient,
            IFlagClient flagClient,
            IWorkspaceManager workspaceManager,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.flagValueClient = flagValueClient;
            this.flagClient = flagClient;
            this.workspaceManager = workspaceManager;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "targeting";

        public string Description => "Manage targeting rules";

        public IEnumerable<string> Aliases => new[] { "t" };

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "create",
                Aliases = new[] { "cr" },
                Description = "Create new targeting rule",
                Handler = this.CreateHandler(nameof(FlagTargeting.AddTargetinRuleAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the rule must be created"),
                    new Option<string>(new[] { "--attribute", "-a" }, "The user attribute to compare"),
                    new Option<string>(new[] { "--comparator", "-c" }, "The comparison operator")
                    {
                        Argument = new Argument<string>()
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                    },
                    new Option<string>(new[] { "--compare-to", "-t" }, "The value to compare against"),
                    new Option<string>(new[] { "--flag-value", "-f" }, "The value to serve when the comparison matches, it must respect the setting type"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Aliases = new[] { "up" },
                Description = "Update targeting rule",
                Handler = this.CreateHandler(nameof(FlagTargeting.UpdateTargetinRuleAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("position", "The position of the updating targeting rule"),
                },
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the update must be applied"),
                    new Option<string>(new[] { "--attribute", "-a" }, "The user attribute to compare"),
                    new Option<string>(new[] { "--comparator", "-c" }, "The comparison operator")
                    {
                        Argument = new Argument<string>()
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                    },
                    new Option<string>(new[] { "--compare-to", "-t" }, "The value to compare against"),
                    new Option<string>(new[] { "--flag-value", "-f" }, "The value to serve when the comparison matches, it must respect the setting type"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete targeting rule",
                Handler = this.CreateHandler(nameof(FlagTargeting.DeleteTargetinRuleAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("position", "The position of the targeting rule to delete"),
                },
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment from where the rule must be deleted"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "move",
                Aliases = new[] { "mv" },
                Description = "Move a targeting rule into a different position",
                Handler = this.CreateHandler(nameof(FlagTargeting.MoveTargetinRuleAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("from", "The actual position of the targeting rule"),
                    new Argument<int>("to", "The desired position of the targeting rule"),
                },
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the move must be applied"),
                },
            },
        };

        public async Task<int> AddTargetinRuleAsync(int? flagId, string environmentId, AddTargetinRuleModel addTargetinRuleModel, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceManager.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceManager.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            await this.ValidateAddModel(addTargetinRuleModel, token);

            if (!addTargetinRuleModel.FlagValue.TryParseFlagValue(flag.SettingType, out var parsed))
                throw new ShowHelpException($"Flag value '{addTargetinRuleModel.FlagValue}' must respect the type '{flag.SettingType}'.");

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Add($"/{Constants.TargetingRuleJsonName}/-", new TargetingModel
            {
                Comparator = addTargetinRuleModel.Comparator,
                ComparisonAttribute = addTargetinRuleModel.Attribute,
                ComparisonValue = addTargetinRuleModel.CompareTo,
                Value = parsed
            });

            await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateTargetinRuleAsync(int? flagId, string environmentId, int position, AddTargetinRuleModel addTargetinRuleModel, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceManager.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceManager.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var value = await this.flagValueClient.GetValueAsync(flag.SettingId, environmentId, token);
            var existing = value.TargetingRules.ElementAtOrDefault(position - 1);

            if (existing is null)
                throw new ShowHelpException($"Rule not found at position {position}.");

            await this.ValidateAddModel(addTargetinRuleModel, token, existing);

            if (!addTargetinRuleModel.FlagValue.TryParseFlagValue(value.Setting.SettingType, out var parsed))
                throw new ShowHelpException($"Flag value '{addTargetinRuleModel.FlagValue}' must respect the type '{value.Setting.SettingType}'.");

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Replace($"/{Constants.TargetingRuleJsonName}/{position - 1}", new TargetingModel
            {
                Comparator = addTargetinRuleModel.Comparator,
                ComparisonAttribute = addTargetinRuleModel.Attribute,
                ComparisonValue = addTargetinRuleModel.CompareTo,
                Value = parsed
            });

            await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteTargetinRuleAsync(int? flagId, string environmentId, int position, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceManager.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceManager.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Remove($"/{Constants.TargetingRuleJsonName}/{position - 1}");

            await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> MoveTargetinRuleAsync(int? flagId, string environmentId, int from, int to, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceManager.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceManager.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Move($"/{Constants.TargetingRuleJsonName}/{from - 1}", $"/{Constants.TargetingRuleJsonName}/{to - 1}");

            await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        private async Task ValidateAddModel(AddTargetinRuleModel addTargetinRuleModel, CancellationToken token, TargetingModel defaultModel = null)
        {
            if (addTargetinRuleModel.Attribute.IsEmpty())
                addTargetinRuleModel.Attribute = await this.prompt.GetStringAsync("Comparison attribute", token, defaultModel?.ComparisonAttribute ?? "Identifier");

            if (addTargetinRuleModel.Comparator.IsEmpty())
            {
                var preSelectedKey = defaultModel?.Comparator ?? "sensitiveIsOneOf";
                var preSelected = Constants.ComparatorTypes.Single(c => c.Key == preSelectedKey);
                var selected = await this.prompt.ChooseFromListAsync("Choose comparator", Constants.ComparatorTypes.ToList(), c => $"{c.Key} [{c.Value}]", token, preSelected);

                addTargetinRuleModel.Comparator = selected.Key;
            }
                
            if (addTargetinRuleModel.CompareTo.IsEmpty())
                addTargetinRuleModel.CompareTo = await this.prompt.GetStringAsync("Value to compare", token, defaultModel?.ComparisonValue);

            if (addTargetinRuleModel.FlagValue.IsEmpty())
                addTargetinRuleModel.FlagValue = await this.prompt.GetStringAsync($"Value", token, defaultModel?.Value?.ToString());

            if (!Constants.ComparatorTypes.Keys.Contains(addTargetinRuleModel.Comparator, StringComparer.OrdinalIgnoreCase))
                throw new ShowHelpException($"Comparator must be one of the following: {string.Join('|', Constants.ComparatorTypes)}");
        }
    }

    class AddTargetinRuleModel
    {
        public string Attribute { get; set; }

        public string Comparator { get; set; }

        public string CompareTo { get; set; }

        public string FlagValue { get; set; }
    }
}

using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;
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
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public FlagTargeting(IFlagValueClient flagValueClient,
            IFlagClient flagClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.flagValueClient = flagValueClient;
            this.flagClient = flagClient;
            this.workspaceLoader = workspaceLoader;
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
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
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
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the update must be applied"),
                    new Option<int?>(new[] { "--position", "-p" }, "The position of the updating targeting rule"),
                    new Option<string>(new[] { "--attribute", "-a" }, "The user attribute to compare"),
                    new Option<string>(new[] { "--comparator", "-c" }, "The comparison operator")
                        .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                    new Option<string>(new[] { "--compare-to", "-t" }, "The value to compare against"),
                    new Option<string>(new[] { "--flag-value", "-f" }, "The value to serve when the comparison matches, it must respect the setting type"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete targeting rule",
                Handler = this.CreateHandler(nameof(FlagTargeting.DeleteTargetinRuleAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment from where the rule must be deleted"),
                    new Option<int?>(new[] { "--position", "-p" }, "The position of the targeting rule to delete"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "move",
                Aliases = new[] { "mv" },
                Description = "Move a targeting rule into a different position",
                Handler = this.CreateHandler(nameof(FlagTargeting.MoveTargetinRuleAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the move must be applied"),
                    new Option<int?>(new[] { "--from" }, "The position of the targeting rule to delete"),
                    new Option<int?>(new[] { "--to" }, "The desired position of the targeting rule"),
                },
            },
        };

        public async Task<int> AddTargetinRuleAsync(int? flagId, string environmentId, AddTargetinRuleModel addTargetinRuleModel, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            await this.ValidateAddModel(addTargetinRuleModel, token);

            if (!addTargetinRuleModel.FlagValue.TryParseFlagValue(flag.SettingType, out var parsed))
                throw new ShowHelpException($"Flag value '{addTargetinRuleModel.FlagValue}' must respect the type '{flag.SettingType}'.");

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Add($"/{FlagValueModel.TargetingRuleJsonName}/-", new TargetingModel
            {
                Comparator = addTargetinRuleModel.Comparator,
                ComparisonAttribute = addTargetinRuleModel.Attribute,
                ComparisonValue = addTargetinRuleModel.CompareTo,
                Value = parsed
            });

            await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
            return ExitCodes.Ok;
        }

        public async Task<int> UpdateTargetinRuleAsync(int? flagId, string environmentId, int? position, AddTargetinRuleModel addTargetinRuleModel, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var (existing, realPosition) = await this.GetRuleAsync("Choose rule to update", flag.SettingId, environmentId, position, token);
            await this.ValidateAddModel(addTargetinRuleModel, token, existing);

            if (!addTargetinRuleModel.FlagValue.TryParseFlagValue(flag.SettingType, out var parsed))
                throw new ShowHelpException($"Flag value '{addTargetinRuleModel.FlagValue}' must respect the type '{flag.SettingType}'.");

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Replace($"/{FlagValueModel.TargetingRuleJsonName}/{realPosition}", new TargetingModel
            {
                Comparator = addTargetinRuleModel.Comparator,
                ComparisonAttribute = addTargetinRuleModel.Attribute,
                ComparisonValue = addTargetinRuleModel.CompareTo,
                Value = parsed
            });

            await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
            return ExitCodes.Ok;
        }

        public async Task<int> DeleteTargetinRuleAsync(int? flagId, string environmentId, int? position, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var (_, realPosition) = await this.GetRuleAsync("Choose rule to delete", flag.SettingId, environmentId, position, token);

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Remove($"/{FlagValueModel.TargetingRuleJsonName}/{realPosition}");

            await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
            return ExitCodes.Ok;
        }

        public async Task<int> MoveTargetinRuleAsync(int? flagId, string environmentId, int? from, int? to, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (environmentId.IsEmpty())
                environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token, flag.ConfigId)).EnvironmentId;

            var (_, realFrom) = await this.GetRuleAsync("Choose rule to re-position", flag.SettingId, environmentId, from, token);
            var (_, realTo) = await this.GetRuleAsync("Choose the position to move", flag.SettingId, environmentId, to, token);

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Move($"/{FlagValueModel.TargetingRuleJsonName}/{realFrom}", $"/{FlagValueModel.TargetingRuleJsonName}/{realTo}");

            await this.flagValueClient.UpdateValueAsync(flag.SettingId, environmentId, jsonPatchDocument.Operations, token);
            return ExitCodes.Ok;
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

        private async Task<(TargetingModel, int)> GetRuleAsync(string label, int settingId, string environmentId, int? positionFromInput, CancellationToken token)
        {
            var value = await this.flagValueClient.GetValueAsync(settingId, environmentId, token);

            TargetingModel existing = null;
            if (positionFromInput is null)
            {
                existing = await this.prompt.ChooseFromListAsync(label, value.TargetingRules, r =>
                {
                    var comparatorName = Constants.ComparatorTypes.GetValueOrDefault(r.Comparator) ?? r.Comparator.ToUpperInvariant();
                    return $"When {r.ComparisonAttribute} {comparatorName} {r.ComparisonValue} then {r.Value}";
                }, token);

            }
            else
                existing = value.TargetingRules.ElementAtOrDefault(positionFromInput.Value - 1);


            if (existing is null)
                throw new ShowHelpException($"Rule not found.");

            return (existing, value.TargetingRules.IndexOf(existing));
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

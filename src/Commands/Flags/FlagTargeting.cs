using ConfigCat.Cli.Api.Flag;
using ConfigCat.Cli.Api.Flag.Value;
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
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public FlagTargeting(IFlagValueClient flagValueClient,
            IFlagClient flagClient,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.flagValueClient = flagValueClient;
            this.flagClient = flagClient;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "targeting";

        public string Description => "Manage targeting rules";

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "add",
                Description = "Add new targeting rule",
                Handler = this.CreateHandler(nameof(FlagTargeting.AddTargetinRuleAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("flag-id") { Description = "ID of the flag" },
                    new Argument<string>("environment-id") { Description = "ID of the environment where the rule must be created" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--attribute", "-a" }) 
                    {
                        Description = $"The user attribute to compare"
                    },
                    new Option<string>(new[] { "--comparator", "-c" })
                    {
                        Argument = new Argument<string>()
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                        Description = $"The comparison operator",
                    },
                    new Option<string>(new[] { "--compare-to", "-t" }) { Description = "The value to compare against" },
                    new Option<string>(new[] { "--flag-value", "-f" }) { Description = "The value to serve when the comparison matches, it must respect the setting type" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Description = "Update targeting rule",
                Handler = this.CreateHandler(nameof(FlagTargeting.UpdateTargetinRuleAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("flag-id") { Description = "ID of the flag" },
                    new Argument<string>("environment-id") { Description = "ID of the environment where the update must be applied" },
                    new Argument<int>("position") { Description = "The position of the updating targeting rule" },
                },
                Options = new[]
                {
                    new Option<string>(new[] { "--attribute", "-a" }) { Description = "The user attribute to compare" },
                    new Option<string>(new[] { "--comparator", "-c" })
                    {
                        Argument = new Argument<string>()
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                        Description = $"The comparison operator"
                    },
                    new Option<string>(new[] { "--compare-to", "-t" }) { Description = "The value to compare against" },
                    new Option<string>(new[] { "--flag-value", "-f" }) { Description = "The value to serve when the comparison matches, it must respect the setting type" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete targeting rule",
                Handler = this.CreateHandler(nameof(FlagTargeting.DeleteTargetinRuleAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("flag-id") { Description = "ID of the flag" },
                    new Argument<string>("environment-id") { Description = "ID of the environment from where the rule must be deleted" },
                    new Argument<int>("position") { Description = "The position of the targeting rule to delete" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "move",
                Description = "Move a targeting rule into a different position",
                Handler = this.CreateHandler(nameof(FlagTargeting.MoveTargetinRuleAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("flag-id") { Description = "ID of the flag" },
                    new Argument<string>("environment-id") { Description = "ID of the environment where the move must be applied" },
                    new Argument<int>("from") { Description = "The actual position of the targeting rule" },
                    new Argument<int>("to") { Description = "The desired position of the targeting rule" },
                },
            },
        };

        public async Task<int> AddTargetinRuleAsync(int flagId, string environmentId, AddTargetinRuleModel addTargetinRuleModel, CancellationToken token)
        {
            if (!this.ValidateAddModel(addTargetinRuleModel, token))
                return Constants.ExitCodes.Error;

            var flag = await this.flagClient.GetFlagAsync(flagId, token);
            if (!addTargetinRuleModel.FlagValue.TryParseFlagValue(flag.SettingType, out var parsed))
            {
                this.accessor.ExecutionContext.Output.WriteError($"Flag value '{addTargetinRuleModel.FlagValue}' must respect the type '{flag.SettingType}'");
                return Constants.ExitCodes.Error;
            }

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Add($"/{Constants.TargetingRuleJsonName}/-", new TargetingModel
            {
                Comparator = addTargetinRuleModel.Comparator,
                ComparisonAttribute = addTargetinRuleModel.Attribute,
                ComparisonValue = addTargetinRuleModel.CompareTo,
                Value = parsed
            });

            await this.flagValueClient.UpdateValueAsync(flagId, environmentId, jsonPatchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateTargetinRuleAsync(int flagId, string environmentId, int position, AddTargetinRuleModel addTargetinRuleModel, CancellationToken token)
        {
            var value = await this.flagValueClient.GetValueAsync(flagId, environmentId, token);
            var existing = value.TargetingRules.ElementAtOrDefault(position - 1);

            if (existing == null)
            {
                this.accessor.ExecutionContext.Output.WriteError($"Rule not found at position {position}");
                return Constants.ExitCodes.Error;
            }

            if (!this.ValidateAddModel(addTargetinRuleModel, token, existing))
                return Constants.ExitCodes.Error;

            if (!addTargetinRuleModel.FlagValue.TryParseFlagValue(value.Setting.SettingType, out var parsed))
            {
                this.accessor.ExecutionContext.Output.WriteError($"Flag value '{addTargetinRuleModel.FlagValue}' must respect the type '{value.Setting.SettingType}'");
                return Constants.ExitCodes.Error;
            }

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Replace($"/{Constants.TargetingRuleJsonName}/{position - 1}", new TargetingModel
            {
                Comparator = addTargetinRuleModel.Comparator,
                ComparisonAttribute = addTargetinRuleModel.Attribute,
                ComparisonValue = addTargetinRuleModel.CompareTo,
                Value = parsed
            });

            await this.flagValueClient.UpdateValueAsync(flagId, environmentId, jsonPatchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteTargetinRuleAsync(int flagId, string environmentId, int position, CancellationToken token)
        {
            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Remove($"/{Constants.TargetingRuleJsonName}/{position - 1}");

            await this.flagValueClient.UpdateValueAsync(flagId, environmentId, jsonPatchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> MoveTargetinRuleAsync(int flagId, string environmentId, int from, int to, CancellationToken token)
        {
            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.Move($"/{Constants.TargetingRuleJsonName}/{from - 1}", $"/{Constants.TargetingRuleJsonName}/{to - 1}");

            await this.flagValueClient.UpdateValueAsync(flagId, environmentId, jsonPatchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        private bool ValidateAddModel(AddTargetinRuleModel addTargetinRuleModel, CancellationToken token, TargetingModel defaultModel = null)
        {
            if (!token.IsCancellationRequested && addTargetinRuleModel.Attribute.IsEmpty())
                addTargetinRuleModel.Attribute = this.prompt.GetString("Comparison attribute", defaultModel?.ComparisonAttribute ?? "Identifier");

            if (!token.IsCancellationRequested && addTargetinRuleModel.Comparator.IsEmpty())
                addTargetinRuleModel.Comparator = this.prompt.GetString($"Comparator <{string.Join('|', Constants.ComparatorTypes.Keys)}>", defaultModel?.Comparator ?? "sensitiveIsOneOf");

            if (!token.IsCancellationRequested && addTargetinRuleModel.CompareTo.IsEmpty())
                addTargetinRuleModel.CompareTo = this.prompt.GetString("Value to compare", defaultModel?.ComparisonValue);

            if (!token.IsCancellationRequested && addTargetinRuleModel.FlagValue.IsEmpty())
                addTargetinRuleModel.FlagValue = this.prompt.GetString($"Value", defaultModel?.Value?.ToString());

            if (!token.IsCancellationRequested && !Constants.ComparatorTypes.Keys.Contains(addTargetinRuleModel.Comparator, StringComparer.OrdinalIgnoreCase))
            {
                this.accessor.ExecutionContext.Output.WriteError($"Comparator must be one of the following: {string.Join('|', Constants.ComparatorTypes)}");
                return false;
            }

            return true;
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

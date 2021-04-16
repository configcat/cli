using ConfigCat.Cli.Api;
using ConfigCat.Cli.Api.Config;
using ConfigCat.Cli.Api.Flag;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Exceptions;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Flag : ICommandDescriptor
    {
        private readonly IFlagClient flagClient;
        private readonly IConfigClient configClient;
        private readonly IProductClient productClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public Flag(IFlagClient flagClient,
            IConfigClient configClient,
            IProductClient productClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.flagClient = flagClient;
            this.configClient = configClient;
            this.productClient = productClient;
            this.workspaceLoader = workspaceLoader;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "flag";

        public string Description => "Manage flags & settings";

        public IEnumerable<string> Aliases => new[] { "setting", "f", "s" };

        public IEnumerable<ICommandDescriptor> SubCommands { get; set; }

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => new[]
        {
            new SubCommandDescriptor
            {
                Name = "ls",
                Description = "List all flags",
                Handler = this.CreateHandler(nameof(Flag.ListAllFlagsAsync)),
                Options = new Option[]
                {
                    new Option<string>(new[] { "--config-id", "-c" }, "Show only a config's flags"),
                    new Option<string>(new[] { "--tag-name", "-n" }, "Filter by a tag's name"),
                    new Option<int>(new[] { "--tag-id", "-t" }, "Filter by a tag's ID"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "create",
                Aliases = new[] { "cr" },
                Description = "Create flag",
                Handler = this.CreateHandler(nameof(Flag.CreateFlagAsync)),
                Options = new Option[]
                {
                    new Option<string>(new[] { "--config-id", "-c" }, "ID of the config where the flag must be created"),
                    new Option<string>(new[] { "--name", "-n" }, "Name of the new flag"),
                    new Option<string>(new[] { "--key", "-k" }, "Key of the new flag"),
                    new Option<string>(new[] { "--hint", "-d" }, "Hint of the new flag"),
                    new Option<string>(new[] { "--type", "-t" }, "Type of the new flag")
                    {
                        Argument = new Argument<string>()
                            .AddSuggestions(Constants.SettingTypes.Collection),
                    },
                    new Option<int[]>(new[] { "--tag-ids", "-g" }, "Tags to attach"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete flag",
                Handler = this.CreateHandler(nameof(Flag.DeleteFlagAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting to delete")
                    {
                        Name = "flag-id"
                    },
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Aliases = new[] { "up" },
                Description = "Update flag",
                Handler = this.CreateHandler(nameof(Flag.UpdateFlagAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting to update")
                    {
                        Name = "flag-id"
                    },
                    new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                    new Option<string>(new[] { "--hint", "-d" }, "The updated hint"),
                    new Option<int[]>(new[] { "--tag-ids", "-g" }, "The updated tag list"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "attach",
                Aliases = new[] { "at" },
                Description = "Attach tag(s) to a flag",
                Handler = this.CreateHandler(nameof(Flag.AttachTagsAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting to attach tags")
                    {
                        Name = "flag-id"
                    },
                    new Option<int[]>(new[] { "--tag-ids", "-g" }, "Tag IDs to attach"),
                },
            },
            new SubCommandDescriptor
            {
                Name = "detach",
                Aliases = new[] { "dt" },
                Description = "Detach tag(s) from a flag",
                Handler = this.CreateHandler(nameof(Flag.DetachTagsAsync)),
                Options = new Option[]
                {
                    new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting to detach tags")
                    {
                        Name = "flag-id"
                    },
                    new Option<int[]>(new[] { "--tag-ids", "-g" }, "Tag IDs to detach"),
                },
            },
        };

        public async Task<int> ListAllFlagsAsync(string configId, string tagName, int? tagId, CancellationToken token)
        {
            var flags = new List<FlagModel>();
            if (!configId.IsEmpty())
                flags.AddRange(await this.flagClient.GetFlagsAsync(configId, token));
            else
            {
                var products = await this.productClient.GetProductsAsync(token);
                foreach (var product in products)
                {
                    var configs = await this.configClient.GetConfigsAsync(product.ProductId, token);
                    foreach (var config in configs)
                        flags.AddRange(await this.flagClient.GetFlagsAsync(config.ConfigId, token));
                }
            }

            if (!tagName.IsEmpty() || tagId is not null)
            {
                flags = flags.Where(f =>
                {
                    var result = true;
                    if (!tagName.IsEmpty())
                        result = result && f.Tags.Any(t => t.Name == tagName);

                    if (tagId is not null)
                        result = result && f.Tags.Any(t => t.TagId == tagId);

                    return result;
                }).ToList();
            }

            var table = new TableView<FlagModel>() { Items = flags };
            table.AddColumn(f => f.SettingId, "ID");
            table.AddColumn(f => f.Name, "NAME");
            table.AddColumn(f => f.Key, "KEY");
            table.AddColumn(f => f.Hint.Length > 30 ? $"\"{f.Hint[0..28]}...\"" : $"\"{f.Hint}\"", "HINT");
            table.AddColumn(f => f.SettingType, "TYPE");
            table.AddColumn(f => $"[{string.Join(", ", f.Tags.Select(t => $"{t.Name} ({t.TagId})"))}]", "TAGS");
            table.AddColumn(f => $"{f.OwnerUserFullName} [{f.OwnerUserEmail}]", "OWNER");
            table.AddColumn(f => $"{f.ConfigName} [{f.ConfigId}]", "CONFIG");

            this.accessor.ExecutionContext.Output.RenderView(table);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> CreateFlagAsync(string configId, CreateFlagModel createConfigModel, CancellationToken token)
        {
            var shouldPromptTags = configId.IsEmpty();

            if (configId.IsEmpty())
                configId = (await this.workspaceLoader.LoadConfigAsync(token)).ConfigId;

            if (createConfigModel.Name.IsEmpty())
                createConfigModel.Name = await this.prompt.GetStringAsync("Name", token);

            if (createConfigModel.Hint.IsEmpty())
                createConfigModel.Hint = await this.prompt.GetStringAsync("Hint", token);

            if (createConfigModel.Key.IsEmpty())
                createConfigModel.Key = await this.prompt.GetStringAsync("Key", token);

            if (createConfigModel.Type.IsEmpty())
                createConfigModel.Type = await this.prompt.ChooseFromListAsync("Choose type", Constants.SettingTypes.Collection.ToList(), t => t, token);

            if (shouldPromptTags && (createConfigModel.TagIds is null || !createConfigModel.TagIds.Any()))
                createConfigModel.TagIds = (await this.workspaceLoader.LoadTagsAsync(token, configId)).Select(t => t.TagId);

            if (!Constants.SettingTypes.Collection.ToList()
                    .Contains(createConfigModel.Type, StringComparer.OrdinalIgnoreCase))
                throw new ShowHelpException($"Type must be one of the following: {string.Join('|', Constants.SettingTypes.Collection)}");

            var result = await this.flagClient.CreateFlagAsync(configId, createConfigModel, token);
            this.accessor.ExecutionContext.Output.Write(result.SettingId.ToString());

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteFlagAsync(int? flagId, CancellationToken token)
        {
            if (flagId is null)
                flagId = (await this.workspaceLoader.LoadFlagAsync(token)).SettingId;

            await this.flagClient.DeleteFlagAsync(flagId.Value, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateFlagAsync(int? flagId, UpdateFlagModel updateFlagModel, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            if (flagId is null)
            {
                if (updateFlagModel.Name.IsEmpty())
                    updateFlagModel.Name = await this.prompt.GetStringAsync("Name", token, flag.Name);

                if (updateFlagModel.Hint.IsEmpty())
                    updateFlagModel.Hint = await this.prompt.GetStringAsync("Hint", token, flag.Hint);

                if (updateFlagModel.TagIds is null || !updateFlagModel.TagIds.Any())
                    updateFlagModel.TagIds = (await this.workspaceLoader.LoadTagsAsync(token, flag.ConfigId, flag.Tags)).Select(t => t.TagId);
            }

            if (updateFlagModel.Hint.IsEmptyOrEquals(flag.Hint) &&
                updateFlagModel.Name.IsEmptyOrEquals(flag.Name) &&
                (updateFlagModel.TagIds is null || 
                !updateFlagModel.TagIds.Any() || 
                Enumerable.SequenceEqual(updateFlagModel.TagIds, flag.Tags.Select(t => t.TagId))))
            {
                this.accessor.ExecutionContext.Output.WriteNoChange();
                return Constants.ExitCodes.Ok;
            }

            var patchDocument = JsonPatch.GenerateDocument(flag.ToUpdateModel(), updateFlagModel);
            await this.flagClient.UpdateFlagAsync(flag.SettingId, patchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> AttachTagsAsync(int? flagId, IEnumerable<int> tagIds, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            var flagTagIds = flag.Tags.Select(t => t.TagId).ToList();

            if (flagId is null && tagIds is null || !tagIds.Any())
                tagIds = (await this.workspaceLoader.LoadTagsAsync(token, flag.ConfigId, flag.Tags)).Select(t => t.TagId);

            if (tagIds is null || 
                !tagIds.Any() || 
                Enumerable.SequenceEqual(tagIds, flagTagIds) ||
                !tagIds.Except(flagTagIds).Any())
            {
                this.accessor.ExecutionContext.Output.WriteNoChange();
                return Constants.ExitCodes.Ok;
            }

            var patchDocument = new JsonPatchDocument();
            foreach (var tagId in tagIds)
                patchDocument.Add("/tags/-", tagId);

            await this.flagClient.UpdateFlagAsync(flag.SettingId, patchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DetachTagsAsync(int? flagId, IEnumerable<int> tagIds, CancellationToken token)
        {
            var flag = flagId is null
                ? await this.workspaceLoader.LoadFlagAsync(token)
                : await this.flagClient.GetFlagAsync(flagId.Value, token);

            var tagsCopy = flag.Tags.ToList();

            if (flagId is null && tagIds is null || !tagIds.Any())
                tagIds = (await this.prompt.ChooseMultipleFromListAsync("Choose tags to detach", tagsCopy, t => t.Name, token)).Select(t => t.TagId);

            var relevantTags = tagsCopy.Where(t => tagIds.Contains(t.TagId)).ToList();
            if (relevantTags.Count == 0)
            {
                this.accessor.ExecutionContext.Output.WriteNoChange();
                return Constants.ExitCodes.Ok;
            }

            // play through the whole remove sequence as the indexes will change after each remove operation
            var tagIndexes = new List<int>();
            foreach (var relevantTag in relevantTags)
            {
                var index = tagsCopy.IndexOf(relevantTag);
                tagIndexes.Add(index);
                tagsCopy.RemoveAt(index);
            }

            var patchDocument = new JsonPatchDocument();
            foreach (var tagIndex in tagIndexes)
                patchDocument.Remove($"/tags/{tagIndex}");

            await this.flagClient.UpdateFlagAsync(flag.SettingId, patchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

using ConfigCat.Cli.Api.Config;
using ConfigCat.Cli.Api.Flag;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Utils;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
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
        private readonly IPrompt prompt;
        private readonly IExecutionContextAccessor accessor;

        public Flag(IFlagClient flagClient,
            IConfigClient configClient,
            IProductClient productClient,
            IPrompt prompt,
            IExecutionContextAccessor accessor)
        {
            this.flagClient = flagClient;
            this.configClient = configClient;
            this.productClient = productClient;
            this.prompt = prompt;
            this.accessor = accessor;
        }

        public string Name => "flag";

        public string Description => "Manage flags";

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
                    new Option<string>(new[] { "--config-id", "-c" }) { Description = "Show only a config's flags" },
                    new Option<string>(new[] { "--tag-name", "-n" }) { Description = "Filter by a tag's name" },
                    new Option<int>(new[] { "--tag-id", "-t" }) { Description = "Filter by a tag's ID" },
                }
            },
            new SubCommandDescriptor
            {
                Name = "create",
                Description = "Create flag",
                Handler = this.CreateHandler(nameof(Flag.CreateFlagAsync)),
                Arguments = new[]
                {
                    new Argument<string>("config-id") { Description = "ID of the config where the flag must be created" },
                },
                Options = new Option[]
                {
                    new Option<string>(new[] { "--name", "-n" }){ Description = "Name of the new flag" },
                    new Option<string>(new[] { "--key", "-k" }) { Description = "Key of the new flag" },
                    new Option<string>(new[] { "--hint", "-i" }) { Description = "Hint of the new flag" },
                    new Option<string>(new[] { "--type", "-t" })
                    {
                        Argument = new Argument<string>()
                            .FromAmong(Constants.SettingTypes.Collection),
                        Description = "Type of the new flag"
                    },
                    new Option<int[]>(new[] { "--tags", "-g" }) { Description = "Tags to attach" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "rm",
                Description = "Delete flag",
                Handler = this.CreateHandler(nameof(Flag.DeleteFlagAsync)),
                Arguments = new[]
                {
                     new Argument<int>("flag-id") { Description = "ID of the flag to delete" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "update",
                Description = "Update flag",
                Handler = this.CreateHandler(nameof(Flag.UpdateFlagAsync)),
                Arguments = new[]
                {
                     new Argument<int>("flag-id") { Description = "ID of the flag to update" },
                },
                Options = new Option[]
                {
                    new Option<string>(new[] { "--name", "-n" }) { Description = "The updated name" },
                    new Option<string>(new[] { "--hint", "-i" }) { Description = "The updated hint" },
                    new Option<int[]>(new[] { "--tags", "-g" }) { Description = "The updated tag list" },
                }
            },
            new SubCommandDescriptor
            {
                Name = "attach",
                Description = "Attach tag(s) to a flag",
                Handler = this.CreateHandler(nameof(Flag.AttachTagAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("flag-id") { Description = "ID of the flag to attach tags" },
                    new Argument<int[]>("tag-ids"){ Description = "Tag IDs to attach" },
                },
            },
            new SubCommandDescriptor
            {
                Name = "detach",
                Description = "Detach tag(s) from a flag",
                Handler = this.CreateHandler(nameof(Flag.DetachTagAsync)),
                Arguments = new Argument[]
                {
                    new Argument<int>("flag-id") { Description = "ID of the flag to detach tags" },
                    new Argument<int[]>("tag-ids") { Description = "Tag IDs to detach" },
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

            if (!tagName.IsEmpty() || tagId != null)
            {
                flags = flags.Where(f =>
                {
                    var result = true;
                    if (!tagName.IsEmpty())
                        result = result && f.Tags.Any(t => t.Name == tagName);

                    if (tagId != null)
                        result = result && f.Tags.Any(t => t.TagId == tagId);

                    return result;
                }).ToList();
            }

            var table = new TableView<FlagModel>() { Items = flags };
            table.AddColumn(f => f.SettingId, "ID");
            table.AddColumn(f => f.Name, "NAME");
            table.AddColumn(f => f.Key, "KEY");
            table.AddColumn(f => f.Hint, "HINT");
            table.AddColumn(f => f.SettingType, "TYPE");
            table.AddColumn(f => $"[{string.Join(", ", f.Tags.Select(t => $"{t.Name} ({t.TagId})"))}]", "TAGS");
            table.AddColumn(f => $"{f.OwnerUserFullName} [{f.OwnerUserEmail}]", "OWNER");
            table.AddColumn(f => $"{f.ConfigName} [{f.ConfigId}]", "CONFIG");

            var console = this.accessor.ExecutionContext.Output.Console;
            var renderer = new ConsoleRenderer(console, resetAfterRender: true);
            table.RenderFitToContent(renderer, console);

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> CreateFlagAsync(string configId, CreateFlagModel createConfigModel, CancellationToken token)
        {
            if (!token.IsCancellationRequested && string.IsNullOrWhiteSpace(createConfigModel.Name))
                createConfigModel.Name = this.prompt.GetString("Name");

            if (!token.IsCancellationRequested && string.IsNullOrWhiteSpace(createConfigModel.Hint))
                createConfigModel.Hint = this.prompt.GetString("Hint");

            if (!token.IsCancellationRequested && string.IsNullOrWhiteSpace(createConfigModel.Key))
                createConfigModel.Key = this.prompt.GetString("Key");

            if (!token.IsCancellationRequested && string.IsNullOrWhiteSpace(createConfigModel.Type))
                createConfigModel.Type = this.prompt.GetString($"Type <{string.Join('|', Constants.SettingTypes.Collection)}>", Constants.SettingTypes.Collection[0]);

            if (!token.IsCancellationRequested && !Constants.SettingTypes.Collection.ToList()
                    .Contains(createConfigModel.Type, StringComparer.OrdinalIgnoreCase))
            {
                this.accessor.ExecutionContext.Output.WriteError($"Type must be one of the following: {string.Join('|', Constants.SettingTypes.Collection)}");
                return Constants.ExitCodes.Error;
            }

            var result = await this.flagClient.CreateFlagAsync(configId, createConfigModel, token);
            this.accessor.ExecutionContext.Output.Write(result.SettingId.ToString());

            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DeleteFlagAsync(int flagId, CancellationToken token)
        {
            await this.flagClient.DeleteFlagAsync(flagId, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> UpdateFlagAsync(int flagId, UpdateFlagModel updateFlagModel, CancellationToken token)
        {
            if (string.IsNullOrEmpty(updateFlagModel.Hint) &&
                string.IsNullOrEmpty(updateFlagModel.Name) &&
                (updateFlagModel.Tags == null || !updateFlagModel.Tags.Any()))
            {
                this.accessor.ExecutionContext.Output.Write($"No changes detected... ");
                this.accessor.ExecutionContext.Output.WriteYellow("Skipped.");
                return Constants.ExitCodes.Ok;
            }

            var flag = await this.flagClient.GetFlagAsync(flagId, token);
            var patchDocument = JsonPatch.GenerateDocument(flag.ToUpdateModel(), updateFlagModel);
            await this.flagClient.UpdateFlagAsync(flagId, patchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> AttachTagAsync(int flagId, IEnumerable<int> tagIds, CancellationToken token)
        {
            if (tagIds == null || !tagIds.Any())
            {
                this.accessor.ExecutionContext.Output.Write($"No change detected... ");
                this.accessor.ExecutionContext.Output.WriteYellow("Skipped.");
                return Constants.ExitCodes.Ok;
            }

            var patchDocument = new JsonPatchDocument();
            foreach (var tagId in tagIds)
                patchDocument.Add("/tags/-", tagId);

            await this.flagClient.UpdateFlagAsync(flagId, patchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }

        public async Task<int> DetachTagAsync(int flagId, IEnumerable<int> tagIds, CancellationToken token)
        {
            var flag = await this.flagClient.GetFlagAsync(flagId, token);
            var tagsCopy = flag.Tags.ToList();
            var relevantTags = tagsCopy.Where(t => tagIds.Contains(t.TagId)).ToList();
            if (relevantTags.Count == 0)
            {
                this.accessor.ExecutionContext.Output.Write($"Tag(s) not found to detach... ");
                this.accessor.ExecutionContext.Output.WriteYellow("Skipped.");
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

            await this.flagClient.UpdateFlagAsync(flagId, patchDocument.Operations, token);
            return Constants.ExitCodes.Ok;
        }
    }
}

using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Options;

namespace ConfigCat.Cli.Commands.Flags;

internal class Flag(
    IFlagClient flagClient,
    IConfigClient configClient,
    IProductClient productClient,
    IEnvironmentClient environmentClient,
    IWorkspaceLoader workspaceLoader,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ListAllFlagsAsync(string configId, string tagName, int? tagId, bool json, CancellationToken token)
    {
        var flags = new List<FlagModel>();
        if (!configId.IsEmpty())
            flags.AddRange(await flagClient.GetFlagsAsync(configId, token));
        else
        {
            var products = await productClient.GetProductsAsync(token);
            foreach (var product in products)
            {
                var configs = await configClient.GetConfigsAsync(product.ProductId, token);
                foreach (var config in configs)
                    flags.AddRange(await flagClient.GetFlagsAsync(config.ConfigId, token));
            }
        }

        if (!tagName.IsEmpty() || tagId is not null)
        {
            flags = flags.Where(f =>
            {
                var result = true;
                if (!tagName.IsEmpty())
                    result = f.Tags.Any(t => t.Name == tagName);

                if (tagId is not null)
                    result = result && f.Tags.Any(t => t.TagId == tagId);

                return result;
            }).ToList();
        }

        if (json)
        {
            output.RenderJson(flags);
            return ExitCodes.Ok;
        }

        var itemsToRender = flags.Select(f => new
        {
            Id = f.SettingId,
            Name = f.Name.TrimToFitColumn(),
            Key = f.Key.TrimToFitColumn(),
            Hint = f.Hint.TrimToFitColumn(),
            Type = f.SettingType,
            Tags = $"[{string.Join(", ", f.Tags.Select(t => $"{t.Name} ({t.TagId})"))}]",
            Owner = $"{f.CreatorFullName} [{f.CreatorEmail}]",
            Config = $"{f.ConfigName} [{f.ConfigId}]",
        });
        output.RenderTable(itemsToRender);

        return ExitCodes.Ok;
    }

    public async Task<int> CreateFlagAsync(string configId, 
        string key,
        string name,
        string hint,
        string type,
        string initValue,
        int[] tagIds,
        InitialValueOption[] initValuesPerEnvironment,
        CancellationToken token)
    {
        var shouldPrompt = configId.IsEmpty();

        if (configId.IsEmpty())
            configId = (await workspaceLoader.LoadConfigAsync(token)).ConfigId;

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token);

        if (hint.IsEmpty())
            hint = await prompt.GetStringAsync("Hint", token);

        if (key.IsEmpty())
            key = await prompt.GetStringAsync("Key", token);

        if (type.IsEmpty())
            type = await prompt.ChooseFromListAsync("Choose type", SettingTypes.Collection.ToList(), t => t, token);

        if (shouldPrompt && tagIds.IsEmpty())
            tagIds = (await workspaceLoader.LoadTagsAsync(token, configId, optional: true)).Select(t => t.TagId).ToArray();

        if (!SettingTypes.Collection.ToList()
                .Contains(type, StringComparer.OrdinalIgnoreCase))
            throw new ShowHelpException($"Type must be one of the following: {string.Join('|', SettingTypes.Collection)}");

        object parsedInitialValue = null;
        if (!initValue.IsEmpty() && !initValue.TryParseFlagValue(type, out parsedInitialValue))
            throw new ShowHelpException($"Initial value '{initValue}' must respect the type '{type}'.");
        
        var createModel = new CreateFlagModel
        {
            Hint = hint,
            Key = key,
            Name = name,
            TagIds = tagIds,
            Type = type
        };

        ConfigModel config = null;
        List<EnvironmentModel> environments = null;
        if (shouldPrompt && initValue.IsEmpty() && initValuesPerEnvironment.IsEmpty())
        {
            config = await configClient.GetConfigAsync(configId, token);
            environments = (await environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token)).ToList();
            var defaultValue = type.GetDefaultValueForType();
            initValuesPerEnvironment = new InitialValueOption[environments.Count];
            output.WriteDarkGray("Please set an initial value for each of your environments.")
                .WriteLine();

            var index = 0;
            foreach (var environment in environments)
            {
                var fromPrompt = await prompt.GetStringAsync(environment.Name, token, defaultValue);
                initValuesPerEnvironment[index++] = new InitialValueOption
                    { EnvironmentId = environment.EnvironmentId, Value = fromPrompt };
            }
        }
        
        if (parsedInitialValue is not null || !initValuesPerEnvironment.IsEmpty())
        {
            config ??= await configClient.GetConfigAsync(configId, token);
            environments ??= (await environmentClient.GetEnvironmentsAsync(config.Product.ProductId, token)).ToList();
            createModel.InitialValues = environments.Select(env =>
            {
                var initial = new InitialValue { EnvironmentId = env.EnvironmentId };
                var perEnv = initValuesPerEnvironment?.FirstOrDefault(i => i.EnvironmentId == env.EnvironmentId);
                if (perEnv is not null)
                {
                    if (!perEnv.Value.TryParseFlagValue(type, out var parsed))
                        throw new ShowHelpException($"Initial value '{perEnv.Value}' must respect the type '{type}'.");

                    initial.Value = parsed;
                }
                else
                    initial.Value = parsedInitialValue;

                return initial.Value is null ? null : initial;
            }).Where(i => i is not null).ToList();
        }
        
        var result = await flagClient.CreateFlagAsync(configId, createModel, token);
        output.Write(result.SettingId.ToString());

        return ExitCodes.Ok;
    }

    public async Task<int> DeleteFlagAsync(int? flagId, CancellationToken token)
    {
        flagId ??= (await workspaceLoader.LoadFlagAsync(token)).SettingId;

        await flagClient.DeleteFlagAsync(flagId.Value, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateFlagAsync(int? flagId, UpdateFlagModel updateFlagModel, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);

        if (flagId is null)
        {
            if (updateFlagModel.Name.IsEmpty())
                updateFlagModel.Name = await prompt.GetStringAsync("Name", token, flag.Name);

            if (updateFlagModel.Hint.IsEmpty())
                updateFlagModel.Hint = await prompt.GetStringAsync("Hint", token, flag.Hint);

            if (updateFlagModel.TagIds.IsEmpty())
                updateFlagModel.TagIds = (await workspaceLoader.LoadTagsAsync(token, flag.ConfigId, flag.Tags, optional: true)).Select(t => t.TagId).ToArray();
        }

        var originalTagIds = flag.Tags?.Select(t => t.TagId).ToList() ?? [];

        if (updateFlagModel.Hint.IsEmptyOrEquals(flag.Hint) &&
            updateFlagModel.Name.IsEmptyOrEquals(flag.Name) &&
            (updateFlagModel.TagIds.IsEmpty() ||
             updateFlagModel.TagIds.SequenceEqual(originalTagIds)))
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        var updatedTagIds = updateFlagModel.TagIds.ToList();

        // prevent auto json patch generation for tag ids, we'll set them manually
        flag.Tags = null;
        updateFlagModel.TagIds = null;

        var patchDocument = JsonPatch.GenerateDocument(flag.ToUpdateModel(), updateFlagModel);

        var tagsToDelete = originalTagIds.Except(updatedTagIds).ToList();
        var tagsToAdd = updatedTagIds.Except(originalTagIds).ToList();

        var tagIndexes = new List<int>();
        foreach (var tagIndex in tagsToDelete.Select(deleteItem => originalTagIds.IndexOf(deleteItem)))
        {
            tagIndexes.Add(tagIndex);
            originalTagIds.RemoveAt(tagIndex);
        }

        foreach (var deleteIndex in tagIndexes)
            patchDocument.Remove($"/tags/{deleteIndex}");

        foreach (var tagIdToAdd in tagsToAdd)
            patchDocument.Add("/tags/-", tagIdToAdd);

        await flagClient.UpdateFlagAsync(flag.SettingId, patchDocument.Operations, token);
        return ExitCodes.Ok;
    }

    public async Task<int> AttachTagsAsync(int? flagId, int[] tagIds, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);

        var flagTagIds = flag.Tags.Select(t => t.TagId).ToList();

        if (flagId is null && tagIds.IsEmpty())
            tagIds = (await workspaceLoader.LoadTagsAsync(token, flag.ConfigId, flag.Tags)).Select(t => t.TagId).ToArray();

        if (tagIds.IsEmpty() ||
            tagIds.SequenceEqual(flagTagIds) ||
            !tagIds.Except(flagTagIds).Any())
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        var patchDocument = new JsonPatchDocument();
        foreach (var tagId in tagIds)
            patchDocument.Add("/tags/-", tagId);

        await flagClient.UpdateFlagAsync(flag.SettingId, patchDocument.Operations, token);
        return ExitCodes.Ok;
    }

    public async Task<int> DetachTagsAsync(int? flagId, int[] tagIds, CancellationToken token)
    {
        var flag = flagId is null
            ? await workspaceLoader.LoadFlagAsync(token)
            : await flagClient.GetFlagAsync(flagId.Value, token);

        if (flagId is null && tagIds.IsEmpty())
            tagIds = (await prompt.ChooseMultipleFromListAsync("Choose tags to detach", flag.Tags, t => t.Name, token)).Select(t => t.TagId).ToArray();

        var relevantTags = flag.Tags.Where(t => tagIds.Contains(t.TagId)).ToList();
        if (relevantTags.Count == 0)
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        // play through the whole remove sequence as the indexes will change after each remove operation
        var tagIndexes = new List<int>();
        foreach (var index in relevantTags.Select(relevantTag => flag.Tags.IndexOf(relevantTag)))
        {
            tagIndexes.Add(index);
            flag.Tags.RemoveAt(index);
        }

        var patchDocument = new JsonPatchDocument();
        foreach (var tagIndex in tagIndexes)
            patchDocument.Remove($"/tags/{tagIndex}");

        await flagClient.UpdateFlagAsync(flag.SettingId, patchDocument.Operations, token);
        return ExitCodes.Ok;
    }
}
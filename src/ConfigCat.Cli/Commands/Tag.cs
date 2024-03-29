using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class Tag(
    ITagClient tagClient,
    IProductClient productClient,
    IWorkspaceLoader workspaceLoader,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ListAllTagsAsync(string productId, bool json, CancellationToken token)
    {
        var tags = new List<TagModel>();
        if (!productId.IsEmpty())
            tags.AddRange(await tagClient.GetTagsAsync(productId, token));
        else
        {
            var products = await productClient.GetProductsAsync(token);
            foreach (var product in products)
                tags.AddRange(await tagClient.GetTagsAsync(product.ProductId, token));
        }

        if (json)
        {
            output.RenderJson(tags);
            return ExitCodes.Ok;
        }

        var itemsToRender = tags.Select(t => new
        {
            Id = t.TagId,
            t.Name,
            t.Color,
            Product = $"{t.Product.Name} [{t.Product.ProductId}]"
        });
        output.RenderTable(itemsToRender);
        return ExitCodes.Ok;
    }

    public async Task<int> CreateTagAsync(string productId, string name, string color, CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await workspaceLoader.LoadProductAsync(token)).ProductId;

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token);

        if (color.IsEmpty())
            color = await prompt.GetStringAsync("Color", token);

        var result = await tagClient.CreateTagAsync(productId, name, color, token);
        output.Write(result.TagId.ToString());

        return ExitCodes.Ok;
    }

    public async Task<int> DeleteTagAsync(int? tagId, CancellationToken token)
    {
        tagId ??= (await workspaceLoader.LoadTagAsync(token)).TagId;

        await tagClient.DeleteTagAsync(tagId.Value, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateTagAsync(int? tagId, string name, string color, CancellationToken token)
    {
        var tag = tagId switch
        {
            null => await workspaceLoader.LoadTagAsync(token),
            _ => await tagClient.GetTagAsync(tagId.Value, token)
        };

        if (tagId is null)
        {
            if (name.IsEmpty())
                name = await prompt.GetStringAsync("Name", token, tag.Name);

            if (color.IsEmpty())
                color = await prompt.GetStringAsync("Color", token, tag.Color);
        }

        if (name.IsEmptyOrEquals(tag.Name) && color.IsEmptyOrEquals(tag.Color))
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        await tagClient.UpdateTagAsync(tag.TagId, name, color, token);
        return ExitCodes.Ok;
    }
}
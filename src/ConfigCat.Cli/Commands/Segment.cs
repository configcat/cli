using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using ConfigCat.Cli.Services.Exceptions;

namespace ConfigCat.Cli.Commands;

internal class Segment(
    ISegmentClient segmentClient,
    IWorkspaceLoader workspaceLoader,
    IProductClient productClient,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ListAllSegmentsAsync(string productId, bool json, CancellationToken token)
    {
        var segments = new List<SegmentModel>();
        if (!productId.IsEmpty())
            segments.AddRange(await segmentClient.GetSegmentsAsync(productId, token));
        else
        {
            var products = await productClient.GetProductsAsync(token);
            foreach (var product in products)
                segments.AddRange(await segmentClient.GetSegmentsAsync(product.ProductId, token));
        }

        if (json)
        {
            output.RenderJson(segments);
            return ExitCodes.Ok;
        }

        var itemsToRender = segments.Select(s => new
        {
            Id = s.SegmentId,
            s.Name,
            Description = s.Description.TrimToFitColumn(),
            Creator = s.CreatorFullName,
            Product = $"{s.Product.Name} [{s.Product.ProductId}]"
        });
        output.RenderTable(itemsToRender);

        return ExitCodes.Ok;
    }

    public async Task<int> CreateSegmentAsync(string productId, 
        string name,
        string description,
        string attribute,
        string comparator,
        string compareTo,
        CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await workspaceLoader.LoadProductAsync(token)).ProductId;

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token);

        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token);

        var model = new CreateOrUpdateSegmentModel
        {
            Name = name,
            Attribute = attribute,
            Comparator = comparator,
            CompareTo = compareTo,
            Description = description
        };
        
        await this.ValidateRuleModel(model, token);

        var result = await segmentClient.CreateSegmentAsync(productId, model, token);
        output.Write(result.SegmentId);
        return ExitCodes.Ok;
    }

    public async Task<int> DeleteSegmentAsync(string segmentId, CancellationToken token)
    {
        if (segmentId.IsEmpty())
            segmentId = (await workspaceLoader.LoadSegmentAsync(token)).SegmentId;

        await segmentClient.DeleteSegmentAsync(segmentId, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateSegmentAsync(string segmentId, 
        string name,
        string description,
        string attribute,
        string comparator,
        string compareTo,
        CancellationToken token)
    {
        var segment = segmentId.IsEmpty()
            ? await workspaceLoader.LoadSegmentAsync(token)
            : await segmentClient.GetSegmentAsync(segmentId, token);

        if (name.IsEmpty())
            name = await prompt.GetStringAsync("Name", token, segment.Name);

        if (description.IsEmpty())
            description = await prompt.GetStringAsync("Description", token, segment.Description);

        var model = new CreateOrUpdateSegmentModel
        {
            Name = name,
            Attribute = attribute,
            Comparator = comparator,
            CompareTo = compareTo,
            Description = description
        };
        
        await this.ValidateRuleModel(model, token, segment);

        if (model.Name.IsEmptyOrEquals(segment.Name) &&
            model.Description.IsEmptyOrEquals(segment.Description) &&
            model.Comparator.IsEmptyOrEquals(segment.Comparator) &&
            model.Attribute.IsEmptyOrEquals(segment.ComparisonAttribute) &&
            model.CompareTo.IsEmptyOrEquals(segment.ComparisonValue))
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        await segmentClient.UpdateSegmentAsync(segment.SegmentId, model, token);
        return ExitCodes.Ok;
    }

    public async Task<int> GetSegmentDetailsAsync(string segmentId, bool json, CancellationToken token)
    {
        var segment = segmentId.IsEmpty()
            ? await workspaceLoader.LoadSegmentAsync(token)
            : await segmentClient.GetSegmentAsync(segmentId, token);

        if (json)
        {
            output.RenderJson(segment);
            return ExitCodes.Ok;
        }

        var separatorLength = segment.Name.Length + segment.SegmentId.Length + 9;

        output.WriteDarkGray(new string('-', separatorLength));
        output.WriteLine().Write(" ");
        output.WriteColor($" {segment.Name} ", ConsoleColor.White, ConsoleColor.DarkGreen);
        output.WriteDarkGray($" [{segment.SegmentId}]").WriteLine();
        output.WriteDarkGray(new string('-', separatorLength)).WriteLine();

        output.Write($" {segment.Description}").WriteLine();

        output.WriteLine()
            .WriteDarkGray($"| ")
            .Write("Created:")
            .WriteDarkGray($" on ")
            .WriteMagenta(segment.CreatedAt.ToShortDateString())
            .WriteDarkGray($" by ")
            .WriteCyan($"{segment.CreatorFullName} ({segment.CreatorEmail})");

        output.WriteLine()
            .WriteDarkGray($"| ")
            .Write("Last updated:")
            .WriteDarkGray($" on ")
            .WriteMagenta(segment.UpdatedAt.ToShortDateString())
            .WriteDarkGray($" by ")
            .WriteCyan($"{segment.LastUpdaterFullName} ({segment.LastUpdaterEmail})");

        output.WriteLine()
            .WriteDarkGray($"| ");

        var comparatorName = Constants.ComparatorTypes.GetValueOrDefault(segment.Comparator) ?? segment.Comparator.ToUpperInvariant();
        output.WriteLine()
            .WriteDarkGray($"| ")
            .Write("Rule:")
            .WriteDarkGray($" when ")
            .WriteCyan($"{segment.ComparisonAttribute} ")
            .WriteYellow($"{comparatorName} ")
            .WriteCyan($"{segment.ComparisonValue} ");

        output.WriteLine()
            .WriteDarkGray(new string('-', separatorLength))
            .WriteLine();

        return ExitCodes.Ok;
    }

    private async Task ValidateRuleModel(CreateOrUpdateSegmentModel createOrUpdateSegmentModel, CancellationToken token, SegmentModel defaultModel = null)
    {
        if (createOrUpdateSegmentModel.Attribute.IsEmpty())
            createOrUpdateSegmentModel.Attribute = await prompt.GetStringAsync("Comparison attribute", token, defaultModel?.ComparisonAttribute ?? "Identifier");

        if (createOrUpdateSegmentModel.Comparator.IsEmpty())
        {
            var preSelectedKey = defaultModel?.Comparator ?? "sensitiveIsOneOf";
            var preSelected = Constants.ComparatorTypes.Single(c => c.Key == preSelectedKey);
            var selected = await prompt.ChooseFromListAsync("Choose comparator", Constants.ComparatorTypes.ToList(), c => $"{c.Key} [{c.Value}]", token, preSelected);

            createOrUpdateSegmentModel.Comparator = selected.Key;
        }

        if (createOrUpdateSegmentModel.CompareTo.IsEmpty())
            createOrUpdateSegmentModel.CompareTo = await prompt.GetStringAsync("Value to compare", token, defaultModel?.ComparisonValue);

        if (!Constants.ComparatorTypes.Keys.Contains(createOrUpdateSegmentModel.Comparator, StringComparer.OrdinalIgnoreCase))
            throw new ShowHelpException($"Comparator must be one of the following: {string.Join('|', Constants.ComparatorTypes)}");
    }
}
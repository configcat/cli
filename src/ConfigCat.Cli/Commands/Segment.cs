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

namespace ConfigCat.Cli.Commands
{
    internal class Segment
    {
        private readonly ISegmentClient segmentClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IProductClient productClient;
        private readonly IPrompt prompt;
        private readonly IOutput output;

        public Segment(ISegmentClient segmentClient,
            IWorkspaceLoader workspaceLoader,
            IProductClient productClient,
            IPrompt prompt,
            IOutput output)
        {
            this.segmentClient = segmentClient;
            this.workspaceLoader = workspaceLoader;
            this.productClient = productClient;
            this.prompt = prompt;
            this.output = output;
        }

        public async Task<int> ListAllSegmentsAsync(string productId, bool json, CancellationToken token)
        {
            var segments = new List<SegmentModel>();
            if (!productId.IsEmpty())
                segments.AddRange(await this.segmentClient.GetSegmentsAsync(productId, token));
            else
            {
                var products = await this.productClient.GetProductsAsync(token);
                foreach (var product in products)
                    segments.AddRange(await this.segmentClient.GetSegmentsAsync(product.ProductId, token));
            }

            if (json)
            {
                this.output.RenderJson(segments);
                return ExitCodes.Ok;
            }

            var itemsToRender = segments.Select(s => new
            {
                Id = s.SegmentId,
                Name = s.Name,
                Description = s.Description.TrimToFitColumn(),
                Creator = s.CreatorFullName,
                Product = $"{s.Product.Name} [{s.Product.ProductId}]"
            });
            this.output.RenderTable(itemsToRender);

            return ExitCodes.Ok;
        }

        public async Task<int> CreateSegmentAsync(string productId, CreateOrUpdateSegmentModel model, CancellationToken token)
        {
            if (productId.IsEmpty())
                productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

            if (model.Name.IsEmpty())
                model.Name = await this.prompt.GetStringAsync("Name", token);

            if (model.Description.IsEmpty())
                model.Description = await this.prompt.GetStringAsync("Description", token);

            await this.ValidateRuleModel(model, token);

            var result = await this.segmentClient.CreateSegmentAsync(productId, model, token);
            this.output.Write(result.SegmentId);
            return ExitCodes.Ok;
        }

        public async Task<int> DeleteSegmentAsync(string segmentId, CancellationToken token)
        {
            if (segmentId.IsEmpty())
                segmentId = (await this.workspaceLoader.LoadSegmentAsync(token)).SegmentId;

            await this.segmentClient.DeleteSegmentAsync(segmentId, token);
            return ExitCodes.Ok;
        }

        public async Task<int> UpdateSegmentAsync(string segmentId, CreateOrUpdateSegmentModel model, CancellationToken token)
        {
            var segment = segmentId.IsEmpty()
                ? await this.workspaceLoader.LoadSegmentAsync(token)
                : await this.segmentClient.GetSegmentAsync(segmentId, token);

            if (model.Name.IsEmpty())
                model.Name = await this.prompt.GetStringAsync("Name", token, segment.Name);

            if (model.Description.IsEmpty())
                model.Description = await this.prompt.GetStringAsync("Description", token, segment.Description);

            await this.ValidateRuleModel(model, token, segment);

            if (model.Name.IsEmptyOrEquals(segment.Name) &&
                model.Description.IsEmptyOrEquals(segment.Description) &&
                model.Comparator.IsEmptyOrEquals(segment.Comparator) &&
                model.Attribute.IsEmptyOrEquals(segment.ComparisonAttribute) &&
                model.CompareTo.IsEmptyOrEquals(segment.ComparisonValue))
            {
                this.output.WriteNoChange();
                return ExitCodes.Ok;
            }

            await this.segmentClient.UpdateSegmentAsync(segment.SegmentId, model, token);
            return ExitCodes.Ok;
        }

        public async Task<int> GetSegmentDetailsAsync(string segmentId, bool json, CancellationToken token)
        {
            var segment = segmentId.IsEmpty()
                ? await this.workspaceLoader.LoadSegmentAsync(token)
                : await this.segmentClient.GetSegmentAsync(segmentId, token);

            if (json)
            {
                this.output.RenderJson(segment);
                return ExitCodes.Ok;
            }

            var separatorLength = segment.Name.Length + segment.SegmentId.Length + 9;

            this.output.WriteDarkGray(new string('-', separatorLength));
            this.output.WriteLine().Write(" ");
            this.output.WriteColor($" {segment.Name} ", ConsoleColor.White, ConsoleColor.DarkGreen);
            this.output.WriteDarkGray($" [{segment.SegmentId}]").WriteLine();
            this.output.WriteDarkGray(new string('-', separatorLength)).WriteLine();

            this.output.Write($" {segment.Description}").WriteLine();

            this.output.WriteLine()
                .WriteDarkGray($"| ")
                .Write("Created:")
                .WriteDarkGray($" on ")
                .WriteMagenta(segment.CreatedAt.ToShortDateString())
                .WriteDarkGray($" by ")
                .WriteCyan($"{segment.CreatorFullName} ({segment.CreatorEmail})");

            this.output.WriteLine()
                .WriteDarkGray($"| ")
                .Write("Last updated:")
                .WriteDarkGray($" on ")
                .WriteMagenta(segment.UpdatedAt.ToShortDateString())
                .WriteDarkGray($" by ")
                .WriteCyan($"{segment.LastUpdaterFullName} ({segment.LastUpdaterEmail})");

            this.output.WriteLine()
                .WriteDarkGray($"| ");

            var comparatorName = Constants.ComparatorTypes.GetValueOrDefault(segment.Comparator) ?? segment.Comparator.ToUpperInvariant();
            this.output.WriteLine()
                .WriteDarkGray($"| ")
                .Write("Rule:")
                .WriteDarkGray($" when ")
                .WriteCyan($"{segment.ComparisonAttribute} ")
                .WriteYellow($"{comparatorName} ")
                .WriteCyan($"{segment.ComparisonValue} ");

            this.output.WriteLine()
                .WriteDarkGray(new string('-', separatorLength))
                .WriteLine();

            return ExitCodes.Ok;
        }

        private async Task ValidateRuleModel(CreateOrUpdateSegmentModel createOrUpdateSegmentModel, CancellationToken token, SegmentModel defaultModel = null)
        {
            if (createOrUpdateSegmentModel.Attribute.IsEmpty())
                createOrUpdateSegmentModel.Attribute = await this.prompt.GetStringAsync("Comparison attribute", token, defaultModel?.ComparisonAttribute ?? "Identifier");

            if (createOrUpdateSegmentModel.Comparator.IsEmpty())
            {
                var preSelectedKey = defaultModel?.Comparator ?? "sensitiveIsOneOf";
                var preSelected = Constants.ComparatorTypes.Single(c => c.Key == preSelectedKey);
                var selected = await this.prompt.ChooseFromListAsync("Choose comparator", Constants.ComparatorTypes.ToList(), c => $"{c.Key} [{c.Value}]", token, preSelected);

                createOrUpdateSegmentModel.Comparator = selected.Key;
            }

            if (createOrUpdateSegmentModel.CompareTo.IsEmpty())
                createOrUpdateSegmentModel.CompareTo = await this.prompt.GetStringAsync("Value to compare", token, defaultModel?.ComparisonValue);

            if (!Constants.ComparatorTypes.Keys.Contains(createOrUpdateSegmentModel.Comparator, StringComparer.OrdinalIgnoreCase))
                throw new ShowHelpException($"Comparator must be one of the following: {string.Join('|', Constants.ComparatorTypes)}");
        }
    }
}

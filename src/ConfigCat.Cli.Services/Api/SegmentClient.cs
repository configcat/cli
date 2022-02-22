using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api
{
    public interface ISegmentClient
    {
        Task<IEnumerable<SegmentModel>> GetSegmentsAsync(string productId, CancellationToken token);

        Task<SegmentModel> GetSegmentAsync(string segmentId, CancellationToken token);

        Task<SegmentModel> CreateSegmentAsync(string productId, CreateOrUpdateSegmentModel model, CancellationToken token);

        Task UpdateSegmentAsync(string segmentId, CreateOrUpdateSegmentModel model, CancellationToken token);

        Task DeleteSegmentAsync(string segmentId, CancellationToken token);
    }

    public class SegmentClient : ApiClient, ISegmentClient
    {
        public SegmentClient(IOutput output,
            CliConfig config,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
            : base(output, config, botPolicy, httpClient)
        { }

        public Task<IEnumerable<SegmentModel>> GetSegmentsAsync(string productId, CancellationToken token) =>
            this.GetAsync<IEnumerable<SegmentModel>>(HttpMethod.Get, $"v1/products/{productId}/segments", token);

        public Task<SegmentModel> GetSegmentAsync(string segmentId, CancellationToken token) =>
            this.GetAsync<SegmentModel>(HttpMethod.Get, $"v1/segments/{segmentId}", token);

        public Task<SegmentModel> CreateSegmentAsync(string productId, CreateOrUpdateSegmentModel model, CancellationToken token) =>
            this.SendAsync<SegmentModel>(HttpMethod.Post, $"v1/products/{productId}/segments", model, token);

        public async Task DeleteSegmentAsync(string segmentId, CancellationToken token)
        {
            this.Output.Write($"Deleting Product... ");
            await this.SendAsync(HttpMethod.Delete, $"v1/segments/{segmentId}", null, token);
            this.Output.WriteSuccess();
            this.Output.WriteLine();
        }

        public async Task UpdateSegmentAsync(string segmentId, CreateOrUpdateSegmentModel model, CancellationToken token)
        {
            this.Output.Write($"Updating Product... ");
            await this.SendAsync(HttpMethod.Put, $"v1/segments/{segmentId}", model, token);
            this.Output.WriteSuccess();
            this.Output.WriteLine();
        }
    }
}

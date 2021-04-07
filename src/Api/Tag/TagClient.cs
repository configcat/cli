using ConfigCat.Cli.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Api.Tag
{
    interface ITagClient
    {
        Task<IEnumerable<TagModel>> GetTagsAsync(string productId, CancellationToken token);

        Task<TagModel> CreateTagAsync(string productId, string name, string color, CancellationToken token);

        Task UpdateTagAsync(int tagId, string name, string color, CancellationToken token);

        Task DeleteTagAsync(int tagId, CancellationToken token);
    }

    class TagClient : ApiClient, ITagClient
    {
        public TagClient(IConfigurationReader configurationReader,
            IExecutionContextAccessor accessor,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient) 
            : base(configurationReader, accessor, botPolicy, httpClient)
        { }

        public Task<IEnumerable<TagModel>> GetTagsAsync(string productId, CancellationToken token) =>
            this.GetAsync<IEnumerable<TagModel>>(HttpMethod.Get, $"v1/products/{productId}/tags", token);

        public Task<TagModel> CreateTagAsync(string productId, string name, string color, CancellationToken token) =>
            this.SendAsync<TagModel>(HttpMethod.Post, $"v1/products/{productId}/tags", new { Name = name, Color = color }, token);

        public async Task DeleteTagAsync(int tagId, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Deleting Tag... ");
            await this.SendAsync(HttpMethod.Delete, $"v1/tags/{tagId}", null, token);
            this.Accessor.ExecutionContext.Output.WriteGreen("Ok.");
            this.Accessor.ExecutionContext.Output.WriteLine();
        }

        public async Task UpdateTagAsync(int tagId, string name, string color, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Updating Tag... ");
            await this.SendAsync(HttpMethod.Put, $"v1/tags/{tagId}", new { Name = name, Color = color }, token);
            this.Accessor.ExecutionContext.Output.WriteGreen("Ok.");
            this.Accessor.ExecutionContext.Output.WriteLine();
        }
    }
}

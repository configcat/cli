using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api;

public interface ITagClient
{
    Task<IEnumerable<TagModel>> GetTagsAsync(string productId, CancellationToken token);

    Task<TagModel> GetTagAsync(int tagId, CancellationToken token);

    Task<TagModel> CreateTagAsync(string productId, string name, string color, CancellationToken token);

    Task UpdateTagAsync(int tagId, string name, string color, CancellationToken token);

    Task DeleteTagAsync(int tagId, CancellationToken token);
}

public class TagClient : ApiClient, ITagClient
{
    public TagClient(IOutput output,
        CliConfig config,
        IBotPolicy<HttpResponseMessage> botPolicy,
        HttpClient httpClient)
        : base(output, config, botPolicy, httpClient)
    { }

    public Task<IEnumerable<TagModel>> GetTagsAsync(string productId, CancellationToken token) =>
        this.GetAsync<IEnumerable<TagModel>>(HttpMethod.Get, $"v1/products/{productId}/tags", token);

    public Task<TagModel> GetTagAsync(int tagId, CancellationToken token) =>
        this.GetAsync<TagModel>(HttpMethod.Get, $"v1/tags/{tagId}", token);

    public Task<TagModel> CreateTagAsync(string productId, string name, string color, CancellationToken token) =>
        this.SendAsync<TagModel>(HttpMethod.Post, $"v1/products/{productId}/tags", new { Name = name, Color = color }, token);

    public async Task DeleteTagAsync(int tagId, CancellationToken token)
    {
        this.Output.Write($"Deleting Tag... ");
        await this.SendAsync(HttpMethod.Delete, $"v1/tags/{tagId}", null, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task UpdateTagAsync(int tagId, string name, string color, CancellationToken token)
    {
        this.Output.Write($"Updating Tag... ");
        await this.SendAsync(HttpMethod.Put, $"v1/tags/{tagId}", new { Name = name, Color = color }, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }
}
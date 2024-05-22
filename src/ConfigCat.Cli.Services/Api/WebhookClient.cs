using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;
using Trybot;

namespace ConfigCat.Cli.Services.Api;

public interface IWebhookClient
{
    Task<IEnumerable<WebhookModel>> GetWebhooksAsync(string productId, CancellationToken token);

    Task<WebhookModel> CreateWebhookAsync(string configId, string environmentId, string url, string httpMethod, string content, CancellationToken token);

    Task<WebhookModel> GetWebhookAsync(int webhookId, CancellationToken token);

    Task UpdateWebhookAsync(int webhookId, List<JsonPatchOperation> operations, CancellationToken token);

    Task DeleteWebhookAsync(int webhookId, CancellationToken token);
}

public class WebhookClient(
    IOutput output,
    CliConfig config,
    IBotPolicy<HttpResponseMessage> botPolicy,
    HttpClient httpClient)
    : ApiClient(output, config, botPolicy, httpClient), IWebhookClient
{
    public Task<IEnumerable<WebhookModel>> GetWebhooksAsync(string productId, CancellationToken token) =>
        this.GetAsync<IEnumerable<WebhookModel>>(HttpMethod.Get, $"v1/products/{productId}/webhooks", token);

    public Task<WebhookModel> GetWebhookAsync(int webhookId, CancellationToken token) =>
        this.GetAsync<WebhookModel>(HttpMethod.Get, $"v1/webhooks/{webhookId}", token);

    public Task<WebhookModel> CreateWebhookAsync(string configId, string environmentId, string url, string httpMethod,
        string content, CancellationToken token) =>
        this.SendAsync<WebhookModel>(HttpMethod.Post, $"v1/configs/{configId}/environments/{environmentId}/webhooks",
            new { Url = url, HttpMethod = httpMethod, Content = content }, token);
    
    public async Task UpdateWebhookAsync(int webhookId, List<JsonPatchOperation> operations, CancellationToken token)
    {
        this.Output.Write($"Updating Webhook... ");
        await this.SendAsync(HttpMethod.Patch, $"v1/webhooks/{webhookId}", operations, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task DeleteWebhookAsync(int webhookId, CancellationToken token)
    {
        this.Output.Write($"Deleting Webhook... ");
        await this.SendAsync(HttpMethod.Delete, $"v1/webhooks/{webhookId}", null, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }
}
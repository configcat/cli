using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api;

public interface ISdkKeyClient
{
    Task<SdkKeyModel> GetSdkKeyAsync(string configId, string environmentId, CancellationToken token);
}

public class SdkKeyClient(
    IOutput output,
    CliConfig config,
    IBotPolicy<HttpResponseMessage> botPolicy,
    HttpClient httpClient)
    : ApiClient(output, config, botPolicy, httpClient), ISdkKeyClient
{
    public Task<SdkKeyModel> GetSdkKeyAsync(string configId, string environmentId, CancellationToken token) =>
        this.GetAsync<SdkKeyModel>(HttpMethod.Get, $"v1/configs/{configId}/environments/{environmentId}", token);
}
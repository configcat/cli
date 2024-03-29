using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api;

public interface IMeClient
{
    Task<MeModel> GetMeAsync(CancellationToken token);
}

public class MeClient(
    IOutput output,
    CliConfig config,
    IBotPolicy<HttpResponseMessage> botPolicy,
    HttpClient httpClient)
    : ApiClient(output, config, botPolicy, httpClient), IMeClient
{
    public Task<MeModel> GetMeAsync(CancellationToken token) =>
        this.GetAsync<MeModel>(HttpMethod.Get, "v1/me", token);
}
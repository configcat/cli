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

public interface IFlagValueV2Client
{
    Task<FlagValueV2Model> GetValueAsync(int settingId, string environmentId, CancellationToken token);

    Task ReplaceValueAsync(int settingId, string environmentId, FlagValueV2Model model, CancellationToken token);

    Task UpdateValueAsync(int settingId, string environmentId, List<JsonPatchOperation> operations, CancellationToken token);
}

public class FlagValueV2Client(
    IOutput output,
    CliConfig config,
    IBotPolicy<HttpResponseMessage> botPolicy,
    HttpClient httpClient)
    : ApiClient(output, config, botPolicy, httpClient), IFlagValueV2Client
{
    public Task<FlagValueV2Model> GetValueAsync(int settingId, string environmentId, CancellationToken token) =>
        this.GetAsync<FlagValueV2Model>(HttpMethod.Get, $"v2/environments/{environmentId}/settings/{settingId}/value", token);

    public async Task ReplaceValueAsync(int settingId, string environmentId, FlagValueV2Model model, CancellationToken token)
    {
        this.Output.Write($"Updating Flag Value... ");
        await this.SendAsync(HttpMethod.Put, $"v2/environments/{environmentId}/settings/{settingId}/value", model, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task UpdateValueAsync(int settingId, string environmentId, List<JsonPatchOperation> operations, CancellationToken token)
    {
        this.Output.Write($"Updating Flag Value... ");
        await this.SendAsync(HttpMethod.Patch, $"v2/environments/{environmentId}/settings/{settingId}/value", operations, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }
}
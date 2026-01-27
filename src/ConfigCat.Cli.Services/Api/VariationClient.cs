using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using Trybot;

namespace ConfigCat.Cli.Services.Api;

public interface IVariationClient
{
    Task<VariationsModel> UpdateVariationsAsync(int flagId, List<VariationModel> updatedModel, CancellationToken token);
}

public class VariationClient(
    IOutput output,
    CliConfig config,
    IBotPolicy<HttpResponseMessage> botPolicy,
    HttpClient httpClient)
    : ApiClient(output, config, botPolicy, httpClient), IVariationClient
{
    public Task<VariationsModel> UpdateVariationsAsync(int flagId, List<VariationModel> updatedModel,
        CancellationToken token) =>
        this.SendAsync<VariationsModel>(HttpMethod.Put, $"v1/settings/{flagId}/predefined-variations",
            new VariationsModel { PredefinedVariations = updatedModel }, token);
}
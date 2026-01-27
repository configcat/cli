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
    Task UpdateVariationsAsync(int flagId, List<VariationModel> updatedModel, CancellationToken token);
}

public class VariationClient(
    IOutput output,
    CliConfig config,
    IBotPolicy<HttpResponseMessage> botPolicy,
    HttpClient httpClient)
    : ApiClient(output, config, botPolicy, httpClient), IVariationClient
{
    public async Task UpdateVariationsAsync(int flagId, List<VariationModel> updatedModel, CancellationToken token)
    {
        this.Output.Write($"Updating Predefined Variations... ");
        await this.SendAsync(HttpMethod.Put, $"v1/settings/{flagId}/predefined-variations", new VariationsModel { PredefinedVariations = updatedModel }, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }
}
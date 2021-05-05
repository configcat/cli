using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api
{
    public interface IFlagValueClient
    {
        Task<FlagValueModel> GetValueAsync(int settingId, string environmentId, CancellationToken token);

        Task ReplaceValueAsync(int settingId, string environmentId, FlagValueModel model, CancellationToken token);

        Task UpdateValueAsync(int settingId, string environmentId, List<JsonPatchOperation> operations, CancellationToken token);
    }

    public class FlagValueClient : ApiClient, IFlagValueClient
    {
        public FlagValueClient(IOutput output,
            CliConfig config,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
            : base(output, config, botPolicy, httpClient)
        { }

        public Task<FlagValueModel> GetValueAsync(int settingId, string environmentId, CancellationToken token) =>
            this.GetAsync<FlagValueModel>(HttpMethod.Get, $"v1/environments/{environmentId}/settings/{settingId}/value", token);

        public async Task ReplaceValueAsync(int settingId, string environmentId, FlagValueModel model, CancellationToken token)
        {
            this.Output.Write($"Updating Flag Value... ");
            await this.SendAsync(HttpMethod.Put, $"v1/environments/{environmentId}/settings/{settingId}/value", model, token);
            this.Output.WriteSuccess();
            this.Output.WriteLine();
        }

        public async Task UpdateValueAsync(int settingId, string environmentId, List<JsonPatchOperation> operations, CancellationToken token)
        {
            this.Output.Write($"Updating Flag Value... ");
            await this.SendAsync(HttpMethod.Patch, $"v1/environments/{environmentId}/settings/{settingId}/value", operations, token);
            this.Output.WriteSuccess();
            this.Output.WriteLine();
        }
    }
}

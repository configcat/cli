using ConfigCat.Cli.Utils;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Api.Flag.Value
{
    interface IFlagValueClient
    {
        Task<FlagValueModel> GetValueAsync(int settingId, string environmentId, CancellationToken token);

        Task ReplaceValueAsync(int settingId, string environmentId, FlagValueModel model, CancellationToken token);

        Task UpdateValueAsync(int settingId, string environmentId, List<JsonPatchOperation> operations, CancellationToken token);
    }

    class FlagValueClient : ApiClient, IFlagValueClient
    {
        public FlagValueClient(IExecutionContextAccessor accessor,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
            : base(accessor, botPolicy, httpClient)
        { }

        public Task<FlagValueModel> GetValueAsync(int settingId, string environmentId, CancellationToken token) =>
            this.GetAsync<FlagValueModel>(HttpMethod.Get, $"v1/environments/{environmentId}/settings/{settingId}/value", token);

        public async Task ReplaceValueAsync(int settingId, string environmentId, FlagValueModel model, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Updating Flag Value... ");
            await this.SendAsync(HttpMethod.Put, $"v1/environments/{environmentId}/settings/{settingId}/value", model, token);
            this.Accessor.ExecutionContext.Output.WriteGreen(Constants.SuccessMessage);
            this.Accessor.ExecutionContext.Output.WriteLine();
        }

        public async Task UpdateValueAsync(int settingId, string environmentId, List<JsonPatchOperation> operations, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Updating Flag Value... ");
            await this.SendAsync(HttpMethod.Patch, $"v1/environments/{environmentId}/settings/{settingId}/value", operations, token);
            this.Accessor.ExecutionContext.Output.WriteGreen(Constants.SuccessMessage);
            this.Accessor.ExecutionContext.Output.WriteLine();
        }
    }
}

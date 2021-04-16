using ConfigCat.Cli.Utils;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Api.Flag
{
    interface IFlagClient
    {
        Task<IEnumerable<FlagModel>> GetFlagsAsync(string configId, CancellationToken token);

        Task<FlagModel> GetFlagAsync(int flagId, CancellationToken token);

        Task<FlagModel> CreateFlagAsync(string configId, CreateFlagModel createFlagModel, CancellationToken token);

        Task UpdateFlagAsync(int flagId, List<JsonPatchOperation> operations, CancellationToken token);

        Task DeleteFlagAsync(int flagId, CancellationToken token);
    }

    class FlagClient : ApiClient, IFlagClient
    {
        public FlagClient(IExecutionContextAccessor accessor,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
            : base(accessor, botPolicy, httpClient)
        { }

        public Task<IEnumerable<FlagModel>> GetFlagsAsync(string configId, CancellationToken token) =>
            this.GetAsync<IEnumerable<FlagModel>>(HttpMethod.Get, $"v1/configs/{configId}/settings", token);

        public Task<FlagModel> GetFlagAsync(int flagId, CancellationToken token) =>
            this.GetAsync<FlagModel>(HttpMethod.Get, $"v1/settings/{flagId}", token);

        public Task<FlagModel> CreateFlagAsync(string configId, CreateFlagModel createFlagModel, CancellationToken token) =>
            this.SendAsync<FlagModel>(HttpMethod.Post, $"v1/configs/{configId}/settings", createFlagModel, token);

        public async Task DeleteFlagAsync(int flagId, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Deleting Flag... ");
            await this.SendAsync(HttpMethod.Delete, $"v1/settings/{flagId}", null, token);
            this.Accessor.ExecutionContext.Output.WriteGreen(Constants.SuccessMessage);
            this.Accessor.ExecutionContext.Output.WriteLine();
        }

        public async Task UpdateFlagAsync(int flagId, List<JsonPatchOperation> operations, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Updating Flag... ");
            await this.SendAsync(HttpMethod.Patch, $"v1/settings/{flagId}", operations, token);
            this.Accessor.ExecutionContext.Output.WriteGreen(Constants.SuccessMessage);
            this.Accessor.ExecutionContext.Output.WriteLine();
        }
    }
}

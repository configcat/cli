using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Api.SdkKey
{
    interface ISdkKeyClient
    {
        Task<SdkKeyModel> GetSdkKeyAsync(string configId, string environmentId, CancellationToken token);
    }

    class SdkKeyClient : ApiClient, ISdkKeyClient
    {
        public SdkKeyClient(IExecutionContextAccessor accessor,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient) 
            : base(accessor, botPolicy, httpClient)
        { }

        public Task<SdkKeyModel> GetSdkKeyAsync(string configId, string environmentId, CancellationToken token) =>
            this.GetAsync<SdkKeyModel>(HttpMethod.Get, $"v1/configs/{configId}/environments/{environmentId}", token);
    }
}

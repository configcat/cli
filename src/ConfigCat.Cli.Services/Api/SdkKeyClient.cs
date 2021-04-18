using ConfigCat.Cli.Models.Api;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api
{
    public interface ISdkKeyClient
    {
        Task<SdkKeyModel> GetSdkKeyAsync(string configId, string environmentId, CancellationToken token);
    }

    public class SdkKeyClient : ApiClient, ISdkKeyClient
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

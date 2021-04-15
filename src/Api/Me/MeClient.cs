using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Api.Me
{
    interface IMeClient
    {
        Task<MeModel> GetMeAsync(CancellationToken token);
    }

    class MeClient : ApiClient, IMeClient
    {
        public MeClient(IExecutionContextAccessor accessor,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient) 
            : base(accessor, botPolicy, httpClient)
        { }

        public Task<MeModel> GetMeAsync(CancellationToken token) =>
            this.GetAsync<MeModel>(HttpMethod.Get, "v1/me", token);
    }
}

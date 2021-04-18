using ConfigCat.Cli.Models.Api;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api
{
    public interface IOrganizationClient
    {
        Task<IEnumerable<OrganizationModel>> GetOrganizationsAsync(CancellationToken token);
    }

    public class OrganizationClient : ApiClient, IOrganizationClient
    {
        public OrganizationClient(IExecutionContextAccessor accessor, 
            IBotPolicy<HttpResponseMessage> botPolicy, 
            HttpClient httpClient) 
            : base(accessor, botPolicy, httpClient)
        {
        }

        public Task<IEnumerable<OrganizationModel>> GetOrganizationsAsync(CancellationToken token) =>
            this.GetAsync<IEnumerable<OrganizationModel>>(HttpMethod.Get, "v1/organizations", token);
    }
}

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using Trybot;

namespace ConfigCat.Cli.Services.Api;

public interface IMemberClient
{
    Task<IEnumerable<Member>> GetOrganizationMembersAsync(string organizationId, CancellationToken token);
    
    Task<IEnumerable<ProductMember>> GetProductMembersAsync(string productId, CancellationToken token);
    
    Task RemoveFromOrganizationAsync(string organizationId, string userId, CancellationToken token);
    
    Task RemoveFromProductAsync(string productId, string userId, CancellationToken token);
}

public class MemberClient : ApiClient, IMemberClient
{
    public MemberClient(IOutput output,
        CliConfig config,
        IBotPolicy<HttpResponseMessage> botPolicy,
        HttpClient httpClient)
        : base(output, config, botPolicy, httpClient)
    { }
    
    public Task<IEnumerable<Member>> GetOrganizationMembersAsync(string organizationId, CancellationToken token) =>
        this.GetAsync<IEnumerable<Member>>(HttpMethod.Get, $"v1/organizations/{organizationId}/members", token);

    public Task<IEnumerable<ProductMember>> GetProductMembersAsync(string productId, CancellationToken token) =>
        this.GetAsync<IEnumerable<ProductMember>>(HttpMethod.Get, $"v1/products/{productId}/members", token);

    public async Task RemoveFromOrganizationAsync(string organizationId, string userId, CancellationToken token)
    {
        this.Output.Write($"Removing Member... ");
        await this.SendAsync(HttpMethod.Delete, $"v1/organizations/{organizationId}/members/{userId}", null, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task RemoveFromProductAsync(string productId, string userId, CancellationToken token)
    {
        this.Output.Write($"Removing Member... ");
        await this.SendAsync(HttpMethod.Delete, $"v1/products/{productId}/members/{userId}", null, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }
}
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
    Task<IEnumerable<MemberModel>> GetOrganizationMembersAsync(string organizationId, CancellationToken token);
    
    Task<IEnumerable<ProductMemberModel>> GetProductMembersAsync(string productId, CancellationToken token);
    
    Task RemoveFromOrganizationAsync(string organizationId, string userId, CancellationToken token);
    
    Task RemoveFromProductAsync(ProductModel product, string userId, CancellationToken token);

    Task InviteMemberAsync(string productId, InviteMemberModel model, CancellationToken token);

    Task UpdateMemberAsync(string organizationId, string userId, UpdateMembersModel model, CancellationToken token);
}

public class MemberClient : ApiClient, IMemberClient
{
    public MemberClient(IOutput output,
        CliConfig config,
        IBotPolicy<HttpResponseMessage> botPolicy,
        HttpClient httpClient)
        : base(output, config, botPolicy, httpClient)
    { }
    
    public Task<IEnumerable<MemberModel>> GetOrganizationMembersAsync(string organizationId, CancellationToken token) =>
        this.GetAsync<IEnumerable<MemberModel>>(HttpMethod.Get, $"v1/organizations/{organizationId}/members", token);

    public Task<IEnumerable<ProductMemberModel>> GetProductMembersAsync(string productId, CancellationToken token) =>
        this.GetAsync<IEnumerable<ProductMemberModel>>(HttpMethod.Get, $"v1/products/{productId}/members", token);

    public async Task RemoveFromOrganizationAsync(string organizationId, string userId, CancellationToken token)
    {
        this.Output.Write($"Removing Member... ");
        await this.SendAsync(HttpMethod.Delete, $"v1/organizations/{organizationId}/members/{userId}", null, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task RemoveFromProductAsync(ProductModel product, string userId, CancellationToken token)
    {
        this.Output.Write($"Removing Member from {product.Name}... ");
        await this.SendAsync(HttpMethod.Delete, $"v1/products/{product.ProductId}/members/{userId}", null, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task InviteMembersAsync(string productId, string userId, CancellationToken token)
    {
        this.Output.Write($"Removing Member... ");
        await this.SendAsync(HttpMethod.Delete, $"v1/products/{productId}/members/{userId}", null, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task InviteMemberAsync(string productId, InviteMemberModel model, CancellationToken token)
    {
        this.Output.Write($"Inviting Member(s)... ");
        await this.SendAsync(HttpMethod.Post, $"v1/products/{productId}/members/invite", model, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task UpdateMemberAsync(string organizationId, string userId, UpdateMembersModel model, CancellationToken token)
    {
        this.Output.Write($"Updating Member... ");
        await this.SendAsync(HttpMethod.Post, $"v1/organizations/{organizationId}/members/{userId}", model, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }
}
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using Trybot;

namespace ConfigCat.Cli.Services.Api;

public interface IPermissionGroupClient
{
    Task<IEnumerable<PermissionGroupModel>> GetPermissionGroupsAsync(string productId, CancellationToken token);

    Task<PermissionGroupModel> CreatePermissionGroupAsync(string productId, PermissionGroupModel model, CancellationToken token);

    Task<PermissionGroupModel> GetPermissionGroupAsync(string configId, CancellationToken token);

    Task UpdatePermissionGroupAsync(string permissionGroupId, PermissionGroupModel model, CancellationToken token);

    Task DeletePermissionGroupAsync(string permissionGroupId, CancellationToken token);
}

public class PermissionGroupClient : ApiClient, IPermissionGroupClient
{
    public PermissionGroupClient(IOutput output,
        CliConfig config,
        IBotPolicy<HttpResponseMessage> botPolicy,
        HttpClient httpClient)
        : base(output, config, botPolicy, httpClient)
    { }

    public Task<IEnumerable<PermissionGroupModel>> GetPermissionGroupsAsync(string productId, CancellationToken token) =>
        this.GetAsync<IEnumerable<PermissionGroupModel>>(HttpMethod.Get, $"v1/products/{productId}/permissions", token);

    public Task<PermissionGroupModel> CreatePermissionGroupAsync(string productId, PermissionGroupModel model, CancellationToken token) =>
        this.SendAsync<PermissionGroupModel>(HttpMethod.Post, $"v1/products/{productId}/permissions", model, token);

    public Task<PermissionGroupModel> GetPermissionGroupAsync(string permissionGroupId, CancellationToken token) =>
        this.GetAsync<PermissionGroupModel>(HttpMethod.Get, $"v1/permissions/{permissionGroupId}", token);

    public async Task DeletePermissionGroupAsync(string permissionGroupId, CancellationToken token)
    {
        this.Output.Write($"Deleting Config... ");
        await this.SendAsync(HttpMethod.Delete, $"v1/permissions/{permissionGroupId}", null, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task UpdatePermissionGroupAsync(string permissionGroupId, PermissionGroupModel model, CancellationToken token)
    {
        this.Output.Write($"Updating Config... ");
        await this.SendAsync(HttpMethod.Put, $"v1/permissions/{permissionGroupId}", model, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }
}
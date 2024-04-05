using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api;

public interface IProductClient
{
    Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken token);

    Task<ProductModel> GetProductAsync(string productId, CancellationToken token);

    Task<ProductModel> CreateProductAsync(string organizationId, string name, string description, CancellationToken token);

    Task UpdateProductAsync(string productId, string name, string description, CancellationToken token);

    Task DeleteProductAsync(string productId, CancellationToken token);
}

public class ProductClient(
    IOutput output,
    CliConfig config,
    IBotPolicy<HttpResponseMessage> botPolicy,
    HttpClient httpClient)
    : ApiClient(output, config, botPolicy, httpClient), IProductClient
{
    public Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken token) =>
        this.GetAsync<IEnumerable<ProductModel>>(HttpMethod.Get, "v1/products", token);

    public Task<ProductModel> GetProductAsync(string productId, CancellationToken token) =>
        this.GetAsync<ProductModel>(HttpMethod.Get, $"v1/products/{productId}", token);

    public Task<ProductModel> CreateProductAsync(string organizationId, string name, string description, CancellationToken token) =>
        this.SendAsync<ProductModel>(HttpMethod.Post, $"v1/organizations/{organizationId}/products", new { Name = name, Description = description }, token);

    public async Task DeleteProductAsync(string productId, CancellationToken token)
    {
        this.Output.Write($"Deleting Product... ");
        await this.SendAsync(HttpMethod.Delete, $"v1/products/{productId}", null, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task UpdateProductAsync(string productId, string name, string description, CancellationToken token)
    {
        this.Output.Write($"Updating Product... ");
        await this.SendAsync(HttpMethod.Put, $"v1/products/{productId}", new { Name = name, Description = description }, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }
}
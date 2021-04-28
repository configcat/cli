using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api
{
    public interface IProductClient
    {
        Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken token);

        Task<ProductModel> GetProductAsync(string productId, CancellationToken token);

        Task<ProductModel> CreateProductAsync(string organizationId, string name, CancellationToken token);

        Task UpdateProductAsync(string productId, string name, CancellationToken token);

        Task DeleteProductAsync(string productId, CancellationToken token);
    }

    public class ProductClient : ApiClient, IProductClient
    {
        public ProductClient(IOutput output,
            CliConfig config,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
            : base(output, config, botPolicy, httpClient)
        { }

        public Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken token) =>
            this.GetAsync<IEnumerable<ProductModel>>(HttpMethod.Get, "v1/products", token);

        public Task<ProductModel> GetProductAsync(string productId, CancellationToken token) =>
            this.GetAsync<ProductModel>(HttpMethod.Get, $"v1/products/{productId}", token);

        public Task<ProductModel> CreateProductAsync(string organizationId, string name, CancellationToken token) =>
            this.SendAsync<ProductModel>(HttpMethod.Post, $"v1/organizations/{organizationId}/products", new { Name = name }, token);

        public async Task DeleteProductAsync(string productId, CancellationToken token)
        {
            this.Output.Write($"Deleting Product... ");
            await this.SendAsync(HttpMethod.Delete, $"v1/products/{productId}", null, token);
            this.Output.WriteGreen(Constants.SuccessMessage);
            this.Output.WriteLine();
        }

        public async Task UpdateProductAsync(string productId, string name, CancellationToken token)
        {
            this.Output.Write($"Updating Product... ");
            await this.SendAsync(HttpMethod.Put, $"v1/products/{productId}", new { Name = name }, token);
            this.Output.WriteGreen(Constants.SuccessMessage);
            this.Output.WriteLine();
        }
    }
}

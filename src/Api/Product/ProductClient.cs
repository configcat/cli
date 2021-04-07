using ConfigCat.Cli.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Api.Product
{
    interface IProductClient
    {
        Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken token);

        Task<ProductModel> CreateProductAsync(string organizationId, string name, CancellationToken token);

        Task UpdateProductAsync(string productId, string name, CancellationToken token);

        Task DeleteProductAsync(string productId, CancellationToken token);
    }

    class ProductClient : ApiClient, IProductClient
    {
        public ProductClient(IConfigurationReader configurationReader,
            IExecutionContextAccessor accessor,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient) : base(configurationReader, accessor, botPolicy, httpClient)
        { }

        public Task<IEnumerable<ProductModel>> GetProductsAsync(CancellationToken token) =>
            this.GetAsync<IEnumerable<ProductModel>>(HttpMethod.Get, "v1/products", token);

        public Task<ProductModel> CreateProductAsync(string organizationId, string name, CancellationToken token) =>
            this.SendAsync<ProductModel>(HttpMethod.Post, $"v1/organizations/{organizationId}/products", new { Name = name }, token);

        public async Task DeleteProductAsync(string productId, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Deleting Product... ");
            await this.SendAsync(HttpMethod.Delete, $"v1/products/{productId}", null, token);
            this.Accessor.ExecutionContext.Output.WriteGreen("Ok.");
            this.Accessor.ExecutionContext.Output.WriteLine();
        }

        public async Task UpdateProductAsync(string productId, string name, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Updating Product... ");
            await this.SendAsync(HttpMethod.Put, $"v1/products/{productId}", new { Name = name }, token);
            this.Accessor.ExecutionContext.Output.WriteGreen("Ok.");
            this.Accessor.ExecutionContext.Output.WriteLine();
        }
    }
}

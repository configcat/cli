using ConfigCat.Cli.Models.Api;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api
{
    public interface IConfigClient
    {
        Task<IEnumerable<ConfigModel>> GetConfigsAsync(string productId, CancellationToken token);

        Task<ConfigModel> CreateConfigAsync(string productId, string name, CancellationToken token);

        Task<ConfigModel> GetConfigAsync(string configId, CancellationToken token);

        Task UpdateConfigAsync(string configId, string name, CancellationToken token);

        Task DeleteConfigAsync(string configId, CancellationToken token);
    }

    public class ConfigClient : ApiClient, IConfigClient
    {
        public ConfigClient(IExecutionContextAccessor accessor,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
            : base(accessor, botPolicy, httpClient)
        { }

        public Task<IEnumerable<ConfigModel>> GetConfigsAsync(string productId, CancellationToken token) =>
            this.GetAsync<IEnumerable<ConfigModel>>(HttpMethod.Get, $"v1/products/{productId}/configs", token);

        public Task<ConfigModel> CreateConfigAsync(string productId, string name, CancellationToken token) =>
            this.SendAsync<ConfigModel>(HttpMethod.Post, $"v1/products/{productId}/configs", new { Name = name }, token);

        public Task<ConfigModel> GetConfigAsync(string configId, CancellationToken token) =>
            this.GetAsync<ConfigModel>(HttpMethod.Get, $"v1/configs/{configId}", token);

        public async Task DeleteConfigAsync(string configId, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Deleting Config... ");
            await this.SendAsync(HttpMethod.Delete, $"v1/configs/{configId}", null, token);
            this.Accessor.ExecutionContext.Output.WriteGreen(Constants.SuccessMessage);
            this.Accessor.ExecutionContext.Output.WriteLine();
        }

        public async Task UpdateConfigAsync(string configId, string name, CancellationToken token)
        {
            this.Accessor.ExecutionContext.Output.Write($"Updating Config... ");
            await this.SendAsync(HttpMethod.Put, $"v1/configs/{configId}", new { Name = name }, token);
            this.Accessor.ExecutionContext.Output.WriteGreen(Constants.SuccessMessage);
            this.Accessor.ExecutionContext.Output.WriteLine();
        }
    }
}

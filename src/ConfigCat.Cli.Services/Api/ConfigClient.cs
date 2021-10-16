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
    public interface IConfigClient
    {
        Task<IEnumerable<ConfigModel>> GetConfigsAsync(string productId, CancellationToken token);

        Task<ConfigModel> CreateConfigAsync(string productId, string name, string description, CancellationToken token);

        Task<ConfigModel> GetConfigAsync(string configId, CancellationToken token);

        Task UpdateConfigAsync(string configId, string name, string description, CancellationToken token);

        Task DeleteConfigAsync(string configId, CancellationToken token);
    }

    public class ConfigClient : ApiClient, IConfigClient
    {
        public ConfigClient(IOutput output,
            CliConfig config,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
            : base(output, config, botPolicy, httpClient)
        { }

        public Task<IEnumerable<ConfigModel>> GetConfigsAsync(string productId, CancellationToken token) =>
            this.GetAsync<IEnumerable<ConfigModel>>(HttpMethod.Get, $"v1/products/{productId}/configs", token);

        public Task<ConfigModel> CreateConfigAsync(string productId, string name, string description, CancellationToken token) =>
            this.SendAsync<ConfigModel>(HttpMethod.Post, $"v1/products/{productId}/configs", new { Name = name, Description = description }, token);

        public Task<ConfigModel> GetConfigAsync(string configId, CancellationToken token) =>
            this.GetAsync<ConfigModel>(HttpMethod.Get, $"v1/configs/{configId}", token);

        public async Task DeleteConfigAsync(string configId, CancellationToken token)
        {
            this.Output.Write($"Deleting Config... ");
            await this.SendAsync(HttpMethod.Delete, $"v1/configs/{configId}", null, token);
            this.Output.WriteSuccess();
            this.Output.WriteLine();
        }

        public async Task UpdateConfigAsync(string configId, string name, string description, CancellationToken token)
        {
            this.Output.Write($"Updating Config... ");
            await this.SendAsync(HttpMethod.Put, $"v1/configs/{configId}", new { Name = name, Description = description }, token);
            this.Output.WriteSuccess();
            this.Output.WriteLine();
        }
    }
}

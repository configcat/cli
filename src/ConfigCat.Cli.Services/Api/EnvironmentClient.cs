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
    public interface IEnvironmentClient
    {
        Task<IEnumerable<EnvironmentModel>> GetEnvironmentsAsync(string productId, CancellationToken token);

        Task<EnvironmentModel> GetEnvironmentAsync(string environmentId, CancellationToken token);

        Task<EnvironmentModel> CreateEnvironmentAsync(string productId, string name, CancellationToken token);

        Task UpdateEnvironmentAsync(string environmentId, string name, CancellationToken token);

        Task DeleteEnvironmentAsync(string environmentId, CancellationToken token);
    }

    public class EnvironmentClient : ApiClient, IEnvironmentClient
    {
        public EnvironmentClient(IOutput output,
            CliConfig config,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
            : base(output, config, botPolicy, httpClient)
        { }

        public Task<IEnumerable<EnvironmentModel>> GetEnvironmentsAsync(string productId, CancellationToken token) =>
            this.GetAsync<IEnumerable<EnvironmentModel>>(HttpMethod.Get, $"v1/products/{productId}/environments", token);

        public Task<EnvironmentModel> GetEnvironmentAsync(string environmentId, CancellationToken token) =>
            this.GetAsync<EnvironmentModel>(HttpMethod.Get, $"v1/environments/{environmentId}", token);

        public Task<EnvironmentModel> CreateEnvironmentAsync(string productId, string name, CancellationToken token) =>
            this.SendAsync<EnvironmentModel>(HttpMethod.Post, $"v1/products/{productId}/environments", new { Name = name }, token);

        public async Task DeleteEnvironmentAsync(string environmentId, CancellationToken token)
        {
            this.Output.Write($"Deleting Environment... ");
            await this.SendAsync(HttpMethod.Delete, $"v1/environments/{environmentId}", null, token);
            this.Output.WriteGreen(Constants.SuccessMessage);
            this.Output.WriteLine();
        }

        public async Task UpdateEnvironmentAsync(string environmentId, string name, CancellationToken token)
        {
            this.Output.Write($"Updating Environment... ");
            await this.SendAsync(HttpMethod.Put, $"v1/environments/{environmentId}", new { Name = name }, token);
            this.Output.WriteGreen(Constants.SuccessMessage);
            this.Output.WriteLine();
        }
    }
}

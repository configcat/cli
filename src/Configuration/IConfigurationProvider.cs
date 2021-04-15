using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Configuration
{
    interface IConfigurationProvider
    {
        Task<CliConfig> GetConfigAsync(CancellationToken cancellationToken);

        Task SaveConfigAsync(CliConfig config, CancellationToken cancellationToken);
    }
}

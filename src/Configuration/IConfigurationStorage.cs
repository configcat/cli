using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Configuration
{
    interface IConfigurationStorage
    {
        Task<CliConfig> ReadConfigOrDefaultAsync(CancellationToken cancellationToken);

        Task WriteConfigAsync(CliConfig configuration, CancellationToken cancellationToken);
    }
}

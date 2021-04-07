using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Configuration
{
    interface IConfigurationReader
    {
        Task<CliConfig> ReadConfigurationAsync(CancellationToken cancellationToken);
    }
}

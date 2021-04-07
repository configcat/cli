using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Configuration
{
    interface IConfigurationWriter
    {
        Task WriteConfigurationAsync(CliConfig configuration, CancellationToken cancellationToken);
    }
}

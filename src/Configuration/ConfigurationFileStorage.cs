using ConfigCat.Cli.Utils;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Configuration
{
    interface IConfigurationStorage
    {
        Task<CliConfig> ReadConfigOrDefaultAsync(CancellationToken cancellationToken);

        Task WriteConfigAsync(CliConfig configuration, CancellationToken cancellationToken);
    }

    class ConfigurationFileStorage : IConfigurationStorage
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IExecutionContextAccessor accessor;

        public ConfigurationFileStorage(IExecutionContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public async Task<CliConfig> ReadConfigOrDefaultAsync(CancellationToken cancellationToken)
        {
            var output = this.accessor.ExecutionContext.Output;

            if(!File.Exists(Constants.ConfigFilePath))
                return null;

            var content = await File.ReadAllTextAsync(Constants.ConfigFilePath, cancellationToken);
            var config = JsonSerializer.Deserialize<CliConfig>(content, Options);
            output.Verbose($"Config loaded from '{Constants.ConfigFilePath}'");
            return config;
        }

        public async Task WriteConfigAsync(CliConfig configuration, CancellationToken cancellationToken)
        {
            var output = this.accessor.ExecutionContext.Output;

            var directory = Path.GetDirectoryName(Constants.ConfigFilePath);
            if (!Directory.Exists(directory))
            {
                output.Verbose($"Directory '{directory}' not found, creating...");
                Directory.CreateDirectory(directory);
            }

            var serialized = JsonSerializer.Serialize(configuration, Options);

            output.Verbose($"Writing the configuration into '{Constants.ConfigFilePath}'");
            using var spinner = output.CreateSpinner(cancellationToken);
            await File.WriteAllTextAsync(Constants.ConfigFilePath, serialized, cancellationToken);
        }
    }
}

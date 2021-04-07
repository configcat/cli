using ConfigCat.Cli.Utils;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Configuration
{
    class ConfigurationHandler : IConfigurationWriter, IConfigurationReader
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IExecutionContextAccessor accessor;
        private CliConfig options;

        public ConfigurationHandler(IExecutionContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public async Task<CliConfig> ReadConfigurationAsync(CancellationToken cancellationToken)
        {
            if(this.options != null)
                return options;

            var output = this.accessor.ExecutionContext.Output;

            var host = Environment.GetEnvironmentVariable(Constants.ApiHostEnvironmentVariableName);
            var user = Environment.GetEnvironmentVariable(Constants.ApiUserNameEnvironmentVariableName);
            var pass = Environment.GetEnvironmentVariable(Constants.ApiPasswordEnvironmentVariableName);

            if(!host.IsEmpty() && 
               !user.IsEmpty() &&
               !pass.IsEmpty())
            { 
                output.Verbose($"Host: {host} (from {Constants.ApiHostEnvironmentVariableName})");
                output.Verbose($"Username: {user} (from {Constants.ApiUserNameEnvironmentVariableName})");
                output.Verbose($"Password: <masked> (from {Constants.ApiPasswordEnvironmentVariableName})");

                this.options = new CliConfig { ApiHost = host, Password = pass, UserName = user };
                return options;
            }

            var hasConfig = File.Exists(Constants.ConfigFilePath);

            if(!hasConfig &&
               !user.IsEmpty() &&
               !pass.IsEmpty())
            {
                output.Verbose($"No config file found");
                output.Verbose($"Host: {Constants.DefaultApiHost} (default)");
                output.Verbose($"Username: {user} (from {Constants.ApiUserNameEnvironmentVariableName})");
                output.Verbose($"Password: <masked> (from {Constants.ApiPasswordEnvironmentVariableName})");

                this.options = new CliConfig { ApiHost = Constants.DefaultApiHost, Password = pass, UserName = user };
                return options;
            }

            if (!hasConfig)
                throw new Exception($"The CLI is not configured properly, please execute the 'configcat setup' command, or set the {Constants.ApiUserNameEnvironmentVariableName} and {Constants.ApiPasswordEnvironmentVariableName} environment variables.");

            var content = await File.ReadAllTextAsync(Constants.ConfigFilePath, cancellationToken);
            var config = JsonSerializer.Deserialize<CliConfig>(content, Options);

            output.Verbose($"Config loaded from '{Constants.ConfigFilePath}'");

            var fromHost = host != null ? $"(from {Constants.ApiHostEnvironmentVariableName})"
                : config.ApiHost != null
                    ? "(from config file)"
                    : "(default)";
            var fromUser = user != null ? $"(from {Constants.ApiUserNameEnvironmentVariableName})" : "(from config file)";
            var fromPass = pass != null ? $"(from {Constants.ApiPasswordEnvironmentVariableName})" : "(from config file)";

            output.Verbose($"Host: {host ?? config.ApiHost ?? Constants.DefaultApiHost} {fromHost}");
            output.Verbose($"Username: {user ?? config.UserName} {fromUser}");
            output.Verbose($"Password: <masked> {fromPass}");

            this.options = new CliConfig 
            { 
                ApiHost = host ?? config.ApiHost ?? Constants.DefaultApiHost, 
                Password = pass ?? config.Password, 
                UserName = user ?? config.UserName
            };

            return options;
        }

        public async Task WriteConfigurationAsync(CliConfig configuration, CancellationToken cancellationToken)
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

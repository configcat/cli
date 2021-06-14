using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Configuration
{
    public interface IConfigurationProvider
    {
        Task<CliConfig> GetConfigAsync(CancellationToken cancellationToken);
    }

    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly IOutput output;
        private readonly IConfigurationStorage configurationStorage;

        public ConfigurationProvider(IOutput output, IConfigurationStorage configurationStorage)
        {
            this.output = output;
            this.configurationStorage = configurationStorage;
        }

        public async Task<CliConfig> GetConfigAsync(CancellationToken cancellationToken)
        {
            var host = Environment.GetEnvironmentVariable(Constants.ApiHostEnvironmentVariableName);
            var user = Environment.GetEnvironmentVariable(Constants.ApiUserNameEnvironmentVariableName);
            var pass = Environment.GetEnvironmentVariable(Constants.ApiPasswordEnvironmentVariableName);

            var config = await this.configurationStorage.ReadConfigOrDefaultAsync(cancellationToken);

            if ((pass is null && config?.Auth?.Password is null) ||
                (user is null && config?.Auth?.UserName is null))
                throw new ShowHelpException($"The CLI is not configured properly, please execute the `configcat setup` command, or set the {Constants.ApiUserNameEnvironmentVariableName} and {Constants.ApiPasswordEnvironmentVariableName} environment variables.");

            var fromHost = host is not null ? $"(from env:{Constants.ApiHostEnvironmentVariableName})"
                : config.Auth.ApiHost is not null
                    ? "(from config file)"
                    : "(default)";
            var fromUser = user is not null ? $"(from env:{Constants.ApiUserNameEnvironmentVariableName})" : "(from config file)";
            var fromPass = pass is not null ? $"(from env:{Constants.ApiPasswordEnvironmentVariableName})" : "(from config file)";

            this.output.Verbose($"Host: {host ?? config.Auth.ApiHost ?? Constants.DefaultApiHost} {fromHost}");
            this.output.Verbose($"Username: {user ?? config.Auth.UserName} {fromUser}");
            this.output.Verbose($"Password: <masked> {fromPass}");

            return new CliConfig
            {
                Auth = new Auth
                {
                    ApiHost = host ?? config.Auth.ApiHost ?? Constants.DefaultApiHost,
                    Password = pass ?? config.Auth.Password,
                    UserName = user ?? config.Auth.UserName
                }
            };
        }
    }
}

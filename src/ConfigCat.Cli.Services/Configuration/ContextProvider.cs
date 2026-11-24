using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Credentials;
using ConfigCat.Cli.Services.Credentials;

namespace ConfigCat.Cli.Services.Configuration;

public interface IContextProvider
{
    Task<CliContext> GetConfigAsync(CancellationToken cancellationToken);
}

public class ContextProvider(IOutput output, IConfigurationStorage configurationStorage, ICredentialsManager credentialsManager) : IContextProvider
{
    public async Task<CliContext> GetConfigAsync(CancellationToken cancellationToken)
    {
        var host = Environment.GetEnvironmentVariable(Constants.ApiHostEnvironmentVariableName);
        var user = Environment.GetEnvironmentVariable(Constants.ApiUserNameEnvironmentVariableName);
        var pass = Environment.GetEnvironmentVariable(Constants.ApiPasswordEnvironmentVariableName);
        
        var config = await configurationStorage.ReadConfigOrDefaultAsync(cancellationToken);

        var credentialSource = "environment variable";

#pragma warning disable CS0612 // Type or member is obsolete
        // Migrate legacy credentials from config file to credential store
        if (config?.Auth is not null &&
            (!string.IsNullOrEmpty(config.Auth.UserName) || !string.IsNullOrEmpty(config.Auth.Password)))
        {
            var legacyUserName = config.Auth.UserName;
            var legacyPassword = config.Auth.Password;

            if (!string.IsNullOrEmpty(legacyUserName) && !string.IsNullOrEmpty(legacyPassword))
            {
                var credentialStore = credentialsManager.CreateBasicAuthCredentialStore();
                if (string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pass))
                { 
                    credentialStore.Set(new CredentialData
                    {
                        UserName = legacyUserName,
                        Password = legacyPassword
                    });
                    user = legacyUserName;
                    pass = legacyPassword;
                }
                else if (!string.IsNullOrEmpty(user) || !string.IsNullOrEmpty(pass))
                {
                    credentialStore.Set(new CredentialData
                    {
                        UserName = legacyUserName,
                        Password = legacyPassword
                    });
                }
            }

            config.Auth.UserName = null;
            config.Auth.Password = null;
            await configurationStorage.WriteConfigAsync(config, cancellationToken);
        }
#pragma warning restore CS0612 // Type or member is obsolete

        if (string.IsNullOrEmpty(user) && string.IsNullOrEmpty(pass))
        {
            var credentialStore = credentialsManager.CreateBasicAuthCredentialStore();
            var credentials = credentialStore.Get();
            if (credentials is not null &&
                !string.IsNullOrEmpty(credentials.UserName) &&
                !string.IsNullOrEmpty(credentials.Password))
            {
                user = credentials.UserName;
                pass = credentials.Password;
                credentialSource = "credential store";
            }
        }

        if (string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(user))
            throw new ShowHelpException($"The CLI is not configured properly, please execute the `configcat setup` command, or set the {Constants.ApiUserNameEnvironmentVariableName} and {Constants.ApiPasswordEnvironmentVariableName} environment variables.");

        var fromHost = host is not null ? $"(from env:{Constants.ApiHostEnvironmentVariableName})"
            : config?.Auth?.ApiHost is not null
                ? "(from config file)"
                : "(default)";

        var fromNotice = $"(from {credentialSource})";

        output.Verbose($"Host: {host ?? config?.Auth?.ApiHost ?? Constants.DefaultApiHost} {fromHost}");
        output.Verbose($"Username: {user} {fromNotice}");
        output.Verbose($"Password: <masked> {fromNotice}");

        var context = new CliContext
        {
            ApiHost = host ?? config?.Auth?.ApiHost ?? Constants.DefaultApiHost,
            Workspace = new Workspace
            {
                Config = config?.Workspace?.Config,
                Product = config?.Workspace?.Product
            }
        };
        context.SetBasicAuth(user, pass);
        return context;
    }
}
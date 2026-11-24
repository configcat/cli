using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Credentials;
using ConfigCat.Cli.Services.Credentials;

namespace ConfigCat.Cli.Commands;

internal class Setup(
    IPrompt prompt,
    IMeClient meClient,
    IConfigurationStorage configurationStorage,
    ICredentialsManager credentialsManager,
    IOutput output,
    CliContext cliContext)
{
    public async Task<int> InvokeAsync(string apiHost, string userName, string password, CancellationToken token)
    {
        if (apiHost.IsEmpty())
            apiHost = await prompt.GetStringAsync("API host", token, Constants.DefaultApiHost);

        if (userName.IsEmpty())
            userName = await prompt.GetStringAsync("Basic auth username", token);

        if (password.IsEmpty())
            password = await prompt.GetMaskedStringAsync("Basic auth password", token);

        output.WriteLine();
        output.Write($"Saving the configuration to '{Constants.ConfigFilePath}'... ");
        var cliConfig = new CliConfig
        {
            Auth = new Auth
            {
                ApiHost = apiHost,
            },
            Workspace = cliContext.Workspace,
        };
        cliContext.ApiHost = apiHost;
        await configurationStorage.WriteConfigAsync(cliConfig, token);

        output.WriteSuccess().WriteLine().Write($"Saving credentials to credential store... ");
        var credentialStore = credentialsManager.CreateApiCredentialStore();
        credentialStore.Set(new CredentialData { UserName = userName, Password = password });
        cliContext.SetBasicAuth(userName, password);

        output.WriteSuccess().WriteLine().Write($"Verifying your credentials against '{apiHost}'... ");

        var me = await meClient.GetMeAsync(token);

        output.WriteSuccess()
            .Write($" Welcome, {me.FullName}.")
            .WriteLine().WriteLine()
            .WriteGreen("Setup complete.")
            .WriteLine().WriteLine();

        return ExitCodes.Ok;
    }
    
    public async Task<int> LogoutAsync(CancellationToken token)
    {
        output.Write("Clearing credentials... ");
        {
            using var spinner = output.CreateSpinner(token);
            var credentialStore = credentialsManager.CreateApiCredentialStore();
            credentialStore.Clear();
            await configurationStorage.WriteConfigAsync(new CliConfig(), token);
        }
        output.WriteSuccess().WriteLine();
        
        return ExitCodes.Ok;
    }
    
    public async Task<int> MeAsync(CancellationToken token)
    {
        var me = await meClient.GetMeAsync(token);
        output.WriteLine($"{me.FullName} ({me.Email})");
        return ExitCodes.Ok;
    }
}
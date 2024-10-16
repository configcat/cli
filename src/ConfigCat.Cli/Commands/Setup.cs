using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class Setup(
    IPrompt prompt,
    IMeClient meClient,
    IConfigurationStorage configurationStorage,
    IOutput output,
    CliConfig cliConfig)
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
        cliConfig.Auth = new Auth
        {
            ApiHost = apiHost,
            UserName = userName,
            Password = password
        };
        await configurationStorage.WriteConfigAsync(cliConfig, token);

        output.WriteSuccess().WriteLine().Write($"Verifying your credentials against '{apiHost}'... ");

        var me = await meClient.GetMeAsync(token);

        output.WriteSuccess()
            .Write($" Welcome, {me.FullName}.")
            .WriteLine().WriteLine()
            .WriteGreen("Setup complete.")
            .WriteLine().WriteLine();

        return ExitCodes.Ok;
    }
}
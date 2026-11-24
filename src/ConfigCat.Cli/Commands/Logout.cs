using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Configuration;
using ConfigCat.Cli.Services.Credentials;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands;

public class Logout(ICredentialStore credentialStore, IConfigurationStorage configurationStorage, IOutput output)
{
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        output.Write("Clearing credentials... ");
        {
            using var spinner = output.CreateSpinner(token);
            credentialStore.Clear();
            await configurationStorage.WriteConfigAsync(new CliConfig(), token);
        }
        output.WriteSuccess().WriteLine();
        
        return ExitCodes.Ok;
    }
}
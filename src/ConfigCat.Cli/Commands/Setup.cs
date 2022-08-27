using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    internal class Setup
    {
        private readonly IPrompt prompt;
        private readonly IMeClient meClient;
        private readonly IConfigurationStorage configurationStorage;
        private readonly IOutput output;
        private readonly CliConfig cliConfig;

        public Setup(IPrompt prompt,
            IMeClient meClient,
            IConfigurationStorage configurationStorage,
            IOutput output,
            CliConfig cliConfig)
        {
            this.prompt = prompt;
            this.meClient = meClient;
            this.configurationStorage = configurationStorage;
            this.output = output;
            this.cliConfig = cliConfig;
        }

        public async Task<int> InvokeAsync(string apiHost, string userName, string password, CancellationToken token)
        {
            if (apiHost.IsEmpty())
                apiHost = await this.prompt.GetStringAsync("API Host", token, Constants.DefaultApiHost);

            if (userName.IsEmpty())
                userName = await this.prompt.GetStringAsync("Username", token);

            if (password.IsEmpty())
                password = await this.prompt.GetMaskedStringAsync("Password", token);

            this.output.WriteLine();
            this.output.Write($"Saving the configuration to '{Constants.ConfigFilePath}'... ");
            this.cliConfig.Auth = new Auth
            {
                ApiHost = apiHost,
                UserName = userName,
                Password = password
            };
            await this.configurationStorage.WriteConfigAsync(this.cliConfig, token);

            this.output.WriteSuccess().WriteLine().Write($"Verifying your credentials against '{apiHost}'... ");

            var me = await this.meClient.GetMeAsync(token);

            this.output.WriteSuccess()
                .Write($" Welcome, {me.FullName}.")
                .WriteLine().WriteLine()
                .WriteGreen("Setup complete.")
                .WriteLine().WriteLine();

            return ExitCodes.Ok;
        }
    }

    public class SetupArgs
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string ApiHost { get; set; }
    }
}
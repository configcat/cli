using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Setup
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

        public string Name => "setup";

        public string Description => $"Setup the CLI with Management API host and credentials." +
            $"{System.Environment.NewLine}You can get your credentials from here: https://app.configcat.com/my-account/public-api-credentials";

        public IEnumerable<Option> Options => new[]
        {
            new Option<string>(new[] { "--api-host", "-s" }, $"The Management API host, also used from {Constants.ApiHostEnvironmentVariableName}. (default '{Constants.DefaultApiHost}')"),
            new Option<string>(new[] { "--username", "-u" }, $"The Management API basic authentication username, also used from {Constants.ApiUserNameEnvironmentVariableName}"),
            new Option<string>(new[] { "--password", "-p" }, $"The Management API basic authentication password, also used from {Constants.ApiPasswordEnvironmentVariableName}"),
        };

        public async Task<int> InvokeAsync(SetupArgs arguments, CancellationToken token)
        {
            if (arguments.ApiHost.IsEmpty())
                arguments.ApiHost = await this.prompt.GetStringAsync("API Host", token, Constants.DefaultApiHost);

            if (arguments.UserName.IsEmpty())
                arguments.UserName = await this.prompt.GetStringAsync("Username", token);

            if (arguments.Password.IsEmpty())
                arguments.Password = await this.prompt.GetMaskedStringAsync("Password", token);

            this.output.WriteLine();
            this.output.Write($"Saving the configuration to '{Constants.ConfigFilePath}'... ");
            this.cliConfig.Auth = new Auth
            {
                ApiHost = arguments.ApiHost,
                UserName = arguments.UserName,
                Password = arguments.Password
            };
            await this.configurationStorage.WriteConfigAsync(this.cliConfig, token);

            output.WriteGreen(Constants.SuccessMessage);
            output.WriteLine();
            output.Write($"Verifying your credentials against '{arguments.ApiHost}'... ");

            var me = await this.meClient.GetMeAsync(token);

            output.WriteGreen(Constants.SuccessMessage);
            output.Write($" Welcome, {me.FullName}.");
            output.WriteLine();
            output.WriteLine();
            output.WriteGreen("Setup complete.");
            output.WriteLine();
            output.WriteLine();

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

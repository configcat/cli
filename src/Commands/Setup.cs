using ConfigCat.Cli.Api.Me;
using ConfigCat.Cli.Configuration;
using ConfigCat.Cli.Utils;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Setup : IExecutableCommand<SetupArgs>
    {
        private readonly IConfigurationWriter configurationWriter;
        private readonly IPrompt prompt;
        private readonly IMeClient meClient;
        private readonly IExecutionContextAccessor accessor;

        public Setup(IConfigurationWriter configurationWriter, IPrompt prompt, IMeClient meClient, IExecutionContextAccessor accessor)
        {
            this.configurationWriter = configurationWriter;
            this.prompt = prompt;
            this.meClient = meClient;
            this.accessor = accessor;
        }

        public string Name => "setup";

        public string Description => $"Setup the CLI with Management API host and credentials." +
            $"{System.Environment.NewLine}You can get your credentials from here: https://app.configcat.com/my-account/public-api-credentials";

        public IEnumerable<Option> Options => new[]
        {
            new Option<string>(new[] { "--api-host", "-s" })
            {
                Description = $"The Management API host, also used from {Constants.ApiHostEnvironmentVariableName}. (default '{Constants.DefaultApiHost}')"
            },

            new Option<string>(new[] { "--username", "-u" })
            {
                Description = $"The Management API basic authentication username, also used from {Constants.ApiUserNameEnvironmentVariableName}"
            },

            new Option<string>(new[] { "--password", "-p" })
            {
                Description = $"The Management API basic authentication password, also used from {Constants.ApiPasswordEnvironmentVariableName}"
            },
        };

        public async Task<int> InvokeAsync(SetupArgs arguments, CancellationToken token)
        {
            if (!token.IsCancellationRequested && string.IsNullOrWhiteSpace(arguments.ApiHost))
                arguments.ApiHost = this.prompt.GetString("API Host", Constants.DefaultApiHost);

            if (!token.IsCancellationRequested && string.IsNullOrWhiteSpace(arguments.UserName))
                arguments.UserName = this.prompt.GetString("Username");

            if (!token.IsCancellationRequested && string.IsNullOrWhiteSpace(arguments.Password))
                arguments.Password = this.prompt.GetMaskedString("Password", token);

            var output = this.accessor.ExecutionContext.Output;
            output.WriteLine();
            output.Write($"Saving the configuration to '{Constants.ConfigFilePath}'... ");
            await this.configurationWriter.WriteConfigurationAsync(new CliConfig
            {
                ApiHost = arguments.ApiHost,
                UserName = arguments.UserName,
                Password = arguments.Password
            }, token);

            output.WriteGreen("Ok.");
            output.WriteLine();
            output.Write($"Verifying your credentials against '{arguments.ApiHost}'... ");

            var me = await this.meClient.GetMeAsync(token);

            output.WriteGreen("Ok.");
            output.Write($" Welcome, {me.FullName}.");
            output.WriteLine();
            output.WriteLine();
            output.WriteGreen("Setup complete.");
            output.WriteLine();
            output.WriteLine();

            return Constants.ExitCodes.Ok;
        }
    }

    public class SetupArgs
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string ApiHost { get; set; }
    }
}

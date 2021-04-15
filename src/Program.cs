using ConfigCat.Cli.Api;
using ConfigCat.Cli.Api.Config;
using ConfigCat.Cli.Api.Environment;
using ConfigCat.Cli.Api.Flag;
using ConfigCat.Cli.Api.Flag.Value;
using ConfigCat.Cli.Api.Me;
using ConfigCat.Cli.Api.Organization;
using ConfigCat.Cli.Api.Product;
using ConfigCat.Cli.Api.SdkKey;
using ConfigCat.Cli.Api.Tag;
using ConfigCat.Cli.Commands;
using ConfigCat.Cli.Configuration;
using ConfigCat.Cli.Exceptions;
using ConfigCat.Cli.Utils;
using Stashbox;
using Stashbox.Lifetime;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Net.Http;
using System.Threading.Tasks;
using Trybot;
using Trybot.Retry.Exceptions;

namespace ConfigCat.Cli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            using var container = new StashboxContainer(c => c.WithDefaultLifetime(Lifetimes.Singleton))
                .Register<ICommandDescriptor, Root>(c => c.WithName("root").WithDependencyBinding(nameof(ICommandDescriptor.SubCommands)))
                    .Register<ICommandDescriptor, Setup>(c => c.WhenDependantIs<Root>())
                    .Register<ICommandDescriptor, Commands.Workspace>(c => c.WhenDependantIs<Root>())
                    .Register<ICommandDescriptor, ListAll>(c => c.WhenDependantIs<Root>())
                    .Register<ICommandDescriptor, Product>(c => c.WhenDependantIs<Root>())
                    .Register<ICommandDescriptor, Config>(c => c.WhenDependantIs<Root>())
                    .Register<ICommandDescriptor, Commands.Environment>(c => c.WhenDependantIs<Root>())
                    .Register<ICommandDescriptor, Tag>(c => c.WhenDependantIs<Root>())

                    .Register<ICommandDescriptor, Flag>(c => c.WhenDependantIs<Root>().WithDependencyBinding(nameof(ICommandDescriptor.SubCommands)))
                        .Register<ICommandDescriptor, FlagValue>(c => c.WhenDependantIs<Flag>())
                        .Register<ICommandDescriptor, FlagTargeting>(c => c.WhenDependantIs<Flag>())
                        .Register<ICommandDescriptor, FlagPercentage>(c => c.WhenDependantIs<Flag>())

                    .Register<ICommandDescriptor, SdkKey>(c => c.WhenDependantIs<Root>())
                    .Register<ICommandDescriptor, Cat>(c => c.WhenDependantIs<Root>());

            container.Register<IConfigurationProvider, ConfigurationProvider>();
            container.Register<IConfigurationStorage, ConfigurationFileStorage>();
            container.Register<IWorkspaceManager, WorkspaceManager>();
            container.Register<IMeClient, MeClient>();
            container.Register<IProductClient, ProductClient>();
            container.Register<IConfigClient, ConfigClient>();
            container.Register<IEnvironmentClient, EnvironmentClient>();
            container.Register<ISdkKeyClient, SdkKeyClient>();
            container.Register<IOrganizationClient, OrganizationClient>();
            container.Register<ITagClient, TagClient>();
            container.Register<IFlagClient, FlagClient>();
            container.Register<IFlagValueClient, FlagValueClient>();
            container.Register<IExecutionContextAccessor, ExecutionContextAccessor>();
            container.Register<ExecutionContext>();
            container.Register<IPrompt, Prompt>();
            container.Register(typeof(IBotPolicy<>), typeof(BotPolicy<>), c => c.WithTransientLifetime());
            container.RegisterInstance(new HttpClient());

            var r = container.Resolve<ICommandDescriptor>("root");
            var root = new RootCommand(r.Description);
            root.AddGlobalOption(Constants.VerboseOption);
            root.Configure(r.SubCommands, r.InlineSubCommands);

            var parser = new CommandLineBuilder(root)
                .UseMiddleware(async (context, next) =>
                {
                    var hasVerboseOption = context.ParseResult.FindResultFor(Constants.VerboseOption) is not null;
                    container.RegisterInstance(context.Console);
                    container.Register<IOutput, Output>(c => c.WithFactory<IConsole>(console => new Output(console, hasVerboseOption)));
                    await next(context);
                })
                .UseMiddleware(async (context, next) =>
                {
                    var executionContext = container.Resolve<ExecutionContext>();
                    var configurationProvider = container.Resolve<IConfigurationProvider>();
                    var config = await configurationProvider.GetConfigAsync(context.GetCancellationToken());
                    executionContext.Config.Auth = config.Auth;
                    executionContext.Config.Workspace = config.Workspace;
                    await next(context);
                })
                .UseVersionOption()
                .UseHelp()
                .UseTypoCorrections()
                .UseParseErrorReporting()
                .UseExceptionHandler(ExceptionHandler)
                .CancelOnProcessTermination()
                .Build();

            return await parser.InvokeAsync(args);
        }

        private static void ExceptionHandler(Exception exception, InvocationContext context)
        {
            var hasVerboseOption = context.ParseResult.FindResultFor(Constants.VerboseOption) is not null;

            if (exception is OperationCanceledException || exception is TaskCanceledException)
                context.Console.WriteErrorOnTerminal("Terminated.");
            else if (exception is HttpStatusException statusException)
                context.Console.WriteErrorOnTerminal($"Http request failed: {(int)statusException.StatusCode} {statusException.ReasonPhrase}.");
            else if (exception is MaxRetryAttemptsReachedException retryException)
            {
                if (retryException.OperationResult is HttpResponseMessage response)
                    context.Console.WriteErrorOnTerminal($"Http request failed: {(int)response.StatusCode} {response.ReasonPhrase}.");
                else if (retryException.InnerException is not null)
                    context.Console.WriteErrorOnTerminal(hasVerboseOption ? retryException.InnerException.ToString() : retryException.InnerException.Message);
                else
                    context.Console.WriteErrorOnTerminal(hasVerboseOption ? retryException.ToString() : retryException.Message);
            }
            else if(exception is ShowHelpException misconfigurationException)
            {
                context.Console.WriteErrorOnTerminal(misconfigurationException.Message);
                context.Console.Error.WriteLine();
                context.InvocationResult = new HelpResult();
            }
            else
                context.Console.WriteErrorOnTerminal(hasVerboseOption ? exception.ToString() : exception.Message);

            context.ResultCode = Constants.ExitCodes.Error;
        }
    }

    static class CommandExtensions
    {
        public static void Configure(this Command command,
            IEnumerable<ICommandDescriptor> commandDescriptors,
            IEnumerable<SubCommandDescriptor> inlineSubCommands)
        {
            foreach (var subCommandDescriptor in inlineSubCommands)
            {
                var inlineSubCommand = new Command(subCommandDescriptor.Name, subCommandDescriptor.Description);

                foreach (var option in subCommandDescriptor.Options)
                    inlineSubCommand.AddOption(option);

                foreach (var argument in subCommandDescriptor.Arguments)
                    inlineSubCommand.AddArgument(argument);

                foreach (var alias in subCommandDescriptor.Aliases)
                    inlineSubCommand.AddAlias(alias);

                inlineSubCommand.TreatUnmatchedTokensAsErrors = true;
                inlineSubCommand.Handler = subCommandDescriptor.Handler;
                command.AddCommand(inlineSubCommand);
            }

            foreach (var commandDescriptor in commandDescriptors)
            {
                var subCommand = new Command(commandDescriptor.Name, commandDescriptor.Description);

                foreach (var option in commandDescriptor.Options)
                    subCommand.AddOption(option);

                foreach (var argument in commandDescriptor.Arguments)
                    subCommand.AddArgument(argument);

                foreach (var alias in commandDescriptor.Aliases)
                    subCommand.AddAlias(alias);

                subCommand.TreatUnmatchedTokensAsErrors = true;
                subCommand.Configure(commandDescriptor.SubCommands, commandDescriptor.InlineSubCommands);

                var method = commandDescriptor.GetType().GetMethod(nameof(IExecutableCommand.InvokeAsync));
                if (method is not null)
                    subCommand.Handler = CommandHandler.Create(method, commandDescriptor);

                command.AddCommand(subCommand);
            }
        }
    }
}

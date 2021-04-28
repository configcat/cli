using ConfigCat.Cli.Commands;
using ConfigCat.Cli.Options;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Configuration;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;
using Stashbox;
using Stashbox.Configuration;
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
using Trybot.Timeout.Exceptions;

namespace ConfigCat.Cli
{
    class Program
    {
        static Option VerboseOption = new VerboseOption();

        static async Task<int> Main(string[] args)
        {
            await using var container = new StashboxContainer(c => c.WithDefaultLifetime(Lifetimes.Singleton));

            container.RegisterAssemblyContaining<ApiClient>(
                type => type != typeof(Output),
                serviceTypeSelector: Rules.ServiceRegistrationFilters.Interfaces,
                registerSelf: false);

            container.Register(typeof(IBotPolicy<>), typeof(BotPolicy<>), c => c.WithTransientLifetime());
            container.RegisterInstance(new HttpClient());

            var root = CommandTree.Build();
            var rootCommand = new RootCommand(root.Description);
            rootCommand.AddGlobalOption(VerboseOption);
            rootCommand.Configure(root.SubCommands, container);

            var parser = new CommandLineBuilder(rootCommand)
                .UseMiddleware(async (context, next) =>
                {
                    var hasVerboseOption = context.ParseResult.FindResultFor(VerboseOption) is not null;
                    container.RegisterInstance(context.Console);
                    container.Register<IOutput, Output>(c => c.WithFactory<IConsole>(console => new Output(console, hasVerboseOption)));
                    await next(context);
                })
                .UseMiddleware(async (context, next) =>
                {
                    if (context.ParseResult.CommandResult.Command.Name == "setup")
                    {
                        await next(context);
                        return;
                    }

                    var configurationProvider = container.Resolve<IConfigurationProvider>();
                    var config = await configurationProvider.GetConfigAsync(context.GetCancellationToken());
                    container.RegisterInstance(config);
                    await next(context);
                })
                .UseMiddleware(async (context, next) =>
                {
                    var descriptor = container.Resolve<HandlerDescriptor>(context.ParseResult.CommandResult.Command.GetHashCode(), nullResultAllowed: true);
                    if (descriptor is not null)
                        context.BindingContext.AddService(descriptor.HandlerType,
                            c => container.Resolve(descriptor.HandlerType));

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
            var hasVerboseOption = context.ParseResult.FindResultFor(VerboseOption) is not null;

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
            else if (exception is OperationTimeoutException)
                context.Console.WriteErrorOnTerminal("Operation timed out.");
            else if (exception is ShowHelpException misconfigurationException)
            {
                context.Console.WriteErrorOnTerminal(misconfigurationException.Message);
                context.Console.Error.WriteLine();
                context.InvocationResult = new HelpResult();
            }
            else
                context.Console.WriteErrorOnTerminal(hasVerboseOption ? exception.ToString() : exception.Message);

            context.ExitCode = ExitCodes.Error;
        }
    }

    static class CommandExtensions
    {
        public static void Configure(this Command command,
            IEnumerable<CommandDescriptor> commandDescriptors,
            IDependencyRegistrator registrator)
        {
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
                subCommand.Configure(commandDescriptor.SubCommands, registrator);

                if (commandDescriptor.Handler is not null)
                {
                    registrator.Register(commandDescriptor.Handler.HandlerType);
                    registrator.RegisterInstance(commandDescriptor.Handler, subCommand.GetHashCode());
                    subCommand.Handler = CommandHandler.Create(commandDescriptor.Handler.Method);
                }

                command.AddCommand(subCommand);
            }
        }
    }
}

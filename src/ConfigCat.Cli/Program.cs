using ConfigCat.Cli.Models;
using ConfigCat.Cli.Models.Configuration;
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
        static async Task<int> Main(string[] args)
        {
            await using var container = new StashboxContainer(c => c.WithDefaultLifetime(Lifetimes.Singleton));

            container.RegisterAssemblyContaining<ApiClient>(
                serviceTypeSelector: Rules.ServiceRegistrationFilters.Interfaces,
                registerSelf: false);

            container.Register(typeof(IBotPolicy<>), typeof(BotPolicy<>), c => c.WithTransientLifetime());
            container.RegisterInstance(new HttpClient());

            var parser = new CommandLineBuilder(CommandBuilder.BuildRootCommand(container))
                .UseMiddleware(async (context, next) =>
                {
                    var hasVerboseOption = context.ParseResult.FindResultFor(CommandBuilder.VerboseOption) is not null;
                    var hasJsonOption = context.ParseResult.FindResultFor(CommandBuilder.JsonOutputOption) is not null;
                    container.RegisterInstance(context.Console);
                    container.RegisterInstance(new CliOptions { IsVerboseEnabled = hasVerboseOption, IsJsonOutputEnabled = hasJsonOption });
                    await next(context);
                })
                .UseMiddleware(async (context, next) =>
                {
                    var commandName = context.ParseResult.CommandResult.Command.Name;
                    if (commandName == "setup" || commandName == "whoisthebestcat")
                    {
                        container.RegisterInstance(new CliConfig());
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
                .UseHelpBuilder(ctx =>
                {
                    int maxWidth = int.MaxValue;
                    if (ctx.Console is SystemConsole systemConsole)
                        maxWidth = systemConsole.GetWindowWidth();

                    return new ExtendedHelpBuilder(ctx.Console, maxWidth);
                })
                .UseTypoCorrections()
                .UseParseErrorReporting()
                .UseExceptionHandler(ExceptionHandler)
                .CancelOnProcessTermination()
                .Build();

            return await parser.InvokeAsync(args);
        }

        private static void ExceptionHandler(Exception exception, InvocationContext context)
        {
            var hasVerboseOption = context.ParseResult.FindResultFor(CommandBuilder.VerboseOption) is not null;

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
}

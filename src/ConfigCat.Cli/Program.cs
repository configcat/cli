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
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Trybot;
using Trybot.Retry.Exceptions;
using Trybot.Timeout.Exceptions;

namespace ConfigCat.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        await using var container = new StashboxContainer(c => c.WithDefaultLifetime(Lifetimes.Singleton));

        container.RegisterAssemblyContaining<ApiClient>(
            serviceTypeSelector: Rules.ServiceRegistrationFilters.Interfaces,
            registerSelf: false);

        RegisterWithDynamicMemberAccess(typeof(IBotPolicy<>), typeof(BotPolicy<>), container);
        container.RegisterInstance(new HttpClient());

        var parser = new CommandLineBuilder(CommandBuilder.BuildRootCommand(container))
            .UseMiddleware(async (context, next) =>
            {
                var verboseResult = context.ParseResult.FindResultFor(CommandBuilder.VerboseOption);
                var nonInteractiveResult = context.ParseResult.FindResultFor(CommandBuilder.NonInteractiveOption);
                var hasVerboseOption = verboseResult?.GetValueOrDefault<bool>() ?? false;
                var hasNonInteractiveOption = nonInteractiveResult?.GetValueOrDefault<bool>() ?? false;
                container.RegisterInstance(new CliOptions { IsVerboseEnabled = hasVerboseOption, IsNonInteractive = hasNonInteractiveOption });
                await next(context);
            })
            .UseMiddleware(async (context, next) =>
            {
                var commandName = context.ParseResult.CommandResult.Command.Name;
                switch (commandName)
                {
                    case "setup" or "whoisthebestcat":
                        container.RegisterInstance(new CliConfig());
                        break;
                    case { } when context.ParseResult.CommandResult.Parent is CommandResult { Command.Name: "config-json" }:
                        if (commandName == "convert")
                        {
                            container.Register<RandomNumberGenerator>(cfg => cfg
                                .WithFactory(() => RandomNumberGenerator.Create())
                                .WithSingletonLifetime());
                        }
                        break;
                    default:
                        var configurationProvider = container.Resolve<IConfigurationProvider>();
                        var config = await configurationProvider.GetConfigAsync(context.GetCancellationToken());
                        container.RegisterInstance(config);
                        break;
                }
                await next(context);
            })
            .UseMiddleware(async (context, next) =>
            {
                var descriptor = container.ResolveOrDefault<HandlerDescriptor>(name: context.ParseResult.CommandResult.Command.GetHashCode());
                if (descriptor is not null)
                    context.BindingContext.AddService(descriptor.HandlerType,
                        c => container.Resolve(descriptor.HandlerType));

                await next(context);
            })
            .UseVersionOption()
            .UseHelp()
            .UseHelpBuilder(ctx =>
            {
                var maxWidth = int.MaxValue;
                if (ctx.Console is SystemConsole systemConsole)
                    maxWidth = systemConsole.GetWindowWidth();

                return new ExtendedHelpBuilder(ctx.Console, maxWidth);
            })
            .UseTypoCorrections()
            .UseParseErrorReporting()
            .UseExceptionHandler((exception, context) =>
            {
                exception = UnwrapTargetInvocationException(exception);
                
                var hasVerboseOption = context.ParseResult.FindResultFor(CommandBuilder.VerboseOption) is not null;
                var output = container.Resolve<IOutput>();
                switch (exception)
                {
                    case OperationCanceledException or TaskCanceledException:
                        output.WriteError("Terminated.");
                        break;
                    case HttpStatusException statusException:
                        output.WriteError($"Http request failed: {(int)statusException.StatusCode} {statusException.ReasonPhrase}.");
                        break;
                    case MaxRetryAttemptsReachedException { OperationResult: HttpResponseMessage response }:
                        output.WriteError($"Http request failed: {(int)response.StatusCode} {response.ReasonPhrase}.");
                        break;
                    case MaxRetryAttemptsReachedException { InnerException: { } } retryException:
                        output.WriteError(hasVerboseOption ? retryException.InnerException.ToString() : retryException.InnerException.Message);
                        break;
                    case MaxRetryAttemptsReachedException retryException:
                        output.WriteError(hasVerboseOption ? retryException.ToString() : retryException.Message);
                        break;
                    case OperationTimeoutException:
                        output.WriteError("Operation timed out.");
                        break;
                    case ShowHelpException misconfigurationException:
                        output.WriteError(misconfigurationException.Message);
                        context.Console.Error.WriteLine();
                        context.InvocationResult = new HelpResult();
                        break;
                    default:
                        output.WriteError(hasVerboseOption ? exception.ToString() : exception.Message);
                        break;
                }

                context.ExitCode = ExitCodes.Error;
            })
            .CancelOnProcessTermination()
            .Build();

        return await parser.InvokeAsync(args);
    }

    private static void RegisterWithDynamicMemberAccess(Type serviceType, 
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType, 
        StashboxContainer container)
    {
        container.Register(serviceType, implementationType, c => c.WithTransientLifetime());
    }

    private static Exception UnwrapTargetInvocationException(Exception ex)
    {
        while (ex is TargetInvocationException { InnerException: not null })
        {
            ex = ex.InnerException;
        }
        return ex;
    }
}
using ConfigCat.Cli;
using Stashbox;
using System.Collections.Generic;
using System.CommandLine.Invocation;

namespace System.CommandLine;

public static class CommandExtensions
{
    public static void Configure(this Command command,
        IEnumerable<CommandDescriptor> commandDescriptors,
        IDependencyRegistrator registrator = null)
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
            subCommand.IsHidden = commandDescriptor.IsHidden;
            subCommand.Configure(commandDescriptor.SubCommands, registrator);

            if (commandDescriptor.Handler is not null && registrator is not null)
            {
                registrator.Register(commandDescriptor.Handler.HandlerType);
                registrator.RegisterInstance(commandDescriptor.Handler, subCommand.GetHashCode());
                subCommand.Handler = CommandHandler.Create(commandDescriptor.Handler.Method);
            }

            command.AddCommand(subCommand);
        }
    }
}
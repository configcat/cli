using System.CommandLine.Invocation;

namespace ConfigCat.Cli.Commands
{
    static class CommandHandlerExtensions
    {
        public static ICommandHandler CreateHandler(this ICommandDescriptor obj, string methodName) =>
            CommandHandler.Create(obj.GetType().GetMethod(methodName), obj);
    }
}

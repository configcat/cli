using System.CommandLine.Invocation;

namespace ConfigCat.Cli.Commands
{
    static class CommandHandlerExtensions
    {
        public static ICommandHandler CreateHandler(this CommandDescriptor obj, string methodName) =>
            CommandHandler.Create(obj.GetType().GetMethod(methodName), obj);
    }
}

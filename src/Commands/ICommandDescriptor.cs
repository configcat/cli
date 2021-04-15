using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    interface IExecutableCommand<TArguments> : ICommandDescriptor
    {
        Task<int> InvokeAsync(TArguments arguments, CancellationToken token);
    }

    interface IExecutableCommand : ICommandDescriptor
    {
        Task<int> InvokeAsync(CancellationToken token);
    }

    class SubCommandDescriptor
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<Option> Options { get; set; } = Enumerable.Empty<Option>();

        public IEnumerable<Argument> Arguments { get; set; } = Enumerable.Empty<Argument>();

        public IEnumerable<string> Aliases { get; set; } = Enumerable.Empty<string>();

        public ICommandHandler Handler { get; set; }
    }

    interface ICommandDescriptor
    {
        string Name { get; }

        string Description { get; }

        IEnumerable<Option> Options => Enumerable.Empty<Option>();

        IEnumerable<Argument> Arguments => Enumerable.Empty<Argument>();

        IEnumerable<string> Aliases => Enumerable.Empty<string>();

        public IEnumerable<SubCommandDescriptor> InlineSubCommands => Enumerable.Empty<SubCommandDescriptor>();

        public IEnumerable<ICommandDescriptor> SubCommands => Enumerable.Empty<ICommandDescriptor>();
    }
}

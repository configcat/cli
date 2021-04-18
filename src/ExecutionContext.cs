using ConfigCat.Cli.Configuration;
using ConfigCat.Cli.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli
{
    class ExecutionContext
    {
        public IOutput Output { get; }

        public CliConfig Config { get; } = new CliConfig();

        public ExecutionContext(IOutput output)
        {
            this.Output = output;
        }
    }

    interface IExecutionContextAccessor
    {
        ExecutionContext ExecutionContext { get; }
    }

    class ExecutionContextAccessor : IExecutionContextAccessor
    {
        private readonly Lazy<ExecutionContext> executionContext;

        public ExecutionContext ExecutionContext => this.executionContext.Value;

        public ExecutionContextAccessor(Func<ExecutionContext> executionContextFunc)
        {
            this.executionContext = new Lazy<ExecutionContext>(executionContextFunc);
        }
    }
}

using ConfigCat.Cli.Configuration;
using ConfigCat.Cli.Utils;
using System;

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
        private readonly IServiceProvider serviceProvider;

        public ExecutionContext ExecutionContext => (ExecutionContext)this.serviceProvider.GetService(typeof(ExecutionContext));

        public ExecutionContextAccessor(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
    }
}

using ConfigCat.Cli.Utils;
using System;

namespace ConfigCat.Cli
{
    class ExecutionContext
    {
        public IOutput Output { get; set; }
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

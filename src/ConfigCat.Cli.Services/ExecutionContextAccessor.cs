using System;

namespace ConfigCat.Cli.Services
{
    public interface IExecutionContextAccessor
    {
        ExecutionContext ExecutionContext { get; }
    }

    public class ExecutionContextAccessor : IExecutionContextAccessor
    {
        private readonly Lazy<ExecutionContext> executionContext;

        public ExecutionContext ExecutionContext => this.executionContext.Value;

        public ExecutionContextAccessor(Func<ExecutionContext> executionContextFunc)
        {
            this.executionContext = new Lazy<ExecutionContext>(executionContextFunc);
        }
    }
}

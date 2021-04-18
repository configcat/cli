using System;

namespace ConfigCat.Cli.Services
{
    public interface IExecutionContextAccessor
    {
        IExecutionContext ExecutionContext { get; }
    }

    public class ExecutionContextAccessor : IExecutionContextAccessor
    {
        private readonly Lazy<IExecutionContext> executionContext;

        public IExecutionContext ExecutionContext => this.executionContext.Value;

        public ExecutionContextAccessor(Func<IExecutionContext> executionContextFunc)
        {
            this.executionContext = new Lazy<IExecutionContext>(executionContextFunc);
        }
    }
}

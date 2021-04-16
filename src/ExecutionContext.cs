using ConfigCat.Cli.Configuration;
using ConfigCat.Cli.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli
{
    class ExecutionContext
    {
        private readonly IConfigurationStorage configurationStorage;

        public IOutput Output { get; }

        public CliConfig Config { get; } = new CliConfig();

        public ExecutionContext(IOutput output, IConfigurationStorage configurationStorage)
        {
            this.Output = output;
            this.configurationStorage = configurationStorage;
        }

        public Task SaveConfigAsync(CancellationToken token) =>
            this.configurationStorage.WriteConfigAsync(this.Config, token);
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

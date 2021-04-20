using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Services
{
    public class ExecutionContext
    {
        public IOutput Output { get; }

        public CliConfig Config { get; } = new CliConfig();

        public ExecutionContext(IOutput output)
        {
            this.Output = output;
        }
    }
}

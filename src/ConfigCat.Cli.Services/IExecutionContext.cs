using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Services
{
    public interface IExecutionContext
    {
        public IOutput Output { get; }

        public CliConfig Config { get; }
    }
}

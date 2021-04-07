using ConfigCat.Cli.Commands;
using System.Collections.Generic;

namespace ConfigCat.Cli
{
    class Root : ICommandDescriptor
    {
        public IEnumerable<ICommandDescriptor> SubCommands { get; set; }

        public string Name => null;

        public string Description => $"This is the Command Line Tool of ConfigCat.{System.Environment.NewLine}ConfigCat is a hosted feature flag service: https://configcat.com{System.Environment.NewLine}For more information, see the documentation here: https://configcat.com/docs/";
    }
}

using Moq;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;

namespace ConfigCat.Cli.DocGenerator
{
    class ExposedHelpBuilder : HelpBuilder
    {
        public ExposedHelpBuilder() : base(new Mock<IConsole>().Object, int.MaxValue)
        { }

        public string ExposeGetUsage(ICommand command) => base.GetUsage(command);

        public IEnumerable<HelpItem> ExposeGetCommandArguments(ICommand command) => base.GetCommandArguments(command);
    }
}

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.IO;

namespace ConfigCat.Cli.DocGenerator;

class ExposedHelpBuilder : HelpBuilder
{
    public ExposedHelpBuilder() : base(new MockConsole(), int.MaxValue)
    { }

    public string ExposeGetUsage(ICommand command) => base.GetUsage(command);

    public IEnumerable<HelpItem> ExposeGetCommandArguments(ICommand command) => base.GetCommandArguments(command);
}

class MockConsole : IConsole
{
    public IStandardStreamWriter Out => throw new System.NotImplementedException();

    public bool IsOutputRedirected => throw new System.NotImplementedException();

    public IStandardStreamWriter Error => throw new System.NotImplementedException();

    public bool IsErrorRedirected => throw new System.NotImplementedException();

    public bool IsInputRedirected => throw new System.NotImplementedException();
}
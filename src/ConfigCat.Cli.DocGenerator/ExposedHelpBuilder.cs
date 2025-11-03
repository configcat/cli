using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.IO;

namespace ConfigCat.Cli.DocGenerator;

internal class ExposedHelpBuilder() : HelpBuilder(new MockConsole())
{
    public string ExposeGetUsage(ICommand command)
    { 
        var description = GetUsage(command);
        if (description.Contains("[options]"))
        {
            description = description.Replace("[options] ", string.Empty) + " [options]";
        }

        return description;
    }
    
    public string ExposeExample(ICommand command)
    { 
        if (command is not ExtendedCommand extendedCommand || string.IsNullOrWhiteSpace(extendedCommand.Example)) return null;
        return extendedCommand.Example;
    }

    public IEnumerable<HelpItem> ExposeGetCommandArguments(ICommand command) => base.GetCommandArguments(command);
}

internal class MockConsole : IConsole
{
    public IStandardStreamWriter Out => throw new System.NotImplementedException();

    public bool IsOutputRedirected => throw new System.NotImplementedException();

    public IStandardStreamWriter Error => throw new System.NotImplementedException();

    public bool IsErrorRedirected => throw new System.NotImplementedException();

    public bool IsInputRedirected => throw new System.NotImplementedException();
}
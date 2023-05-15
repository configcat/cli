using System.CommandLine;

namespace ConfigCat.Cli;

public class ExtendedCommand : Command
{
    public string Example { get; set; }

    public ExtendedCommand(string name, string description = null, string example = null) : base(name, description)
    {
        this.Example = example;
    }
}
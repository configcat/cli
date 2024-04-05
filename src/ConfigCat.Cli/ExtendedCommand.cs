using System.CommandLine;

namespace ConfigCat.Cli;

public class ExtendedCommand(string name, string description = null, string example = null)
    : Command(name, description)
{
    public string Example { get; set; } = example;
}
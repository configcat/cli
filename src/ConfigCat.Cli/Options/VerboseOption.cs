using System.CommandLine;

namespace ConfigCat.Cli.Options;

internal class VerboseOption() : Option<bool>([
    "--verbose",
    "-v",
    "/v"
], "Print detailed execution information")
{
    public override bool Equals(object obj)
    {
        return obj is VerboseOption;
    }

    public override int GetHashCode()
    {
        return typeof(VerboseOption).GetHashCode();
    }
}
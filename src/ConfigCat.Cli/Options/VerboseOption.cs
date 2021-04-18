using System.CommandLine;

namespace ConfigCat.Cli.Options
{
    class VerboseOption : Option
    {
        public VerboseOption() : base(new []
        {
            "--verbose",
            "-v",
            "/v",
        }, "Print detailed execution information")
        { }

        public override bool Equals(object obj)
        {
            return obj is VerboseOption;
        }

        public override int GetHashCode()
        {
            return typeof(VerboseOption).GetHashCode();
        }
    }
}

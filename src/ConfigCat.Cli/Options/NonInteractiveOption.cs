using System.CommandLine;

namespace ConfigCat.Cli.Options
{
    internal class NonInteractiveOption : Option<bool>
    {
        public NonInteractiveOption() : base(new[]
        {
            "--non-interactive",
            "-ni"
        }, "Turn off progress rendering and interactive features.")
        { }

        public override bool Equals(object obj)
        {
            return obj is NonInteractiveOption;
        }

        public override int GetHashCode()
        {
            return typeof(NonInteractiveOption).GetHashCode();
        }
    }
}

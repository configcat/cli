using System.CommandLine;

namespace ConfigCat.Cli.Options
{
    class JsonOutputOption : Option
    {
        public JsonOutputOption() : base(new[]
        {
            "--json",
        }, "Format the output in JSON")
        { }

        public override bool Equals(object obj)
        {
            return obj is JsonOutputOption;
        }

        public override int GetHashCode()
        {
            return typeof(JsonOutputOption).GetHashCode();
        }
    }
}

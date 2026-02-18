using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace ConfigCat.Cli.Options;

internal class FlagPredefinedVariationOption : Option<PredefinedVariationOption[]>
{
    public FlagPredefinedVariationOption() : base(["--predefined-variations", "-pv"], argumentResult =>
    {
        var length = argumentResult.Tokens.Count;
        if (length == 0)
            return [];

        var result = new PredefinedVariationOption[length];
        for (var i = 0; i < length; i++)
        {
            var expression = argumentResult.Tokens.ElementAt(i).Value;
            var indexOfSeparator = expression.IndexOf(':');
            if (indexOfSeparator == -1)
            {
                var value = expression;
                if (value.IsEmpty())
                {
                    argumentResult.ErrorMessage = $"The <value> part of the expression `{expression}` is invalid.";
                    return null;
                }

                result[i] = new PredefinedVariationOption { Value = value };
            }
            else
            {
                var name = expression[..indexOfSeparator];
                var value = expression[(indexOfSeparator + 1)..];

                if (name.IsEmpty())
                {
                    argumentResult.ErrorMessage = $"The <name> part of the expression `{expression}` is invalid.";
                    return null;
                }

                if (value.IsEmpty())
                {
                    argumentResult.ErrorMessage = $"The <value> part of the expression `{expression}` is invalid.";
                    return null;
                }

                result[i] = new PredefinedVariationOption { Name = name, Value = value };
            }
        }

        return result;
    }, false, "Predefined variations of the Feature Flag or Setting. Format: `<value>` or `<name>:<value>`.")
    {
    }
}

internal class PredefinedVariationOption
{
    public string Name { get; init; }

    public string Value { get; init; }
}
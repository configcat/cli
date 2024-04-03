using System;
using System.CommandLine;
using System.Linq;
using ConfigCat.Cli.Models.Api;

namespace ConfigCat.Cli.Options;

internal class ComparisonValueOption : Option<ComparisonValueModel>
{
    public ComparisonValueOption() : base(["--comparison-value", "-cv"], argumentResult =>
    {
        var length = argumentResult.Tokens.Count;
        switch (length)
        {
            case 0:
                return null;
            case > 1:
            {
                var result = new ComparisonValueListModel[length];
                for (var i = 0; i < length; i++)
                {
                    var expression = argumentResult.Tokens.ElementAt(i).Value;
                    var indexOfSeparator = expression.IndexOf(':');
                    if (indexOfSeparator == -1)
                    {
                        argumentResult.ErrorMessage = $"The expression `{expression}` is invalid. Required format: <value>:<hint>";
                        return null;
                    }

                    var value = expression[..indexOfSeparator];
                    var hint = expression[(indexOfSeparator + 1)..];

                    if (value.IsEmpty())
                    {
                        argumentResult.ErrorMessage = $"The <value> part of the expression `{expression}` is invalid.";
                        return null;
                    }

                    result[i] = new ComparisonValueListModel { Value = value, Hint = hint };
                }
                return new ComparisonValueModel { ListValue = result };
            }
            default:
            {
                var stringVal = argumentResult.Tokens[0].Value;
                if (stringVal.IsEmpty()) return null;
                return double.TryParse(stringVal, out var doubleVal) ? new ComparisonValueModel { DoubleValue = doubleVal } : new ComparisonValueModel { StringValue = stringVal };
            }
        }
    }, false, "The value that the User Object attribute is compared to. Can be a double, string, or value-hint list in the format: `<value>:<hint>`")
    { }
}
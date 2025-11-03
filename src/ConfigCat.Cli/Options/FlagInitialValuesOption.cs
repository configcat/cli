using System;
using System.CommandLine;
using System.Linq;

namespace ConfigCat.Cli.Options;

internal sealed class FlagInitialValuesOption : Option<InitialValueOption[]>
{
    public FlagInitialValuesOption() : base(["--init-values-per-environment", "-ive"], argumentResult =>
    {
        var length = argumentResult.Tokens.Count;
        if (length == 0)
            return [];

        var result = new InitialValueOption[length];
        for (var i = 0; i < length; i++)
        {
            var expression = argumentResult.Tokens.ElementAt(i).Value;
            var indexOfSeparator = expression.IndexOf(':');
            if (indexOfSeparator == -1)
            {
                argumentResult.ErrorMessage = $"The expression `{expression}` is invalid. Required format: <environment-id>:<initial-value>";
                return null;
            }

            var environmentId = expression[..indexOfSeparator];
            var initValue = expression[(indexOfSeparator + 1)..];

            if (!Guid.TryParse(environmentId, out _))
            {
                argumentResult.ErrorMessage = $"The <environment-id> part of the expression `{expression}` is not a valid GUID.";
                return null;
            }

            if (initValue.IsEmpty())
            {
                argumentResult.ErrorMessage = $"The <initial-value> part of the expression `{expression}` is invalid.";
                return null;
            }

            result[i] = new InitialValueOption { EnvironmentId = environmentId, Value = initValue };
        }

        return result;
    }, false, "Initial value for specific Environments. Format: `<environment-id>:<initial-value>`")
    { }
}

internal class InitialValueOption
{
    public string EnvironmentId { get; set; }

    public string Value { get; set; }
}
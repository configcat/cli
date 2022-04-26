using System;
using System.CommandLine;
using System.Linq;

namespace ConfigCat.Cli.Options;

internal sealed class PercentageRuleArgument : Argument<UpdatePercentageModel[]>
{
    public PercentageRuleArgument() : base(argumentResult =>
    {
        var length = argumentResult.Tokens.Count;
        if (length == 0)
            return Array.Empty<UpdatePercentageModel>();

        var result = new UpdatePercentageModel[length];
        for (var i = 0; i < length; i++)
        {
            var value = argumentResult.Tokens.ElementAt(i).Value;
            var indexOfSeparator = value.IndexOf(':');
            if (indexOfSeparator == -1)
            {
                argumentResult.ErrorMessage = $"The expression `{value}` is invalid. Argument format must be: <percentage>:<value> e.g., 30:true 70:false";
                return null;
            }

            var percentageString = value[..indexOfSeparator];
            var valueString = value[(indexOfSeparator + 1)..];

            if (!int.TryParse(percentageString, out var percentage))
            {
                argumentResult.ErrorMessage = $"The <percentage> part of the expression `{value}` is not a number. Argument format must be: <percentage>:<value> e.g., 30:true 70:false";
                return null;
            }

            if (percentage < 0)
            {
                argumentResult.ErrorMessage = $"Percentage must be a non-negative number.";
                return null;
            }

            if (valueString.IsEmpty())
            {
                argumentResult.ErrorMessage = $"The <value> part of the expression `{value}` is empty. Argument format must be: <percentage>:<value> e.g., 30:true 70:false";
                return null;
            }

            result[i] = new UpdatePercentageModel { Percentage = percentage, Value = valueString };
        }

        var sum = result.Sum(m => m.Percentage);
        if (sum != 100)
        {
            argumentResult.ErrorMessage = $"The sum of the percentages must be 100.";
            return null;
        }

        if (result.Length != 1) return result;
        argumentResult.ErrorMessage = $"There must be at least 2 rules.";
        return null;

    })
    {
        Name = "rules";
        Description = "Format: `<percentage>:<value>`, e.g., `30:true 70:false`";
    }

    public override bool Equals(object obj)
    {
        return obj is PercentageRuleArgument;
    }

    public override int GetHashCode()
    {
        return typeof(PercentageRuleArgument).GetHashCode();
    }
}

internal class UpdatePercentageModel
{
    public int Percentage { get; init; }

    public string Value { get; init; }
}
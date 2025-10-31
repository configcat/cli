using System;
using System.CommandLine;
using System.Linq;

namespace ConfigCat.Cli.Options;

internal sealed class UserAttributeOption : Option<UserAttributeModel[]>
{
    public UserAttributeOption() : base(["-user-attributes", "-ua"], argumentResult =>
    {
        var length = argumentResult.Tokens.Count;
        if (length == 0)
            return [];

        var result = new UserAttributeModel[length];
        for (var i = 0; i < length; i++)
        {
            var expression = argumentResult.Tokens.ElementAt(i).Value;
            var indexOfSeparator = expression.IndexOf(':');
            if (indexOfSeparator == -1)
            {
                argumentResult.ErrorMessage = $"The expression `{expression}` is invalid. Required format: <key>:<value>";
                return null;
            }

            var key = expression[..indexOfSeparator];
            var value = expression[(indexOfSeparator + 1)..];

            if (key.IsEmpty())
            {
                argumentResult.ErrorMessage = $"The <key> part of the expression `{expression}` is invalid.";
                return null;
            }

            if (value.IsEmpty())
            {
                argumentResult.ErrorMessage = $"The <value> part of the expression `{expression}` is invalid.";
                return null;
            }

            result[i] = new UserAttributeModel { Key = key, Value = value };
        }

        return result;
    }, false, "User attributes for flag evaluation. Format: `<key>:<value>`. Dedicated User Object attributes are mapped like the following: Identifier => id, Email => email, Country => country")
    { }
}

internal class UserAttributeModel
{
    public string Key { get; init; }

    public object Value { get; init; }
}
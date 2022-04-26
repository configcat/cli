using ConfigCat.Cli.Services;
using System;
using System.CommandLine;
using System.Linq;

namespace ConfigCat.Cli.Options;

internal sealed class PermissionGroupEnvironmentAccessOption : Option<EnvironmentSpecificAccess[]>
{
    public PermissionGroupEnvironmentAccessOption() : base(new string[] { "--environment-specific-access-types", "-esat" }, argumentResult =>
    {
        var length = argumentResult.Tokens.Count;
        if (length == 0)
            return Array.Empty<EnvironmentSpecificAccess>();

        var result = new EnvironmentSpecificAccess[length];
        for (var i = 0; i < length; i++)
        {
            var value = argumentResult.Tokens.ElementAt(i).Value;
            var indexOfSeparator = value.IndexOf(':');
            if (indexOfSeparator == -1)
            {
                argumentResult.ErrorMessage = $"The expression `{value}` is invalid. Required format: <environment-id>:<access-type>";
                return null;
            }

            var environmentId = value[..indexOfSeparator];
            var accessType = value[(indexOfSeparator + 1)..];

            if (!Guid.TryParse(environmentId, out var parsedEnvironmentId))
            {
                argumentResult.ErrorMessage = $"The <environment-id> part of the expression `{value}` is not a valid GUID.";
                return null;
            }

            if (accessType.IsEmpty() || !Constants.EnvironmentAccessTypes.Keys.Contains(accessType, StringComparer.OrdinalIgnoreCase))
            {
                argumentResult.ErrorMessage = $"The <access-type> part of the expression `{value}` is invalid. Possible values: {string.Join(", ", Constants.EnvironmentAccessTypes.Keys)}";
                return null;
            }

            result[i] = new EnvironmentSpecificAccess { EnvironmentId = environmentId, AccessType = accessType };
        }

        return result;
    }, false, "Format: `<environment-id>:<access-type>`. Interpreted only when the --access-type is `custom` which translates to `Environment specific`")
    { }
}

internal class EnvironmentSpecificAccess
{
    public string EnvironmentId { get; set; }

    public string AccessType { get; set; }
}

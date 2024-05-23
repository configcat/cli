using System;
using System.CommandLine;
using System.Linq;
using ConfigCat.Cli.Models.Api;

namespace ConfigCat.Cli.Options;

public class ReasonRequiredEnvironmentOption : Option<ReasonRequiredEnvironmentModel[]>
{
    public ReasonRequiredEnvironmentOption() : base(["--environments", "-ei"], argumentResult =>
    {
        var length = argumentResult.Tokens.Count;
        if (length == 0)
            return [];

        var result = new ReasonRequiredEnvironmentModel[length];
        for (var i = 0; i < length; i++)
        {
            var value = argumentResult.Tokens.ElementAt(i).Value;
            var indexOfSeparator = value.IndexOf(':');
            if (indexOfSeparator == -1)
            {
                argumentResult.ErrorMessage = $"The expression `{value}` is invalid. Required format: <environment-id>:<reason-required>";
                return null;
            }

            var environmentId = value[..indexOfSeparator];
            var reasonRequired = value[(indexOfSeparator + 1)..];

            if (!Guid.TryParse(environmentId, out _))
            {
                argumentResult.ErrorMessage = $"The <environment-id> part of the expression `{value}` is not a valid GUID.";
                return null;
            }

            if (reasonRequired.IsEmpty() || !bool.TryParse(reasonRequired, out var reasonReq))
            {
                argumentResult.ErrorMessage = $"The <reason-required> part of the expression `{value}` is not a valid boolean";
                return null;
            }

            result[i] = new ReasonRequiredEnvironmentModel { EnvironmentId = environmentId, ReasonRequired = reasonReq };
        }

        return result;
    }, false, "Format: `<environment-id>:<reason-required>`.")
    { }
}
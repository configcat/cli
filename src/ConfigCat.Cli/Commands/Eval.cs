using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models;
using ConfigCat.Cli.Options;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;
using ConfigCat.Client;
using ConfigCat.Client.Configuration;
using DataGovernance = ConfigCat.Client.DataGovernance;

namespace ConfigCat.Cli.Commands;

internal class Eval(IPrompt prompt, IOutput output, CliOptions options)
{
    private const string IdAttributeKey = "id";
    private const string EmailAttributeKey = "email";
    private const string CountryAttributeKey = "country";

    public async Task<int> InvokeAsync(string sdkKey,
        string[] flagKeys,
        string dataGovernance,
        string baseUrl,
        UserAttributeModel[] userAttributes,
        bool json,
        bool map,
        CancellationToken token)
    {
        if (sdkKey.IsEmpty())
        {
            sdkKey = System.Environment.GetEnvironmentVariable(Constants.SdkKeyEnvironmentVariableName);
            if (sdkKey.IsEmpty())
            {
                sdkKey = await prompt.GetStringAsync("SDK key", token);
            }
        }

        if (sdkKey.IsEmpty())
            throw new ShowHelpException($"Either the --sdk-key argument or the {Constants.SdkKeyEnvironmentVariableName} environment variable must be set.");
        
        if (flagKeys.IsEmpty())
            flagKeys = (await prompt.GetRepeatedValuesAsync("Set the feature flag keys that you want to evaluate",
                token, ["Flag key"])).SelectMany(t => t).ToArray();

        var client = CreateConfigCatClient(sdkKey, baseUrl, dataGovernance);
        await client.ForceRefreshAsync(token);

        User user = null;
        if (userAttributes.Length > 0)
        {
            user = new User(
                userAttributes.FirstOrDefault(ua => ua.Key == IdAttributeKey)?.Value?.ToString() ?? "")
            {
                Country = userAttributes.FirstOrDefault(ua => ua.Key == CountryAttributeKey)?.Value?.ToString(),
                Email = userAttributes.FirstOrDefault(ua => ua.Key == EmailAttributeKey)?.Value?.ToString()
            };
            var custom = userAttributes.Where(ua =>
                    ua.Key != IdAttributeKey && ua.Key != EmailAttributeKey && ua.Key != CountryAttributeKey)
                .ToDictionary(ua => ua.Key, ua => ua.Value);
            user.Custom = custom;
        }

        var evalResults = new List<EvaluationDetails>();
        foreach (var flagKey in flagKeys)
        {
            evalResults.Add(await client.GetValueDetailsAsync<object>(flagKey, null, user, token));
        }
     
        if (json)
        {
            var final = evalResults.ToDictionary(r => r.Key, r => new EvalResult
            {
                Value = r.Value,
                VariationId = r.VariationId,
                IsDefaultValue = r.IsDefaultValue,
                FetchTime = r.FetchTime,
                ErrorCode = (int)r.ErrorCode,
                ErrorMessage = r.ErrorMessage,
                TargetingMatch = r.MatchedTargetingRule is not null || r.MatchedPercentageOption is not null,
                User = r.User?.GetAllAttributes()
            });
            output.RenderJson(final, renderNulls: true);
            return ExitCodes.Ok;
        }

        if (map)
        {
            output.Write(string.Join(";", evalResults.Select(r => $"{r.Key}={r.Value.FormatIfBool()}")));
            return ExitCodes.Ok;
        }

        if (flagKeys.Length == 1)
        {
            output.Write(evalResults[0].Value?.FormatIfBool().ToString());
        }
        else
        {
            output.RenderTable(evalResults.Select(f => new
            {
                f.Key,
                Value = f.Value.FormatIfBool().ToString(),
            }));
        }
        
        return ExitCodes.Ok;
    }

    private IConfigCatClient CreateConfigCatClient(string sdkKey, string baseUrl, string dataGovernance)
    {
        var dataGovernanceEnum = dataGovernance switch
        {
            "global" => DataGovernance.Global,
            "eu" => DataGovernance.EuOnly,
            _ => DataGovernance.Global
        };

        return ConfigCatClient.Get(sdkKey, opts =>
        {
            if (!baseUrl.IsEmpty())
                opts.BaseUrl = new Uri(baseUrl);

            opts.DataGovernance = dataGovernanceEnum;
            opts.PollingMode = new ManualPoll();

            if (options.IsVerboseEnabled)
                opts.Logger = new ConsoleLogger(LogLevel.Debug);
        });
    }
}

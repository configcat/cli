using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.Api;

public class FlagValueModel
{
    public const string TargetingRuleJsonName = "rolloutRules";

    public const string PercentageRuleJsonName = "rolloutPercentageItems";

    public FlagModel Setting { get; set; }

    [JsonPropertyName(PercentageRuleJsonName)]
    public List<PercentageModel> PercentageRules { get; set; }

    [JsonPropertyName(TargetingRuleJsonName)]
    public List<TargetingModel> TargetingRules { get; set; }

    public object Value { get; set; }
}
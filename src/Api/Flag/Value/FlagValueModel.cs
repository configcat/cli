using ConfigCat.Cli.Utils;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Api.Flag.Value
{
    class FlagValueModel
    {
        public FlagModel Setting { get; set; }

        [JsonPropertyName(Constants.PercentageRuleJsonName)]
        public List<PercentageModel> PercentageRules { get; set; }

        [JsonPropertyName(Constants.TargetingRuleJsonName)]
        public List<TargetingModel> TargetingRules { get; set; }

        public object Value { get; set; }
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class TargetingRuleV6
    {
        /// <summary>
        /// Targeting rule conditions (AND stuff)
        /// </summary>
        [JsonPropertyName("c")]
        public List<ConditionV6> Conditions { get; set; }

        [JsonPropertyName("s")]
        public ValueAndVariationIdV6 ServedValue { get; set; }

        [JsonPropertyName("p")]
        public List<PercentageOptionV6> PercentageOptions { get; set; }
    }
}

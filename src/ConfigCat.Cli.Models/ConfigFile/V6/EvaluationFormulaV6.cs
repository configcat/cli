using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class EvaluationFormulaV6: ValueAndVariationIdV6
    {
        /// <summary>
        /// Setting type (bool, string, int, double)
        /// </summary>
        [JsonPropertyName("t")]
        public SettingType SettingType { get; set; }

        /// <summary>
        /// The percentage rule evaluation will hash this attribute of the User object to calculate the buckets.
        /// </summary>
        [JsonPropertyName("a")]
        public string PercentageRuleAttribute { get; set; }

        /// <summary>
        /// Targeting rules (OR stuff)
        /// </summary>
        [JsonPropertyName("r")]
        public List<TargetingRuleV6> TargetingRules { get; set; }

        /// <summary>
        /// Percentage options without conditions.
        /// </summary>
        [JsonPropertyName("p")]
        public List<PercentageOptionV6> PercentageOptions { get; set; }
    }
}

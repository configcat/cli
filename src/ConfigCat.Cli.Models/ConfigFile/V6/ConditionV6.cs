using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class ConditionV6
    {
        /// <summary>
        /// Classic targeting rule
        /// </summary>
        [JsonPropertyName("t")]
        public ComparisonRuleV6 ComparisonRule { get; set; }

        /// <summary>
        /// Segment targeting rule
        /// </summary>
        [JsonPropertyName("s")]
        public SegmentConditionV6 SegmentCondition { get; set; }

        /// <summary>
        /// Prerequisite targeting rule
        /// </summary>
        [JsonPropertyName("d")]
        public DependentFlagConditionV6 DependentFlagCondition { get; set; }
    }
}

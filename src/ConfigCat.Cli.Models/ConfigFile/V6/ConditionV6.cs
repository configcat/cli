using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class ConditionV6
    {
        /// <summary>
        /// Classic targeting rule
        /// </summary>
        [JsonPropertyName("u")]
        public ComparisonRuleV6 UserCondition { get; set; }

        /// <summary>
        /// Segment targeting rule
        /// </summary>
        [JsonPropertyName("s")]
        public SegmentConditionV6 SegmentCondition { get; set; }

        /// <summary>
        /// Prerequisite targeting rule
        /// </summary>
        [JsonPropertyName("p")]
        public PrerequisiteFlagConditionV6 PrerequisiteFlagCondition { get; set; }
    }
}

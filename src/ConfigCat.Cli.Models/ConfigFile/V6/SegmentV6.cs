using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class SegmentV6
    {
        /// <summary>
        /// The first 4 characters of the Segment's name
        /// </summary>
        [JsonPropertyName("n")]
        public string Name { get; set; }

        /// <summary>
        /// Segment rule conditions (AND)
        /// </summary>
        [JsonPropertyName("r")]
        public List<ComparisonRuleV6> SegmentRules { get; set; }
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class ComparisonRuleV6
    {
        /// <summary>
        /// The attribute of the user object that should be used to evaluate this rule
        /// </summary>
        [JsonPropertyName("a")]
        public string ComparisonAttribute { get; set; }

        [JsonPropertyName("c")]
        public UserComparator Comparator { get; set; }

        [JsonPropertyName("s")]
        public string StringValue { get; set; }

        [JsonPropertyName("d")]
        public double? DoubleValue { get; set; }

        [JsonPropertyName("l")]
        public List<string> StringListValue { get; set; }
    }
}

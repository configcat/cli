using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V5
{
    public class RolloutRuleV5
    {
        [JsonPropertyName("o")]
        public short Order { get; set; }

        [JsonPropertyName("a")]
        public string ComparisonAttribute { get; set; }

        [JsonPropertyName("t")]
        public RolloutRuleComparator Comparator { get; set; }

        [JsonPropertyName("c")]
        public string ComparisonValue { get; set; }

        [JsonPropertyName("v")]
        public JsonElement Value { get; set; }

        [JsonPropertyName("i")]
        public string VariationId { get; set; }
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V5
{
    public class SettingV5
    {
        [JsonPropertyName("v")]
        public JsonElement Value { get; set; }

        [JsonPropertyName("t")]
        public SettingType? SettingType { get; set; }

        [JsonPropertyName("p")]
        public RolloutPercentageItem[] RolloutPercentageItems { get; set; }

        [JsonPropertyName("r")]
        public RolloutRuleV5[] RolloutRules { get; set; }

        [JsonPropertyName("i")]
        public string VariationId { get; set; }
    }
}
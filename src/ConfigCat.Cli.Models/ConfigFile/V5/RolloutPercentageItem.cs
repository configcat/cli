using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V5
{
    public class RolloutPercentageItem
    {
        [JsonPropertyName("o")]
        public short Order { get; set; }

        [JsonPropertyName("v")]
        public JsonElement Value { get; set; }

        [JsonPropertyName("p")]
        public int Percentage { get; set; }

        [JsonPropertyName("i")]
        public string VariationId { get; set; }
    }
}
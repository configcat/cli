using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class ValueAndVariationIdV6
    {
        [JsonPropertyName("v")]
        public ValueV6 Value { get; set; }

        [JsonPropertyName("i")]
        public string VariationId { get; set; }
    }
}

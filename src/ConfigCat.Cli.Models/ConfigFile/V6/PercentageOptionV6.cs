using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class PercentageOptionV6 : ValueAndVariationIdV6
    {
        [JsonPropertyName("p")]
        public byte Percentage { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class ValueV6
    {
        [JsonPropertyName("b")]
        public bool? BoolValue { get; set; }

        [JsonPropertyName("s")]
        public string StringValue { get; set; }

        [JsonPropertyName("i")]
        public int? IntValue { get; set; }

        [JsonPropertyName("d")]
        public double? DoubleValue { get; set; }
    }
}

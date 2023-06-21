using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class DependentFlagConditionV6
    {
        [JsonPropertyName("f")]
        public string DependencySettingKey { get; set; }

        [JsonPropertyName("c")]
        public DependencyComparator DependencyComparator { get; set; }

        [JsonPropertyName("v")]
        public ValueV6 Value { get; set; }
    }
}

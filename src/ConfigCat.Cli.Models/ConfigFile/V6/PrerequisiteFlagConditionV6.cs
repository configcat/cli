using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class PrerequisiteFlagConditionV6
    {
        [JsonPropertyName("f")]
        public string PrerequisiteSettingKey { get; set; }

        [JsonPropertyName("c")]
        public PrerequisiteComparator PrerequisiteComparator { get; set; }

        [JsonPropertyName("v")]
        public ValueV6 Value { get; set; }
    }
}

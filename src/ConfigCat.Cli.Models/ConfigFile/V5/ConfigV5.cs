using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V5
{
    public class ConfigV5
    {
        [JsonPropertyName("f")]
        public Dictionary<string, SettingV5> Settings { get; set; }

        [JsonPropertyName("p")]
        public PreferencesV5 Preferences { get; set; }
    }
}
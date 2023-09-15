using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V5
{
    public class PreferencesV5
    {
        [JsonPropertyName("u")]
        public string Url { get; set; }

        [JsonPropertyName("r")]
        public int RedirectMode { get; set; }
    }
}
using System;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class PreferenceV6
    {
        [JsonPropertyName("u")]
        public Uri BaseUrl { get; set; }

        // Note: this property maybe will be overriden in the CdnService
        [JsonPropertyName("r")]
        public int? Redirect { get; set; }

        [JsonPropertyName("s")]
        public string Salt { get; set; }
    }
}

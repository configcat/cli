using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class ConfigV6
    {
        [JsonPropertyName("p")]
        public PreferenceV6 Preferences { get; set; }

        [JsonPropertyName("s")]
        public List<SegmentV6> Segments { get; set; }

        [JsonPropertyName("f")]
        public Dictionary<string, EvaluationFormulaV6> FeatureFlags { get; set; }
    }
}

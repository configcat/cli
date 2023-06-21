using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.ConfigFile.V6
{
    public class SegmentConditionV6
    {
        [JsonPropertyName("s")]
        public int SegmentIndex { get; set; }

        [JsonPropertyName("c")]
        public SegmentComparator SegmentComparator { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.Api
{
    public class TargetingModel
    {
        public string ComparisonAttribute { get; set; }

        public string Comparator { get; set; }

        public string ComparisonValue { get; set; }

        public object Value { get; set; }

        public string SegmentComparator { get; set; }

        public string SegmentId { get; set; }

        [JsonIgnore]
        public SegmentModel Segment { get; set; }
    }
}
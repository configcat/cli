using System;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.Api
{
    public class SegmentModel
    {
        public ProductModel Product { get; set; }

        public string SegmentId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string CreatorEmail { get; set; }

        public string CreatorFullName { get; set; }

        public DateTime CreatedAt { get; set; }

        public string LastUpdaterEmail { get; set; }

        public string LastUpdaterFullName { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string ComparisonAttribute { get; set; }

        public string Comparator { get; set; }

        public string ComparisonValue { get; set; }
    }

    public class CreateOrUpdateSegmentModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        [JsonPropertyName("comparisonAttribute")]
        public string Attribute { get; set; }

        public string Comparator { get; set; }

        [JsonPropertyName("comparisonValue")]
        public string CompareTo { get; set; }
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Api.Flag
{
    class UpdateFlagModel
    {
        public string Name { get; set; }

        public string Hint { get; set; }

        [JsonPropertyName("tags")]
        public IEnumerable<int> TagIds { get; set; }
    }
}

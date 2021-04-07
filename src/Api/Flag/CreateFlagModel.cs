using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Api.Flag
{
    class CreateFlagModel
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public string Hint { get; set; }

        [JsonPropertyName("settingType")]
        public string Type { get; set; }

        public IEnumerable<int> Tags { get; set; }
    }
}

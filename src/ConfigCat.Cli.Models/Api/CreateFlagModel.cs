using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.Api;

public class CreateFlagModel
{
    public string Key { get; set; }

    public string Name { get; set; }

    public string Hint { get; set; }

    [JsonPropertyName("settingType")]
    public string Type { get; set; }

    [JsonPropertyName("tags")]
    public IEnumerable<int> TagIds { get; set; }
}
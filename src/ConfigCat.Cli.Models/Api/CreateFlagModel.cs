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
    public List<int> TagIds { get; set; }

    public List<InitialValue> InitialValues { get; set; }
    
    public List<VariationModel> PredefinedVariations { get; set; }
}

public class InitialValue
{
    public string EnvironmentId { get; set; }

    public object Value { get; set; }
}
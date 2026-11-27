using System;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.Configuration;

public class Auth
{
    [Obsolete]
    [JsonPropertyName("user")]
    public string UserName { get; set; }

    [Obsolete]
    [JsonPropertyName("pass")]
    public string Password { get; set; }

    [JsonPropertyName("host")]
    public string ApiHost { get; set; }
}
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.Credentials;

public class CredentialData
{
    [JsonPropertyName("user")]
    public string UserName { get; set; }

    [JsonPropertyName("pass")]
    public string Password { get; set; }
}
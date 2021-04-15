using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Configuration
{
    class Auth
    {
        [JsonPropertyName("user")]
        public string UserName { get; set; }

        [JsonPropertyName("pass")]
        public string Password { get; set; }

        [JsonPropertyName("host")]
        public string ApiHost { get; set; }
    }
}

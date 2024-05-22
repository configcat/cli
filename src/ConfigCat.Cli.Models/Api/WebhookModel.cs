using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConfigCat.Cli.Models.Api;

public class WebhookModel
{
    public int WebhookId { get; set; }

    public string Url { get; set; }

    public string HttpMethod { get; set; }

    public string Content { get; set; }

    public ConfigModel Config { get; set; }

    public EnvironmentModel Environment { get; set; }

    [JsonPropertyName("webHookHeaders")]
    public IEnumerable<WebhookHeaderModel> WebhookHeaders { get; set; }

    public UpdateWebhookModel ToUpdateModel() => new()
    {
        Url = this.Url,
        HttpMethod = this.HttpMethod,
        Content = this.Content,
    };
}

public class WebhookHeaderModel
{
    public string Key { get; set; }

    public string Value { get; set; }

    public bool IsSecure { get; set; }
}

public class UpdateWebhookModel
{
    public string Url { get; set; }

    public string HttpMethod { get; set; }

    public string Content { get; set; }
}
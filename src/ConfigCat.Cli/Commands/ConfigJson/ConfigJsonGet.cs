#nullable enable

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands.ConfigJson;

internal class ConfigJsonGet(IOutput output, HttpClient httpClient)
{
    public const string ConfigV5 = "v5";
    public const string ConfigV6 = "v6";

    public async Task<int> ExecuteAsync(
        string sdkKey,
        string format,
        bool eu,
        bool test,
        string? baseUrl,
        bool pretty,
        CancellationToken token)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = test switch
            {
                false => !eu ? "https://cdn-global.configcat.com" : "https://cdn-eu.configcat.com",
                true => !eu ? "https://test-cdn-global.configcat.com" : "https://test-cdn-eu.configcat.com",
            };
        }

        var configFileName = $"config_{format}.json";

        var configJsonUrl = new Uri(new Uri(baseUrl), "configuration-files/" + sdkKey + "/" + configFileName);

        if (output.IsOutputRedirected)
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        else
        {
            output.WriteColor($"Downloading {configJsonUrl}...", ConsoleColor.Cyan);
            output.WriteLine();
        }

        var configJson = await httpClient.GetStringAsync(configJsonUrl, token);

        if (pretty)
        {
            var jsonNode = JsonNode.Parse(configJson);
            if (jsonNode is not null)
            {
                configJson = jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            }
        }

        if (!output.IsOutputRedirected)
        {
            output.WriteColor("Config JSON content:", ConsoleColor.Cyan);
            output.WriteLine();
        }

        output.WriteLine(configJson);

        return ExitCodes.Ok;
    }

}

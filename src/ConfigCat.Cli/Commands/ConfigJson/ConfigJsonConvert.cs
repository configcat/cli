﻿#nullable enable
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.ConfigFile.V5;
using ConfigCat.Cli.Models.ConfigFile.V6;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.ConfigFile;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands.ConfigJson;

internal class ConfigJsonConvert
{
    public const string V5TestConversion = "test-v5";
    public const string V6TestConversion = "test-v6";
    public const string V5ToV6Conversion = "v5-to-v6";

    private static JsonSerializerOptions SerializerOptionsFrom(JsonDocumentOptions documentOptions) => new()
    {
        AllowTrailingCommas = documentOptions.AllowTrailingCommas,
        ReadCommentHandling = documentOptions.CommentHandling,
        MaxDepth = documentOptions.MaxDepth,
    };

    private readonly IOutput output;
    private readonly IConfigJsonConverter configJsonConverter;

    public ConfigJsonConvert(IOutput output, IConfigJsonConverter configJsonConverter)
    {
        this.output = output;
        this.configJsonConverter = configJsonConverter;
    }

    public async Task<int> ExecuteAsync(
        string conversion,
        FileInfo? hashMap,
        bool skipSalt,
        bool pretty,
        CancellationToken token)
    {
        Func<string, string?>? reverseComparisonValueHash = null;
        if (hashMap is not null)
        {
            using var fileStream = hashMap.OpenRead();
            var comparisonValueHashMap = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fileStream, cancellationToken: token)
                ?? throw new InvalidOperationException("Invalid reverse hash map JSON content.");
            reverseComparisonValueHash = hash => comparisonValueHashMap.GetValueOrDefault(hash);
        }

        if (Console.IsInputRedirected)
        {
            Console.InputEncoding = Encoding.UTF8;
        }

        if (this.output.IsOutputRedirected)
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        else if (!Console.IsInputRedirected)
        {
            this.output.WriteColor($"Conversion selected: {conversion}", ConsoleColor.Cyan);
            this.output.WriteLine();

            this.output.WriteColor(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Input config JSON, then press Ctrl+Z and finally ENTER."
                : "Input config JSON, then press Ctrl+D.",
                ConsoleColor.Cyan);

            this.output.WriteLine();
        }

        Func<Stream, Task> writeOutput;

        var deserializeOptions = new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        };

        using (var inputStream = Console.OpenStandardInput())
        {
            switch (conversion)
            {
                case V5TestConversion:
                    var jsonNode = JsonNode.Parse(inputStream, documentOptions: deserializeOptions);

                    var configV5 = JsonSerializer.Deserialize<ConfigV5>(jsonNode, SerializerOptionsFrom(deserializeOptions))
                        ?? throw new InvalidOperationException("Invalid config JSON content.");

                    token.ThrowIfCancellationRequested();

                    writeOutput = outputStream =>
                    {
                        var serializerOptions = this.configJsonConverter.CreateSerializerOptionsV5(pretty);
                        return JsonSerializer.SerializeAsync(outputStream, jsonNode, serializerOptions);
                    };
                    break;

                case V6TestConversion:
                    jsonNode = JsonNode.Parse(inputStream, documentOptions: deserializeOptions);

                    var configV6 = JsonSerializer.Deserialize<ConfigV6>(jsonNode, SerializerOptionsFrom(deserializeOptions))
                        ?? throw new InvalidOperationException("Invalid config JSON content.");

                    token.ThrowIfCancellationRequested();

                    writeOutput = outputStream =>
                    {
                        var serializerOptions = this.configJsonConverter.CreateSerializerOptionsV6(pretty);
                        return JsonSerializer.SerializeAsync(outputStream, jsonNode, serializerOptions);
                    };
                    break;

                case V5ToV6Conversion:
                    configV5 = await JsonSerializer.DeserializeAsync<ConfigV5>(inputStream, SerializerOptionsFrom(deserializeOptions), cancellationToken: token)
                        ?? throw new InvalidOperationException("Invalid config JSON content.");

                    writeOutput = outputStream =>
                    {
                        configV6 = this.configJsonConverter.ConvertV5ToV6(configV5, skipSalt, reverseComparisonValueHash,
                            reportWarning: static msg => Console.Error.WriteLine(msg));

                        var serializerOptions = this.configJsonConverter.CreateSerializerOptionsV6(pretty);
                        return JsonSerializer.SerializeAsync(outputStream, configV6, serializerOptions);
                    };
                    break;
                default:
                    throw new ShowHelpException($"The --conversion argument is invalid. Conversion '{conversion}' is not supported.");
            }
        }

        if (!this.output.IsOutputRedirected && !Console.IsInputRedirected)
        {
            this.output.WriteLine();
            this.output.WriteColor("Conversion result:", ConsoleColor.Cyan);
            this.output.WriteLine();
        }

        using (var outputStream = Console.OpenStandardOutput())
        {
            await writeOutput(outputStream);
        }

        this.output.WriteLine();

        return ExitCodes.Ok;
    }
}

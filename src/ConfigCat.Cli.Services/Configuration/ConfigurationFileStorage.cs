using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Rendering;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Configuration;

public interface IConfigurationStorage
{
    Task<CliConfig> ReadConfigOrDefaultAsync(CancellationToken cancellationToken);

    Task WriteConfigAsync(CliConfig configuration, CancellationToken cancellationToken);
}

public class ConfigurationFileStorage(IOutput output) : IConfigurationStorage
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<CliConfig> ReadConfigOrDefaultAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(Constants.ConfigFilePath))
            return null;

        var content = await File.ReadAllTextAsync(Constants.ConfigFilePath, cancellationToken);
        var config = JsonSerializer.Deserialize<CliConfig>(content, Options);
        output.Verbose($"Config loaded from '{Constants.ConfigFilePath}'");
        return config;
    }

    public async Task WriteConfigAsync(CliConfig configuration, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(Constants.ConfigFilePath);
        if (!Directory.Exists(directory))
        {
            output.Verbose($"Directory '{directory}' not found, creating...");
            Directory.CreateDirectory(directory);
        }

        var serialized = JsonSerializer.Serialize(configuration, Options);

        output.Verbose($"Writing the configuration into '{Constants.ConfigFilePath}'");
        using var spinner = output.CreateSpinner(cancellationToken);
        await File.WriteAllTextAsync(Constants.ConfigFilePath, serialized, cancellationToken);
    }
}
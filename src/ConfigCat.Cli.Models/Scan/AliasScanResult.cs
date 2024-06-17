using ConfigCat.Cli.Models.Api;
using System.Collections.Concurrent;
using System.IO;

namespace ConfigCat.Cli.Models.Scan;

public class AliasScanResult
{
    public FileInfo ScannedFile { get; set; }

    public ConcurrentDictionary<string, ConcurrentBag<string>> FlagAliases { get; set; } = new();
}
using ConfigCat.Cli.Models.Api;
using System.Collections.Concurrent;
using System.IO;

namespace ConfigCat.Cli.Models.Scan
{
    public class AliasScanResult
    {
        public FileInfo ScannedFile { get; set; }

        public ConcurrentDictionary<FlagModel, ConcurrentBag<string>> FlagAliases { get; set; } = new ConcurrentDictionary<FlagModel, ConcurrentBag<string>>();

        public ConcurrentBag<FlagModel> FoundFlags { get; set; } = new ConcurrentBag<FlagModel>();
    }
}
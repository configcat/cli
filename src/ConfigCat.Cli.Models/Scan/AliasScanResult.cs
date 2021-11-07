using ConfigCat.Cli.Models.Api;
using System.Collections.Generic;
using System.IO;

namespace ConfigCat.Cli.Models.Scan
{
    public class AliasScanResult
    {
        public FileInfo ScannedFile { get; set; }

        public IDictionary<FlagModel, List<string>> FlagAliases { get; set; } = new Dictionary<FlagModel, List<string>>();
    }
}

using System.Collections.Generic;
using System.IO;

namespace ConfigCat.Cli.Models.Scan
{
    public class FlagReferenceResult
    {
        public FileInfo File { get; set; }

        public List<Reference> References { get; set; }
    }
}
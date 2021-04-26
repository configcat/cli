using ConfigCat.Cli.Models.Api;
using System.Collections.Generic;
using System.IO;

namespace ConfigCat.Cli.Services.Scan
{
    public class FlagReferenceResult
    {
        public FileInfo File { get; set; }

        public List<FlagReference> FlagReferences { get; set; }
    }

    public class FlagReference
    {
        public FlagModel FoundFlag { get; set; }

        public List<Line> PreLines { get; set; } = new List<Line>();

        public List<Line> PostLines { get; set; } = new List<Line>();

        public Line ReferenceLine { get; set; }
    }

    public class Line
    {
        public string LineText { get; set; }

        public int LineNumber { get; set; }
    }
}

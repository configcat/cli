using ConfigCat.Cli.Models.Api;
using System.Collections.Generic;

namespace ConfigCat.Cli.Models.Scan
{
    public class Reference
    {
        public FlagModel FoundFlag { get; set; }

        public List<Line> PreLines { get; set; } = new List<Line>();

        public List<Line> PostLines { get; set; } = new List<Line>();

        public Line ReferenceLine { get; set; }
    }
}

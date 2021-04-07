using System.Collections.Generic;

namespace ConfigCat.Cli.Api.Flag
{
    class UpdateFlagModel
    {
        public string Name { get; set; }

        public string Hint { get; set; }

        public IEnumerable<int> Tags { get; set; }
    }
}

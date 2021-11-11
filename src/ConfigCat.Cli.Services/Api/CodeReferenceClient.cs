using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api
{
    public interface ICodeReferenceClient
    {
        Task UploadAsync(CodeReferenceRequest request, CancellationToken token);
    }

    public class CodeReferenceClient : ApiClient, ICodeReferenceClient
    {
        public CodeReferenceClient(IOutput output,
            CliConfig config,
            IBotPolicy<HttpResponseMessage> botPolicy,
            HttpClient httpClient)
            : base(output, config, botPolicy, httpClient)
        { }

        public async Task UploadAsync(CodeReferenceRequest request, CancellationToken token)
        {
            this.Output.Write($"Uploading code references... ");
            await this.SendAsync(HttpMethod.Post, $"v1/code-references", request, token);
            this.Output.WriteSuccess();
            this.Output.WriteLine();
        }
    }

    public class CodeReferenceRequest
    {
        public string CommitUrl { get; set; }

        public string CommitHash { get; set; }

        public string Repository { get; set; }

        public string Branch { get; set; }

        public string ConfigId { get; set; }

        public List<string> ActiveBranches { get; set; }

        public List<FlagReference> FlagReferences { get; set; }

        public string Uploader { get; set; }
    }

    public class FlagReference
    {
        public int SettingId { get; set; }

        public List<ReferenceLines> References { get; set; }
    }

    public class ReferenceLines
    {
        public string File { get; set; }

        public string FileUrl { get; set; }

        public List<Line> PreLines { get; set; } = new();

        public List<Line> PostLines { get; set; } = new();

        public Line ReferenceLine { get; set; }
    }
}

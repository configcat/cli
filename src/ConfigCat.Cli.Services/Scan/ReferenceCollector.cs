using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Scan
{
    public interface IReferenceCollector
    {
        Task<IEnumerable<FlagReferenceResult>> CollectAsync(IEnumerable<FlagModel> flags,
            IEnumerable<FileInfo> filesToScan,
            int contextLines,
            CancellationToken token);
    }

    public class ReferenceCollector : IReferenceCollector
    {
        private readonly IFileScanner fileScanner;
        private readonly IBotPolicy<IEnumerable<FlagReferenceResult>> botPolicy;
        private readonly IOutput output;

        public ReferenceCollector(IFileScanner fileScanner,
            IBotPolicy<IEnumerable<FlagReferenceResult>> botPolicy,
            IOutput output)
        {
            this.fileScanner = fileScanner;
            this.botPolicy = botPolicy;
            this.output = output;
            this.botPolicy.Configure(p => p.Timeout(t => t.After(TimeSpan.FromSeconds(600))));
        }

        public async Task<IEnumerable<FlagReferenceResult>> CollectAsync(IEnumerable<FlagModel> flags,
            IEnumerable<FileInfo> filesToScan,
            int contextLines,
            CancellationToken token)
        {
            using var spinner = this.output.CreateSpinner(token);

            return await this.botPolicy.ExecuteAsync(async (ctx, cancellation) =>
            {
                var tasks = new List<Task<FlagReferenceResult>>();
                foreach (var file in filesToScan)
                {
                    if (cancellation.IsCancellationRequested)
                        break;

                    tasks.Add(this.fileScanner.ScanAsync(flags, file, contextLines, cancellation));
                }
                return (await Task.WhenAll(tasks)).Where(r => r is not null);
            }, token);
        }
    }
}

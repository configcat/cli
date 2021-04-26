using ConfigCat.Cli.Models.Api;
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
        private readonly IExecutionContextAccessor executionContextAccessor;

        public ReferenceCollector(IFileScanner fileScanner,
            IBotPolicy<IEnumerable<FlagReferenceResult>> botPolicy,
            IExecutionContextAccessor executionContextAccessor)
        {
            this.fileScanner = fileScanner;
            this.botPolicy = botPolicy;
            this.executionContextAccessor = executionContextAccessor;
            this.botPolicy.Configure(p => p.Timeout(t => t.After(TimeSpan.FromSeconds(300))));
        }

        public async Task<IEnumerable<FlagReferenceResult>> CollectAsync(IEnumerable<FlagModel> flags,
            IEnumerable<FileInfo> filesToScan,
            int contextLines,
            CancellationToken token)
        {
            var output = this.executionContextAccessor.ExecutionContext.Output;
            using var spinner = output.CreateSpinner(token);

            return await this.botPolicy.ExecuteAsync(async (ctx, cancellation) =>
            {
                var tasks = new List<Task<FlagReferenceResult>>();
                foreach (var file in filesToScan)
                {
                    if (cancellation.IsCancellationRequested)
                        break;

                    tasks.Add(this.fileScanner.ScanForKeysAsync(flags, file, contextLines, cancellation));
                }
                return (await Task.WhenAll(tasks)).Where(r => r is not null);
            }, token);
        }
    }
}

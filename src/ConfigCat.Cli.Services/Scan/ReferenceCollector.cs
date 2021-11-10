using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IAliasCollector aliasCollector;
        private readonly IBotPolicy<IEnumerable<FlagReferenceResult>> botPolicy;
        private readonly IOutput output;

        public ReferenceCollector(IFileScanner fileScanner,
            IAliasCollector aliasCollector,
            IBotPolicy<IEnumerable<FlagReferenceResult>> botPolicy,
            IOutput output)
        {
            this.fileScanner = fileScanner;
            this.aliasCollector = aliasCollector;
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
                var sw = new Stopwatch();
                sw.Start();
                this.output.Verbose($"Scanning for flag ALIASES...");
                var aliasTasks = filesToScan.TakeWhile(file => !cancellation.IsCancellationRequested)
                    .Select(file => this.aliasCollector.CollectAsync(flags, file, token));

                var scanResults = (await Task.WhenAll(aliasTasks)).Where(r => r is not null);

                foreach (var scanResult in scanResults.SelectMany(r => r.FlagAliases))
                    scanResult.Key.Aliases = scanResult.Value.Distinct().ToList();

                Console.WriteLine(sw.ElapsedMilliseconds);
                sw.Restart();

                this.output.Verbose($"Scanning for CODE REFERENCES...");
                var scanTasks = scanResults
                    .Select(r => r.ScannedFile)
                    .TakeWhile(file => !cancellation.IsCancellationRequested)
                    .Select(file => this.fileScanner.ScanAsync(flags, file, contextLines, cancellation));

                var result = (await Task.WhenAll(scanTasks)).Where(r => r is not null);
                Console.WriteLine(sw.ElapsedMilliseconds);
                return result;
            }, token);
        }
    }
}

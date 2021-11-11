using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Scan
{
    public interface IFileScanner
    {
        Task<IEnumerable<FlagReferenceResult>> ScanAsync(IEnumerable<FlagModel> flags,
            IEnumerable<FileInfo> filesToScan,
            int contextLines,
            CancellationToken token);
    }

    public class FileScanner : IFileScanner
    {
        private readonly IReferenceCollector referenceCollector;
        private readonly IAliasCollector aliasCollector;
        private readonly IBotPolicy<IEnumerable<FlagReferenceResult>> botPolicy;
        private readonly IOutput output;

        public FileScanner(IReferenceCollector referenceCollector,
            IAliasCollector aliasCollector,
            IBotPolicy<IEnumerable<FlagReferenceResult>> botPolicy,
            IOutput output)
        {
            this.referenceCollector = referenceCollector;
            this.aliasCollector = aliasCollector;
            this.botPolicy = botPolicy;
            this.output = output;
            this.botPolicy.Configure(p => p.Timeout(t => t.After(TimeSpan.FromSeconds(600))));
        }

        public async Task<IEnumerable<FlagReferenceResult>> ScanAsync(IEnumerable<FlagModel> flags,
            IEnumerable<FileInfo> filesToScan,
            int contextLines,
            CancellationToken token)
        {
            using var spinner = this.output.CreateSpinner(token);

            return await this.botPolicy.ExecuteAsync(async (ctx, cancellation) =>
            {
                this.output.Verbose($"Searching for flag ALIASES...", ConsoleColor.Magenta);
                var aliasResults = new ConcurrentBag<AliasScanResult>();
                await Parallel.ForEachAsync(filesToScan, token, async (file, t) =>
                {
                    var result = await this.aliasCollector.CollectAsync(flags, file, t);
                    if (result is not null)
                        aliasResults.Add(result);
                });

                foreach (var scan in aliasResults.SelectMany(k => k.FlagAliases))
                    scan.Key.Aliases = scan.Value.Distinct().ToList();

                this.output.Verbose($"Scanning for flag REFERENCES...", ConsoleColor.Magenta);
                var referenceResults = new ConcurrentBag<FlagReferenceResult>();
                await Parallel.ForEachAsync(aliasResults.Select(r => r.ScannedFile), token, async (file, t) =>
                {
                    var result = await referenceCollector.CollectAsync(flags, file, contextLines, t);
                    if (result is not null)
                        referenceResults.Add(result);
                });

                return referenceResults.AsEnumerable();
            }, token);
        }
    }
}

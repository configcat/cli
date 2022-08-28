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

namespace ConfigCat.Cli.Services.Scan;

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
            var aliasTasks = filesToScan.TakeWhile(file => !cancellation.IsCancellationRequested)
                .Select(file => this.aliasCollector.CollectAsync(flags, file, token));

            var aliasResults = (await Task.WhenAll(aliasTasks)).Where(r => r is not null).ToArray();

            foreach (var (key, value) in aliasResults.SelectMany(a => a.FlagAliases))
                key.Aliases = value.Distinct().ToList();

            var foundFlags = aliasResults.SelectMany(a => a.FoundFlags).Distinct().ToArray();

            this.output.Verbose($"Scanning for flag REFERENCES...", ConsoleColor.Magenta);
            var scanTasks = aliasResults
                .Select(r => r.ScannedFile)
                .TakeWhile(file => !cancellation.IsCancellationRequested)
                .Select(file => this.referenceCollector.CollectAsync(foundFlags, file, contextLines, cancellation));

            var referenceResults = (await Task.WhenAll(scanTasks)).Where(r => r is not null);

            return referenceResults;
        }, token);
    }
}
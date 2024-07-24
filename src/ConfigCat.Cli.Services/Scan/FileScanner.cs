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

namespace ConfigCat.Cli.Services.Scan;

public interface IFileScanner
{
    Task<IEnumerable<FlagReferenceResult>> ScanAsync(FlagModel[] flags,
        FileInfo[] filesToScan,
        string[] matchPatterns,
        string[] usagePatterns,
        int contextLines,
        ConcurrentBag<string> warningTracker,
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
        this.botPolicy.Configure(p => p.Timeout(t => t.After(TimeSpan.FromSeconds(1800))));
    }

    public async Task<IEnumerable<FlagReferenceResult>> ScanAsync(FlagModel[] flags,
        FileInfo[] filesToScan,
        string[] matchPatterns,
        string[] usagePatterns,
        int contextLines,
        ConcurrentBag<string> warningTracker,
        CancellationToken token)
    {
        using var spinner = this.output.CreateSpinner(token);
        return await this.botPolicy.ExecuteAsync(async (_, cancellation) =>
        {
            this.output.Verbose($"Searching for flag ALIASES...", ConsoleColor.Magenta);
            if (matchPatterns.Length > 0)
                this.output.Verbose($"Using the following custom alias patterns: {string.Join(", ", matchPatterns.Select(p => $"'{p}'"))}");
            if (usagePatterns.Length > 0)
                this.output.Verbose($"Using the following custom usage patterns: {string.Join(", ", usagePatterns.Select(p => $"'{p}'"))}");
            var aliasTasks = filesToScan.TakeWhile(_ => !cancellation.IsCancellationRequested)
                .Select(file => this.aliasCollector.CollectAsync(flags, file, matchPatterns, warningTracker, token));

            var aliasResults = (await Task.WhenAll(aliasTasks)).Where(r => r is not null).ToArray();

            foreach (var (key, aliases) in aliasResults.SelectMany(a => a.FlagAliases))
            {
                var flag = flags.FirstOrDefault(f => f.Key == key);
                if (flag is null) continue;
                
                flag.Aliases ??= [];
                flag.Aliases.AddRange(aliases);
                flag.Aliases = flag.Aliases.Distinct().ToList();
            }

            this.output.Verbose($"Scanning for flag REFERENCES...", ConsoleColor.Magenta);
            var scanTasks = aliasResults
                .Select(r => r.ScannedFile)
                .TakeWhile(file => !cancellation.IsCancellationRequested)
                .Select(file => this.referenceCollector.CollectAsync(flags, file, contextLines, usagePatterns, warningTracker, cancellation));

            var referenceResults = (await Task.WhenAll(scanTasks)).Where(r => r is not null);

            return referenceResults;
        }, token);
    }
}
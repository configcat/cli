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
        TimeSpan timeout,
        List<string> warningTracker,
        CancellationToken token);
}

public class FileScanner : IFileScanner
{
    private readonly IReferenceCollector referenceCollector;
    private readonly IAliasCollector aliasCollector;
    private readonly IBotPolicy<IEnumerable<FlagReferenceResult>> botPolicy;
    private readonly IOutput output;
    private const int BatchCount = 100;

    public FileScanner(IReferenceCollector referenceCollector,
        IAliasCollector aliasCollector,
        IBotPolicy<IEnumerable<FlagReferenceResult>> botPolicy,
        IOutput output)
    {
        this.referenceCollector = referenceCollector;
        this.aliasCollector = aliasCollector;
        this.botPolicy = botPolicy;
        this.output = output;
    }

    public async Task<IEnumerable<FlagReferenceResult>> ScanAsync(FlagModel[] flags,
        FileInfo[] filesToScan,
        string[] matchPatterns,
        string[] usagePatterns,
        int contextLines,
        TimeSpan timeout,
        List<string> warningTracker,
        CancellationToken token)
    {
        this.botPolicy.Configure(p => p.Timeout(t => t.After(timeout)));
        using var spinner = this.output.CreateSpinner(token);
        return await this.botPolicy.ExecuteAsync(async (_, cancellation) =>
        {
            this.output.Verbose($"Searching for flag ALIASES...", ConsoleColor.Magenta);
            if (matchPatterns.Length > 0)
                this.output.Verbose($"Using the following custom alias patterns: {string.Join(", ", matchPatterns.Select(p => $"'{p}'"))}");
            if (usagePatterns.Length > 0)
                this.output.Verbose($"Using the following custom usage patterns: {string.Join(", ", usagePatterns.Select(p => $"'{p}'"))}");
            
            var aliasResults = new List<ConcurrentDictionary<string, ConcurrentBag<string>>>();
            foreach (var files in filesToScan.Chunk(BatchCount))
            {
                var aliasTasks = files.Select(file =>
                    this.aliasCollector.CollectAsync(flags, file, matchPatterns, warningTracker, cancellation));
                aliasResults.AddRange((await Task.WhenAll(aliasTasks)).Where(r => r is not null));
            }
            
            foreach (var (key, aliases) in aliasResults.SelectMany(r => r))
            {
                var flag = flags.FirstOrDefault(f => f.Key == key);
                if (flag is null) continue;
                
                flag.Aliases ??= [];
                flag.Aliases.AddRange(aliases);
                flag.Aliases = flag.Aliases.Distinct().ToList();
            }

            this.output.Verbose($"Scanning for flag REFERENCES...", ConsoleColor.Magenta);
            var referenceResults = new List<FlagReferenceResult>();
            foreach (var files in filesToScan.Chunk(BatchCount))
            {
                var referenceTasks = files
                    .Select(file => this.referenceCollector.CollectAsync(flags, file, contextLines, usagePatterns, warningTracker, cancellation));
                referenceResults.AddRange((await Task.WhenAll(referenceTasks)).Where(r => r is not null));
            }

            return referenceResults.AsEnumerable();
        }, token);
    }
}
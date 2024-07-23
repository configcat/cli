using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.FileSystem;
using ConfigCat.Cli.Services.Git;
using ConfigCat.Cli.Services.Rendering;
using ConfigCat.Cli.Services.Scan;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class Scan(
    IWorkspaceLoader workspaceLoader,
    IFlagClient flagClient,
    ICodeReferenceClient codeReferenceClient,
    IFileCollector fileCollector,
    IFileScanner fileScanner,
    IGitClient gitClient,
    IOutput output)
{
    private const int MaxReferenceCountToUpload = 150;
    
    private static readonly Lazy<string> Version = new(() =>
        Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion);

    public async Task<int> InvokeAsync(DirectoryInfo directory,
        string configId,
        int lineCount,
        bool print,
        bool upload,
        string repo,
        string branch,
        string commitHash,
        string fileUrlTemplate,
        string commitUrlTemplate,
        string runner,
        string[] aliasPatterns,
        string[] usagePatterns,
        string[] excludeFlagKeys,
        CancellationToken token)
    {
        if (upload && repo.IsEmpty())
            throw new ShowHelpException("The --repo argument is required for code reference upload.");

        if (configId.IsEmpty())
        {
            output.WriteLine("Comparing the feature flags in the code to the feature flags in the ConfigCat Dashboard.");
            configId = (await workspaceLoader.LoadConfigAsync(token)).ConfigId;
        }

        if (excludeFlagKeys is {Length: 1} && excludeFlagKeys[0].Contains(','))
            excludeFlagKeys = excludeFlagKeys[0].Split(',')
                .Select(t => t.Trim())
                .Where(t => !t.IsEmpty())
                .ToArray();
        
        lineCount = lineCount is < 0 or > 10
            ? 4
            : lineCount;

        var flags = await flagClient.GetFlagsAsync(configId, token);
        if (excludeFlagKeys is {Length: > 0})
            flags = flags.Where(f => !excludeFlagKeys.Contains(f.Key));
        var deletedFlags = await flagClient.GetDeletedFlagsAsync(configId, token);
        deletedFlags = deletedFlags
            .Where(d => flags.All(f => f.Key != d.Key))
            .Distinct(new FlagModelEqualityComparer());
        if (excludeFlagKeys is {Length: > 0})
            deletedFlags = deletedFlags.Where(f => !excludeFlagKeys.Contains(f.Key));

        var files = await fileCollector.CollectAsync(directory, token);

        var patternsFromEnv =
            System.Environment.GetEnvironmentVariable(Constants.AliasPatternsEnvironmentVariableName)?.Split(',') ?? [];
        var usagePatternsFromEnv = 
            System.Environment.GetEnvironmentVariable(Constants.UsagePatternsEnvironmentVariableName)?.Split(',') ?? [];
        var warningTracker = new ConcurrentBag<string>();
        var flagReferences = await fileScanner.ScanAsync(flags.Concat(deletedFlags).ToArray(), files.ToArray(), 
            patternsFromEnv.Concat(aliasPatterns).ToArray(), usagePatternsFromEnv.Concat(usagePatterns).ToArray(), lineCount, warningTracker, token);

        if (!warningTracker.IsEmpty)
            output.WriteYellow(string.Join(System.Environment.NewLine, warningTracker.Select(w => $"[warning]: {w}"))).WriteLine();
        
        var flagReferenceResults = flagReferences as FlagReferenceResult[] ?? flagReferences.ToArray();
        var aliveFlagReferences = Filter(flagReferenceResults, r => r.FoundFlag is not DeletedFlagModel).ToArray();
        var deletedFlagReferences = Filter(flagReferenceResults, r => r.FoundFlag is DeletedFlagModel).ToArray();

        output.Write("Found ")
            .WriteCyan(aliveFlagReferences.Sum(f => f.References.Count).ToString())
            .Write($" feature flag / setting reference(s) in ")
            .WriteCyan(aliveFlagReferences.Length.ToString())
            .Write(" file(s). " +
                   $"Keys: [{string.Join(", ", aliveFlagReferences.SelectMany(r => r.References).Select(r => r.FoundFlag.Key).Distinct())}]")
            .WriteLine();

        if (print)
            this.PrintReferences(aliveFlagReferences, token);

        if (deletedFlagReferences.Length > 0)
            output.WriteWarning(
                $"{deletedFlagReferences.Sum(f => f.References.Count)} deleted feature flag/setting " +
                $"reference(s) found in {deletedFlagReferences.Length} file(s). " +
                $"Keys: [{string.Join(", ", deletedFlagReferences.SelectMany(r => r.References).Select(r => r.FoundFlag.Key).Distinct())}]");
        else
            output.WriteGreen("OK. Didn't find any deleted feature flag / setting references.");

        output.WriteLine();

        if (print)
            this.PrintReferences(deletedFlagReferences, token);

        if (!upload) return ExitCodes.Ok;

        output.WriteLine("Initiating code reference upload...");

        var gitInfo = await gitClient.GatherGitInfo(directory.FullName);

        branch = branch.NullIfEmpty() ?? gitInfo?.Branch;
        commitHash = commitHash.NullIfEmpty() ?? gitInfo?.CurrentCommitHash;

        if (branch.IsEmpty())
            throw new ShowHelpException(
                "Could not determine the current branch name, make sure the scanned folder is inside a Git repository, or use the --branch argument.");

        output.Write("Repository").Write(":").WriteCyan($" {repo}").WriteLine()
            .Write("Branch").Write(":").WriteCyan($" {branch}").WriteLine()
            .Write("Commit").Write(":").WriteCyan($" {commitHash}").WriteLine();

        var repositoryDirectory = gitInfo == null || gitInfo.WorkingDirectory.IsEmpty()
            ? directory.FullName.AsSlash()
            : gitInfo.WorkingDirectory;
        await codeReferenceClient.UploadAsync(new CodeReferenceRequest
        {
            FlagReferences = aliveFlagReferences
                .SelectMany(referenceResult => referenceResult.References, (file, reference) => new { file.File, reference })
                .GroupBy(r => r.reference.FoundFlag)
                .Select(r => new FlagReference
                {
                    SettingId = r.Key.SettingId,
                    References = r.OrderBy(it => it.reference.IsAlias).Select(item => new ReferenceLines
                    {
                        File = item.File.FullName.AsSlash().Replace(repositoryDirectory, string.Empty, StringComparison.OrdinalIgnoreCase).Trim('/'),
                        FileUrl = !commitHash.IsEmpty() && !fileUrlTemplate.IsEmpty()
                            ? fileUrlTemplate
                                .Replace("{commitHash}", commitHash)
                                .Replace("{filePath}", item.File.FullName.AsSlash().Replace(repositoryDirectory, string.Empty, StringComparison.OrdinalIgnoreCase).Trim('/'))
                                .Replace("{lineNumber}", item.reference.ReferenceLine.LineNumber.ToString())
                            : null,
                        PostLines = item.reference.PostLines,
                        PreLines = item.reference.PreLines,
                        ReferenceLine = item.reference.ReferenceLine
                    }).Take(MaxReferenceCountToUpload).ToList()
                }).ToList(),
            Repository = repo,
            Branch = branch,
            CommitHash = commitHash,
            CommitUrl = !commitHash.IsEmpty() && !commitUrlTemplate.IsEmpty()
                ? commitUrlTemplate.Replace("{commitHash}", commitHash)
                : null,
            ActiveBranches = gitInfo?.ActiveBranches,
            ConfigId = configId,
            Uploader = runner.NullIfEmpty() ?? $"ConfigCat CLI {Version.Value}",
        }, token);


        return ExitCodes.Ok;
    }

    private void PrintReferences(FlagReferenceResult[] references, CancellationToken token)
    {
        if (references.Length == 0)
            return;

        output.WriteLine();
        foreach (var fileReference in references)
        {
            if (token.IsCancellationRequested)
                break;

            output.WriteYellow(fileReference.File.FullName).WriteLine();
            foreach (var reference in fileReference.References)
            {
                if (token.IsCancellationRequested)
                    break;

                var maxDigitCount = reference.PostLines.Count > 0
                    ? reference.PostLines.Max(pl => pl.LineNumber).GetDigitCount()
                    : reference.ReferenceLine.LineNumber.GetDigitCount();
                foreach (var preLine in reference.PreLines)
                    this.PrintRegularLine(preLine, maxDigitCount);

                this.PrintSelectedLine(reference.ReferenceLine, maxDigitCount, reference);

                foreach (var postLine in reference.PostLines)
                    this.PrintRegularLine(postLine, maxDigitCount);

                output.WriteLine();
            }
        }
    }

    private void PrintRegularLine(Line line, int maxDigitCount)
    {
        var spaces = maxDigitCount - line.LineNumber.GetDigitCount();
        output.WriteCyan($"{line.LineNumber}:")
            .Write($"{new string(' ', spaces)} ")
            .WriteDarkGray(line.LineText)
            .WriteLine();
    }

    private void PrintSelectedLine(Line line, int maxDigitCount, Reference reference)
    {
        var spaces = maxDigitCount - line.LineNumber.GetDigitCount();
        output.WriteCyan($"{line.LineNumber}:")
            .Write($"{new string(' ', spaces)} ");

        this.SearchKeyInText(line.LineText, reference);

        output.WriteLine();
    }

    private void SearchKeyInText(string text, Reference reference)
    {
        var keyIndex = text.IndexOf(reference.FoundFlag.Key, StringComparison.Ordinal);
        var key = reference.FoundFlag.Key;
        if (keyIndex == -1)
        {
            if (reference.FoundFlag.Aliases != null)
            {
                foreach (var alias in reference.FoundFlag.Aliases)
                {
                    keyIndex = text.IndexOf(alias, StringComparison.Ordinal);
                    key = alias;
                    if (keyIndex != -1)
                        break;
                }
            }

            if (keyIndex == -1)
            {
                if (reference.MatchedSample != null)
                {
                    keyIndex = text.IndexOf(reference.MatchedSample, StringComparison.Ordinal);
                    key = reference.MatchedSample;
                }

                if (keyIndex == -1)
                {
                    output.Write(text);
                    return;
                }
            }
        }

        var preText = text[..keyIndex];
        var postText = text[(keyIndex + key.Length)..text.Length];
        output.Write(preText)
            .WriteColor(key, ConsoleColor.White, ConsoleColor.DarkMagenta);
        this.SearchKeyInText(postText, reference);
    }

    private static IEnumerable<FlagReferenceResult> Filter(IEnumerable<FlagReferenceResult> source,
        Predicate<Reference> filter)
    {
        foreach (var item in source)
        {
            var references = FilterReference(item.References, filter).ToList();
            if (references.Count == 0)
                continue;

            yield return new FlagReferenceResult { File = item.File, References = references };
        }

        IEnumerable<Reference> FilterReference(IEnumerable<Reference> references, Predicate<Reference> predicate)
        {
            foreach (var item in references)
            {
                if (!predicate(item))
                    continue;

                yield return item;
            }
        }
    }
}

internal class FlagModelEqualityComparer : IEqualityComparer<DeletedFlagModel>
{
    public bool Equals([AllowNull] DeletedFlagModel x, [AllowNull] DeletedFlagModel y)
    {
        if (x is null || y is null)
            return false;

        return x.Key == y.Key;
    }

    public int GetHashCode([DisallowNull] DeletedFlagModel obj)
    {
        return obj.Key.GetHashCode();
    }
}
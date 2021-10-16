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
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Scan
    {
        private static readonly Lazy<string> Version = new(() => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IFlagClient flagClient;
        private readonly ICodeReferenceClient codeReferenceClient;
        private readonly IFileCollector fileCollector;
        private readonly IReferenceCollector referenceCollector;
        private readonly IGitClient gitClient;
        private readonly IOutput output;

        public Scan(IWorkspaceLoader workspaceLoader,
            IFlagClient flagClient,
            ICodeReferenceClient codeReferenceClient,
            IFileCollector fileCollector,
            IReferenceCollector referenceCollector,
            IGitClient gitClient,
            IOutput output)
        {
            this.workspaceLoader = workspaceLoader;
            this.flagClient = flagClient;
            this.codeReferenceClient = codeReferenceClient;
            this.fileCollector = fileCollector;
            this.referenceCollector = referenceCollector;
            this.gitClient = gitClient;
            this.output = output;
        }

        public async Task<int> InvokeAsync(ScanArguments scanArguments, CancellationToken token)
        {
            if (scanArguments.ConfigId.IsEmpty())
                scanArguments.ConfigId = (await this.workspaceLoader.LoadConfigAsync(token)).ConfigId;

            scanArguments.LineCount = scanArguments.LineCount < 0 || scanArguments.LineCount > 10 ? 4 : scanArguments.LineCount;

            var flags = await this.flagClient.GetFlagsAsync(scanArguments.ConfigId, token);
            var deletedFlags = await this.flagClient.GetDeletedFlagsAsync(scanArguments.ConfigId, token);
            deletedFlags = deletedFlags
                .Where(d => !flags.Any(f => f.Key == d.Key))
                .Distinct(new FlagModelEqualityComparer());

            var files = await this.fileCollector.CollectAsync(scanArguments.Directory, token);
            var flagReferences = await this.referenceCollector.CollectAsync(flags.Concat(deletedFlags), files, scanArguments.LineCount, token);

            var aliveFlagReferences = Filter(flagReferences, r => r.FoundFlag is not DeletedFlagModel);
            var deletedFlagReferences = Filter(flagReferences, r => r.FoundFlag is DeletedFlagModel);

            this.output.Write("Found ")
                .WriteColored(aliveFlagReferences.Sum(f => f.References.Count()).ToString(), ForegroundColorSpan.LightCyan())
                .Write($" feature flag / setting reference(s) in ")
                .WriteColored(aliveFlagReferences.Count().ToString(), ForegroundColorSpan.LightCyan())
                .Write(" file(s). " +
                    $"Keys: [{string.Join(", ", aliveFlagReferences.SelectMany(r => r.References).Select(r => r.FoundFlag.Key).Distinct())}]")
                .WriteLine();

            if (scanArguments.Print)
                this.PrintReferences(aliveFlagReferences);

            if (deletedFlagReferences.Any())
                this.output.WriteWarning($"{deletedFlagReferences.Sum(f => f.References.Count())} deleted feature flag/setting " +
                    $"reference(s) found in {deletedFlagReferences.Count()} file(s). " +
                    $"Keys: [{string.Join(", ", deletedFlagReferences.SelectMany(r => r.References).Select(r => r.FoundFlag.Key).Distinct())}]");
            else
                this.output.WriteGreen("OK. Didn't find any deleted feature flag / setting references.");

            this.output.WriteLine();

            if (scanArguments.Print)
                this.PrintReferences(deletedFlagReferences);

            if (scanArguments.Upload)
            {
                this.output.WriteLine("Initiating code reference upload...");

                if (scanArguments.Repo.IsEmpty())
                    throw new ShowHelpException("The --repo argument is required for code reference upload.");

                var gitInfo = this.gitClient.GatherGitInfo(scanArguments.Directory.FullName);

                if((gitInfo == null || gitInfo.Branch.IsEmpty()) && scanArguments.Branch.IsEmpty())
                    throw new ShowHelpException("Could not determine the current branch name, make sure the scanned folder is inside a Git repository, or use the --branch argument.");

                var branch = gitInfo == null || gitInfo.Branch.IsEmpty() ? scanArguments.Branch : gitInfo.Branch;
                var commitHash = gitInfo?.CurrentCommitHash ?? scanArguments.CommitHash;
                this.output.WriteUnderline("Repository").Write(":").WriteColored($" {scanArguments.Repo}", ForegroundColorSpan.LightCyan()).WriteLine()
                    .WriteUnderline("Branch").Write(":").WriteColored($" {branch}", ForegroundColorSpan.LightCyan()).WriteLine()
                    .WriteUnderline("Commit").Write(":").WriteColored($" {commitHash}", ForegroundColorSpan.LightCyan()).WriteLine();
                var repositoryDirectory = gitInfo == null || gitInfo.WorkingDirectory.IsEmpty() ? scanArguments.Directory.FullName : gitInfo.WorkingDirectory;
                await this.codeReferenceClient.UploadAsync(new CodeReferenceRequest
                {
                    FlagReferences = aliveFlagReferences
                    .SelectMany(files => files.References, (file, reference) => new { file.File, reference })
                    .GroupBy(r => r.reference.FoundFlag)
                    .Select(r => new FlagReference
                    {
                        SettingId = r.Key.SettingId,
                        References = r.Select(item => new ReferenceLines
                        {
                            File = item.File.FullName.Replace(repositoryDirectory, string.Empty).AsSlash().Trim('/'),
                            FileUrl = !scanArguments.FileUrlTemplate.IsEmpty()
                                ? scanArguments.FileUrlTemplate
                                    .Replace("{branch}", branch)
                                    .Replace("{filePath}", item.File.FullName.Replace(repositoryDirectory, string.Empty).AsSlash().Trim('/'))
                                    .Replace("{lineNumber}", item.reference.ReferenceLine.LineNumber.ToString())
                                : null,
                            PostLines = item.reference.PostLines,
                            PreLines = item.reference.PreLines,
                            ReferenceLine = item.reference.ReferenceLine
                        }).ToList()
                    }).ToList(),
                    Repository = scanArguments.Repo,
                    Branch = branch,
                    CommitHash = commitHash,
                    CommitUrl = commitHash != null && !scanArguments.CommitUrlTemplate.IsEmpty()
                        ? scanArguments.CommitUrlTemplate.Replace("{commitHash}", gitInfo?.CurrentCommitHash ?? scanArguments.CommitHash)
                        : null,
                    ActiveBranches = gitInfo?.ActiveBranches,
                    ConfigId = scanArguments.ConfigId,
                    Uploader = scanArguments.Runner ?? $"ConfigCat CLI {Version.Value}",
                }, token);
            }

            return ExitCodes.Ok;
        }

        private void PrintReferences(IEnumerable<FlagReferenceResult> references)
        {
            if (!references.Any())
                return;

            this.output.WriteLine();
            foreach (var fileReference in references)
            {
                this.output.WriteColored(fileReference.File.FullName, ForegroundColorSpan.LightYellow()).WriteLine();
                foreach (var reference in fileReference.References)
                {
                    var maxDigitCount = reference.PostLines.Count > 0
                        ? reference.PostLines.Max(pl => pl.LineNumber).GetDigitCount()
                        : reference.ReferenceLine.LineNumber.GetDigitCount();
                    foreach (var preLine in reference.PreLines)
                        this.PrintRegularLine(preLine, maxDigitCount);

                    this.PrintSelectedLine(reference.ReferenceLine, maxDigitCount, reference.FoundFlag.Key);

                    foreach (var postLine in reference.PostLines)
                        this.PrintRegularLine(postLine, maxDigitCount);

                    this.output.WriteLine();
                }
            }
        }

        private void PrintRegularLine(Line line, int maxDigitCount)
        {
            var spaces = maxDigitCount - line.LineNumber.GetDigitCount();
            this.output.WriteColored($"{line.LineNumber}:", ForegroundColorSpan.LightCyan())
                .Write($"{new string(' ', spaces)} ")
                .WriteColored(line.LineText, ForegroundColorSpan.DarkGray())
                .WriteLine();
        }

        private void PrintSelectedLine(Line line, int maxDigitCount, string key)
        {
            var spaces = maxDigitCount - line.LineNumber.GetDigitCount();
            this.output.WriteColored($"{line.LineNumber}:", ForegroundColorSpan.LightCyan())
                .Write($"{new string(' ', spaces)} ");

            this.SearchKeyInText(line.LineText, key);

            this.output.WriteLine();
        }

        private void SearchKeyInText(string text, string key)
        {
            var keyIndex = text.IndexOf(key);
            if (keyIndex == -1)
            {
                this.output.Write(text);
                return;
            }

            var preText = text[0..keyIndex];
            var postText = text[(keyIndex + key.Length)..text.Length];
            this.output.Write(preText)
                .WriteNonAnsiColor(key, ConsoleColor.White, ConsoleColor.DarkMagenta);
            this.SearchKeyInText(postText, key);
        }

        private IEnumerable<FlagReferenceResult> Filter(IEnumerable<FlagReferenceResult> source, Predicate<Reference> filter)
        {
            foreach (var item in source)
            {
                var references = FilterReference(item.References, filter);
                if(!references.Any())
                    continue;

                yield return item;
            }

            IEnumerable<Reference> FilterReference(IEnumerable<Reference> references, Predicate<Reference> filter)
            {
                foreach (var item in references)
                {
                    if(!filter(item))
                        continue;

                    yield return item;
                }
            }
        }
    }

    class FlagModelEqualityComparer : IEqualityComparer<DeletedFlagModel>
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

    class ScanArguments
    {
        public DirectoryInfo Directory { get; set; }
        public string ConfigId { get; set; }
        public int LineCount { get; set; }
        public bool Print { get; set; }
        public bool Upload { get; set; }
        public string Repo { get; set; }
        public string Branch { get; set; }
        public string CommitHash { get; set; }
        public string FileUrlTemplate { get; set; }
        public string CommitUrlTemplate { get; set; }
        public string Runner { get; set; }
    }
}

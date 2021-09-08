using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.FileSystem;
using ConfigCat.Cli.Services.Rendering;
using ConfigCat.Cli.Services.Scan;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Scan
    {
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IFlagClient flagClient;
        private readonly IFileCollector fileCollector;
        private readonly IReferenceCollector referenceCollector;
        private readonly IOutput output;

        public Scan(IWorkspaceLoader workspaceLoader,
            IFlagClient flagClient,
            IFileCollector fileCollector,
            IReferenceCollector referenceCollector,
            IOutput output)
        {
            this.workspaceLoader = workspaceLoader;
            this.flagClient = flagClient;
            this.fileCollector = fileCollector;
            this.referenceCollector = referenceCollector;
            this.output = output;
        }

        public async Task<int> InvokeAsync(DirectoryInfo directory, string configId, int lineCount, bool print, CancellationToken token)
        {
            if (configId.IsEmpty())
                configId = (await this.workspaceLoader.LoadConfigAsync(token)).ConfigId;

            var flags = await this.flagClient.GetFlagsAsync(configId, token);
            var deletedFlags = await this.flagClient.GetDeletedFlagsAsync(configId, token);
            deletedFlags = deletedFlags
                .Where(d => !flags.Any(f => f.Key == d.Key))
                .Distinct(new FlagModelEqualityComparer());

            var files = await this.fileCollector.CollectAsync(directory, token);
            var flagReferences = await this.referenceCollector.CollectAsync(flags.Concat(deletedFlags), files, lineCount, token);

            var liveFlagReferences = flagReferences.Where(f => f.References.Where(r => r.FoundFlag is not DeletedFlagModel).Any());
            var deletedFlagReferences = flagReferences.Where(f => f.References.Where(r => r.FoundFlag is DeletedFlagModel).Any());

            this.output.Write("Found ");
            this.output.WriteColored(liveFlagReferences.Sum(f => f.References.Where(r => r.FoundFlag is not DeletedFlagModel).Count()).ToString(), ForegroundColorSpan.LightCyan());
            this.output.Write($" feature flag/setting reference(s) in ");
            this.output.WriteColored(liveFlagReferences.Count().ToString(), ForegroundColorSpan.LightCyan());
            this.output.Write(" file(s). " +
                $"Keys: [{string.Join(", ", liveFlagReferences.SelectMany(r => r.References.Where(r => r.FoundFlag is not DeletedFlagModel)).Select(r => r.FoundFlag.Key).Distinct())}]");
            this.output.WriteLine();

            if (print)
                this.PrintReferences(liveFlagReferences, r => r.FoundFlag is not DeletedFlagModel);

            if (deletedFlagReferences.Any())
                this.output.WriteWarning($"{deletedFlagReferences.Sum(f => f.References.Where(r => r.FoundFlag is DeletedFlagModel).Count())} deleted feature flag/setting " +
                    $"reference(s) found in {deletedFlagReferences.Count()} file(s). " +
                    $"Keys: [{string.Join(", ", deletedFlagReferences.SelectMany(r => r.References.Where(r => r.FoundFlag is DeletedFlagModel)).Select(r => r.FoundFlag.Key).Distinct())}]");
            else
                this.output.WriteGreen("OK. Didn't find any deleted feature flag/setting references.");

            this.output.WriteLine();

            if (print)
                this.PrintReferences(deletedFlagReferences, r => r.FoundFlag is DeletedFlagModel);

            return ExitCodes.Ok;
        }

        private void PrintReferences(IEnumerable<FlagReferenceResult> references, Func<Reference, bool> filter)
        {
            if (!references.Any())
                return;

            this.output.WriteLine();
            foreach (var fileReference in references)
            {
                this.output.WriteColored(fileReference.File.FullName, ForegroundColorSpan.LightYellow());
                this.output.WriteLine();
                foreach (var reference in fileReference.References.Where(filter))
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
            this.output.WriteColored($"{line.LineNumber}:", ForegroundColorSpan.LightCyan());
            this.output.Write($"{new string(' ', spaces)} ");
            this.output.WriteColored(line.LineText, ForegroundColorSpan.DarkGray());
            this.output.WriteLine();
        }

        private void PrintSelectedLine(Line line, int maxDigitCount, string key)
        {
            var spaces = maxDigitCount - line.LineNumber.GetDigitCount();
            this.output.WriteColored($"{line.LineNumber}:", ForegroundColorSpan.LightCyan());
            this.output.Write($"{new string(' ', spaces)} ");

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
            this.output.Write(preText);
            this.output.WriteColoredWithBackground(key, ForegroundColorSpan.Rgb(255, 255, 255), BackgroundColorSpan.Magenta());
            this.SearchKeyInText(postText, key);
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
}

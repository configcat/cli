using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Trybot;
using Trybot.Timeout.Exceptions;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConfigCat.Cli.Services.Scan
{
    public interface IReferenceCollector
    {
        Task<FlagReferenceResult> CollectAsync(IEnumerable<FlagModel> flags, FileInfo file, int contextLines, CancellationToken token);
    }

    public class ReferenceCollector : IReferenceCollector
    {
        private static readonly string[] Prefixes = new[] { ".", "->" };
        private readonly IBotPolicy<FlagReferenceResult> botPolicy;
        private readonly IOutput output;

        public ReferenceCollector(IBotPolicy<FlagReferenceResult> botPolicy,
            IOutput output)
        {
            this.botPolicy = botPolicy;
            this.output = output;

            this.botPolicy.Configure(p => p.Timeout(t => t.After(TimeSpan.FromSeconds(10))));
        }

        public async Task<FlagReferenceResult> CollectAsync(IEnumerable<FlagModel> flags, FileInfo file, int contextLines, CancellationToken token)
        {
            try
            {
                return await this.botPolicy.ExecuteAsync(async (ctx, cancellation) =>
                {
                    await using var stream = file.OpenRead();
                    if (await stream.IsBinaryAsync(cancellation))
                    {
                        this.output.Verbose($"{file.FullName} is binary, skipping.", ConsoleColor.Yellow);
                        return null;
                    }

                    this.output.Verbose($"{file.FullName} scanning...");

                    var lineTracker = new LineTracker(contextLines);
                    var lineNumber = 1;

                    using var reader = new StreamReader(stream);
                    while (!reader.EndOfStream && !cancellation.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line.Length > Constants.MaxCharCountPerLine)
                        {
                            this.output.Verbose($"{file.FullName} contains a line that has more than {Constants.MaxCharCountPerLine} characters, skipping.", ConsoleColor.Yellow);
                            return null;
                        }

                        foreach (var flag in flags)
                        {
                            if (line.Contains($"\"{flag.Key}\"") ||
                                line.Contains($"'{flag.Key}'") ||
                                line.Contains($"`{flag.Key}`") ||
                                (flag.Aliases != null && flag.Aliases.Any(a => line.Contains(a, StringComparison.OrdinalIgnoreCase))))
                            {
                                lineTracker.TrackReference(flag, line, lineNumber);
                                continue;
                            }

                            var matchedSample = SearchForSampleVariations(flag, line);
                            if (matchedSample != null)
                                lineTracker.TrackReference(flag, line, lineNumber, matchedSample);
                        }

                        lineTracker.AddLine(line, lineNumber);
                        lineNumber++;
                    }

                    lineTracker.FinishAll();

                    this.output.Verbose($"{file.FullName} scan completed.", ConsoleColor.Green);
                    return new FlagReferenceResult { File = file, References = lineTracker.FinishedReferences };
                }, token);
            }
            catch (OperationTimeoutException)
            {
                this.output.Verbose($"{file.FullName} scan timed out.", ConsoleColor.Red);
                return null;
            }
        }

        private static string SearchForSampleVariations(FlagModel flag, string line)
        {
            var originals = new[] { flag.Key }.Concat(flag.Aliases);
            foreach (var original in originals)
            {
                foreach (var sample in ProduceVariationSamples(original, flag.SettingType == SettingTypes.Boolean).Distinct())
                {
                    if (!Prefixes.Any(p => line.Contains($"{p}{sample}", StringComparison.OrdinalIgnoreCase))) continue;
                    var originalFromLine = line.IndexOf(sample, StringComparison.OrdinalIgnoreCase);
                    return line.Substring(originalFromLine, sample.Length);
                }
            }

            return null;
        }

        private static IEnumerable<string> GetSampleVariations(FlagModel flag)
        {
            var originals = new[] { flag.Key }.Concat(flag.Aliases);
            foreach (var original in originals)
            {
                var samples = ProduceVariationSamples(original, flag.SettingType == SettingTypes.Boolean).Distinct();
                return samples.SelectMany(s => Prefixes.Select(p => $"{p}{s}"));
            }

            return null;
        }

        private static IEnumerable<string> ProduceVariationSamples(string original, bool isBoolFlag)
        {
            var trimmed = original.RemoveDashes();
            var includeUnderscore = trimmed != original;

            yield return original;
            yield return trimmed;
            yield return $"get{trimmed}";

            if (includeUnderscore)
                yield return $"get_{original}";

            if (isBoolFlag)
            {
                yield return $"is{trimmed}";
                yield return $"is{trimmed}enabled";

                if (includeUnderscore)
                {
                    yield return $"is_{original}";
                    yield return $"is_{original}_enabled";
                }
            }
        }

        private class LineTracker
        {
            private readonly List<Trackable> trackedReferences;
            private readonly Queue<Line> preContextLineBuffer;
            private readonly int contextLineCount;

            public List<Reference> FinishedReferences { get; }

            public LineTracker(int contextLineCount)
            {
                this.preContextLineBuffer = new Queue<Line>(contextLineCount);
                this.trackedReferences = new List<Trackable>();
                this.FinishedReferences = new List<Reference>();
                this.contextLineCount = contextLineCount;
            }

            public void AddLine(string line, int lineNumber)
            {
                var currentLine = new Line { LineText = line, LineNumber = lineNumber };
                this.HandlePostLines(currentLine);
                this.HandleBufferQueue(currentLine);
            }

            public void TrackReference(FlagModel flag, string line, int lineNumber, string matchedSample = null)
            {
                var reference = new Reference
                {
                    FoundFlag = flag,
                    ReferenceLine = new Line { LineText = line, LineNumber = lineNumber },
                    PreLines = this.preContextLineBuffer.ToList(),
                    MatchedSample = matchedSample
                };
                this.trackedReferences.Add(new Trackable
                { RemainingContextLines = this.contextLineCount, FlagReference = reference });
            }

            public void FinishAll()
            {
                this.FinishedReferences.AddRange(this.trackedReferences.Select(t => t.FlagReference));
                this.trackedReferences.Clear();
                this.preContextLineBuffer.Clear();
            }

            private void HandlePostLines(Line currentLine)
            {
                foreach (var trackable in this.trackedReferences)
                {
                    if (trackable.FlagReference.ReferenceLine.LineNumber != currentLine.LineNumber)
                    {
                        trackable.FlagReference.PostLines.Add(currentLine);
                        trackable.RemainingContextLines--;
                    }

                    if (trackable.RemainingContextLines <= 0)
                        this.FinishedReferences.Add(trackable.FlagReference);
                }

                this.trackedReferences.RemoveAll(t => t.RemainingContextLines <= 0);
            }

            private void HandleBufferQueue(Line currentLine)
            {
                if (this.contextLineCount <= 0)
                    return;

                if (this.preContextLineBuffer.Count == this.contextLineCount)
                    this.preContextLineBuffer.Dequeue();

                this.preContextLineBuffer.Enqueue(currentLine);
            }

            private class Trackable
            {
                public int RemainingContextLines { get; set; }

                public Reference FlagReference { get; set; }
            }
        }
    }
}

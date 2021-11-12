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

namespace ConfigCat.Cli.Services.Scan
{
    public interface IReferenceCollector
    {
        Task<FlagReferenceResult> CollectAsync(IEnumerable<FlagModel> flags, FileInfo file, int contextLines, CancellationToken token);
    }

    public class ReferenceCollector : IReferenceCollector
    {
        private static readonly string[] Prefixes = { "::", ".", "->" };
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

                    var flagSamples = flags.Select(f => new FlagSample
                    {
                        Flag = f,
                        KeySamples = ProduceKeySamples(f).Distinct().ToArray(),
                        Samples = ProduceVariationSamples(f).Distinct().ToArray()
                    }).ToArray();

                    using var reader = new StreamReader(stream);
                    while (!reader.EndOfStream && !cancellation.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line.Length > Constants.MaxCharCountPerLine)
                        {
                            this.output.Verbose($"{file.FullName} contains a line that has more than {Constants.MaxCharCountPerLine} characters, skipping.", ConsoleColor.Yellow);
                            return null;
                        }

                        foreach (var flagSample in flagSamples)
                        {
                            if (flagSample.KeySamples.Any(k => line.Contains(k)) || flagSample.Flag.Aliases.Any(a => line.Contains(a)))
                            {
                                lineTracker.TrackReference(flagSample.Flag, line, lineNumber);
                                continue;
                            }

                            foreach (var sample in flagSample.Samples)
                            {
                                if (line.Contains(sample, StringComparison.OrdinalIgnoreCase))
                                {
                                    var originalFromLine = line.IndexOf(sample, StringComparison.OrdinalIgnoreCase);
                                    lineTracker.TrackReference(flagSample.Flag, line, lineNumber, line.Substring(originalFromLine, sample.Length).Remove(Prefixes));
                                }
                            }
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

        private static IEnumerable<string> ProduceKeySamples(FlagModel flag)
        {
            yield return $"\"{flag.Key}\"";
            yield return $"'{flag.Key}'";
            yield return $"`{flag.Key}`";
        }

        private static IEnumerable<string> ProduceVariationSamples(FlagModel flag)
        {
            var originals = new[] { flag.Key }.Concat(flag.Aliases);
            var isBoolFlag = flag.SettingType == SettingTypes.Boolean;

            foreach (var original in originals)
            {
                var trimmed = original.RemoveDashes();
                var includeUnderscore = trimmed != original;

                foreach (var prefix in Prefixes)
                {
                    yield return $"{prefix}{original}";
                    yield return $"{prefix}get{trimmed}";

                    if (includeUnderscore)
                    {
                        yield return $"{prefix}{trimmed}";
                        yield return $"{prefix}get_{original}";
                    }

                    if (!isBoolFlag) continue;
                    yield return $"{prefix}is{trimmed}";
                    yield return $"{prefix}is{trimmed}enabled";

                    if (!includeUnderscore) continue;
                    yield return $"{prefix}is_{original}";
                    yield return $"{prefix}is_{original}_enabled";
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

        private class FlagSample
        {
            public FlagModel Flag { get; set; }

            public string[] Samples { get; set; }

            public string[] KeySamples { get; set; }
        }
    }
}

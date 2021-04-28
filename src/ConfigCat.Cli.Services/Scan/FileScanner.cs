using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trybot;
using Trybot.Timeout.Exceptions;

namespace ConfigCat.Cli.Services.Scan
{
    public interface IFileScanner
    {
        Task<FlagReferenceResult> ScanAsync(IEnumerable<FlagModel> flags, FileInfo file, int contextLines, CancellationToken token);
    }

    public class FileScanner : IFileScanner
    {
        private readonly IBotPolicy<FlagReferenceResult> botPolicy;
        private readonly IOutput output;

        public FileScanner(IBotPolicy<FlagReferenceResult> botPolicy,
            IOutput output)
        {
            this.botPolicy = botPolicy;
            this.output = output;

            this.botPolicy.Configure(p => p.Timeout(t => t.After(TimeSpan.FromSeconds(30))));
        }

        public async Task<FlagReferenceResult> ScanAsync(IEnumerable<FlagModel> flags, FileInfo file, int contextLines, CancellationToken token)
        {
            try
            {
                return await this.botPolicy.ExecuteAsync(async (ctx, cancellation) =>
                {
                    await using var stream = file.OpenRead();
                    if (await stream.IsBinaryAsync(cancellation))
                    {
                        this.output.Verbose($"{file.FullName} is binary, skipping.", ForegroundColorSpan.LightYellow());
                        return null;
                    }

                    this.output.Verbose($"{file.FullName} start scanning...");

                    using var reader = new StreamReader(stream);
                    var tracker = new LineTracker(contextLines);
                    var lineNumber = 1;
                    while (!reader.EndOfStream && !cancellation.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync();
                        foreach (var flag in flags)
                        {
                            if (line.Contains(flag.Key))
                                tracker.TrackReference(flag, line, lineNumber);
                        }

                        tracker.AddLine(line, lineNumber);
                        lineNumber++;
                    }

                    tracker.FinishAll();
                    this.output.Verbose($"{file.FullName} scan completed.", ForegroundColorSpan.LightGreen());
                    return new FlagReferenceResult { File = file, References = tracker.FinishedReferences };
                }, token);
            }
            catch (OperationTimeoutException)
            {
                this.output.Verbose($"{file.FullName} scan timed out.", ForegroundColorSpan.LightRed());
                return null;
            }
        }

        class LineTracker
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

            public void TrackReference(FlagModel flag, string line, int lineNumber)
            {
                var reference = new Reference
                {
                    FoundFlag = flag,
                    ReferenceLine = new Line { LineText = line, LineNumber = lineNumber },
                    PreLines = this.preContextLineBuffer.ToList()
                };
                this.trackedReferences.Add(new Trackable { RemainingContextLines = this.contextLineCount, FlagReference = reference });
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
        }

        class Trackable
        {
            public int RemainingContextLines { get; set; }

            public Reference FlagReference { get; set; }
        }
    }
}

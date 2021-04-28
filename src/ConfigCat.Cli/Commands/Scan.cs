﻿using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.FileSystem;
using ConfigCat.Cli.Services.Rendering;
using ConfigCat.Cli.Services.Scan;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
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

        public string Name => "scan";

        public string Description => "Scans files for feature flag or setting usages";

        public IEnumerable<Argument> Arguments => new Argument[]
        {
            new Argument<DirectoryInfo>("directory", "Directory to scan").ExistingOnly(),
        };

        public IEnumerable<Option> Options => new Option[]
        {
            new Option<string>(new[] { "--config-id", "-c" }, "ID of the config to scan against"),
            new Option<int>(new[] { "--line-count", "-l" }, () => 4, "Context line count before and after the reference line"),
        };

        public async Task<int> InvokeAsync(DirectoryInfo directory, string configId, int lineCount, CancellationToken token)
        {
            if (configId.IsEmpty())
                configId = (await this.workspaceLoader.LoadConfigAsync(token)).ConfigId;

            var flags = await this.flagClient.GetFlagsAsync(configId, token);

            var files = await this.fileCollector.CollectAsync(directory, token);
            var references = await this.referenceCollector.CollectAsync(flags, files, lineCount, token);

            this.output.WriteLine();
            foreach (var fileReference in references.Where(r => r.References.Count > 0))
            {
                this.output.WriteColored(fileReference.File.FullName, ForegroundColorSpan.LightYellow());
                this.output.WriteLine();
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

            return ExitCodes.Ok;
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
}
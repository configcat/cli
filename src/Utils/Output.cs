using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.Threading;

namespace ConfigCat.Cli.Utils
{
    interface IOutput
    {
        IConsole Console { get; }
        void Verbose(string text);
        void Write(string text);
        void WriteLine(string text = null);
        void WriteGreen(string text);
        void WriteYellow(string text);
        void WriteError(string text);
        void WriteUnderline(string text);
        void WriteBlink(string text);
        void WriteReverse(string text);
        void WriteStandout(string text);
        void WriteColored(string text, ForegroundColorSpan color);
        void WriteColoredWithBackground(string text, ForegroundColorSpan foreground, BackgroundColorSpan background);
        Spinner CreateSpinner(CancellationToken token);
    }

    class Output : IOutput
    {
        public IConsole Console { get; }
        private readonly bool showVerboseOutput;

        public Output(IConsole console, bool showVerboseOutput)
        {
            this.Console = console;
            this.showVerboseOutput = showVerboseOutput;
        }

        public void Verbose(string text)
        {
            if (!this.showVerboseOutput)
                return;

            this.Console.Out.WriteLine($"[verbose]: {text}");
        }

        public void Write(string text) => this.Console.Out.Write(text);

        public void WriteLine(string text = null) => this.Console.Out.WriteLine(text);

        public void WriteError(string text) => this.Console.WriteError(text);

        public void WriteGreen(string text) => this.Console.WriteStyle(text.Color(ForegroundColorSpan.LightGreen()), text);

        public void WriteYellow(string text) => this.Console.WriteStyle(text.Color(ForegroundColorSpan.LightYellow()), text);

        public void WriteUnderline(string text) => this.Console.WriteStyle(text.Underline(), text);

        public void WriteBlink(string text) => this.Console.WriteStyle(text.Blink(), text);

        public void WriteReverse(string text) => this.Console.WriteStyle(text.Reverse(), text);

        public void WriteStandout(string text) => this.Console.WriteStyle(text.Standout(), text);

        public void WriteColored(string text, ForegroundColorSpan color) => this.Console.WriteStyle(text.Color(color), text);

        public void WriteColoredBackground(string text, BackgroundColorSpan color) => this.Console.WriteStyle(text.Background(color), text);

        public void WriteColoredWithBackground(string text, ForegroundColorSpan foreground, BackgroundColorSpan background) =>
            this.Console.WriteStyle(text.ColorWithBackground(foreground, background), text);

        public Spinner CreateSpinner(CancellationToken token) => new Spinner(token, this.Console, this.showVerboseOutput);

    }
}

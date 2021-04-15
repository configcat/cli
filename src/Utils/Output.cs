using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Utils
{
    interface IOutput
    {
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
        void WriteNonAnsiColor(string text, ConsoleColor foreground, ConsoleColor? background = null);
        void WriteNoChange();

        void MoveCursorLeft();
        void MoveCursorRight();
        void MoveCursorUp(int left);
        void MoveCursorDown(int left);
        void SetCursorPosition(int left, int top);
        (int left, int top) GetCursorPosition();
        void ClearBack(int charCount);
        void ClearCurrentLine();
        void HideCursor();
        void ShowCursor();

        bool IsOutputRedirected { get; }

        Task<ConsoleKeyInfo> ReadKeyAsync(CancellationToken token, bool intercept = false);
        Task<string> ReadLineAsync(CancellationToken token, bool masked = false);

        Spinner CreateSpinner(CancellationToken token);
        void RenderView(View view);
    }

    class Output : IOutput
    {
        public bool IsOutputRedirected { get; } = Console.IsOutputRedirected;

        private readonly IConsole console;

        private readonly IConsole originalConsole;

        private readonly bool showVerboseOutput;

        public Output(IConsole console, bool showVerboseOutput)
        {
            this.console = console.GetTerminal() ?? console;
            this.originalConsole = console;
            this.showVerboseOutput = showVerboseOutput;
        }

        public void Verbose(string text)
        {
            if (!this.showVerboseOutput)
                return;

            this.console.Out.WriteLine($"[verbose]: {text}");
        }

        public void Write(string text) => this.console.Out.Write(text);

        public void WriteLine(string text = null) => this.console.Out.WriteLine(text);

        public void WriteError(string text) => this.console.WriteError(text);

        public void WriteGreen(string text) => this.console.WriteStyle(text.Color(ForegroundColorSpan.LightGreen()), text);

        public void WriteYellow(string text) => this.console.WriteStyle(text.Color(ForegroundColorSpan.LightYellow()), text);

        public void WriteUnderline(string text) => this.console.WriteStyle(text.Underline(), text);

        public void WriteBlink(string text) => this.console.WriteStyle(text.Blink(), text);

        public void WriteReverse(string text) => this.console.WriteStyle(text.Reverse(), text);

        public void WriteStandout(string text) => this.console.WriteStyle(text.Standout(), text);

        public void WriteColored(string text, ForegroundColorSpan color) => this.console.WriteStyle(text.Color(color), text);

        public void WriteColoredBackground(string text, BackgroundColorSpan color) => this.console.WriteStyle(text.Background(color), text);

        public void WriteNonAnsiColor(string text, ConsoleColor foreground, ConsoleColor? background = null)
        {
            Console.ForegroundColor = foreground;
            if (background is not null)
                Console.BackgroundColor = background.Value;
            Console.Write(text);
            Console.ResetColor();
        }

        public void WriteColoredWithBackground(string text, ForegroundColorSpan foreground, BackgroundColorSpan background) =>
            this.console.WriteStyle(text.ColorWithBackground(foreground, background), text);

        public void WriteNoChange()
        {
            this.Write($"No changes detected... ");
            this.WriteYellow("Skipped.");
        }

        public Spinner CreateSpinner(CancellationToken token) => new Spinner(token, this.console, this.showVerboseOutput);

        public void RenderView(View view)
        {
            var renderer = new ConsoleRenderer(this.originalConsole, resetAfterRender: true);
            view.RenderFitToContent(renderer, this.originalConsole);
        }

        public void MoveCursorLeft()
        {
            var top = Console.CursorTop;
            var left = Console.CursorLeft;
            var width = Console.BufferWidth;
            var isStartReached = left <= 0;
            this.SetCursorPosition(isStartReached ? width - 1 : left - 1, isStartReached ? top - 1 : top);
        }

        public void MoveCursorRight()
        {
            var top = Console.CursorTop;
            var left = Console.CursorLeft;
            var width = Console.BufferWidth;
            var isEndReached = left >= width;
            this.SetCursorPosition(isEndReached ? 0 : left + 1, isEndReached ? top + 1 : top);
        }

        public void MoveCursorUp(int left) => this.SetCursorPosition(left, Console.CursorTop - 1);

        public void MoveCursorDown(int left) => this.SetCursorPosition(left, Console.CursorTop + 1);

        public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

        public (int left, int top) GetCursorPosition() => (Console.CursorLeft, Console.CursorTop);

        public void HideCursor() => Console.CursorVisible = false;

        public void ShowCursor() => Console.CursorVisible = true;

        public void ClearBack(int charCount)
        {
            for (int i = 0; charCount-- > i;)
            {
                this.MoveCursorLeft();
                this.console.Out.Write("\x1b[1P");
            }
        }

        public void ClearCurrentLine()
        {
            this.SetCursorPosition(0, Console.CursorTop);
            this.console.Out.Write("\x1b[K");
        }

        public Task<ConsoleKeyInfo> ReadKeyAsync(CancellationToken token, bool intercept = false) =>
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                        return Console.ReadKey(intercept);

                    await Task.Delay(TimeSpan.FromMilliseconds(50), token);
                }

                return default;
            }, token);

        public async Task<string> ReadLineAsync(CancellationToken token, bool masked = false)
        {
            var builder = new StringBuilder();

            ConsoleKeyInfo key;
            do
            {
                key = await this.ReadKeyAsync(token, true);

                if (key.Key == ConsoleKey.Escape)
                {
                    builder.Clear();
                    return null;
                }
                else if (key.Key == ConsoleKey.Backspace && builder.Length > 0)
                {
                    this.ClearBack(1);
                    builder.Remove(builder.Length - 1, 1);
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    this.Write(masked ? "*" : key.KeyChar.ToString());
                    builder.Append(key.KeyChar);
                }


            } while (!token.IsCancellationRequested &&
                     key.Key != ConsoleKey.Enter &&
                     key.Key != ConsoleKey.Escape);

            if (builder.Length == 0)
                return null;

            return builder.ToString().Trim();
        }
    }
}

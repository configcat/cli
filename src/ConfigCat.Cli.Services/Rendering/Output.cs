using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Rendering
{
    public interface IOutput
    {
        void Verbose(string text, ForegroundColorSpan color = null);

        void Write(string text);
        void WriteLine(string text = null);
        void WriteGreen(string text);
        void WriteYellow(string text);
        void WriteError(string text);
        void WriteWarning(string text);
        void WriteUnderline(string text);
        void WriteBlink(string text);
        void WriteReverse(string text);
        void WriteStandout(string text);
        void WriteColored(string text, ForegroundColorSpan color);
        void WriteColoredWithBackground(string text, ForegroundColorSpan foreground, BackgroundColorSpan background);
        void WriteNonAnsiColor(string text, ConsoleColor foreground, ConsoleColor? background = null);
        void WriteNoChange();
        void WriteSuccess();

        void MoveCursorLeft();
        void MoveCursorRight();
        void MoveCursorUp(int left);
        void MoveCursorDown(int left);
        void SetCursorPosition(int left, int top);
        void ClearCurrentLine();
        void HideCursor();
        void ShowCursor();
        int CursorTop { get; }
        int CursorLeft { get; }

        bool IsOutputRedirected { get; }

        Task<ConsoleKeyInfo> ReadKeyAsync(CancellationToken token, bool intercept = false);
        Task<string> ReadLineAsync(CancellationToken token, bool masked = false);

        Spinner CreateSpinner(CancellationToken token);
        CursorHider CreateCursorHider();
        void RenderView(View view);
    }

    public class Output : IOutput
    {
        public bool IsOutputRedirected { get; } = Console.IsOutputRedirected;
        public int CursorTop => Console.CursorTop;
        public int CursorLeft => Console.CursorLeft;

        private readonly object consoleLock = new object();

        private readonly IConsole console;

        private readonly IConsole originalConsole;

        private readonly bool showVerboseOutput;

        public Output(IConsole console, bool showVerboseOutput)
        {
            this.console = console.GetTerminal() ?? console;
            this.originalConsole = console;
            this.showVerboseOutput = showVerboseOutput;
        }

        public void Verbose(string text, ForegroundColorSpan color = null)
        {
            if (!this.showVerboseOutput)
                return;

            lock (this.consoleLock)
            {
                this.WriteColored("[verbose]: ", ForegroundColorSpan.DarkGray());
                this.WriteColored(text, color ?? ForegroundColorSpan.DarkGray());
                this.WriteLine();
            }
        }

        public void Write(string text) => this.console.Out.Write(text);

        public void WriteLine(string text = null) => this.console.Out.WriteLine(text);

        public void WriteError(string text)
        {
            lock (this.consoleLock)
            {
                this.WriteColored("[error]: ", ForegroundColorSpan.DarkGray());
                this.console.WriteError(text);
            }
        }

        public void WriteWarning(string text)
        {
            lock (this.consoleLock)
            {
                this.WriteColored("[warning]: ", ForegroundColorSpan.DarkGray());
                this.WriteYellow(text);
            }
        }

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
            lock (this.consoleLock)
            {
                this.Write($"No changes detected... ");
                this.WriteYellow("Skipped.");
            }
        }

        public void WriteSuccess() => this.WriteGreen($"Ok.");

        public Spinner CreateSpinner(CancellationToken token) => new Spinner(token, this, this.showVerboseOutput);

        public CursorHider CreateCursorHider() => new CursorHider(this);

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
            var isEndReached = left >= width - 1;
            var newTop = isEndReached ? top + 1 : top;
            if (newTop >= Console.BufferHeight)
                Console.BufferHeight++;

            this.SetCursorPosition(isEndReached ? 0 : left + 1, newTop);
        }

        public void MoveCursorUp(int left) => this.SetCursorPosition(left, Console.CursorTop - 1);

        public void MoveCursorDown(int left) => this.SetCursorPosition(left, Console.CursorTop + 1);

        public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

        public (int left, int top) GetCursorPosition() => (Console.CursorLeft, Console.CursorTop);

        public void HideCursor() => Console.CursorVisible = false;

        public void ShowCursor() => Console.CursorVisible = true;

        public void ClearCurrentLine()
        {
            this.SetCursorPosition(0, Console.CursorTop);
            this.console.Out.Write("\x1b[K");
        }

        public async Task<string> ReadLineAsync(CancellationToken token, bool masked = false)
        {
            var builder = new StringBuilder();
            int position = 0;
            var initialLeft = this.CursorLeft;
            ConsoleKeyInfo key;
            do
            {
                key = await this.ReadKeyAsync(token, true);

                if (key.Key == ConsoleKey.Escape)
                {
                    builder.Clear();
                    return null;
                }
                else if (key.Key == ConsoleKey.Backspace && position > 0)
                {
                    using var _ = this.CreateCursorHider();
                    var chunk = position < builder.Length ? builder.ToString().Substring(position) : string.Empty;
                    builder.Remove(position - 1, 1);
                    this.MoveCursorLeft();
                    var left = this.CursorLeft;
                    this.Write($"{(masked ? new string('*', chunk.Length) : chunk)} ");
                    this.SetCursorPosition(left, this.GetTopPositionFromInitial(initialLeft, --position, builder.Length));
                }
                else if (key.Key == ConsoleKey.Delete && position < builder.Length)
                {
                    using var _ = this.CreateCursorHider();
                    var left = this.CursorLeft;
                    var chunk = position < builder.Length ? builder.ToString().Substring(position + 1) : string.Empty;
                    builder.Remove(position, 1);
                    this.Write($"{(masked ? new string('*', chunk.Length) : chunk)} ");
                    this.SetCursorPosition(left, this.GetTopPositionFromInitial(initialLeft, position, builder.Length));
                }
                else if (key.Key == ConsoleKey.LeftArrow && position > 0)
                {
                    this.MoveCursorLeft();
                    position--;
                }
                else if (key.Key == ConsoleKey.RightArrow && position < builder.Length)
                {
                    this.MoveCursorRight();
                    position++;
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    using var _ = this.CreateCursorHider();
                    var left = this.CursorLeft;
                    var chunk = position < builder.Length ? builder.ToString().Substring(position) : string.Empty;
                    builder.Insert(position, key.KeyChar);
                    this.Write($"{(masked ? new string('*', chunk.Length + 1) : key.KeyChar + chunk)}");
                    this.SetCursorPosition(left, this.GetTopPositionFromInitial(initialLeft, position++, builder.Length - 1));
                    this.MoveCursorRight();
                }

            } while (!token.IsCancellationRequested &&
                     key.Key != ConsoleKey.Enter &&
                     key.Key != ConsoleKey.Escape);

            if (builder.Length == 0)
                return null;

            return builder.ToString().Trim();
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

        private int GetTopPositionFromInitial(int intial, int position, int textLength)
        {
            var whichLine = (intial + position) / Console.BufferWidth;
            var lineCount = (intial + textLength) / Console.BufferWidth;
            return this.CursorTop - lineCount + whichLine;
        }
    }
}

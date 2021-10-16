using ConfigCat.Cli.Models;
using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Rendering
{
    public interface IOutput
    {
        IOutput Verbose(string text, ForegroundColorSpan color = null);

        IOutput Write(string text);
        IOutput WriteLine(string text = null);
        IOutput WriteGreen(string text);
        IOutput WriteYellow(string text);
        IOutput WriteError(string text);
        IOutput WriteWarning(string text);
        IOutput WriteUnderline(string text);
        IOutput WriteBlink(string text);
        IOutput WriteReverse(string text);
        IOutput WriteStandout(string text);
        IOutput WriteColored(string text, ForegroundColorSpan color);
        IOutput WriteNonAnsiColor(string text, ConsoleColor foreground, ConsoleColor? background = null);
        IOutput WriteNoChange(string noChangeText = "No changes detected... ");
        IOutput WriteSuccess();

        IOutput MoveCursorLeft();
        IOutput MoveCursorRight();
        IOutput MoveCursorUp(int left);
        IOutput MoveCursorDown(int left);
        IOutput SetCursorPosition(int left, int top);
        IOutput ClearCurrentLine();
        IOutput HideCursor();
        IOutput ShowCursor();
        int CursorTop { get; }
        int CursorLeft { get; }
        int BufferHeight { get; }
        bool IsOutputRedirected { get; }

        Task<ConsoleKeyInfo> ReadKeyAsync(CancellationToken token, bool intercept = false);
        Task<string> ReadLineAsync(CancellationToken token, bool masked = false);

        Spinner CreateSpinner(CancellationToken token);
        CursorHider CreateCursorHider();
        IOutput RenderView(View view);
        IOutput RenderJson(object toRender);
    }

    public class Output : IOutput
    {
        public bool IsOutputRedirected { get; } = Console.IsOutputRedirected;
        public int CursorTop => Console.CursorTop;
        public int CursorLeft => Console.CursorLeft;
        public int BufferHeight => Console.BufferHeight;

        private readonly object consoleLock = new object();

        private readonly IConsole console;

        private readonly IConsole originalConsole;

        private readonly bool showVerboseOutput;

        public Output(IConsole console, CliOptions options)
        {
            this.console = console.GetTerminal() ?? console;
            this.originalConsole = console;
            this.showVerboseOutput = options.IsVerboseEnabled;
        }

        public IOutput Verbose(string text, ForegroundColorSpan color = null)
        {
            if (!this.showVerboseOutput)
                return this;

            lock (this.consoleLock)
            {
                this.WriteColored("[verbose]: ", ForegroundColorSpan.DarkGray());
                this.WriteColored(text, color ?? ForegroundColorSpan.DarkGray());
                this.WriteLine();

                return this;
            }
        }

        public IOutput Write(string text) { this.console.Out.Write(text); return this; }

        public IOutput WriteLine(string text = null) { this.console.Out.WriteLine(text); return this; }

        public IOutput WriteError(string text)
        {
            lock (this.consoleLock)
            {
                this.WriteColored("[error]: ", ForegroundColorSpan.DarkGray());
                this.console.WriteError(text);

                return this;
            }
        }

        public IOutput WriteWarning(string text)
        {
            lock (this.consoleLock)
            {
                this.WriteColored("[warning]: ", ForegroundColorSpan.DarkGray());
                this.WriteYellow(text);

                return this;
            }
        }

        public IOutput WriteGreen(string text) { this.console.WriteStyle(text.Color(ForegroundColorSpan.LightGreen()), text); return this; }

        public IOutput WriteYellow(string text) { this.console.WriteStyle(text.Color(ForegroundColorSpan.LightYellow()), text); return this; }

        public IOutput WriteUnderline(string text) { this.console.WriteStyle(text.Underline(), text); return this; }

        public IOutput WriteBlink(string text) { this.console.WriteStyle(text.Blink(), text); return this; }

        public IOutput WriteReverse(string text) { this.console.WriteStyle(text.Reverse(), text); return this; }

        public IOutput WriteStandout(string text) { this.console.WriteStyle(text.Standout(), text); return this; }

        public IOutput WriteColored(string text, ForegroundColorSpan color) { this.console.WriteStyle(text.Color(color), text); return this; }

        public IOutput WriteColoredBackground(string text, BackgroundColorSpan color) { this.console.WriteStyle(text.Background(color), text); return this; }

        public IOutput WriteNonAnsiColor(string text, ConsoleColor foreground, ConsoleColor? background = null)
        {
            Console.ForegroundColor = foreground;
            if (background is not null)
                Console.BackgroundColor = background.Value;
            Console.Write(text);
            Console.ResetColor();

            return this;
        }

        public IOutput WriteColoredWithBackground(string text, ForegroundColorSpan foreground, BackgroundColorSpan background)
        {
            this.console.WriteStyle(text.ColorWithBackground(foreground, background), text); return this;
        }

        public IOutput WriteNoChange(string noChangeText = "No changes detected... ")
        {
            lock (this.consoleLock)
            {
                this.Write(noChangeText);
                this.WriteYellow("Skipped.");

                return this;
            }
        }

        public IOutput WriteSuccess() => this.WriteGreen($"Ok.");

        public Spinner CreateSpinner(CancellationToken token) => new Spinner(token, this, this.showVerboseOutput);

        public CursorHider CreateCursorHider() => new CursorHider(this);

        public IOutput RenderView(View view)
        {
            var renderer = new ConsoleRenderer(this.originalConsole, resetAfterRender: true);
            view.RenderFitToContent(renderer, this.originalConsole);
            return this;

        }

        public IOutput MoveCursorLeft()
        {
            var top = Console.CursorTop;
            var left = Console.CursorLeft;
            var width = Console.BufferWidth;
            var isStartReached = left <= 0;
            return this.SetCursorPosition(isStartReached ? width - 1 : left - 1, isStartReached ? top - 1 : top);
        }

        public IOutput MoveCursorRight()
        {
            var top = Console.CursorTop;
            var left = Console.CursorLeft;
            var width = Console.BufferWidth;
            var isEndReached = left >= width - 1;
            var newTop = isEndReached ? top + 1 : top;
            if (newTop >= Console.BufferHeight)
                Console.BufferHeight++;

            return this.SetCursorPosition(isEndReached ? 0 : left + 1, newTop);
        }

        public IOutput MoveCursorUp(int left) { this.SetCursorPosition(left, Console.CursorTop - 1); return this; }

        public IOutput MoveCursorDown(int left) { this.SetCursorPosition(left, Console.CursorTop + 1); return this; }

        public IOutput SetCursorPosition(int left, int top) { Console.SetCursorPosition(left, top); return this; }

        public (int left, int top) GetCursorPosition() => (Console.CursorLeft, Console.CursorTop);

        public IOutput HideCursor() { Console.CursorVisible = false; return this; }

        public IOutput ShowCursor() { Console.CursorVisible = true; return this; }

        public IOutput ClearCurrentLine()
        {
            this.SetCursorPosition(0, Console.CursorTop);
            this.console.Out.Write("\x1b[K");
            return this;
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

        public IOutput RenderJson(object toRender) =>
            this.Write(JsonSerializer.Serialize(toRender, Constants.CamelCaseOptions));

    }
}

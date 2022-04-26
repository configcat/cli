using ConfigCat.Cli.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Rendering
{
    public interface IOutput
    {
        IOutput Verbose(string text, ConsoleColor color = default);
        IOutput Write(string text);
        IOutput WriteLine(string text = null);
        IOutput WriteGreen(string text);
        IOutput WriteYellow(string text);
        IOutput WriteDarkGray(string text);
        IOutput WriteMagenta(string text);
        IOutput WriteCyan(string text);
        IOutput WriteError(string text);
        IOutput WriteWarning(string text);
        IOutput WriteColor(string text, ConsoleColor foreground, ConsoleColor? background = null);
        IOutput WriteNoChange(string noChangeText = "No changes detected... ");
        IOutput WriteSuccess();

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
        IOutput RenderTable<T>(IEnumerable<T> items);
        IOutput RenderJson(object toRender);
    }

    public class Output : IOutput
    {
        public bool IsOutputRedirected { get; } = Console.IsOutputRedirected;
        public int CursorTop => Console.CursorTop;
        public int CursorLeft => Console.CursorLeft;
        public int BufferHeight => Console.BufferHeight;

        private readonly object consoleLock = new();

        private readonly CliOptions options;

        public Output(CliOptions options)
        {
            this.options = options;
        }

        public IOutput Verbose(string text, ConsoleColor color = default)
        {
            if (!this.options.IsVerboseEnabled)
                return this;

            lock (this.consoleLock)
            {
                this.WriteDarkGray("[verbose]: ");
                if (color != default)
                    this.WriteColor(text, color);
                else
                    this.WriteDarkGray(text);

                this.WriteLine();

                return this;
            }
        }

        public IOutput Write(string text) { Console.Write(text); return this; }

        public IOutput WriteLine(string text = null) { Console.WriteLine(text); return this; }

        public IOutput WriteError(string text)
        {
            lock (this.consoleLock)
            {
                if (!this.IsOutputRedirected)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"[error]: {text}");
                    Console.ResetColor();
                }
                else
                    Console.Error.WriteLine(text);

                return this;
            }
        }

        public IOutput WriteWarning(string text)
        {
            lock (this.consoleLock)
            {
                this.WriteYellow($"[warning]: {text}");
                return this;
            }
        }

        public IOutput WriteGreen(string text) => this.WriteColor(text, ConsoleColor.Green);

        public IOutput WriteYellow(string text) => this.WriteColor(text, ConsoleColor.Yellow);

        public IOutput WriteDarkGray(string text) => this.WriteColor(text, ConsoleColor.DarkGray);

        public IOutput WriteMagenta(string text) => this.WriteColor(text, ConsoleColor.Magenta);

        public IOutput WriteCyan(string text) => this.WriteColor(text, ConsoleColor.Cyan);

        public IOutput WriteColor(string text, ConsoleColor foreground, ConsoleColor? background = null)
        {
            if (this.IsOutputRedirected)
            {
                this.Write(text);
                return this;
            }

            Console.ForegroundColor = foreground;
            if (background is not null)
                Console.BackgroundColor = background.Value;
            Console.Write(text);
            Console.ResetColor();

            return this;
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

        public Spinner CreateSpinner(CancellationToken token) => new Spinner(token, this, this.options.IsVerboseEnabled, this.options.IsNonInteractive);

        public CursorHider CreateCursorHider() => new(this);

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

        public IOutput HideCursor() { Console.CursorVisible = false; return this; }

        public IOutput ShowCursor() { Console.CursorVisible = true; return this; }

        public IOutput ClearCurrentLine()
        {
            Console.SetCursorPosition(0, this.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, this.CursorTop);
            return this;
        }

        public async Task<string> ReadLineAsync(CancellationToken token, bool masked = false)
        {
            var builder = new StringBuilder();
            var position = 0;
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

        public IOutput RenderTable<T>(IEnumerable<T> items)
        {
            const int padding = 2;

            var builder = new StringBuilder();
            var itemType = typeof(T);
            var properties = itemType.GetProperties();

            var headers = properties.Select(p => p.Name).ToArray();
            var values = items.Select(i => properties.Select(p => p.GetValue(i)?.ToString() ?? string.Empty).ToArray());

            var lengthCounter = values.ToList();
            lengthCounter.Add(headers);

            var maxColumnLengths = headers.Select((h, i) => lengthCounter.Select(c => c[i].Length + padding).Max());

            builder.AppendLine(FormatLine(headers.Select(h => h.ToUpperInvariant()), maxColumnLengths));
            builder.AppendLine(FormatLine(headers.Select(h => new string('-', h.Length)), maxColumnLengths));
            foreach (var item in values)
                builder.AppendLine(FormatLine(item, maxColumnLengths));

            this.Write(builder.ToString());
            return this;


            string FormatLine(IEnumerable<string> texts, IEnumerable<int> columnLengths) =>
                texts.Select((t, i) => FormatColumn(t, columnLengths.ElementAt(i)))
                    .Aggregate((a, b) => a + b);

            string FormatColumn(string text, int columnLength) => $"{text}{new string(' ', columnLength - text.Length)}";
        }

        public IOutput RenderJson(object toRender)
        {
            var jsonOptions = this.options.IsVerboseEnabled
                ? Constants.PrettyFormattedCamelCaseOptions
                : Constants.CamelCaseOptions;
            return this.Write(JsonSerializer.Serialize(toRender, jsonOptions));
        }

    }
}
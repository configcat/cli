using System;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Rendering
{
    public interface IOutput
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
        void ClearBack(int charCount);
        void ClearCurrentLine();
        void HideCursor();
        void ShowCursor();
        int CursorTop { get; }
        int CursorLeft { get; }

        bool IsOutputRedirected { get; }

        Task<ConsoleKeyInfo> ReadKeyAsync(CancellationToken token, bool intercept = false);
        Task<string> ReadLineAsync(CancellationToken token, bool masked = false);

        Spinner CreateSpinner(CancellationToken token);
        void RenderView(View view);
    }
}

using System;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Rendering
{
    public class Spinner : IDisposable
    {
        private readonly CancellationTokenSource combinedToken;
        private readonly ITerminal terminal;
        private readonly int top;
        private readonly int left;
        private readonly bool isVerboseEnabled;

        private static string[] SpinnerFragments = new[]
        {
            "/",
            "-",
            "\\",
            "|",
        };

        public Spinner(CancellationToken token, IConsole console, bool isVerboseEnabled)
        {
            if (isVerboseEnabled)
                return;

            if (console is not ITerminal terminal)
                return;

            this.terminal = terminal;

            this.top = terminal.CursorTop;
            this.left = terminal.CursorLeft;
            this.combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token);

            this.terminal.HideCursor();

            var task = Task.Run(async () =>
            {
                var counter = 0;
                while (!this.combinedToken.IsCancellationRequested)
                {
                    var spinnerFragment = SpinnerFragments[counter++ % SpinnerFragments.Length];

                    this.terminal.SetCursorPosition(this.left, this.top);
                    this.terminal.Out.Write(spinnerFragment);
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(70), this.combinedToken.Token);
                    }
                    catch (OperationCanceledException)
                    { }
                }
            });
            this.isVerboseEnabled = isVerboseEnabled;
        }

        public void Stop()
        {
            if (terminal is null || isVerboseEnabled)
                return;

            this.combinedToken.Cancel();

            var fragmentSize = SpinnerFragments[0].Length;
            this.terminal.SetCursorPosition(this.left + fragmentSize, this.top);
            this.terminal.Out.Write(string.Concat(Enumerable.Repeat("\b \b", fragmentSize)));
            this.terminal.SetCursorPosition(this.left, this.top);
            this.terminal.ShowCursor();
        }

        public void Dispose() => this.Stop();
    }
}

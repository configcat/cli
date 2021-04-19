using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Rendering
{
    public class Spinner : IDisposable
    {
        private readonly CancellationTokenSource combinedToken;
        private readonly IOutput output;
        private readonly CursorHider cursorHider;
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

        public Spinner(CancellationToken token, IOutput output, bool isVerboseEnabled)
        {
            if (isVerboseEnabled)
                return;

            if (output.IsOutputRedirected)
                return;

            this.output = output;

            this.top = output.CursorTop;
            this.left = output.CursorLeft;
            this.combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token);

            this.cursorHider = output.CreateCursorHider();

            var task = Task.Run(async () =>
            {
                var counter = 0;
                while (!this.combinedToken.IsCancellationRequested)
                {
                    var spinnerFragment = SpinnerFragments[counter++ % SpinnerFragments.Length];

                    this.output.SetCursorPosition(this.left, this.top);
                    this.output.Write(spinnerFragment);
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(50), this.combinedToken.Token);
                    }
                    catch (OperationCanceledException)
                    { }
                }
            });
            this.isVerboseEnabled = isVerboseEnabled;
        }

        public void Stop()
        {
            if (this.output is null || isVerboseEnabled)
                return;

            this.combinedToken.Cancel();

            var fragmentSize = SpinnerFragments[0].Length;
            this.output.SetCursorPosition(this.left + fragmentSize, this.top);
            this.output.Write(string.Concat(Enumerable.Repeat("\b \b", fragmentSize)));
            this.output.SetCursorPosition(this.left, this.top);
            this.cursorHider.ShowCursor();
        }

        public void Dispose() => this.Stop();
    }
}

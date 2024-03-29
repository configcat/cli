using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Rendering;

public class Spinner : IDisposable
{
    private readonly CancellationTokenSource spinnerTokenSource;
    private readonly CancellationTokenSource linkedTokenSource;
    private readonly IOutput output;
    private readonly CursorHider cursorHider;
    private readonly int top;
    private readonly int left;
    private readonly bool isVerboseEnabled;

    private static readonly string[] SpinnerFragments =
    [
        "/",
        "-",
        "\\",
        "|"
    ];

    public Spinner(CancellationToken token, IOutput output, bool isVerboseEnabled, bool isNonInteractive)
    {
        if (isVerboseEnabled || isNonInteractive || output.IsOutputRedirected)
            return;

        this.output = output;
        this.cursorHider = output.CreateCursorHider();

        this.top = output.CursorTop;
        this.left = output.CursorLeft;
        this.spinnerTokenSource = new CancellationTokenSource();
        this.linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, this.spinnerTokenSource.Token);

        var task = Task.Run(async () =>
        {
            var counter = 0;
            while (!this.linkedTokenSource.Token.IsCancellationRequested)
            {
                var spinnerFragment = SpinnerFragments[counter++ % SpinnerFragments.Length];

                this.output.SetCursorPosition(this.left, this.top);
                this.output.Write(spinnerFragment);
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50), this.linkedTokenSource.Token);
                }
                catch (OperationCanceledException)
                { }
            }
        }, this.linkedTokenSource.Token);
        this.isVerboseEnabled = isVerboseEnabled;
    }

    private void Stop()
    {
        if (this.output is null || this.isVerboseEnabled)
            return;

        this.spinnerTokenSource.Cancel();
        this.spinnerTokenSource.Dispose();
        this.linkedTokenSource.Dispose();

        var fragmentSize = SpinnerFragments[0].Length;
        this.output.SetCursorPosition(this.left + fragmentSize, this.top);
        this.output.Write(string.Concat(Enumerable.Repeat("\b \b", fragmentSize)));
        this.output.SetCursorPosition(this.left, this.top);
        this.cursorHider.ShowCursor();
    }

    public void Dispose() => this.Stop();
}
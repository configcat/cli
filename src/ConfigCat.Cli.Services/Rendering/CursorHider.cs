using System;

namespace ConfigCat.Cli.Services.Rendering
{
    public class CursorHider : IDisposable
    {
        private readonly IOutput output;

        public CursorHider(IOutput output)
        {
            this.output = output;
            output.HideCursor();
        }

        public void ShowCursor() => this.output?.ShowCursor();

        public void Dispose() => this.ShowCursor();
    }
}
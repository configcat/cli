using ConfigCat.Cli.Services;
using System.CommandLine.IO;

namespace System.CommandLine.Rendering.Views
{
    public static class ViewExtensions
    {
        public static void RenderFitToContent(this View view, ConsoleRenderer renderer, IConsole console)
        {
            var measured = view.Measure(renderer, Constants.MaxSize);
            view.Render(renderer, new Region(0, 0, measured));
            console.Out.WriteLine();
        }
    }
}

using System.CommandLine.IO;
using System.CommandLine.Rendering;

namespace System.CommandLine
{
    static class ConsoleExtensions
    {
        public static void WriteError(this IConsole console, string value)
        {
            var terminal = console.GetTerminal();
            if (terminal != null)
            { 
                terminal.Render(value.Color(ForegroundColorSpan.LightRed()));
                console.Out.WriteLine();
            }
            else
                console.Error.WriteLine(value);
        }

        public static void WriteStyle(this IConsole console, TextSpan span, string original)
        {
            var terminal = console.GetTerminal();
            if (terminal != null)
                terminal.Render(span);
            else
                console.Out.Write(original);
        }
    }
}

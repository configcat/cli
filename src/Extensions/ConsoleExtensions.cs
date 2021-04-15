using System.CommandLine.IO;
using System.CommandLine.Rendering;

namespace System.CommandLine
{
    static class ConsoleExtensions
    {
        public static void WriteError(this IConsole console, string value)
        {
            if (console is ITerminal terminal)
            {
                terminal.Render(value.Color(ForegroundColorSpan.LightRed()));
                terminal.Out.WriteLine();
            }
            else
                console.Error.WriteLine(value);
        }

        public static void WriteErrorOnTerminal(this IConsole console, string value)
        {
            var terminal = console.GetTerminal();
            if (terminal is not null)
            {
                terminal.Render(value.Color(ForegroundColorSpan.LightRed()));
                terminal.Out.WriteLine();
            }
            else
                console.Error.WriteLine(value);
        }

        public static void WriteStyle(this IConsole console, TextSpan span, string original)
        {
            if (console is ITerminal terminal)
                terminal.Render(span);
            else
                console.Out.Write(original);
        }
    }
}

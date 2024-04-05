using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.IO;
using System.Linq;

namespace ConfigCat.Cli.Options;

public class ExtendedHelpBuilder(IConsole console, int maxWidth = int.MaxValue) : HelpBuilder(console, maxWidth)
{
    public override void Write(ICommand command)
    {
        base.Write(command);

        if (!base.GetSubcommands(command).Any()) return;
        var parents = GetCommandList(command).Reverse().ToArray();
        var parentsPart = parents.Length != 0 ? $"{string.Join(' ', parents)} " : string.Empty;
        base.Console.Out.WriteLine($"Use \"{parentsPart}[command] -?\" for more information about a command.");

        return;

        IEnumerable<string> GetCommandList(ISymbol commandToCheck)
        {
            yield return commandToCheck.Name;
            var parent = commandToCheck.Parents.FirstOrDefault(p => p is ICommand);
            while (parent != null)
            {
                yield return parent.Name;
                parent = parent.Parents.FirstOrDefault(p => p is ICommand);
            }
        }
    }

    protected override void AddUsage(ICommand command)
    {
        var description = GetUsage(command);
        if (description.Contains("[options]"))
        {
            description = description.Replace("[options] ", string.Empty) + " [options]";
        }
        WriteHeading(Resources.Instance.HelpUsageTile(), description);
        Console.Out.WriteLine();

        if (command is not ExtendedCommand extendedCommand || string.IsNullOrWhiteSpace(extendedCommand.Example)) return;
        WriteHeading("Example:", extendedCommand.Example);
        Console.Out.WriteLine();
    }
}
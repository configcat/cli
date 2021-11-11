using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.IO;
using System.Linq;

namespace ConfigCat.Cli.Options
{
    public class ExtendedHelpBuilder : HelpBuilder
    {
        public ExtendedHelpBuilder(IConsole console, int maxWidth = int.MaxValue) : base(console, maxWidth)
        { }

        public override void Write(ICommand command)
        {
            base.Write(command);

            if (base.GetSubcommands(command).Any())
            { 
                var parents = GetCommandList(command).Reverse().ToArray();
                var parentsPart = parents.Any() ? $"{string.Join(' ', parents)} " : string.Empty;
                base.Console.Out.WriteLine($"Use \"{parentsPart}[command] -?\" for more information about a command.");
            }

            IEnumerable<string> GetCommandList(ISymbol commandToCheck)
            {
                yield return commandToCheck.Name;
                var parent = commandToCheck.Parents.FirstOrDefault(p => p is ICommand);
                while(parent != null)
                {
                    yield return parent.Name;
                    parent = parent.Parents.FirstOrDefault(p => p is ICommand);
                }
            }
        }
    }
}

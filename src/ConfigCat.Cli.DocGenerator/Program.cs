using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConfigCat.Cli.DocGenerator
{
    class Program
    {
        private readonly static ExposedHelpBuilder helpBuilder = new ExposedHelpBuilder();
        private readonly static string currentPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "docs");

        static async Task Main()
        {
            if (!Directory.Exists(currentPath))
                Directory.CreateDirectory(currentPath);

            var rootCommand = CommandBuilder.BuildRootCommand(asRootCommand: false);
            var commandLineBuilder = new CommandLineBuilder(rootCommand)
                .UseVersionOption()
                .UseHelp();
            var output = new StringBuilder();

            output.AppendLine($"# Command Line Interface Reference");
            output.AppendLine("[GitHub](https://github.com/configcat/cli) | [Documentation](https://configcat.com/docs/advanced/cli)");
            output.AppendLine("");
            output.AppendLine("This is a reference for the ConfigCat CLI. It allows you to interact with the ConfigCat Management API via the command line. It supports most functionality found on the ConfigCat Dashboard. You can manage ConfigCat resources like Feature Flags, Targeting / Percentage rules, Products, Configs, Environments, and more.");

            var options = rootCommand.Options.Where(o => !o.IsHidden);
            if (options.Any())
            {
                output.AppendLine("## Options");
                output.AppendLine("| Option | Description |");
                output.AppendLine("| ------ | ----------- |");
                foreach (var option in options)
                    output.AppendLine($"| {string.Join(", ", option.Aliases.Select(a => $"`{a}`"))} | {option.Description.Replace(Environment.NewLine, "<br/>")} |");
            }

            output.AppendLine("## Commands");
            output.AppendLine("This is the complete list of the available commands provided by the CLI.");

            foreach (var subCommand in rootCommand.Children.OfType<Command>().Where(c => !c.IsHidden))
            {
                output.AppendLine($"### configcat {subCommand.Name}");
                output.AppendLine("| Command | Description |");
                output.AppendLine("| ------ | ----------- |");
                var generatedSubCommandDocs = new Dictionary<string, string>();
                await GenerateDocsForSubCommand(subCommand, new Stack<Command>(new[] { rootCommand }), generatedSubCommandDocs);

                foreach (var subCommandDoc in generatedSubCommandDocs)
                    output.AppendLine($"| {subCommandDoc.Key} | {subCommandDoc.Value} |");
            }

            await File.WriteAllTextAsync(Path.Combine(currentPath, $"index.md"), output.ToString());
        }

        static async Task GenerateDocsForSubCommand(Command command, Stack<Command> parents, IDictionary<string, string> generatedDocs)
        {
            var output = new StringBuilder();

            var parentNamesInOrder = parents.Reverse().Select(c => c.Name);
            var selfName = parentNamesInOrder.Append(command.Name);

            var selfLink = ProduceMarkdownLinkFromNames(selfName);
            generatedDocs.Add(selfLink, command.Description.Replace(Environment.NewLine, "<br/>"));

            output.AppendLine($"# {string.Join(' ', selfName)}");
            output.AppendLine("[GitHub](https://github.com/configcat/cli) | [Documentation](https://configcat.com/docs/advanced/cli)");
            output.AppendLine("");
            output.AppendLine(command.Description.Replace(Environment.NewLine, "<br/>"));
            output.AppendLine("## Usage");
            output.AppendLine("```");
            output.AppendLine(helpBuilder.ExposeGetUsage(command));
            output.AppendLine("```");

            var aliases = command.Aliases.Except(new[] { command.Name });
            if (aliases.Any())
            {
                output.AppendLine("## Aliases");
                output.AppendLine(string.Join(", ", aliases.Select(a => $"`{a}`")));
            }

            var options = command.Options.Where(o => !o.IsHidden).Concat(parents.SelectMany(p => p.GlobalOptions.Where(go => !go.IsHidden)));
            if (options.Any())
            {
                output.AppendLine("## Options");
                output.AppendLine("| Option | Description |");
                output.AppendLine("| ------ | ----------- |");
                foreach (var option in options)
                    output.AppendLine($"| {string.Join(", ", option.Aliases.Select(a => $"`{a}`"))} | {option.Description.Replace(Environment.NewLine, "<br/>")} |");
            }

            var arguments = helpBuilder.ExposeGetCommandArguments(command);
            if (arguments.Any())
            {
                output.AppendLine("## Arguments");
                output.AppendLine("| Argument | Description |");
                output.AppendLine("| ------ | ----------- |");
                foreach (var argument in arguments)
                    output.AppendLine($"| `{argument.Descriptor}` | {argument.Description.Replace(Environment.NewLine, "<br/>")} |");
            }

            output.AppendLine("## Parent Command");
            output.AppendLine("| Command | Description |");
            output.AppendLine("| ------ | ----------- |");
            output.AppendLine($"| {ProduceMarkdownLinkFromNames(parentNamesInOrder)} | {parents.Peek().Description.Replace(Environment.NewLine, "<br/>")} |");

            var subCommands = command.Children.OfType<Command>();
            if (subCommands.Any())
            {
                output.AppendLine("## Subcommands");
                output.AppendLine("| Command | Description |");
                output.AppendLine("| ------ | ----------- |");
                parents.Push(command);
                var commandNameChain = parents.Reverse().Where(c => !c.IsHidden).Select(c => c.Name);

                foreach (var subCommand in subCommands)
                {
                    var commandNames = commandNameChain.Append(subCommand.Name);
                    output.AppendLine($"| {ProduceMarkdownLinkFromNames(commandNames)} | {subCommand.Description.Replace(Environment.NewLine, "<br/>")} |");
                    await GenerateDocsForSubCommand(subCommand, parents, generatedDocs);
                }

                parents.Pop();
            }
            var path = Path.Combine(currentPath, $"{string.Join('-', selfName)}.md");
            await File.WriteAllTextAsync(path, output.ToString());
        }

        private static string ProduceMarkdownLinkFromNames(IEnumerable<string> names)
            => $"[{string.Join(' ', names)}]({(names.Count() == 1 ? "index" : string.Join('-', names))}.md)";
    }
}

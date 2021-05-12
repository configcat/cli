using ConfigCat.Cli.Commands;
using ConfigCat.Cli.Options;
using ConfigCat.Cli.Services;
using Stashbox;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace ConfigCat.Cli
{
    public class CommandBuilder
    {
        public readonly static Option VerboseOption = new VerboseOption();

        public static Command BuildRootCommand(IDependencyRegistrator dependencyRegistrator = null, bool asRootCommand = true)
        {
            var root = BuildDescriptors();
            var rootCommand = asRootCommand ? new RootCommand(root.Description) : new Command("configcat", root.Description);
            rootCommand.AddGlobalOption(VerboseOption);
            rootCommand.Configure(root.SubCommands, dependencyRegistrator);
            return rootCommand;
        }

        private static CommandDescriptor BuildDescriptors() =>
            new CommandDescriptor(null, $"This is the Command Line Tool of ConfigCat.{System.Environment.NewLine}ConfigCat is a " +
                $"hosted feature flag service: https://configcat.com{System.Environment.NewLine}For more information, " +
                $"see the documentation here: https://configcat.com/docs/advanced/cli")
            {
                SubCommands = new CommandDescriptor[]
                {
                    BuildSetupCommand(),
                    BuildListAllCommand(),
                    BuildProductCommand(),
                    BuildConfigCommand(),
                    BuildEnvironmentCommand(),
                    BuildTagCommand(),
                    BuildFlagCommand(),
                    BuildSdkKeyCommand(),
                    BuildScanCommand(),
                    BuildCatCommand(),
                }
            };

        private static CommandDescriptor BuildSetupCommand() =>
            new CommandDescriptor("setup", $"Setup the CLI with Management API host and credentials." +
                        $"{System.Environment.NewLine}You can get your credentials from here: https://app.configcat.com/my-account/public-api-credentials")
            {
                Options = new[]
                {
                    new Option<string>(new[] { "--api-host", "-H" }, $"The Management API host, also used from {Constants.ApiHostEnvironmentVariableName}. (default '{Constants.DefaultApiHost}')"),
                    new Option<string>(new[] { "--username", "-u" }, $"The Management API basic authentication username, also used from {Constants.ApiUserNameEnvironmentVariableName}"),
                    new Option<string>(new[] { "--password", "-p" }, $"The Management API basic authentication password, also used from {Constants.ApiPasswordEnvironmentVariableName}"),
                },
                Handler = CreateHandler<Setup>(nameof(Setup.InvokeAsync))
            };

        private static CommandDescriptor BuildListAllCommand() =>
            new CommandDescriptor("ls", "List all Product, Config, and Environment IDs")
            {
                Handler = CreateHandler<ListAll>(nameof(ListAll.InvokeAsync))
            };

        private static CommandDescriptor BuildProductCommand() =>
            new CommandDescriptor("product", "Manage Products")
            {
                Aliases = new[] { "p" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all Products that belongs to the configured user")
                    {
                        Handler = CreateHandler<Product>(nameof(Product.ListAllProductsAsync))
                    },
                    new CommandDescriptor("create", "Create a new Product in a specified Organization identified by the `--organization-id` option")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Product>(nameof(Product.CreateProductAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--organization-id", "-o" }, "The Organization's ID where the Product must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "Name of the new Product"),
                        }
                    },
                    new CommandDescriptor("rm", "Remove a Product identified by the `--product-id` option")
                    {
                        Handler = CreateHandler<Product>(nameof(Product.DeleteProductAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-i" }, "ID of the Product to delete"),
                        }
                    },
                    new CommandDescriptor("update", "Update a Product identified by the `--product-id` option")
                    {
                        Aliases = new [] { "up" },
                        Handler = CreateHandler<Product>(nameof(Product.UpdateProductAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-i" }, "ID of the Product to update"),
                            new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                        }
                    },
                },
            };

        private static CommandDescriptor BuildConfigCommand() =>
            new CommandDescriptor("config", "Manage Configs")
            {
                Aliases = new[] { "c" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all Configs that belongs to the configured user")
                    {
                        Options = new[]
                        {
                            new Option<string>(new string[] { "--product-id", "-p" }, "Show only a Product's Config"),
                        },
                        Handler = CreateHandler<Config>(nameof(Config.ListAllConfigsAsync))
                    },
                    new CommandDescriptor("create", "Create a new Config in a specified Product identified by the `--product-id` option")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Config>(nameof(Config.CreateConfigAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-p" }, "ID of the Product where the Config must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "Name of the new Config"),
                        }
                    },
                    new CommandDescriptor("rm", "Remove a Config identified by the `--config-id` option")
                    {
                        Handler = CreateHandler<Config>(nameof(Config.DeleteConfigAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--config-id", "-i" }, "ID of the Config to delete"),
                        }
                    },
                    new CommandDescriptor("update", "Update a Config identified by the `--config-id` option")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<Config>(nameof(Config.UpdateConfigAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--config-id", "-i" }, "ID of the Config to update"),
                            new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                        }
                    },
                },
            };

        private static CommandDescriptor BuildEnvironmentCommand() =>
            new CommandDescriptor("environment", "Manage Environments")
            {
                Aliases = new[] { "e" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all Environments that belongs to the configured user")
                    {
                        Options = new[]
                        {
                            new Option<string>(new string[] { "--product-id", "-p" }, "Show only a Product's Environments"),
                        },
                        Handler = CreateHandler<Environment>(nameof(Environment.ListAllEnvironmentsAsync))
                    },
                    new CommandDescriptor("create", "Create a new Environment in a specified Product identified by the `--product-id` option")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Environment>(nameof(Environment.CreateEnvironmentAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-p" }, "ID of the Product where the Environment must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "Name of the new Environment"),
                        }
                    },
                    new CommandDescriptor("rm", "Remove an Environment identified by the `--environment-id` option")
                    {
                        Handler = CreateHandler<Environment>(nameof(Environment.DeleteEnvironmentAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--environment-id", "-i" }, "ID of the Environment to delete"),
                        }
                    },
                    new CommandDescriptor("update", "Update environment")
                    {
                        Aliases = new [] { "up" },
                        Handler = CreateHandler<Environment>(nameof(Environment.UpdateEnvironmentAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--environment-id", "-i" }, "ID of the environment to update"),
                            new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                        }
                    },
                },
            };

        private static CommandDescriptor BuildTagCommand() =>
            new CommandDescriptor("tag", "Manage Tags")
            {
                Aliases = new[] { "t" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all Tags that belongs to the configured user")
                    {
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-p" }, "Show only a Product's tags"),
                        },
                        Handler = CreateHandler<Tag>(nameof(Tag.ListAllTagsAsync))
                    },
                    new CommandDescriptor("create", "Create a new Tag in a specified Product identified by the `--product-id` option")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Tag>(nameof(Tag.CreateTagAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-p" }, "ID of the Product where the Tag must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "The name of the new Tag"),
                            new Option<string>(new[] { "--color", "-c" }, "The color of the new Tag"),
                        }
                    },
                    new CommandDescriptor("rm", "Remove a Tag identified by the `--tag-id` option")
                    {
                        Handler = CreateHandler<Tag>(nameof(Tag.DeleteTagAsync)),
                        Options = new[]
                        {
                            new Option<int>(new[] { "--tag-id", "-i" }, "ID of the Tag to delete"),
                        }
                    },
                    new CommandDescriptor("update", "Update a Tag identified by the `--tag-id` option")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<Tag>(nameof(Tag.UpdateTagAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--tag-id", "-i" }, "ID of the Tag to update"),
                            new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                            new Option<string>(new[] { "--color", "-c" }, "The updated color"),
                        }
                    },
                },
            };


        private static CommandDescriptor BuildFlagCommand() =>
            new CommandDescriptor("flag", "Manage Feature Flags & Settings")
            {
                Aliases = new[] { "setting", "f", "s" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all Feature Flags & Settings that belongs to the configured user")
                    {
                        Options = new Option[]
                        {
                            new Option<string>(new[] { "--config-id", "-c" }, "Show only a Config's Flags & Settings"),
                            new Option<string>(new[] { "--tag-name", "-n" }, "Filter by a Tag's name"),
                            new Option<int>(new[] { "--tag-id", "-t" }, "Filter by a Tag's ID"),
                        },
                        Handler = CreateHandler<Flag>(nameof(Flag.ListAllFlagsAsync))
                    },
                    new CommandDescriptor("create", "Create a new Feature Flag or Setting in a specified Config identified by the `--config-id` option")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Flag>(nameof(Flag.CreateFlagAsync)),
                        Options = new Option[]
                        {
                            new Option<string>(new[] { "--config-id", "-c" }, "ID of the Config where the Flag must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "Name of the new Flag or Setting"),
                            new Option<string>(new[] { "--key", "-k" }, "Key of the new Flag or Setting (must be unique within the given Config)"),
                            new Option<string>(new[] { "--hint", "-H" }, "Hint of the new Flag or Setting"),
                            new Option<string>(new[] { "--type", "-t" }, "Type of the new Flag or Setting")
                                .AddSuggestions(SettingTypes.Collection),
                            new Option<int[]>(new[] { "--tag-ids", "-g" }, "Tags to attach"),
                        }
                    },
                    new CommandDescriptor("rm", "Remove a Feature Flag or Setting identified by the `--flag-id` option")
                    {
                        Handler = CreateHandler<Flag>(nameof(Flag.DeleteFlagAsync)),
                        Options = new[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting to delete")
                            {
                                Name = "--flag-id"
                            },
                        }
                    },
                    new CommandDescriptor("update", "Update a Feature Flag or Setting identified by the `--flag-id` option")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<Flag>(nameof(Flag.UpdateFlagAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting to update")
                            {
                                Name = "--flag-id"
                            },
                            new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                            new Option<string>(new[] { "--hint", "-H" }, "The updated hint"),
                            new Option<int[]>(new[] { "--tag-ids", "-g" }, "The updated Tag list"),
                        }
                    },
                    new CommandDescriptor("attach", "Attach Tag(s) to a Feature Flag or Setting")
                    {
                        Aliases = new[] { "at" },
                        Handler = CreateHandler<Flag>(nameof(Flag.AttachTagsAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting to attach Tags")
                            {
                                Name = "--flag-id"
                            },
                            new Option<int[]>(new[] { "--tag-ids", "-g" }, "Tag IDs to attach"),
                        }
                    },
                    new CommandDescriptor("detach", "Detach Tag(s) from a Feature Flag or Setting")
                    {
                        Aliases = new[] { "dt" },
                        Handler = CreateHandler<Flag>(nameof(Flag.DetachTagsAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting to detach Tags")
                            {
                                Name = "--flag-id"
                            },
                            new Option<int[]>(new[] { "--tag-ids", "-g" }, "Tag IDs to detach"),
                        }
                    },

                    BuildFlagValueCommand(),
                    BuildFlagTargetingCommand(),
                    BuildFlagPercentageCommand()
                },
            };


        private static CommandDescriptor BuildFlagValueCommand() =>
            new CommandDescriptor("value", "Show, and update Feature Flag or Setting values in different Environments")
            {
                Aliases = new[] { "v" },
                SubCommands = new[]
                {
                    new CommandDescriptor("show", "Show Feature Flag or Setting values, targeting, and percentage rules for each environment")
                    {
                        Aliases = new[] { "sh", "pr", "print" },
                        Handler = CreateHandler<FlagValue>(nameof(FlagValue.ListAllAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting")
                            {
                                Name = "--flag-id"
                            },
                        }
                    },
                    new CommandDescriptor("update", "Update the value of a Feature Flag or Setting")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<FlagValue>(nameof(FlagValue.UpdateFlagValueAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting")
                            {
                                Name = "--flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the Environment where the update must be applied"),
                            new Option<string>(new[] { "--flag-value", "-f" }, "The value to serve, it must respect the setting type"),
                        }
                    }
                }
            };

        private static CommandDescriptor BuildFlagTargetingCommand() =>
            new CommandDescriptor("targeting", "Manage targeting rules")
            {
                Aliases = new[] { "t" },
                SubCommands = new[]
                {
                    new CommandDescriptor("create", "Create new targeting rule")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<FlagTargeting>(nameof(FlagTargeting.AddTargetinRuleAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting")
                            {
                                Name = "--flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the Environment where the rule must be created"),
                            new Option<string>(new[] { "--attribute", "-a" }, "The user attribute to compare"),
                            new Option<string>(new[] { "--comparator", "-c" }, "The comparison operator")
                                    .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                            new Option<string>(new[] { "--compare-to", "-t" }, "The value to compare against"),
                            new Option<string>(new[] { "--flag-value", "-f" }, "The value to serve when the comparison matches, it must respect the setting type"),
                        }
                    },
                    new CommandDescriptor("update", "Update targeting rule")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<FlagTargeting>(nameof(FlagTargeting.UpdateTargetinRuleAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting")
                            {
                                Name = "--flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the Environment where the update must be applied"),
                            new Option<int?>(new[] { "--position", "-p" }, "The position of the updating targeting rule"),
                            new Option<string>(new[] { "--attribute", "-a" }, "The user attribute to compare"),
                            new Option<string>(new[] { "--comparator", "-c" }, "The comparison operator")
                                .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                            new Option<string>(new[] { "--compare-to", "-t" }, "The value to compare against"),
                            new Option<string>(new[] { "--flag-value", "-f" }, "The value to serve when the comparison matches, it must respect the setting type"),
                        }
                    },
                    new CommandDescriptor("rm", "Delete targeting rule")
                    {
                        Handler = CreateHandler<FlagTargeting>(nameof(FlagTargeting.DeleteTargetinRuleAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting")
                            {
                                Name = "--flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the Environment from where the rule must be deleted"),
                            new Option<int?>(new[] { "--position", "-p" }, "The position of the targeting rule to delete"),
                        }
                    },
                    new CommandDescriptor("move", "Move a targeting rule into a different position")
                    {
                        Aliases = new[] { "mv" },
                        Handler = CreateHandler<FlagTargeting>(nameof(FlagTargeting.MoveTargetinRuleAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting")
                            {
                                Name = "--flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the Environment where the move must be applied"),
                            new Option<int?>(new[] { "--from" }, "The position of the targeting rule to delete"),
                            new Option<int?>(new[] { "--to" }, "The desired position of the targeting rule"),
                        }
                    },
                }
            };

        private static CommandDescriptor BuildFlagPercentageCommand() =>
            new CommandDescriptor("percentage", "Manage percentage rules")
            {
                Aliases = new[] { "%" },
                SubCommands = new[]
                {
                    new CommandDescriptor("update", "Update percentage rules")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<FlagPercentage>(nameof(FlagPercentage.UpdatePercentageRulesAsync)),
                        Arguments = new Argument[]
                        {
                            new PercentageRuleArgument()
                        },
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting")
                            {
                                Name = "--flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the Environment where the update must be applied"),
                        }
                    },
                    new CommandDescriptor("clear", "Delete all percentage rules")
                    {
                        Aliases = new[] { "clr" },
                        Handler = CreateHandler<FlagPercentage>(nameof(FlagPercentage.DeletePercentageRulesAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting")
                            {
                                Name = "--flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the Environment from where the rules must be deleted"),
                        }
                    },
                }
            };

        private static CommandDescriptor BuildSdkKeyCommand() =>
            new CommandDescriptor("sdk-key", "List SDK Keys")
            {
                Aliases = new[] { "k" },
                Handler = CreateHandler<SdkKey>(nameof(SdkKey.InvokeAsync)),
            };

        private static CommandDescriptor BuildScanCommand() =>
            new CommandDescriptor("scan", "Scans files for Feature Flag or Setting usages")
            {
                Handler = CreateHandler<Scan>(nameof(Scan.InvokeAsync)),
                Arguments = new[]
                {
                    new Argument<DirectoryInfo>("directory", "Directory to scan").ExistingOnly(),
                },
                Options = new Option[]
                {
                    new Option<string>(new[] { "--config-id", "-c" }, "ID of the Config to scan against"),
                    new Option<int>(new[] { "--line-count", "-l" }, () => 4, "Context line count before and after the reference line"),
                    new Option<bool>(new[] { "--print", "-p" }, "Print found references"),
                }
            };

        private static CommandDescriptor BuildCatCommand() =>
            new CommandDescriptor("whoisthebestcat", "Well, who?")
            {
                Aliases = new[] { "cat" },
                Handler = CreateHandler<Cat>(nameof(Cat.InvokeAsync)),
                IsHidden = true,
            };

        private static HandlerDescriptor CreateHandler<THandler>(string methodName)
        {
            var handlerType = typeof(THandler);
            return new HandlerDescriptor(handlerType, handlerType.GetMethod(methodName));
        }
    }
}

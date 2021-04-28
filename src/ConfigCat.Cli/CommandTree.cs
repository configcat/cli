using ConfigCat.Cli.Commands;
using ConfigCat.Cli.Options;
using ConfigCat.Cli.Services;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace ConfigCat.Cli
{
    class CommandTree
    {
        public static CommandDescriptor Build() =>
            new CommandDescriptor(null, $"This is the Command Line Tool of ConfigCat.{System.Environment.NewLine}ConfigCat is a " +
                $"hosted feature flag service: https://configcat.com{System.Environment.NewLine}For more information, " +
                $"see the documentation here: https://configcat.com/docs/")
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
                    new Option<string>(new[] { "--api-host", "-s" }, $"The Management API host, also used from {Constants.ApiHostEnvironmentVariableName}. (default '{Constants.DefaultApiHost}')"),
                    new Option<string>(new[] { "--username", "-u" }, $"The Management API basic authentication username, also used from {Constants.ApiUserNameEnvironmentVariableName}"),
                    new Option<string>(new[] { "--password", "-p" }, $"The Management API basic authentication password, also used from {Constants.ApiPasswordEnvironmentVariableName}"),
                },
                Handler = CreateHandler<Setup>(nameof(Setup.InvokeAsync))
            };

        private static CommandDescriptor BuildListAllCommand() =>
            new CommandDescriptor("ls", "List all products, configs, and environments IDs")
            {
                Handler = CreateHandler<ListAll>(nameof(ListAll.InvokeAsync))
            };

        private static CommandDescriptor BuildProductCommand() =>
            new CommandDescriptor("product", "Manage products")
            {
                Aliases = new[] { "p" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all products")
                    {
                        Handler = CreateHandler<Product>(nameof(Product.ListAllProductsAsync))
                    },
                    new CommandDescriptor("create", "Create product")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Product>(nameof(Product.CreateProductAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--organization-id", "-o" }, "The organization's ID where the product must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "Name of the new product"),
                        }
                    },
                    new CommandDescriptor("rm", "Delete product")
                    {
                        Handler = CreateHandler<Product>(nameof(Product.DeleteProductAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-i" }, "ID of the product to delete"),
                        }
                    },
                    new CommandDescriptor("update", "Update product")
                    {
                        Aliases = new [] { "up" },
                        Handler = CreateHandler<Product>(nameof(Product.UpdateProductAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-i" }, "ID of the product to update"),
                            new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                        }
                    },
                },
            };

        private static CommandDescriptor BuildConfigCommand() =>
            new CommandDescriptor("config", "Manage configs")
            {
                Aliases = new[] { "c" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all configs")
                    {
                        Options = new[]
                        {
                            new Option<string>(new string[] { "--product-id", "-p" }, "Show only a product's configs"),
                        },
                        Handler = CreateHandler<Config>(nameof(Config.ListAllConfigsAsync))
                    },
                    new CommandDescriptor("create", "Create config")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Config>(nameof(Config.CreateConfigAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-p" }, "ID of the product where the config must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "Name of the new config"),
                        }
                    },
                    new CommandDescriptor("rm", "Delete config")
                    {
                        Handler = CreateHandler<Config>(nameof(Config.DeleteConfigAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--config-id", "-i" }, "ID of the config to delete"),
                        }
                    },
                    new CommandDescriptor("update", "Update Config")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<Config>(nameof(Config.UpdateConfigAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--config-id", "-i" }, "ID of the config to update"),
                            new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                        }
                    },
                },
            };

        private static CommandDescriptor BuildEnvironmentCommand() =>
            new CommandDescriptor("environment", "Manage environments")
            {
                Aliases = new[] { "e" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all environments")
                    {
                        Options = new[]
                        {
                            new Option<string>(new string[] { "--product-id", "-p" }, "Show only a product's environments"),
                        },
                        Handler = CreateHandler<Environment>(nameof(Environment.ListAllEnvironmentsAsync))
                    },
                    new CommandDescriptor("create", "Create environment")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Environment>(nameof(Environment.CreateEnvironmentAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-p" }, "ID of the product where the environment must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "Name of the new environment"),
                        }
                    },
                    new CommandDescriptor("rm", "Delete environment")
                    {
                        Handler = CreateHandler<Environment>(nameof(Environment.DeleteEnvironmentAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--environment-id", "-i" }, "ID of the environment to delete"),
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
            new CommandDescriptor("tag", "Manage tags")
            {
                Aliases = new[] { "t" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all tags")
                    {
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-p" }, "Show only a product's tags"),
                        },
                        Handler = CreateHandler<Tag>(nameof(Tag.ListAllTagsAsync))
                    },
                    new CommandDescriptor("create", "Create tag")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Tag>(nameof(Tag.CreateTagAsync)),
                        Options = new[]
                        {
                            new Option<string>(new[] { "--product-id", "-p" }, "ID of the product where the tag must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "The name of the new tag"),
                            new Option<string>(new[] { "--color", "-c" }, "The color of the new tag"),
                        }
                    },
                    new CommandDescriptor("rm", "Delete tag")
                    {
                        Handler = CreateHandler<Tag>(nameof(Tag.DeleteTagAsync)),
                        Options = new[]
                        {
                            new Option<int>(new[] { "--tag-id", "-i" }, "ID of the tag to delete"),
                        }
                    },
                    new CommandDescriptor("update", "Update tag")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<Tag>(nameof(Tag.UpdateTagAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--tag-id", "-i" }, "ID of the tag to update"),
                            new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                            new Option<string>(new[] { "--color", "-c" }, "The updated color"),
                        }
                    },
                },
            };


        private static CommandDescriptor BuildFlagCommand() =>
            new CommandDescriptor("flag", "Manage flags & settings")
            {
                Aliases = new[] { "setting", "f", "s" },
                SubCommands = new[]
                {
                    new CommandDescriptor("ls", "List all flags")
                    {
                        Options = new Option[]
                        {
                            new Option<string>(new[] { "--config-id", "-c" }, "Show only a config's flags"),
                            new Option<string>(new[] { "--tag-name", "-n" }, "Filter by a tag's name"),
                            new Option<int>(new[] { "--tag-id", "-t" }, "Filter by a tag's ID"),
                        },
                        Handler = CreateHandler<Flag>(nameof(Flag.ListAllFlagsAsync))
                    },
                    new CommandDescriptor("create", "Create flag")
                    {
                        Aliases = new[] { "cr" },
                        Handler = CreateHandler<Flag>(nameof(Flag.CreateFlagAsync)),
                        Options = new Option[]
                        {
                            new Option<string>(new[] { "--config-id", "-c" }, "ID of the config where the flag must be created"),
                            new Option<string>(new[] { "--name", "-n" }, "Name of the new flag"),
                            new Option<string>(new[] { "--key", "-k" }, "Key of the new flag"),
                            new Option<string>(new[] { "--hint", "-d" }, "Hint of the new flag"),
                            new Option<string>(new[] { "--type", "-t" }, "Type of the new flag")
                                .AddSuggestions(SettingTypes.Collection),
                            new Option<int[]>(new[] { "--tag-ids", "-g" }, "Tags to attach"),
                        }
                    },
                    new CommandDescriptor("rm", "Delete flag")
                    {
                        Handler = CreateHandler<Flag>(nameof(Flag.DeleteFlagAsync)),
                        Options = new[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting to delete")
                            {
                                Name = "flag-id"
                            },
                        }
                    },
                    new CommandDescriptor("update", "Update flag")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<Flag>(nameof(Flag.UpdateFlagAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting to update")
                            {
                                Name = "flag-id"
                            },
                            new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                            new Option<string>(new[] { "--hint", "-d" }, "The updated hint"),
                            new Option<int[]>(new[] { "--tag-ids", "-g" }, "The updated tag list"),
                        }
                    },
                    new CommandDescriptor("attach", "Attach tag(s) to a flag")
                    {
                        Aliases = new[] { "at" },
                        Handler = CreateHandler<Flag>(nameof(Flag.AttachTagsAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting to attach tags")
                            {
                                Name = "flag-id"
                            },
                            new Option<int[]>(new[] { "--tag-ids", "-g" }, "Tag IDs to attach"),
                        }
                    },
                    new CommandDescriptor("detach", "Detach tag(s) from a flag")
                    {
                        Aliases = new[] { "dt" },
                        Handler = CreateHandler<Flag>(nameof(Flag.DetachTagsAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting to detach tags")
                            {
                                Name = "flag-id"
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
            new CommandDescriptor("value", "Show, and update flag values in different environments")
            {
                Aliases = new[] { "v" },
                SubCommands = new[]
                {
                    new CommandDescriptor("show", "Show flag values, targeting, and percentage rules for each environment")
                    {
                        Aliases = new[] { "sh", "pr", "print" },
                        Handler = CreateHandler<FlagValue>(nameof(FlagValue.ListAllAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                            {
                                Name = "flag-id"
                            },
                        }
                    },
                    new CommandDescriptor("update", "Update the flag's value")
                    {
                        Aliases = new[] { "up" },
                        Handler = CreateHandler<FlagValue>(nameof(FlagValue.UpdateFlagValueAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                            {
                                Name = "flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the update must be applied"),
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
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                            {
                                Name = "flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the rule must be created"),
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
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                            {
                                Name = "flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the update must be applied"),
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
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                            {
                                Name = "flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment from where the rule must be deleted"),
                            new Option<int?>(new[] { "--position", "-p" }, "The position of the targeting rule to delete"),
                        }
                    },
                    new CommandDescriptor("move", "Move a targeting rule into a different position")
                    {
                        Aliases = new[] { "mv" },
                        Handler = CreateHandler<FlagTargeting>(nameof(FlagTargeting.MoveTargetinRuleAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                            {
                                Name = "flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the move must be applied"),
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
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                            {
                                Name = "flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment where the update must be applied"),
                        }
                    },
                    new CommandDescriptor("clear", "Delete all percentage rules")
                    {
                        Aliases = new[] { "clr" },
                        Handler = CreateHandler<FlagPercentage>(nameof(FlagPercentage.DeletePercentageRulesAsync)),
                        Options = new Option[]
                        {
                            new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the flag or setting")
                            {
                                Name = "flag-id"
                            },
                            new Option<string>(new[] { "--environment-id", "-e" }, "ID of the environment from where the rules must be deleted"),
                        }
                    },
                }
            };

        private static CommandDescriptor BuildSdkKeyCommand() =>
            new CommandDescriptor("sdk-key", "List sdk keys")
            {
                Aliases = new[] { "k" },
                Handler = CreateHandler<SdkKey>(nameof(SdkKey.InvokeAsync)),
            };

        private static CommandDescriptor BuildScanCommand() =>
            new CommandDescriptor("scan", "Scans files for feature flag or setting usages")
            {
                Handler = CreateHandler<Scan>(nameof(Scan.InvokeAsync)),
                Arguments = new[]
                {
                    new Argument<DirectoryInfo>("directory", "Directory to scan").ExistingOnly(),
                },
                Options = new Option[]
                {
                    new Option<string>(new[] { "--config-id", "-c" }, "ID of the config to scan against"),
                    new Option<int>(new[] { "--line-count", "-l" }, () => 4, "Context line count before and after the reference line"),
                }
            };

        private static CommandDescriptor BuildCatCommand() =>
            new CommandDescriptor("whoisthebestcat", "Well, who?")
            {
                Aliases = new[] { "cat" },
                Handler = CreateHandler<Cat>(nameof(Cat.InvokeAsync)),
            };

        private static HandlerDescriptor CreateHandler<THandler>(string methodName)
        {
            var handlerType = typeof(THandler);
            return new HandlerDescriptor(handlerType, handlerType.GetMethod(methodName));
        }
    }
}

using System.Collections.Generic;
using ConfigCat.Cli.Commands;
using ConfigCat.Cli.Commands.Flags;
using ConfigCat.Cli.Options;
using ConfigCat.Cli.Services;
using Stashbox;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ConfigCat.Cli.Commands.PermissionGroups;

namespace ConfigCat.Cli;

public static class CommandBuilder
{
    public static readonly Option VerboseOption = new VerboseOption();
    public static readonly Option NonInteractiveOption = new NonInteractiveOption();

    public static Command BuildRootCommand(IDependencyRegistrator dependencyRegistrator = null, bool asRootCommand = true)
    {
        var root = BuildDescriptors();
        var rootCommand = asRootCommand ? new RootCommand(root.Description) : new Command("configcat", root.Description);
        rootCommand.AddGlobalOption(VerboseOption);
        rootCommand.AddGlobalOption(NonInteractiveOption);
        rootCommand.Configure(root.SubCommands, dependencyRegistrator);
        return rootCommand;
    }

    private static CommandDescriptor BuildDescriptors() =>
        new(null, $"This is the Command Line Tool of ConfigCat.{System.Environment.NewLine}ConfigCat is a " +
                  $"hosted feature flag service: https://configcat.com{System.Environment.NewLine}For more information, " +
                  $"see the documentation here: https://configcat.com/docs/advanced/cli")
        {
            SubCommands = new[]
            {
                BuildSetupCommand(),
                BuildListAllCommand(),
                BuildProductCommand(),
                BuildConfigCommand(),
                BuildEnvironmentCommand(),
                BuildFlagCommand(),
                BuildSegmentCommand(),
                BuildPermissionGroupCommand(),
                BuildMemberCommand(),
                BuildTagCommand(),
                BuildSdkKeyCommand(),
                BuildScanCommand(),
                BuildCatCommand(),
            }
        };

    private static CommandDescriptor BuildSetupCommand() =>
        new("setup", $"Setup the CLI with Public Management API host and credentials")
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
        new("ls", "List all Product, Config, and Environment IDs")
        {
            Handler = CreateHandler<ListAll>(nameof(ListAll.InvokeAsync)),
            Options = new[]
            {
                new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
            }
        };

    private static CommandDescriptor BuildProductCommand() =>
        new("product", "Manage Products")
        {
            Aliases = new[] { "p" },
            SubCommands = new[]
            {
                new CommandDescriptor("ls", "List all Products that belongs to the configured user")
                {
                    Handler = CreateHandler<Product>(nameof(Product.ListAllProductsAsync)),
                    Options = new[]
                    {
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("create", "Create a new Product in a specified Organization identified by the `--organization-id` option")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<Product>(nameof(Product.CreateProductAsync)),
                    Options = new[]
                    {
                        new Option<string>(new[] { "--organization-id", "-o" }, "The Organization's ID where the Product must be created"),
                        new Option<string>(new[] { "--name", "-n" }, "Name of the new Product"),
                        new Option<string>(new[] { "--description", "-d" }, "Description of the new Product"),
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
                        new Option<string>(new[] { "--description", "-d" }, "The updated description"),
                    }
                },
            },
        };

    private static CommandDescriptor BuildMemberCommand() =>
        new("member", "Manage Members")
        {
            Aliases = new[] { "m" },
            SubCommands = new[]
            {
                new CommandDescriptor("lso", "List all Members that belongs to an Organization")
                {
                    Handler = CreateHandler<Member>(nameof(Member.ListOrganizationMembersAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--organization-id", "-o" }, "Show only an Organization's Members"),
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("lsp", "List all Members that belongs to a Product")
                {
                    Handler = CreateHandler<Member>(nameof(Member.ListProductMembersAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--product-id", "-p" }, "Show only a Product's Members"),
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("rm", "Remove Member from an Organization")
                {
                    Handler = CreateHandler<Member>(nameof(Member.RemoveMemberFromOrganizationAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--organization-id", "-o" }, "The Organization's ID from where the Member must be removed"),
                        new Option<string>(new[] { "--user-id", "-i" }, "ID of the Member to remove"),
                    }
                },

                new CommandDescriptor("invite", "Invite Member(s) into a Product")
                {
                    Aliases = new [] { "inv" },
                    Handler = CreateHandler<Member>(nameof(Member.InviteMembersAsync)),
                    Arguments = new[]
                    {
                        new Argument<string[]>("emails", "List of email addresses to invite")
                    },
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--product-id", "-p" }, "The Product's ID to where the Members will be invited"),
                        new Option<long?>(new[] { "--permission-group-id", "-pgi" }, "The Permission Group's ID to where the invited Members will join"),
                    }
                },
                new CommandDescriptor("add-permission", "Add Member to Permission Groups")
                {
                    Aliases = new [] { "a" },
                    Handler = CreateHandler<Member>(nameof(Member.AddPermissionsAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--organization-id", "-o" }, "ID of the Organization"),
                        new Option<string>(new[] { "--user-id", "-i" }, "ID of the Member to add"),
                        new Option<long[]>(new[] { "--permission-group-ids", "-pgi" }, "Permission Group IDs the Member must be put into"),
                    }
                },
                new CommandDescriptor("rm-permission", "Remove Member from Permission Groups")
                {
                    Aliases = new [] { "rmp" },
                    Handler = CreateHandler<Member>(nameof(Member.RemovePermissionsAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--organization-id", "-o" }, "ID of the Organization"),
                        new Option<string>(new[] { "--user-id", "-i" }, "ID of the Member to remove"),
                        new Option<long[]>(new[] { "--permission-group-ids", "-pgi" }, "Permission Group IDs the Member must be removed from"),
                    }
                },
            },
        };


    private static CommandDescriptor BuildConfigCommand() =>
        new("config", "Manage Configs")
        {
            Aliases = new[] { "c" },
            SubCommands = new[]
            {
                new CommandDescriptor("ls", "List all Configs that belongs to the configured user")
                {
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--product-id", "-p" }, "Show only a Product's Configs"),
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
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
                        new Option<string>(new[] { "--description", "-d" }, "Description of the new Config"),
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
                        new Option<string>(new[] { "--description", "-d" }, "The updated description"),
                    }
                },
            },
        };

    private static CommandDescriptor BuildPermissionGroupCommand() =>
        new("permission-group", "Manage Permission Groups")
        {
            Aliases = new[] { "pg" },
            SubCommands = new[]
            {
                new CommandDescriptor("ls", "List all Permission Groups that manageable by the configured user")
                {
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--product-id", "-p" }, "Show only a Product's Permission Groups"),
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
                    },
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.ListAllPermissionGroupsAsync))
                },
                new CommandDescriptor("create", "Create a new Permission Group in a specified Product identified by the `--product-id` option")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.CreatePermissionGroupAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--product-id", "-p" }, "ID of the Product where the Permission Group must be created"),
                        new Option<string>(new[] { "--name", "-n" }, "Name of the new Permission Group"),
                        new Option<bool>(new[] { "--can-manage-members" }, Constants.Permissions[0]),
                        new Option<bool>(new[] { "--can-create-or-update-config" }, Constants.Permissions[1]),
                        new Option<bool>(new[] { "--can-delete-config" }, Constants.Permissions[2]),
                        new Option<bool>(new[] { "--can-create-or-update-environment" }, Constants.Permissions[3]),
                        new Option<bool>(new[] { "--can-delete-environment" }, Constants.Permissions[4]),
                        new Option<bool>(new[] { "--can-create-or-update-setting" }, Constants.Permissions[5]),
                        new Option<bool>(new[] { "--can-tag-setting" }, Constants.Permissions[6]),
                        new Option<bool>(new[] { "--can-delete-setting" }, Constants.Permissions[7]),
                        new Option<bool>(new[] { "--can-create-or-update-tag" }, Constants.Permissions[8]),
                        new Option<bool>(new[] { "--can-delete-tag" }, Constants.Permissions[9]),
                        new Option<bool>(new[] { "--can-manage-webhook" }, Constants.Permissions[10]),
                        new Option<bool>(new[] { "--can-use-export-import" }, Constants.Permissions[11]),
                        new Option<bool>(new[] { "--can-manage-product-preferences" }, Constants.Permissions[12]),
                        new Option<bool>(new[] { "--can-manage-integrations" }, Constants.Permissions[13]),
                        new Option<bool>(new[] { "--can-view-sdk-key" }, Constants.Permissions[14]),
                        new Option<bool>(new[] { "--can-rotate-sdk-key" }, Constants.Permissions[15]),
                        new Option<bool>(new[] { "--can-view-product-statistics" }, Constants.Permissions[16]),
                        new Option<bool>(new[] { "--can-view-product-audit-log" }, Constants.Permissions[17]),
                        new Option<bool>(new[] { "--can-create-or-update-segments" }, Constants.Permissions[18]),
                        new Option<bool>(new[] { "--can-delete-segments" }, Constants.Permissions[19]),
                    }
                },
                new CommandDescriptor("rm", "Remove a Permission Group identified by the `--permission-group-id` option")
                {
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.DeletePermissionGroupAsync)),
                    Options = new[]
                    {
                        new Option<long?>(new[] { "--permission-group-id", "-i" }, "ID of the Permission Group to delete"),
                    }
                },
                new CommandDescriptor("update", "Update a Permission Group identified by the `--permission-group-id` option")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.UpdatePermissionGroupAsync)),
                    Options = new Option[]
                    {
                        new Option<long?>(new[] { "--permission-group-id", "-i" }, "ID of the Permission Group to update"),
                        new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                        new Option<bool?>(new[] { "--can-manage-members" }, Constants.Permissions[0]),
                        new Option<bool?>(new[] { "--can-create-or-update-config" }, Constants.Permissions[1]),
                        new Option<bool?>(new[] { "--can-delete-config" }, Constants.Permissions[2]),
                        new Option<bool?>(new[] { "--can-create-or-update-environment" }, Constants.Permissions[3]),
                        new Option<bool?>(new[] { "--can-delete-environment" }, Constants.Permissions[4]),
                        new Option<bool?>(new[] { "--can-create-or-update-setting" }, Constants.Permissions[5]),
                        new Option<bool?>(new[] { "--can-tag-setting" }, Constants.Permissions[6]),
                        new Option<bool?>(new[] { "--can-delete-setting" }, Constants.Permissions[7]),
                        new Option<bool?>(new[] { "--can-create-or-update-tag" }, Constants.Permissions[8]),
                        new Option<bool?>(new[] { "--can-delete-tag" }, Constants.Permissions[9]),
                        new Option<bool?>(new[] { "--can-manage-webhook" }, Constants.Permissions[10]),
                        new Option<bool?>(new[] { "--can-use-export-import" }, Constants.Permissions[11]),
                        new Option<bool?>(new[] { "--can-manage-product-preferences" }, Constants.Permissions[12]),
                        new Option<bool?>(new[] { "--can-manage-integrations" }, Constants.Permissions[13]),
                        new Option<bool?>(new[] { "--can-view-sdk-key" }, Constants.Permissions[14]),
                        new Option<bool?>(new[] { "--can-rotate-sdk-key" }, Constants.Permissions[15]),
                        new Option<bool?>(new[] { "--can-view-product-statistics" }, Constants.Permissions[16]),
                        new Option<bool?>(new[] { "--can-view-product-audit-log" }, Constants.Permissions[17]),
                        new Option<bool?>(new[] { "--can-create-or-update-segments" }, Constants.Permissions[18]),
                        new Option<bool?>(new[] { "--can-delete-segments" }, Constants.Permissions[19]),
                    }
                },
                new CommandDescriptor("show", "Show details of a Permission Group identified by the `--permission-group-id` option")
                {
                    Aliases = new[] { "sh", "pr", "print" },
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.ShowPermissionGroupAsync)),
                    Options = new Option[]
                    {
                        new Option<long?>(new[] { "--permission-group-id", "-i" }, "ID of the Permission Group"),
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("env", "Update the environment specific permissions of a Permission Group")
                {
                    Handler = CreateHandler<PermissionGroupEnvironmentAccess>(nameof(PermissionGroupEnvironmentAccess.UpdatePermissionGroupEnvironmentAccessesAsync)),
                    Options = new Option[]
                    {
                        new Option<long?>(new[] { "--permission-group-id", "-i" }, "ID of the Permission Group"),
                        new Option<string>(new[] { "--access-type", "-a" }, "Access configuration for all environments")
                            .AddSuggestions(Constants.AccessTypes.Keys.ToArray()),
                        new Option<string>(new[] { "--new-environment-access-type", "-na" }, "Access configuration for newly created environments. Interpreted only when the --access-type option is `custom` which translates to `Environment specific`")
                            .AddSuggestions(Constants.EnvironmentAccessTypes.Keys.ToArray()),
                        new PermissionGroupEnvironmentAccessOption()
                    }
                },
            },
        };

    private static CommandDescriptor BuildSegmentCommand() =>
        new("segment", "Manage Segments")
        {
            Aliases = new[] { "seg" },
            SubCommands = new[]
            {
                new CommandDescriptor("ls", "List all Segments that belongs to the configured user")
                {
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--product-id", "-p" }, "Show only a Product's Segments"),
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
                    },
                    Handler = CreateHandler<Segment>(nameof(Segment.ListAllSegmentsAsync))
                },
                new CommandDescriptor("create", "Create a new Segment in a specified Product identified by the `--product-id` option")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<Segment>(nameof(Segment.CreateSegmentAsync)),
                    Options = new[]
                    {
                        new Option<string>(new[] { "--product-id", "-p" }, "ID of the Product where the Segment must be created"),
                        new Option<string>(new[] { "--name", "-n" }, "Name of the new Segment"),
                        new Option<string>(new[] { "--description", "-d" }, "Description of the new Segment"),
                        new Option<string>(new[] { "--attribute", "-a" }, "The user attribute to compare"),
                        new Option<string>(new[] { "--comparator", "-c" }, "The comparison operator")
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                        new Option<string>(new[] { "--compare-to", "-t" }, "The value to compare against"),
                    }
                },
                new CommandDescriptor("rm", "Remove a Segment identified by the `--segment-id` option")
                {
                    Handler = CreateHandler<Segment>(nameof(Segment.DeleteSegmentAsync)),
                    Options = new[]
                    {
                        new Option<string>(new[] { "--segment-id", "-i" }, "ID of the Segment to delete"),
                    }
                },
                new CommandDescriptor("update", "Update a Segment identified by the `--segment-id` option")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<Segment>(nameof(Segment.UpdateSegmentAsync)),
                    Options = new[]
                    {
                        new Option<string>(new[] { "--segment-id", "-i" }, "ID of the Segment to update"),
                        new Option<string>(new[] { "--name", "-n" }, "The updated name"),
                        new Option<string>(new[] { "--description", "-d" }, "The updated description"),
                        new Option<string>(new[] { "--attribute", "-a" }, "The user attribute to compare"),
                        new Option<string>(new[] { "--comparator", "-c" }, "The comparison operator")
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                        new Option<string>(new[] { "--compare-to", "-t" }, "The value to compare against"),
                    }
                },
                new CommandDescriptor("show", "Show details of a Segment identified by the `--segment-id` option")
                {
                    Aliases = new[] { "sh", "pr", "print" },
                    Handler = CreateHandler<Segment>(nameof(Segment.GetSegmentDetailsAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--segment-id", "-i" }, "ID of the Segment"),
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
                    }
                },
            },
        };

    private static CommandDescriptor BuildEnvironmentCommand() =>
        new("environment", "Manage Environments")
        {
            Aliases = new[] { "e" },
            SubCommands = new[]
            {
                new CommandDescriptor("ls", "List all Environments that belongs to the configured user")
                {
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--product-id", "-p" }, "Show only a Product's Environments"),
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
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
                        new Option<string>(new[] { "--description", "-d" }, "Description of the new Environment"),
                        new Option<string>(new[] { "--color", "-c" }, "Color of the new Environment"),
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
                        new Option<string>(new[] { "--description", "-d" }, "The updated description"),
                        new Option<string>(new[] { "--color", "-c" }, "The updated color"),
                    }
                },
            },
        };

    private static CommandDescriptor BuildTagCommand() =>
        new("tag", "Manage Tags")
        {
            Aliases = new[] { "t" },
            SubCommands = new[]
            {
                new CommandDescriptor("ls", "List all Tags that belongs to the configured user")
                {
                    Options = new Option[]
                    {
                        new Option<string>(new[] { "--product-id", "-p" }, "Show only a Product's tags"),
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
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
        new("flag", "Manage Feature Flags & Settings")
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
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
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
        new("value", "Manage Feature Flag & Setting values in different Environments")
        {
            Aliases = new[] { "v" },
            SubCommands = new[]
            {
                new CommandDescriptor("show", "Show Feature Flag or Setting values, targeting, and percentage rules for each environment")
                {
                    Aliases = new[] { "sh", "pr", "print" },
                    Handler = CreateHandler<FlagValue>(nameof(FlagValue.ShowValueAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(new[] { "--flag-id", "-i", "--setting-id" }, "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
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
        new("targeting", "Manage targeting rules")
        {
            Aliases = new[] { "t" },
            SubCommands = new[]
            {
                new CommandDescriptor("create", "Create new targeting rule")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<FlagTargeting>(nameof(FlagTargeting.AddTargetingRuleAsync)),
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
                        new Option<string>(new[] { "--segment-id", "-si" }, "ID of the Segment used in the rule"),
                        new Option<string>(new[] { "--segment-comparator", "-sc" }, "The segment comparison operator")
                            .AddSuggestions(Constants.SegmentComparatorTypes.Keys.ToArray()),
                    }
                },
                new CommandDescriptor("update", "Update targeting rule")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<FlagTargeting>(nameof(FlagTargeting.UpdateTargetingRuleAsync)),
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
                        new Option<string>(new[] { "--segment-id", "-si" }, "ID of the Segment used in the rule"),
                        new Option<string>(new[] { "--segment-comparator", "-sc" }, "The segment comparison operator")
                            .AddSuggestions(Constants.SegmentComparatorTypes.Keys.ToArray()),
                    }
                },
                new CommandDescriptor("rm", "Delete targeting rule")
                {
                    Handler = CreateHandler<FlagTargeting>(nameof(FlagTargeting.DeleteTargetingRuleAsync)),
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
                    Handler = CreateHandler<FlagTargeting>(nameof(FlagTargeting.MoveTargetingRuleAsync)),
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
        new("percentage", "Manage percentage rules")
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
        new("sdk-key", "List SDK Keys")
        {
            Aliases = new[] { "k" },
            Handler = CreateHandler<SdkKey>(nameof(SdkKey.InvokeAsync)),
            Options = new[]
            {
                new Option<bool>(new[] { "--json" }, "Format the output in JSON"),
            }
        };

    private static CommandDescriptor BuildScanCommand() =>
        new("scan", "Scan files for Feature Flag & Setting usages")
        {
            Handler = CreateHandler<Scan>(nameof(Scan.InvokeAsync)),
            Arguments = new[]
            {
                new Argument<DirectoryInfo>("directory", "Directory to scan").ExistingOnly(),
            },
            Options = new Option[]
            {
                new Option<string>(new[] { "--config-id", "-c" }, "ID of the Config to scan against"),
                new Option<int>(new[] { "--line-count", "-l" }, () => 4, "Context line count before and after the reference line (min: 1, max: 10)"),
                new Option<bool>(new[] { "--print", "-p" }, "Print found references to output"),
                new Option<bool>(new[] { "--upload", "-u" }, "Upload references to ConfigCat"),
                new Option<string>(new[] { "--repo", "-r" }, "Repository name. Mandatory for code reference upload"),
                new Option<string>(new[] { "--branch", "-b" }, "Branch name. When the scanned folder is a Git repo, it's determined automatically, otherwise, it must be set manually"),
                new Option<string>(new[] { "--commit-hash", "-cm" }, "Commit hash. When the scanned folder is a Git repo, it's determined automatically, otherwise, it must be set manually"),
                new Option<string>(new[] { "--file-url-template", "-f" }, "Template url used to generate VCS file links. Available template parameters: `commitHash`, `filePath`, `lineNumber`. Example: https://github.com/my/repo/blob/{commitHash}/{filePath}#L{lineNumber}"),
                new Option<string>(new[] { "--commit-url-template", "-ct" }, "Template url used to generate VCS commit links. Available template parameters: `commitHash`. Example: https://github.com/my/repo/commit/{commitHash}"),
                new Option<string>(new[] { "--runner", "-ru" }, "Overrides the default `ConfigCat CLI {version}` executor label on the ConfigCat dashboard"),
                new Option<string[]>(new[] { "--exclude-flag-keys", "-ex" }, "Exclude the given Feature Flag keys from scanning. E.g.: `-ex flag1 flag2` or `-ex 'flag1,flag2'`"),

            }
        };

    private static CommandDescriptor BuildCatCommand() =>
        new("whoisthebestcat", "Well, who?")
        {
            Aliases = new[] { "cat" },
            Handler = CreateHandler<Cat>(nameof(Cat.InvokeAsync)),
            IsHidden = true,
        };

    private static HandlerDescriptor CreateHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(string methodName)
    {
        var handlerType = typeof(THandler);
        return new HandlerDescriptor(handlerType, handlerType.GetMethod(methodName));
    }
}
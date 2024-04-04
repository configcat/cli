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
using ConfigCat.Cli.Commands.ConfigJson;
using ConfigCat.Cli.Commands.Flags.V2;
using Workspace = ConfigCat.Cli.Models.Configuration.Workspace;

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
                  $"see the documentation here: https://configcat.com/docs/advanced/cli", string.Empty)
        {
            SubCommands = new[]
            {
                BuildSetupCommand(),
                BuildListAllCommand(),
                BuildProductCommand(),
                BuildConfigCommand(),
                BuildEnvironmentCommand(),
                BuildFlagCommand(),
                BuildFlagV2Command(),
                BuildSegmentCommand(),
                BuildPermissionGroupCommand(),
                BuildMemberCommand(),
                BuildTagCommand(),
                BuildSdkKeyCommand(),
                BuildScanCommand(),
                BuildCatCommand(),
                BuildConfigJsonCommand(),
                BuildWorkspaceCommand(),
            }
        };

    private static CommandDescriptor BuildSetupCommand() =>
        new("setup", $"Setup the CLI with Public Management API host and credentials", "configcat setup -H api.configcat.com -u <user-name> -p <password>")
        {
            Options = new[]
            {
                new Option<string>(["--api-host", "-H"], $"The Management API host, also used from {Constants.ApiHostEnvironmentVariableName}. (default '{Constants.DefaultApiHost}')"),
                new Option<string>(["--username", "-u"], $"The Management API basic authentication username, also used from {Constants.ApiUserNameEnvironmentVariableName}"),
                new Option<string>(["--password", "-p"], $"The Management API basic authentication password, also used from {Constants.ApiPasswordEnvironmentVariableName}"),
            },
            Handler = CreateHandler<Setup>(nameof(Setup.InvokeAsync))
        };

    private static CommandDescriptor BuildListAllCommand() =>
        new("ls", "List all Product, Config, and Environment IDs", "configcat ls")
        {
            Handler = CreateHandler<ListAll>(nameof(ListAll.InvokeAsync)),
            Options = new[]
            {
                new Option<bool>(["--json"], "Format the output in JSON"),
            }
        };

    private static CommandDescriptor BuildProductCommand() =>
        new("product", "Manage Products")
        {
            Aliases = new[] { "p" },
            SubCommands = new[]
            {
                new CommandDescriptor("ls", "List all Products that belongs to the configured user", "configcat product ls")
                {
                    Handler = CreateHandler<Product>(nameof(Product.ListAllProductsAsync)),
                    Options = new[]
                    {
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("create", "Create a new Product in a specified Organization identified by the `--organization-id` option", "configcat product create -o <organization-id> -n \"My Product\" -d \"Product Description\"")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<Product>(nameof(Product.CreateProductAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--organization-id", "-o"], "The Organization's ID where the Product must be created"),
                        new Option<string>(["--name", "-n"], "Name of the new Product"),
                        new Option<string>(["--description", "-d"], "Description of the new Product"),
                    }
                },
                new CommandDescriptor("rm", "Remove a Product identified by the `--product-id` option", "configcat product rm -i <product-id>")
                {
                    Handler = CreateHandler<Product>(nameof(Product.DeleteProductAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--product-id", "-i"], "ID of the Product to delete"),
                    }
                },
                new CommandDescriptor("update", "Update a Product identified by the `--product-id` option", "configcat product update -i <product-id> -n \"My Product\" -d \"Product Description\"")
                {
                    Aliases = new [] { "up" },
                    Handler = CreateHandler<Product>(nameof(Product.UpdateProductAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--product-id", "-i"], "ID of the Product to update"),
                        new Option<string>(["--name", "-n"], "The updated name"),
                        new Option<string>(["--description", "-d"], "The updated description"),
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
                new CommandDescriptor("lso", "List all Members that belongs to an Organization", "configcat member lso -o <organization-id>")
                {
                    Handler = CreateHandler<Member>(nameof(Member.ListOrganizationMembersAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(["--organization-id", "-o"], "Show only an Organization's Members"),
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("lsp", "List all Members that belongs to a Product", "configcat member lsp -p <product-id>")
                {
                    Handler = CreateHandler<Member>(nameof(Member.ListProductMembersAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(["--product-id", "-p"], "Show only a Product's Members"),
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("rm", "Remove Member from an Organization", "configcat member rm -o <organization-id> -i <user-id>")
                {
                    Handler = CreateHandler<Member>(nameof(Member.RemoveMemberFromOrganizationAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(["--organization-id", "-o"], "The Organization's ID from where the Member must be removed"),
                        new Option<string>(["--user-id", "-i"], "ID of the Member to remove"),
                    }
                },

                new CommandDescriptor("invite", "Invite Member(s) into a Product", "configcat member invite user1@example.com user2@example.com -p <product-id> -pgi <permission-group-id>")
                {
                    Aliases = new [] { "inv" },
                    Handler = CreateHandler<Member>(nameof(Member.InviteMembersAsync)),
                    Arguments = new[]
                    {
                        new Argument<string[]>("emails", "List of email addresses to invite")
                    },
                    Options = new Option[]
                    {
                        new Option<string>(["--product-id", "-p"], "The Product's ID to where the Members will be invited"),
                        new Option<long?>(["--permission-group-id", "-pgi"], "The Permission Group's ID to where the invited Members will join"),
                    }
                },
                new CommandDescriptor("add-permission", "Add Member to Permission Groups", "configcat member add-permission -o <organization-id> -i <user-id> -pgi <permission-group-id1> <permission-group-id2>")
                {
                    Aliases = new [] { "a" },
                    Handler = CreateHandler<Member>(nameof(Member.AddPermissionsAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(["--organization-id", "-o"], "ID of the Organization"),
                        new Option<string>(["--user-id", "-i"], "ID of the Member to add"),
                        new Option<long[]>(["--permission-group-ids", "-pgi"], "Permission Group IDs the Member must be put into"),
                    }
                },
                new CommandDescriptor("rm-permission", "Remove Member from Permission Groups", "configcat member rm-permission -o <organization-id> -i <user-id> -pgi <permission-group-id1> <permission-group-id2>")
                {
                    Aliases = new [] { "rmp" },
                    Handler = CreateHandler<Member>(nameof(Member.RemovePermissionsAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(["--organization-id", "-o"], "ID of the Organization"),
                        new Option<string>(["--user-id", "-i"], "ID of the Member to remove"),
                        new Option<long[]>(["--permission-group-ids", "-pgi"], "Permission Group IDs the Member must be removed from"),
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
                new CommandDescriptor("ls", "List all Configs that belongs to the configured user", "configcat config ls")
                {
                    Options = new Option[]
                    {
                        new Option<string>(["--product-id", "-p"], "Show only a Product's Configs"),
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    },
                    Handler = CreateHandler<Config>(nameof(Config.ListAllConfigsAsync))
                },
                new CommandDescriptor("create", "Create a new Config in a specified Product identified by the `--product-id` option", "configcat config create -p <product-id> -n \"NewConfig\" -d \"Config description\"")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<Config>(nameof(Config.CreateConfigAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--product-id", "-p"], "ID of the Product where the Config must be created"),
                        new Option<string>(["--name", "-n"], "Name of the new Config"),
                        new Option<string>(["--eval-version", "-e"], "Determines the evaluation version of the Config. Using `v2` enables the new features of Config V2")
                            .AddSuggestions(EvalVersion.Collection)
                            .UseDefaultValue(EvalVersion.V1),
                        new Option<string>(["--description", "-d"], "Description of the new Config"),
                    }
                },
                new CommandDescriptor("rm", "Remove a Config identified by the `--config-id` option", "configcat config rm -i <config-id>")
                {
                    Handler = CreateHandler<Config>(nameof(Config.DeleteConfigAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--config-id", "-i"], "ID of the Config to delete"),
                    }
                },
                new CommandDescriptor("update", "Update a Config identified by the `--config-id` option", "configcat config update -i <config-id> -n \"NewConfig\" -d \"Config description\"")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<Config>(nameof(Config.UpdateConfigAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--config-id", "-i"], "ID of the Config to update"),
                        new Option<string>(["--name", "-n"], "The updated name"),
                        new Option<string>(["--description", "-d"], "The updated description"),
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
                new CommandDescriptor("ls", "List all Permission Groups that manageable by the configured user", "configcat permission-group ls")
                {
                    Options = new Option[]
                    {
                        new Option<string>(["--product-id", "-p"], "Show only a Product's Permission Groups"),
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    },
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.ListAllPermissionGroupsAsync))
                },
                new CommandDescriptor("create", "Create a new Permission Group in a specified Product identified by the `--product-id` option", "configcat permission-group create -p <product-id> -n Developers --can-view-sdk-key --can-view-product-statistics --default-when-not-set false")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.CreatePermissionGroupAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(["--product-id", "-p"], "ID of the Product where the Permission Group must be created"),
                        new Option<string>(["--name", "-n"], "Name of the new Permission Group"),
                        new Option<bool>(["--can-manage-members"], Constants.Permissions[0]),
                        new Option<bool>(["--can-create-or-update-config"], Constants.Permissions[1]),
                        new Option<bool>(["--can-delete-config"], Constants.Permissions[2]),
                        new Option<bool>(["--can-create-or-update-environment"], Constants.Permissions[3]),
                        new Option<bool>(["--can-delete-environment"], Constants.Permissions[4]),
                        new Option<bool>(["--can-create-or-update-setting"], Constants.Permissions[5]),
                        new Option<bool>(["--can-tag-setting"], Constants.Permissions[6]),
                        new Option<bool>(["--can-delete-setting"], Constants.Permissions[7]),
                        new Option<bool>(["--can-create-or-update-tag"], Constants.Permissions[8]),
                        new Option<bool>(["--can-delete-tag"], Constants.Permissions[9]),
                        new Option<bool>(["--can-manage-webhook"], Constants.Permissions[10]),
                        new Option<bool>(["--can-use-export-import"], Constants.Permissions[11]),
                        new Option<bool>(["--can-manage-product-preferences"], Constants.Permissions[12]),
                        new Option<bool>(["--can-manage-integrations"], Constants.Permissions[13]),
                        new Option<bool>(["--can-view-sdk-key"], Constants.Permissions[14]),
                        new Option<bool>(["--can-rotate-sdk-key"], Constants.Permissions[15]),
                        new Option<bool>(["--can-view-product-statistics"], Constants.Permissions[16]),
                        new Option<bool>(["--can-view-product-audit-log"], Constants.Permissions[17]),
                        new Option<bool>(["--can-create-or-update-segments"], Constants.Permissions[18]),
                        new Option<bool>(["--can-delete-segments"], Constants.Permissions[19]),
                        new Option<bool>(["--default-when-not-set", "-def"], "Indicates whether each unspecified permission should be enabled or disabled by default"),
                    }
                },
                new CommandDescriptor("rm", "Remove a Permission Group identified by the `--permission-group-id` option", "configcat permission-group rm -i <permission-group-id>")
                {
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.DeletePermissionGroupAsync)),
                    Options = new[]
                    {
                        new Option<long?>(["--permission-group-id", "-i"], "ID of the Permission Group to delete"),
                    }
                },
                new CommandDescriptor("update", "Update a Permission Group identified by the `--permission-group-id` option", "configcat permission-group update -i <permission-group-id> -n Developers --can-view-product-audit-log")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.UpdatePermissionGroupAsync)),
                    Options = new Option[]
                    {
                        new Option<long?>(["--permission-group-id", "-i"], "ID of the Permission Group to update"),
                        new Option<string>(["--name", "-n"], "The updated name"),
                        new Option<bool?>(["--can-manage-members"], Constants.Permissions[0]),
                        new Option<bool?>(["--can-create-or-update-config"], Constants.Permissions[1]),
                        new Option<bool?>(["--can-delete-config"], Constants.Permissions[2]),
                        new Option<bool?>(["--can-create-or-update-environment"], Constants.Permissions[3]),
                        new Option<bool?>(["--can-delete-environment"], Constants.Permissions[4]),
                        new Option<bool?>(["--can-create-or-update-setting"], Constants.Permissions[5]),
                        new Option<bool?>(["--can-tag-setting"], Constants.Permissions[6]),
                        new Option<bool?>(["--can-delete-setting"], Constants.Permissions[7]),
                        new Option<bool?>(["--can-create-or-update-tag"], Constants.Permissions[8]),
                        new Option<bool?>(["--can-delete-tag"], Constants.Permissions[9]),
                        new Option<bool?>(["--can-manage-webhook"], Constants.Permissions[10]),
                        new Option<bool?>(["--can-use-export-import"], Constants.Permissions[11]),
                        new Option<bool?>(["--can-manage-product-preferences"], Constants.Permissions[12]),
                        new Option<bool?>(["--can-manage-integrations"], Constants.Permissions[13]),
                        new Option<bool?>(["--can-view-sdk-key"], Constants.Permissions[14]),
                        new Option<bool?>(["--can-rotate-sdk-key"], Constants.Permissions[15]),
                        new Option<bool?>(["--can-view-product-statistics"], Constants.Permissions[16]),
                        new Option<bool?>(["--can-view-product-audit-log"], Constants.Permissions[17]),
                        new Option<bool?>(["--can-create-or-update-segments"], Constants.Permissions[18]),
                        new Option<bool?>(["--can-delete-segments"], Constants.Permissions[19]),
                    }
                },
                new CommandDescriptor("show", "Show details of a Permission Group identified by the `--permission-group-id` option", "configcat permission-group show -i <permission-group-id>")
                {
                    Aliases = new[] { "sh", "pr", "print" },
                    Handler = CreateHandler<PermissionGroup>(nameof(PermissionGroup.ShowPermissionGroupAsync)),
                    Options = new Option[]
                    {
                        new Option<long?>(["--permission-group-id", "-i"], "ID of the Permission Group"),
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("env", "Update the environment specific permissions of a Permission Group", "configcat permission-group env -i <permission-group-id> -a custom -na readOnly -esat <environment-id>:full -def readOnly")
                {
                    Handler = CreateHandler<PermissionGroupEnvironmentAccess>(nameof(PermissionGroupEnvironmentAccess.UpdatePermissionGroupEnvironmentAccessesAsync)),
                    Options = new Option[]
                    {
                        new Option<long?>(["--permission-group-id", "-i"], "ID of the Permission Group"),
                        new Option<string>(["--access-type", "-a"], "Access configuration for all environments")
                            .AddSuggestions(Constants.AccessTypes.Keys.ToArray()),
                        new Option<string>(["--new-environment-access-type", "-na"], "Access configuration for newly created environments. Interpreted only when the --access-type option is `custom` which translates to `Environment specific`")
                            .AddSuggestions(Constants.EnvironmentAccessTypes.Keys.ToArray()),
                        new PermissionGroupEnvironmentAccessOption(),
                        new Option<string>(["--default-access-type-when-not-set", "-def"], "Access configuration for each environment not specified with --environment-specific-access-types. Interpreted only when the --access-type option is `custom` which translates to `Environment specific`")
                            .AddSuggestions(Constants.EnvironmentAccessTypes.Keys.ToArray()),
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
                new CommandDescriptor("ls", "List all Segments that belongs to the configured user", "configcat segment ls")
                {
                    Options = new Option[]
                    {
                        new Option<string>(["--product-id", "-p"], "Show only a Product's Segments"),
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    },
                    Handler = CreateHandler<Segment>(nameof(Segment.ListAllSegmentsAsync))
                },
                new CommandDescriptor("create", "Create a new Segment in a specified Product identified by the `--product-id` option", "configcat segment create -p <product-id> -n \"Beta users\" -d \"Beta users\" -a Email -c contains -t @example.com")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<Segment>(nameof(Segment.CreateSegmentAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--product-id", "-p"], "ID of the Product where the Segment must be created"),
                        new Option<string>(["--name", "-n"], "Name of the new Segment"),
                        new Option<string>(["--description", "-d"], "Description of the new Segment"),
                        new Option<string>(["--attribute", "-a"], "The user attribute to compare"),
                        new Option<string>(["--comparator", "-c"], "The comparison operator")
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                        new Option<string>(["--compare-to", "-t"], "The value to compare against"),
                    }
                },
                new CommandDescriptor("rm", "Remove a Segment identified by the `--segment-id` option", "configcat segment rm -i <segment-id>")
                {
                    Handler = CreateHandler<Segment>(nameof(Segment.DeleteSegmentAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--segment-id", "-i"], "ID of the Segment to delete"),
                    }
                },
                new CommandDescriptor("update", "Update a Segment identified by the `--segment-id` option", "configcat segment update -i <segment-id> -n \"Beta users\" -d \"Beta users\" -a Email -c contains -t @example.com")
                
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<Segment>(nameof(Segment.UpdateSegmentAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--segment-id", "-i"], "ID of the Segment to update"),
                        new Option<string>(["--name", "-n"], "The updated name"),
                        new Option<string>(["--description", "-d"], "The updated description"),
                        new Option<string>(["--attribute", "-a"], "The user attribute to compare"),
                        new Option<string>(["--comparator", "-c"], "The comparison operator")
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                        new Option<string>(["--compare-to", "-t"], "The value to compare against"),
                    }
                },
                new CommandDescriptor("show", "Show details of a Segment identified by the `--segment-id` option", "configcat segment show -i <segment-id>")
                {
                    Aliases = new[] { "sh", "pr", "print" },
                    Handler = CreateHandler<Segment>(nameof(Segment.GetSegmentDetailsAsync)),
                    Options = new Option[]
                    {
                        new Option<string>(["--segment-id", "-i"], "ID of the Segment"),
                        new Option<bool>(["--json"], "Format the output in JSON"),
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
                new CommandDescriptor("ls", "List all Environments that belongs to the configured user", "configcat environment ls")
                {
                    Options = new Option[]
                    {
                        new Option<string>(["--product-id", "-p"], "Show only a Product's Environments"),
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    },
                    Handler = CreateHandler<Environment>(nameof(Environment.ListAllEnvironmentsAsync))
                },
                new CommandDescriptor("create", "Create a new Environment in a specified Product identified by the `--product-id` option", "configcat environment create -p <product-id> -n Test -d \"Test Environment\" -c #7D3C98")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<Environment>(nameof(Environment.CreateEnvironmentAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--product-id", "-p"], "ID of the Product where the Environment must be created"),
                        new Option<string>(["--name", "-n"], "Name of the new Environment"),
                        new Option<string>(["--description", "-d"], "Description of the new Environment"),
                        new Option<string>(["--color", "-c"], "Color of the new Environment"),
                    }
                },
                new CommandDescriptor("rm", "Remove an Environment identified by the `--environment-id` option", "configcat environment rm -i <environment-id>")
                {
                    Handler = CreateHandler<Environment>(nameof(Environment.DeleteEnvironmentAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--environment-id", "-i"], "ID of the Environment to delete"),
                    }
                },
                new CommandDescriptor("update", "Update environment", "configcat environment update -i <environment-id> -n Test -d \"Test Environment\" -c #7D3C98")
                {
                    Aliases = new [] { "up" },
                    Handler = CreateHandler<Environment>(nameof(Environment.UpdateEnvironmentAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--environment-id", "-i"], "ID of the environment to update"),
                        new Option<string>(["--name", "-n"], "The updated name"),
                        new Option<string>(["--description", "-d"], "The updated description"),
                        new Option<string>(["--color", "-c"], "The updated color"),
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
                new CommandDescriptor("ls", "List all Tags that belongs to the configured user", "configcat tag ls")
                {
                    Options = new Option[]
                    {
                        new Option<string>(["--product-id", "-p"], "Show only a Product's tags"),
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    },
                    Handler = CreateHandler<Tag>(nameof(Tag.ListAllTagsAsync))
                },
                new CommandDescriptor("create", "Create a new Tag in a specified Product identified by the `--product-id` option", "configcat tag create -n \"temp_tag\"")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<Tag>(nameof(Tag.CreateTagAsync)),
                    Options = new[]
                    {
                        new Option<string>(["--product-id", "-p"], "ID of the Product where the Tag must be created"),
                        new Option<string>(["--name", "-n"], "The name of the new Tag"),
                        new Option<string>(["--color", "-c"], "The color of the new Tag"),
                    }
                },
                new CommandDescriptor("rm", "Remove a Tag identified by the `--tag-id` option", "configcat tag rm -i <tag-id>")
                {
                    Handler = CreateHandler<Tag>(nameof(Tag.DeleteTagAsync)),
                    Options = new[]
                    {
                        new Option<int>(["--tag-id", "-i"], "ID of the Tag to delete"),
                    }
                },
                new CommandDescriptor("update", "Update a Tag identified by the `--tag-id` option", "configcat tag update -i <tag-id> -n \"temp_tag\"")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<Tag>(nameof(Tag.UpdateTagAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--tag-id", "-i"], "ID of the Tag to update"),
                        new Option<string>(["--name", "-n"], "The updated name"),
                        new Option<string>(["--color", "-c"], "The updated color"),
                    }
                },
            },
        };


    private static CommandDescriptor BuildFlagCommand() =>
        new("flag", "Manage Feature Flags & Settings")
        {
            Aliases = new[] { "setting", "f", "s" },
            SubCommands = ManageFlagCommands.Concat(
            [
                BuildFlagValueCommand(),
                BuildFlagTargetingCommand(),
                BuildFlagPercentageCommand()
            ])
        };
    
    private static CommandDescriptor BuildFlagValueCommand() =>
        new("value", "Manage Feature Flag & Setting values in different Environments")
        {
            Aliases = new[] { "v" },
            SubCommands = new[]
            {
                new CommandDescriptor("show", "Show Feature Flag or Setting values, targeting, and percentage rules for each environment", "configcat flag value show -i <flag-id>")
                {
                    Aliases = new[] { "sh", "pr", "print" },
                    Handler = CreateHandler<FlagValue>(nameof(FlagValue.ShowValueAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("update", "Update the value of a Feature Flag or Setting", "configcat flag value update -i <flag-id> -e <environment-id> -f true")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<FlagValue>(nameof(FlagValue.UpdateFlagValueAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the update must be applied"),
                        new Option<string>(["--flag-value", "-f"], "The value to serve, it must respect the setting type"),
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
                new CommandDescriptor("create", "Create new targeting rule", "configcat flag targeting create -i <flag-id> -e <environment-id> -a Email -c contains -t @example.com -f true")
                {
                    Aliases = new[] { "cr" },
                    Handler = CreateHandler<Commands.Flags.FlagTargeting>(nameof(Commands.Flags.FlagTargeting.AddTargetingRuleAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the rule must be created"),
                        new Option<string>(["--attribute", "-a"], "The user attribute to compare"),
                        new Option<string>(["--comparator", "-c"], "The comparison operator")
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                        new Option<string>(["--compare-to", "-t"], "The value to compare against"),
                        new Option<string>(["--flag-value", "-f"], "The value to serve when the comparison matches, it must respect the setting type"),
                        new Option<string>(["--segment-id", "-si"], "ID of the Segment used in the rule"),
                        new Option<string>(["--segment-comparator", "-sc"], "The segment comparison operator")
                            .AddSuggestions(Constants.SegmentComparatorTypes.Keys.ToArray()),
                    }
                },
                new CommandDescriptor("update", "Update targeting rule", "configcat flag targeting update -i <flag-id> -e <environment-id> -p 1 -a Email -c contains -t @example.com -f true")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<Commands.Flags.FlagTargeting>(nameof(Commands.Flags.FlagTargeting.UpdateTargetingRuleAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the update must be applied"),
                        new Option<int?>(["--position", "-p"], "The position of the updating targeting rule"),
                        new Option<string>(["--attribute", "-a"], "The user attribute to compare"),
                        new Option<string>(["--comparator", "-c"], "The comparison operator")
                            .AddSuggestions(Constants.ComparatorTypes.Keys.ToArray()),
                        new Option<string>(["--compare-to", "-t"], "The value to compare against"),
                        new Option<string>(["--flag-value", "-f"], "The value to serve when the comparison matches, it must respect the setting type"),
                        new Option<string>(["--segment-id", "-si"], "ID of the Segment used in the rule"),
                        new Option<string>(["--segment-comparator", "-sc"], "The segment comparison operator")
                            .AddSuggestions(Constants.SegmentComparatorTypes.Keys.ToArray()),
                    }
                },
                new CommandDescriptor("rm", "Delete targeting rule", "configcat flag targeting rm -i <flag-id> -e <environment-id> -p 1")
                {
                    Handler = CreateHandler<Commands.Flags.FlagTargeting>(nameof(Commands.Flags.FlagTargeting.DeleteTargetingRuleAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<string>(["--environment-id", "-e"], "ID of the Environment from where the rule must be deleted"),
                        new Option<int?>(["--position", "-p"], "The position of the targeting rule to delete"),
                    }
                },
                new CommandDescriptor("move", "Move a targeting rule into a different position", "configcat flag targeting move -i <flag-id> -e <environment-id> --from 0 --to 1")
                {
                    Aliases = new[] { "mv" },
                    Handler = CreateHandler<Commands.Flags.FlagTargeting>(nameof(Commands.Flags.FlagTargeting.MoveTargetingRuleAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the move must be applied"),
                        new Option<int?>(["--from"], "The position of the targeting rule to move"),
                        new Option<int?>(["--to"], "The desired position of the targeting rule"),
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
                new CommandDescriptor("update", "Update percentage rules", "configcat flag % update -i <flag-id> -e <environment-id> 30:true 70:false")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<Commands.Flags.FlagPercentage>(nameof(Commands.Flags.FlagPercentage.UpdatePercentageRulesAsync)),
                    Arguments = new Argument[]
                    {
                        new PercentageRuleArgument()
                    },
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the update must be applied"),
                    }
                },
                new CommandDescriptor("clear", "Delete all percentage rules", "configcat flag % clear -i <flag-id> -e <environment-id>")
                {
                    Aliases = new[] { "clr" },
                    Handler = CreateHandler<Commands.Flags.FlagPercentage>(nameof(Commands.Flags.FlagPercentage.DeletePercentageRulesAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<string>(["--environment-id", "-e"], "ID of the Environment from where the rules must be deleted"),
                    }
                },
            }
        };
    
    private static CommandDescriptor BuildFlagV2Command() =>
        new("flag-v2", "Manage V2 Feature Flags & Settings")
        {
            Aliases = new[] { "setting-v2", "f2", "s2" },
            SubCommands = ManageFlagCommands.Concat(
            [
                BuildFlagValueV2Command(), 
                BuildV2FlagTargetingCommand()
            ])
        };

    private static CommandDescriptor BuildFlagValueV2Command() =>
        new("value", "Manage V2 Feature Flag & Setting default values in different Environments")
        {
            Aliases = new[] { "v" },
            SubCommands = new[]
            {
                new CommandDescriptor("show", "Show Feature Flag or Setting values, targeting, and percentage rules for each environment", "configcat flag-v2 value show -i <flag-id>")
                {
                    Aliases = new[] { "sh", "pr", "print" },
                    Handler = CreateHandler<FlagValueV2>(nameof(FlagValueV2.ShowValueAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<bool>(["--json"], "Format the output in JSON"),
                    }
                },
                new CommandDescriptor("update", "Update the value of a Feature Flag or Setting", "configcat flag-v2 value update -i <flag-id> -e <environment-id> -f true")
                {
                    Aliases = new[] { "up" },
                    Handler = CreateHandler<FlagValueV2>(nameof(FlagValueV2.UpdateFlagValueAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                        {
                            Name = "--flag-id"
                        },
                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the update must be applied"),
                        new Option<string>(["--flag-value", "-f"], "The value to serve, it must respect the setting type"),
                    }
                }
            }
        };

    private static CommandDescriptor BuildV2FlagTargetingCommand() =>
        new("targeting", "Manage V2 Feature Flag & Setting targeting options")
        {
            Aliases = ["t"],
            SubCommands = 
            [
                 new CommandDescriptor("rule", "Manage targeting rules")
                 {
                     Aliases = ["r"],
                     SubCommands = 
                     [
                        new CommandDescriptor("create", "Create new targeting rule")
                        {
                            Aliases = ["cr"],
                            SubCommands = 
                            [
                                new CommandDescriptor("user", "Create user based targeting rule",
                                    "configcat flag-v2 targeting rule create user -i <flag-id> -e <environment-id> -a Email -c contains -cv @example.com -sv true")
                                {
                                    Aliases = ["u"],
                                    Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.AddUserTargetingRuleAsync)),
                                    Options = new Option[]
                                    {
                                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                        {
                                            Name = "--flag-id"
                                        },
                                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the rule must be created"),
                                        new Option<string>(["--attribute", "-a"], "The User Object attribute that the condition is based on"),
                                        new Option<string>(["--comparator", "-c"], "The operator which defines the relation between the comparison attribute and the comparison value")
                                            .AddSuggestions(Constants.UserComparatorTypes.Keys.ToArray()),
                                        new Option<string[]>(["--comparison-value", "-cv"], "The value that the User Object attribute is compared to. Can be a double, string, or value-hint list in the format: `<value>:<hint>`"),
                                        new Option<string>(["--served-value", "-sv"], "The value associated with the targeting rule. Leave it empty if the targeting rule has percentage options. It must respect the setting type"),
                                        new PercentageOptionArgument()
                                    }
                                },
                                new CommandDescriptor("segment", "Create segment based targeting rule",
                                    "configcat flag-v2 targeting rule create segment -i <flag-id> -e <environment-id> -c isIn -si <segment-id> -sv true")
                                {
                                    Aliases = ["sg"],
                                    Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.AddSegmentTargetingRuleAsync)),
                                    Options = new Option[]
                                    {
                                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                        {
                                            Name = "--flag-id"
                                        },
                                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the rule must be created"),
                                        new Option<string>(["--comparator", "-c"], "The operator which defines the expected result of the evaluation of the segment")
                                            .AddSuggestions(Constants.SegmentComparatorTypes.Keys.ToArray()),
                                        new Option<string>(["--segment-id", "-si"], "ID of the segment that the condition is based on"),
                                        new Option<string>(["--served-value", "-sv"], "The value associated with the targeting rule. Leave it empty if the targeting rule has percentage options. It must respect the setting type"),
                                        new PercentageOptionArgument()
                                    }
                                },
                                new CommandDescriptor("prerequisite", "Create prerequisite flag based targeting rule",
                                    "configcat flag-v2 targeting rule create prerequisite -i <flag-id> -e <environment-id> -c equals -pi <prerequisite-id> -pv true -sv true")
                                {
                                    Aliases = ["pr"],
                                    Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.AddPrerequisiteTargetingRuleAsync)),
                                    Options = new Option[]
                                    {
                                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                        {
                                            Name = "--flag-id"
                                        },
                                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the rule must be created"),
                                        new Option<string>(["--comparator", "-c"], "The operator which defines the relation between the evaluated value of the prerequisite flag and the comparison value")
                                            .AddSuggestions(Constants.PrerequisiteComparatorTypes.Keys.ToArray()),
                                        new Option<int>(["--prerequisite-id", "-pi"], "ID of the prerequisite flag that the condition is based on"),
                                        new Option<string>(["--prerequisite-value", "-pv"], "The evaluated value of the prerequisite flag is compared to. It must respect the prerequisite flag's setting type"),
                                        new Option<string>(["--served-value", "-sv"], "The value associated with the targeting rule. Leave it empty if the targeting rule has percentage options. It must respect the setting type"),
                                        new PercentageOptionArgument()
                                    }
                                },
                            ]
                        },
                        new CommandDescriptor("rm", "Remove targeting rule",
                            "configcat flag-v2 targeting rule rm -i <flag-id> -e <environment-id> -rp 1")
                        {
                            Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.DeleteRuleAsync)),
                            Options = new Option[]
                            {
                                new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                {
                                    Name = "--flag-id"
                                },
                                new Option<string>(["--environment-id", "-e"], "ID of the Environment where the rule must be removed"),
                                new Option<int>(["--rule-position", "-rp"], "The position of the targeting rule to remove"),
                            }
                        },
                        new CommandDescriptor("move", "Move targeting rule",
                            "configcat flag-v2 targeting rule mv -i <flag-id> -e <environment-id> --from 1 --to 2")
                        {
                            Aliases = ["mv"],
                            Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.MoveTargetingRuleAsync)),
                            Options = new Option[]
                            {
                                new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                {
                                    Name = "--flag-id"
                                },
                                new Option<string>(["--environment-id", "-e"], "ID of the Environment where the rule should be moved"),
                                new Option<int?>(["--from"], "The position of the targeting rule to move"),
                                new Option<int?>(["--to"], "The desired position of the targeting rule"),
                            }
                        },
                        new CommandDescriptor("update-served-value", "Update a targeting rule's served value",
                            "configcat flag-v2 targeting rule usv -i <flag-id> -e <environment-id> -rp 1 -sv true")
                        {
                            Aliases = ["usv"],
                            Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.UpdateRuleServedValueAsync)),
                            Options = new Option[]
                            {
                                new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                {
                                    Name = "--flag-id"
                                },
                                new Option<string>(["--environment-id", "-e"], "ID of the Environment where the rule should be moved"),
                                new Option<int>(["--rule-position", "-rp"], "The position of the targeting rule"),
                                new Option<string>(["--served-value", "-sv"], "The value associated with the targeting rule. Leave it empty if the targeting rule has percentage options. It must respect the setting type"),
                                new PercentageOptionArgument()
                            }
                        }
                     ]
                 },
                 new CommandDescriptor("condition", "Manage conditions")
                 {
                     Aliases = ["c"],
                     SubCommands = 
                     [
                     new CommandDescriptor("add", "Add new condition")
                        {
                            Aliases = ["a"],
                            SubCommands = 
                            [
                                new CommandDescriptor("user", "Add new user based condition",
                                    "configcat flag-v2 targeting condition add user -i <flag-id> -e <environment-id> -rp 1 -a Email -c contains -cv @example.com -f true")
                                {
                                    Aliases = ["u"],
                                    Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.AddUserConditionAsync)),
                                    Options = new Option[]
                                    {
                                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                        {
                                            Name = "--flag-id"
                                        },
                                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the condition must be created"),
                                        new Option<int>(["--rule-position", "-rp"], "The position of the targeting rule to which the condition is added"),
                                        new Option<string>(["--attribute", "-a"], "The User Object attribute that the condition is based on"),
                                        new Option<string>(["--comparator", "-c"], "The operator which defines the relation between the comparison attribute and the comparison value")
                                            .AddSuggestions(Constants.UserComparatorTypes.Keys.ToArray()),
                                        new Option<string[]>(["--comparison-value", "-cv"], "The value that the User Object attribute is compared to. Can be a double, string, or value-hint list in the format: `<value>:<hint>`"),
                                    }
                                },
                                new CommandDescriptor("segment", "Add new segment based condition",
                                    "configcat flag-v2 targeting condition add segment -i <flag-id> -e <environment-id> -rp 1 -c isIn -si <segment-id> -f true")
                                {
                                    Aliases = ["sg"],
                                    Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.AddSegmentConditionAsync)),
                                    Options = new Option[]
                                    {
                                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                        {
                                            Name = "--flag-id"
                                        },
                                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the condition must be created"),
                                        new Option<int>(["--rule-position", "-rp"], "The position of the targeting rule to which the condition is added"),
                                        new Option<string>(["--comparator", "-c"], "The operator which defines the expected result of the evaluation of the segment")
                                            .AddSuggestions(Constants.SegmentComparatorTypes.Keys.ToArray()),
                                        new Option<string>(["--segment-id", "-si"], "ID of the segment that the condition is based on")
                                    }
                                },
                                new CommandDescriptor("prerequisite", "Add new prerequisite flag based condition",
                                    "configcat flag-v2 targeting condition add prerequisite -i <flag-id> -e <environment-id> -rp 1 -c equals -pi <prerequisite-id> -pv true")
                                {
                                    Aliases = ["pr"],
                                    Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.AddPrerequisiteConditionAsync)),
                                    Options = new Option[]
                                    {
                                        new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                        {
                                            Name = "--flag-id"
                                        },
                                        new Option<string>(["--environment-id", "-e"], "ID of the Environment where the condition must be created"),
                                        new Option<int>(["--rule-position", "-rp"], "The position of the targeting rule to which the condition is added"),
                                        new Option<string>(["--comparator", "-c"], "The operator which defines the relation between the evaluated value of the prerequisite flag and the comparison value")
                                            .AddSuggestions(Constants.PrerequisiteComparatorTypes.Keys.ToArray()),
                                        new Option<int>(["--prerequisite-id", "-pi"], "ID of the prerequisite flag that the condition is based on"),
                                        new Option<string>(["--prerequisite-value", "-pv"], "The evaluated value of the prerequisite flag is compared to. It must respect the prerequisite flag's setting type")
                                    }
                                },
                            ]
                        },
                        new CommandDescriptor("rm", "Remove condition",
                            "configcat flag-v2 targeting condition rm -i <flag-id> -e <environment-id> -rp 1")
                        {
                            Handler = CreateHandler<Commands.Flags.V2.FlagTargeting>(nameof(Commands.Flags.V2.FlagTargeting.DeleteConditionAsync)),
                            Options = new Option[]
                            {
                                new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                {
                                    Name = "--flag-id"
                                },
                                new Option<string>(["--environment-id", "-e"], "ID of the Environment where the condition must be removed"),
                                new Option<int>(["--rule-position", "-rp"], "The position of the targeting rule"),
                                new Option<int>(["--condition-position", "-cp"], "The position of the condition to remove"),
                            }
                        },
                     ]
                 },
                 new CommandDescriptor("percentage", "Manage percentage-only rules")
                 {
                     Aliases = new[] { "%" },
                     SubCommands = new[]
                     {
                         new CommandDescriptor("update", "Update or add the last percentage-only targeting rule", "configcat flag-v2 targeting % update -i <flag-id> -e <environment-id> -po 30:true 70:false")
                         {
                             Aliases = new[] { "up" },
                             Handler = CreateHandler<Commands.Flags.V2.FlagPercentage>(nameof(Commands.Flags.V2.FlagPercentage.UpdatePercentageRulesAsync)),
                             Options = new Option[]
                             {
                                 new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                 {
                                     Name = "--flag-id"
                                 },
                                 new Option<string>(["--environment-id", "-e"], "ID of the Environment where the update must be applied"),
                                 new PercentageOptionArgument()
                             }
                         },
                         new CommandDescriptor("clear", "Delete the last percentage-only rule", "configcat flag-v2 targeting % clear -i <flag-id> -e <environment-id>")
                         {
                             Aliases = new[] { "clr" },
                             Handler = CreateHandler<Commands.Flags.V2.FlagPercentage>(nameof(Commands.Flags.V2.FlagPercentage.DeletePercentageRulesAsync)),
                             Options = new Option[]
                             {
                                 new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                 {
                                     Name = "--flag-id"
                                 },
                                 new Option<string>(["--environment-id", "-e"], "ID of the Environment from where the rule must be deleted"),
                             }
                         },
                         new CommandDescriptor("attribute", "Set the percentage evaluation attribute", "configcat flag-v2 targeting % attribute -i <flag-id> -e <environment-id>")
                         {
                             Aliases = new[] { "at" },
                             Handler = CreateHandler<Commands.Flags.V2.FlagPercentage>(nameof(Commands.Flags.V2.FlagPercentage.UpdatePercentageAttributeAsync)),
                             Options = new Option[]
                             {
                                 new Option<int>(["--flag-id", "-i", "--setting-id"], "ID of the Feature Flag or Setting")
                                 {
                                     Name = "--flag-id"
                                 },
                                 new Option<string>(["--environment-id", "-e"], "ID of the Environment from where the rules must be deleted"),
                                 new Option<string>(["--attribute-name", "-n"], "The User Object attribute which serves as the basis of percentage options evaluation")
                             }
                         },
                     }
                 }
            ],
        };
    
    private static CommandDescriptor[] ManageFlagCommands =
    [
        new CommandDescriptor("ls", "List all Feature Flags & Settings that belongs to the configured user",
            "configcat flag ls -n my_tag")
        {
            Options = new Option[]
            {
                new Option<string>(["--config-id", "-c"], "Show only a Config's Flags & Settings"),
                new Option<string>(["--tag-name", "-n"], "Filter by a Tag's name"),
                new Option<int>(["--tag-id", "-t"], "Filter by a Tag's ID"),
                new Option<bool>(["--json"], "Format the output in JSON"),
            },
            Handler = CreateHandler<Flag>(nameof(Flag.ListAllFlagsAsync))
        },
        new CommandDescriptor("create",
            "Create a new Feature Flag or Setting in a specified Config identified by the `--config-id` option",
            "configcat flag create -c <config-id> -n \"My awesome flag\" -k myAwesomeFlag -t boolean -H \"This is the most awesome flag.\" -ive <env1-id>:true <env2-id>:false -g <tag1-id> <tag2-id>")
        {
            Aliases = new[] { "cr" },
            Handler = CreateHandler<Flag>(nameof(Flag.CreateFlagAsync)),
            Options = new Option[]
            {
                new Option<string>(["--config-id", "-c"], "ID of the Config where the Flag must be created"),
                new Option<string>(["--name", "-n"], "Name of the new Flag or Setting"),
                new Option<string>(["--key", "-k"],
                    "Key of the new Flag or Setting (must be unique within the given Config)"),
                new Option<string>(["--hint", "-H"], "Hint of the new Flag or Setting"),
                new Option<string>(["--init-value", "-iv"], "Initial value for each Environment"),
                new FlagInitialValuesOption(),
                new Option<string>(["--type", "-t"], "Type of the new Flag or Setting")
                    .AddSuggestions(SettingTypes.Collection),
                new Option<int[]>(["--tag-ids", "-g"], "Tags to attach"),
            }
        },
        new CommandDescriptor("rm", "Remove a Feature Flag or Setting identified by the `--flag-id` option",
            "configcat flag rm -i <flag-id>")
        {
            Handler = CreateHandler<Flag>(nameof(Flag.DeleteFlagAsync)),
            Options = new[]
            {
                new Option<int>(["--flag-id", "-i", "--setting-id"],
                    "ID of the Feature Flag or Setting to delete")
                {
                    Name = "--flag-id"
                },
            }
        },
        new CommandDescriptor("update", "Update a Feature Flag or Setting identified by the `--flag-id` option",
            "configcat flag update -i <flag-id> -n \"My awesome flag\" -H \"This is the most awesome flag.\" -g <tag1-id> <tag2-id>")
        {
            Aliases = new[] { "up" },
            Handler = CreateHandler<Flag>(nameof(Flag.UpdateFlagAsync)),
            Options = new Option[]
            {
                new Option<int>(["--flag-id", "-i", "--setting-id"],
                    "ID of the Feature Flag or Setting to update")
                {
                    Name = "--flag-id"
                },
                new Option<string>(["--name", "-n"], "The updated name"),
                new Option<string>(["--hint", "-H"], "The updated hint"),
                new Option<int[]>(["--tag-ids", "-g"], "The updated Tag list"),
            }
        },
        new CommandDescriptor("attach", "Attach Tag(s) to a Feature Flag or Setting",
            "configcat flag attach -i <flag-id> -g <tag1-id> <tag2-id>")
        {
            Aliases = new[] { "at" },
            Handler = CreateHandler<Flag>(nameof(Flag.AttachTagsAsync)),
            Options = new Option[]
            {
                new Option<int>(["--flag-id", "-i", "--setting-id"],
                    "ID of the Feature Flag or Setting to attach Tags")
                {
                    Name = "--flag-id"
                },
                new Option<int[]>(["--tag-ids", "-g"], "Tag IDs to attach"),
            }
        },
        new CommandDescriptor("detach", "Detach Tag(s) from a Feature Flag or Setting",
            "configcat flag detach -i <flag-id> -g <tag1-id> <tag2-id>")
        {
            Aliases = new[] { "dt" },
            Handler = CreateHandler<Flag>(nameof(Flag.DetachTagsAsync)),
            Options = new Option[]
            {
                new Option<int>(["--flag-id", "-i", "--setting-id"],
                    "ID of the Feature Flag or Setting to detach Tags")
                {
                    Name = "--flag-id"
                },
                new Option<int[]>(["--tag-ids", "-g"], "Tag IDs to detach"),
            }
        }
    ];
    
    private static CommandDescriptor BuildSdkKeyCommand() =>
        new("sdk-key", "List SDK Keys", "configcat sdk-key")
        {
            Aliases = new[] { "k" },
            Handler = CreateHandler<SdkKey>(nameof(SdkKey.InvokeAsync)),
            Options = new[]
            {
                new Option<bool>(["--json"], "Format the output in JSON"),
            }
        };

    private static CommandDescriptor BuildScanCommand() =>
        new("scan", "Scan files for Feature Flag & Setting usages", "configcat scan ./dir -c <config-id> -l 5 --print")
        {
            Handler = CreateHandler<Scan>(nameof(Scan.InvokeAsync)),
            Arguments = new[]
            {
                new Argument<DirectoryInfo>("directory", "Directory to scan").ExistingOnly(),
            },
            Options = new Option[]
            {
                new Option<string>(["--config-id", "-c"], "ID of the Config to scan against"),
                new Option<int>(["--line-count", "-l"], () => 4, "Context line count before and after the reference line (min: 1, max: 10)"),
                new Option<bool>(["--print", "-p"], "Print found references to output"),
                new Option<bool>(["--upload", "-u"], "Upload references to ConfigCat"),
                new Option<string>(["--repo", "-r"], "Repository name. Mandatory for code reference upload"),
                new Option<string>(["--branch", "-b"], "Branch name. When the scanned folder is a Git repo, it's determined automatically, otherwise, it must be set manually"),
                new Option<string>(["--commit-hash", "-cm"], "Commit hash. When the scanned folder is a Git repo, it's determined automatically, otherwise, it must be set manually"),
                new Option<string>(["--file-url-template", "-f"], "Template url used to generate VCS file links. Available template parameters: `commitHash`, `filePath`, `lineNumber`. Example: https://github.com/my/repo/blob/{commitHash}/{filePath}#L{lineNumber}"),
                new Option<string>(["--commit-url-template", "-ct"], "Template url used to generate VCS commit links. Available template parameters: `commitHash`. Example: https://github.com/my/repo/commit/{commitHash}"),
                new Option<string>(["--runner", "-ru"], "Overrides the default `ConfigCat CLI {version}` executor label on the ConfigCat dashboard"),
                new Option<string[]>(["--exclude-flag-keys", "-ex"], "Exclude the given Feature Flag keys from scanning. E.g.: `-ex flag1 flag2` or `-ex 'flag1,flag2'`"),

            }
        };

    private static CommandDescriptor BuildConfigJsonCommand() =>
        new("config-json", "Config JSON-related utilities")
        {
            SubCommands = new CommandDescriptor[]
            {
                new("convert", "Convert between config JSON versions", "configcat config-json convert v5-to-v6 < config_v5.json")
                {
                    Handler = CreateHandler<ConfigJsonConvert>(nameof(ConfigJsonConvert.ExecuteAsync)),
                    Arguments = new[]
                    {
                        new Argument<string>("conversion", "The conversion to perform.")
                            .AddSuggestions(ConfigJsonConvert.V5TestConversion, ConfigJsonConvert.V6TestConversion, ConfigJsonConvert.V5ToV6Conversion)
                            .UseDefaultValue(ConfigJsonConvert.V5ToV6Conversion)
                    },
                    Options = new Option[]
                    {
                        new Option<FileInfo>(["--hash-map", "-h"], "A JSON file containing the original values of hashed comparison values" +
                                                                   "in the following format: '{\"<hash>\":\"<original-value>\"}'.").ExistingOnly(),
                        new Option<bool>(["--skip-salt", "-s"], "Skip writing salt into the converted JSON if it would be unused."),
                        new Option<bool>(["--pretty", "-p"], "Pretty print the converted JSON."),
                    },
                },
                new("get", "Download a config JSON from the CDN servers.", "configcat config-json get -f v6 PKDVCLf-Hq-h-kCzMp-L7Q/HhOWfwVtZ0mb30i9wi17GQ > config.json")
                {
                    Handler = CreateHandler<ConfigJsonGet>(nameof(ConfigJsonGet.ExecuteAsync)),
                    Arguments = new[]
                    {
                        new Argument<string>("sdk-key", "The SDK key identifying the config to download.")
                    },
                    Options = new Option[]
                    {
                        new Option<string>(["-f", "--format"], "The config JSON format version.")
                            .AddSuggestions(ConfigJsonGet.ConfigV5, ConfigJsonGet.ConfigV6)
                            .UseDefaultValue(ConfigJsonGet.ConfigV6),
                        new Option<bool>(["--eu"], "Use the ConfigCat CDN servers located in the EU. Specify this option if you enabled EU Only data governance."),
                        new Option<bool>(["--test", "-t"], "Use the ConfigCat CDN servers of the test infrastructure.") { IsHidden = true },
                        new Option<bool>(["--base-url", "-u"], "Use the server accessible at the specified URL. Specify this option if you set up a proxy server."),
                        new Option<bool>(["--pretty", "-p"], "Pretty print the downloaded JSON."),
                    },
                }
            }
        };
    
    private static CommandDescriptor BuildWorkspaceCommand() =>
        new("workspace", "Manage the CLI workspace. When set, the CLI's interactive mode filters Product and Config selectors by the values set in the workspace")
        {
            Aliases = new[] { "w" },
            SubCommands = new[]
            {
                new CommandDescriptor("set", "Set the workspace", "configcat workspace set -p <product-id> -c <config-id>")
                {
                    Aliases = new[] { "s" },
                    Handler = CreateHandler<Commands.Workspace>(nameof(Commands.Workspace.SetAsync)),
                    Options = new Option[]
                    {
                        new Option<int>(["--product-id", "-p"], "ID of the Product"),
                        new Option<bool>(["--config-id", "-c"], "ID of the Config"),
                    }
                },
                new CommandDescriptor("clr", "Clear the workspace", "configcat workspace clr")
                {
                    Handler = CreateHandler<Commands.Workspace>(nameof(Commands.Workspace.UnSetAsync)),
                },
                new CommandDescriptor("show", "Show the values saved in the workspace", "configcat workspace show")
                {
                    Aliases = ["sh", "p", "print"],
                    Handler = CreateHandler<Commands.Workspace>(nameof(Commands.Workspace.ShowAsync)),
                },
            }
        };

    private static CommandDescriptor BuildCatCommand() =>
        new("whoisthebestcat", "Well, who?", "configcat cat")
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
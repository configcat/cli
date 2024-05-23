# ConfigCat CLI Reference Documentation
[GitHub](https://github.com/configcat/cli) | [Documentation](https://configcat.com/docs/advanced/cli) | [ConfigCat](https://configcat.com)

The ConfigCat CLI allows you to interact with the ConfigCat Management API via the command line. It supports most functionality found on the ConfigCat Dashboard. You can manage ConfigCat resources like Feature Flags, Targeting / Percentage rules, Products, Configs, Environments, and more.
## Options
| Option | Description |
| ------ | ----------- |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `--version` | Show version information |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Commands
This is the complete list of the available commands provided by the CLI.
### configcat setup
| Command | Description |
| ------ | ----------- |
| [configcat setup](configcat-setup.md) | Setup the CLI with Public Management API host and credentials |
### configcat ls
| Command | Description |
| ------ | ----------- |
| [configcat ls](configcat-ls.md) | List all Product, Config, and Environment IDs |
### configcat product
| Command | Description |
| ------ | ----------- |
| [configcat product](configcat-product.md) | Manage Products |
| [configcat product ls](configcat-product-ls.md) | List all Products that belongs to the configured user |
| [configcat product create](configcat-product-create.md) | Create a new Product in a specified Organization identified by the `--organization-id` option |
| [configcat product rm](configcat-product-rm.md) | Remove a Product identified by the `--product-id` option |
| [configcat product update](configcat-product-update.md) | Update a Product identified by the `--product-id` option |
| [configcat product preferences](configcat-product-preferences.md) | Manage Product preferences |
| [configcat product preferences show](configcat-product-preferences-show.md) | Show a Product's preferences |
| [configcat product preferences update](configcat-product-preferences-update.md) | Update a Product's preferences |
| [configcat product preferences update env](configcat-product-preferences-update-env.md) | Update per-environment required reason |
### configcat config
| Command | Description |
| ------ | ----------- |
| [configcat config](configcat-config.md) | Manage Configs |
| [configcat config ls](configcat-config-ls.md) | List all Configs that belongs to the configured user |
| [configcat config create](configcat-config-create.md) | Create a new Config in a specified Product identified by the `--product-id` option |
| [configcat config rm](configcat-config-rm.md) | Remove a Config identified by the `--config-id` option |
| [configcat config update](configcat-config-update.md) | Update a Config identified by the `--config-id` option |
### configcat webhook
| Command | Description |
| ------ | ----------- |
| [configcat webhook](configcat-webhook.md) | Manage Webhooks |
| [configcat webhook ls](configcat-webhook-ls.md) | List all Webhooks that belongs to the configured user |
| [configcat webhook show](configcat-webhook-show.md) | Print a Webhook identified by the `--webhook-id` option |
| [configcat webhook create](configcat-webhook-create.md) | Create a new Webhook |
| [configcat webhook rm](configcat-webhook-rm.md) | Remove a Webhook identified by the `--webhook-id` option |
| [configcat webhook update](configcat-webhook-update.md) | Update a Webhook identified by the `--webhook-id` option |
| [configcat webhook headers](configcat-webhook-headers.md) | Manage Webhook headers |
| [configcat webhook headers add](configcat-webhook-headers-add.md) | Add new header |
| [configcat webhook headers rm](configcat-webhook-headers-rm.md) | Remove header |
### configcat environment
| Command | Description |
| ------ | ----------- |
| [configcat environment](configcat-environment.md) | Manage Environments |
| [configcat environment ls](configcat-environment-ls.md) | List all Environments that belongs to the configured user |
| [configcat environment create](configcat-environment-create.md) | Create a new Environment in a specified Product identified by the `--product-id` option |
| [configcat environment rm](configcat-environment-rm.md) | Remove an Environment identified by the `--environment-id` option |
| [configcat environment update](configcat-environment-update.md) | Update environment |
### configcat flag
| Command | Description |
| ------ | ----------- |
| [configcat flag](configcat-flag.md) | Manage Feature Flags & Settings |
| [configcat flag ls](configcat-flag-ls.md) | List all Feature Flags & Settings that belongs to the configured user |
| [configcat flag create](configcat-flag-create.md) | Create a new Feature Flag or Setting in a specified Config identified by the `--config-id` option |
| [configcat flag rm](configcat-flag-rm.md) | Remove a Feature Flag or Setting identified by the `--flag-id` option |
| [configcat flag update](configcat-flag-update.md) | Update a Feature Flag or Setting identified by the `--flag-id` option |
| [configcat flag attach](configcat-flag-attach.md) | Attach Tag(s) to a Feature Flag or Setting |
| [configcat flag detach](configcat-flag-detach.md) | Detach Tag(s) from a Feature Flag or Setting |
| [configcat flag value](configcat-flag-value.md) | Manage Feature Flag & Setting values in different Environments |
| [configcat flag value show](configcat-flag-value-show.md) | Show Feature Flag or Setting values, targeting, and percentage rules for each environment |
| [configcat flag value update](configcat-flag-value-update.md) | Update the value of a Feature Flag or Setting |
| [configcat flag targeting](configcat-flag-targeting.md) | Manage targeting rules |
| [configcat flag targeting create](configcat-flag-targeting-create.md) | Create new targeting rule |
| [configcat flag targeting update](configcat-flag-targeting-update.md) | Update targeting rule |
| [configcat flag targeting rm](configcat-flag-targeting-rm.md) | Delete targeting rule |
| [configcat flag targeting move](configcat-flag-targeting-move.md) | Move a targeting rule into a different position |
| [configcat flag percentage](configcat-flag-percentage.md) | Manage percentage rules |
| [configcat flag percentage update](configcat-flag-percentage-update.md) | Update percentage rules |
| [configcat flag percentage clear](configcat-flag-percentage-clear.md) | Delete all percentage rules |
### configcat flag-v2
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2](configcat-flag-v2.md) | Manage V2 Feature Flags & Settings |
| [configcat flag-v2 ls](configcat-flag-v2-ls.md) | List all Feature Flags & Settings that belongs to the configured user |
| [configcat flag-v2 create](configcat-flag-v2-create.md) | Create a new Feature Flag or Setting in a specified Config identified by the `--config-id` option |
| [configcat flag-v2 rm](configcat-flag-v2-rm.md) | Remove a Feature Flag or Setting identified by the `--flag-id` option |
| [configcat flag-v2 update](configcat-flag-v2-update.md) | Update a Feature Flag or Setting identified by the `--flag-id` option |
| [configcat flag-v2 attach](configcat-flag-v2-attach.md) | Attach Tag(s) to a Feature Flag or Setting |
| [configcat flag-v2 detach](configcat-flag-v2-detach.md) | Detach Tag(s) from a Feature Flag or Setting |
| [configcat flag-v2 value](configcat-flag-v2-value.md) | Manage V2 Feature Flag & Setting default values in different Environments |
| [configcat flag-v2 value show](configcat-flag-v2-value-show.md) | Show Feature Flag or Setting values, targeting, and percentage rules for each environment |
| [configcat flag-v2 value update](configcat-flag-v2-value-update.md) | Update the value of a Feature Flag or Setting |
| [configcat flag-v2 targeting](configcat-flag-v2-targeting.md) | Manage V2 Feature Flag & Setting targeting options |
| [configcat flag-v2 targeting rule](configcat-flag-v2-targeting-rule.md) | Manage targeting rules |
| [configcat flag-v2 targeting rule create](configcat-flag-v2-targeting-rule-create.md) | Create new targeting rule |
| [configcat flag-v2 targeting rule create user](configcat-flag-v2-targeting-rule-create-user.md) | Create user based targeting rule |
| [configcat flag-v2 targeting rule create segment](configcat-flag-v2-targeting-rule-create-segment.md) | Create segment based targeting rule |
| [configcat flag-v2 targeting rule create prerequisite](configcat-flag-v2-targeting-rule-create-prerequisite.md) | Create prerequisite flag based targeting rule |
| [configcat flag-v2 targeting rule rm](configcat-flag-v2-targeting-rule-rm.md) | Remove targeting rule |
| [configcat flag-v2 targeting rule move](configcat-flag-v2-targeting-rule-move.md) | Move targeting rule |
| [configcat flag-v2 targeting rule update-served-value](configcat-flag-v2-targeting-rule-update-served-value.md) | Update a targeting rule's served value |
| [configcat flag-v2 targeting condition](configcat-flag-v2-targeting-condition.md) | Manage conditions |
| [configcat flag-v2 targeting condition add](configcat-flag-v2-targeting-condition-add.md) | Add new condition |
| [configcat flag-v2 targeting condition add user](configcat-flag-v2-targeting-condition-add-user.md) | Add new user based condition |
| [configcat flag-v2 targeting condition add segment](configcat-flag-v2-targeting-condition-add-segment.md) | Add new segment based condition |
| [configcat flag-v2 targeting condition add prerequisite](configcat-flag-v2-targeting-condition-add-prerequisite.md) | Add new prerequisite flag based condition |
| [configcat flag-v2 targeting condition rm](configcat-flag-v2-targeting-condition-rm.md) | Remove condition |
| [configcat flag-v2 targeting percentage](configcat-flag-v2-targeting-percentage.md) | Manage percentage-only rules |
| [configcat flag-v2 targeting percentage update](configcat-flag-v2-targeting-percentage-update.md) | Update or add the last percentage-only targeting rule |
| [configcat flag-v2 targeting percentage clear](configcat-flag-v2-targeting-percentage-clear.md) | Delete the last percentage-only rule |
| [configcat flag-v2 targeting percentage attribute](configcat-flag-v2-targeting-percentage-attribute.md) | Set the percentage evaluation attribute |
### configcat segment
| Command | Description |
| ------ | ----------- |
| [configcat segment](configcat-segment.md) | Manage Segments |
| [configcat segment ls](configcat-segment-ls.md) | List all Segments that belongs to the configured user |
| [configcat segment create](configcat-segment-create.md) | Create a new Segment in a specified Product identified by the `--product-id` option |
| [configcat segment rm](configcat-segment-rm.md) | Remove a Segment identified by the `--segment-id` option |
| [configcat segment update](configcat-segment-update.md) | Update a Segment identified by the `--segment-id` option |
| [configcat segment show](configcat-segment-show.md) | Show details of a Segment identified by the `--segment-id` option |
### configcat permission-group
| Command | Description |
| ------ | ----------- |
| [configcat permission-group](configcat-permission-group.md) | Manage Permission Groups |
| [configcat permission-group ls](configcat-permission-group-ls.md) | List all Permission Groups that manageable by the configured user |
| [configcat permission-group create](configcat-permission-group-create.md) | Create a new Permission Group in a specified Product identified by the `--product-id` option |
| [configcat permission-group rm](configcat-permission-group-rm.md) | Remove a Permission Group identified by the `--permission-group-id` option |
| [configcat permission-group update](configcat-permission-group-update.md) | Update a Permission Group identified by the `--permission-group-id` option |
| [configcat permission-group show](configcat-permission-group-show.md) | Show details of a Permission Group identified by the `--permission-group-id` option |
| [configcat permission-group env](configcat-permission-group-env.md) | Update the environment specific permissions of a Permission Group |
### configcat member
| Command | Description |
| ------ | ----------- |
| [configcat member](configcat-member.md) | Manage Members |
| [configcat member lsio](configcat-member-lsio.md) | List all pending Invitations that belongs to an Organization |
| [configcat member lsip](configcat-member-lsip.md) | List all pending Invitations that belongs to a Product |
| [configcat member lso](configcat-member-lso.md) | List all Members that belongs to an Organization |
| [configcat member lsp](configcat-member-lsp.md) | List all Members that belongs to a Product |
| [configcat member rm](configcat-member-rm.md) | Remove Member from an Organization |
| [configcat member invite](configcat-member-invite.md) | Invite Member(s) into a Product |
| [configcat member add-permission](configcat-member-add-permission.md) | Add Member to Permission Groups |
| [configcat member rm-permission](configcat-member-rm-permission.md) | Remove Member from Permission Groups |
### configcat tag
| Command | Description |
| ------ | ----------- |
| [configcat tag](configcat-tag.md) | Manage Tags |
| [configcat tag ls](configcat-tag-ls.md) | List all Tags that belongs to the configured user |
| [configcat tag create](configcat-tag-create.md) | Create a new Tag in a specified Product identified by the `--product-id` option |
| [configcat tag rm](configcat-tag-rm.md) | Remove a Tag identified by the `--tag-id` option |
| [configcat tag update](configcat-tag-update.md) | Update a Tag identified by the `--tag-id` option |
### configcat sdk-key
| Command | Description |
| ------ | ----------- |
| [configcat sdk-key](configcat-sdk-key.md) | List SDK Keys |
### configcat scan
| Command | Description |
| ------ | ----------- |
| [configcat scan](configcat-scan.md) | Scan files for Feature Flag & Setting usages |
### configcat config-json
| Command | Description |
| ------ | ----------- |
| [configcat config-json](configcat-config-json.md) | Config JSON-related utilities |
| [configcat config-json convert](configcat-config-json-convert.md) | Convert between config JSON versions |
| [configcat config-json get](configcat-config-json-get.md) | Download a config JSON from the CDN servers. |
### configcat workspace
| Command | Description |
| ------ | ----------- |
| [configcat workspace](configcat-workspace.md) | Manage the CLI workspace. When set, the CLI's interactive mode filters Product and Config selectors by the values set in the workspace |
| [configcat workspace set](configcat-workspace-set.md) | Set the workspace |
| [configcat workspace clr](configcat-workspace-clr.md) | Clear the workspace |
| [configcat workspace show](configcat-workspace-show.md) | Show the values saved in the workspace |

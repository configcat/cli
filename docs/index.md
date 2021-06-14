# Command Line Interface Reference
[GitHub](https://github.com/configcat/cli) | [Documentation](https://configcat.com/docs/advanced/cli)

This is a reference for the ConfigCat CLI. It allows you to interact with the ConfigCat Management API via the command line. It supports most functionality found on the ConfigCat Dashboard. You can manage ConfigCat resources like Feature Flags, Targeting / Percentage rules, Products, Configs, Environments, and more.
## Options
| Option | Description |
| ------ | ----------- |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--version` | Show version information |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Commands
This is the complete list of the available commands provided by the CLI.
### configcat setup
| Command | Description |
| ------ | ----------- |
| [configcat setup](configcat-setup.md) | Setup the CLI with Public Management API host and credentials.<br/>You can get your credentials from here: https://app.configcat.com/my-account/public-api-credentials |
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
### configcat config
| Command | Description |
| ------ | ----------- |
| [configcat config](configcat-config.md) | Manage Configs |
| [configcat config ls](configcat-config-ls.md) | List all Configs that belongs to the configured user |
| [configcat config create](configcat-config-create.md) | Create a new Config in a specified Product identified by the `--product-id` option |
| [configcat config rm](configcat-config-rm.md) | Remove a Config identified by the `--config-id` option |
| [configcat config update](configcat-config-update.md) | Update a Config identified by the `--config-id` option |
### configcat environment
| Command | Description |
| ------ | ----------- |
| [configcat environment](configcat-environment.md) | Manage Environments |
| [configcat environment ls](configcat-environment-ls.md) | List all Environments that belongs to the configured user |
| [configcat environment create](configcat-environment-create.md) | Create a new Environment in a specified Product identified by the `--product-id` option |
| [configcat environment rm](configcat-environment-rm.md) | Remove an Environment identified by the `--environment-id` option |
| [configcat environment update](configcat-environment-update.md) | Update environment |
### configcat tag
| Command | Description |
| ------ | ----------- |
| [configcat tag](configcat-tag.md) | Manage Tags |
| [configcat tag ls](configcat-tag-ls.md) | List all Tags that belongs to the configured user |
| [configcat tag create](configcat-tag-create.md) | Create a new Tag in a specified Product identified by the `--product-id` option |
| [configcat tag rm](configcat-tag-rm.md) | Remove a Tag identified by the `--tag-id` option |
| [configcat tag update](configcat-tag-update.md) | Update a Tag identified by the `--tag-id` option |
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
| [configcat flag value](configcat-flag-value.md) | Show, and update Feature Flag or Setting values in different Environments |
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
### configcat sdk-key
| Command | Description |
| ------ | ----------- |
| [configcat sdk-key](configcat-sdk-key.md) | List SDK Keys |
### configcat scan
| Command | Description |
| ------ | ----------- |
| [configcat scan](configcat-scan.md) | Scans files for Feature Flag or Setting usages |

# Command Line Interface Reference
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
| [configcat setup](configcat-setup.md) | Setup the CLI with Management API host and credentials.<br/>You can get your credentials from here: https://app.configcat.com/my-account/public-api-credentials |
### configcat ls
| Command | Description |
| ------ | ----------- |
| [configcat ls](configcat-ls.md) | List all products, configs, and environments IDs |
### configcat product
| Command | Description |
| ------ | ----------- |
| [configcat product](configcat-product.md) | Manage products |
| [configcat product ls](configcat-product-ls.md) | List all products |
| [configcat product create](configcat-product-create.md) | Create product |
| [configcat product rm](configcat-product-rm.md) | Delete product |
| [configcat product update](configcat-product-update.md) | Update product |
### configcat config
| Command | Description |
| ------ | ----------- |
| [configcat config](configcat-config.md) | Manage configs |
| [configcat config ls](configcat-config-ls.md) | List all configs |
| [configcat config create](configcat-config-create.md) | Create config |
| [configcat config rm](configcat-config-rm.md) | Delete config |
| [configcat config update](configcat-config-update.md) | Update Config |
### configcat environment
| Command | Description |
| ------ | ----------- |
| [configcat environment](configcat-environment.md) | Manage environments |
| [configcat environment ls](configcat-environment-ls.md) | List all environments |
| [configcat environment create](configcat-environment-create.md) | Create environment |
| [configcat environment rm](configcat-environment-rm.md) | Delete environment |
| [configcat environment update](configcat-environment-update.md) | Update environment |
### configcat tag
| Command | Description |
| ------ | ----------- |
| [configcat tag](configcat-tag.md) | Manage tags |
| [configcat tag ls](configcat-tag-ls.md) | List all tags |
| [configcat tag create](configcat-tag-create.md) | Create tag |
| [configcat tag rm](configcat-tag-rm.md) | Delete tag |
| [configcat tag update](configcat-tag-update.md) | Update tag |
### configcat flag
| Command | Description |
| ------ | ----------- |
| [configcat flag](configcat-flag.md) | Manage flags & settings |
| [configcat flag ls](configcat-flag-ls.md) | List all flags |
| [configcat flag create](configcat-flag-create.md) | Create flag |
| [configcat flag rm](configcat-flag-rm.md) | Delete flag |
| [configcat flag update](configcat-flag-update.md) | Update flag |
| [configcat flag attach](configcat-flag-attach.md) | Attach tag(s) to a flag |
| [configcat flag detach](configcat-flag-detach.md) | Detach tag(s) from a flag |
| [configcat flag value](configcat-flag-value.md) | Show, and update flag values in different environments |
| [configcat flag value show](configcat-flag-value-show.md) | Show flag values, targeting, and percentage rules for each environment |
| [configcat flag value update](configcat-flag-value-update.md) | Update the flag's value |
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
| [configcat sdk-key](configcat-sdk-key.md) | List sdk keys |
### configcat scan
| Command | Description |
| ------ | ----------- |
| [configcat scan](configcat-scan.md) | Scans files for feature flag or setting usages |

# configcat product preferences update
Update a Product's preferences
## Aliases
`up`
## Usage
```
configcat product preferences update [command] [options]
```
## Example
```
configcat product preferences update -i <product-id> --reason-required true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-i` | ID of the Product |
| `--reason-required`, `-rr` | Indicates that a mandatory note is required for saving and publishing |
| `--key-gen-mode`, `-kg` | Determines the Feature Flag key generation mode<br/><br/>*Possible values*: `camelCase`, `kebabCase`, `lowerCase`, `pascalCase`, `upperCase` |
| `--show-variation-id`, `-vi` | Indicates whether a variation ID's must be shown on the ConfigCat Dashboard |
| `--mandatory-setting-hint`, `-msh` | Indicates whether Feature flags and Settings must have a hint |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat product preferences](configcat-product-preferences.md) | Manage Product preferences |
## Subcommands
| Command | Description |
| ------ | ----------- |
| [configcat product preferences update env](configcat-product-preferences-update-env.md) | Update per-environment required reason |

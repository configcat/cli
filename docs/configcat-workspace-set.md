# configcat workspace set
Set the workspace
## Aliases
`s`
## Usage
```
configcat workspace set [options]
```
## Example
```
configcat workspace set -p <product-id> -c <config-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-p` | ID of the Product |
| `--config-id`, `-c` | ID of the Config |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat workspace](configcat-workspace.md) | Manage the CLI workspace. When set, the CLI's interactive mode filters Product and Config selectors by the values set in the workspace |

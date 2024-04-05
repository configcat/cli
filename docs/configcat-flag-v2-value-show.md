# configcat flag-v2 value show
Show Feature Flag or Setting values, targeting, and percentage rules for each environment
## Aliases
`sh`, `pr`, `print`
## Usage
```
configcat flag-v2 value show [options]
```
## Example
```
configcat flag-v2 value show -i <flag-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--json` | Format the output in JSON |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 value](configcat-flag-v2-value.md) | Manage V2 Feature Flag & Setting default values in different Environments |

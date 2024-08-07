# configcat flag value show
Show Feature Flag or Setting values, targeting, and percentage rules for each environment
## Aliases
`sh`, `pr`, `print`
## Usage
```
configcat flag value show [options]
```
## Example
```
configcat flag value show -i <flag-id>
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
| [configcat flag value](configcat-flag-value.md) | Manage Feature Flag & Setting values in different Environments |

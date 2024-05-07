# configcat flag-v2 targeting percentage clear
Delete the last percentage-only rule
## Aliases
`clr`
## Usage
```
configcat flag-v2 targeting percentage clear [options]
```
## Example
```
configcat flag-v2 targeting % clear -i <flag-id> -e <environment-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment from where the rule must be deleted |
| `--reason`, `-r` | The reason note for the Audit Log if the Product's 'Config changes require a reason' preference is turned on |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting percentage](configcat-flag-v2-targeting-percentage.md) | Manage percentage-only rules |

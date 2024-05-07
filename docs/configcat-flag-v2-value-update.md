# configcat flag-v2 value update
Update the value of a Feature Flag or Setting
## Aliases
`up`
## Usage
```
configcat flag-v2 value update [options]
```
## Example
```
configcat flag-v2 value update -i <flag-id> -e <environment-id> -f true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the update must be applied |
| `--flag-value`, `-f` | The value to serve, it must respect the setting type |
| `--reason`, `-r` | The reason note for the Audit Log if the Product's 'Config changes require a reason' preference is turned on |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 value](configcat-flag-v2-value.md) | Manage V2 Feature Flag & Setting default values in different Environments |

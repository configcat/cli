# configcat flag-v2 targeting percentage update
Update or add the last percentage-only targeting rule
## Aliases
`up`
## Usage
```
configcat flag-v2 targeting percentage update [options]
```
## Example
```
configcat flag-v2 targeting % update -i <flag-id> -e <environment-id> -po 30:true 70:false
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the update must be applied |
| `-po`, `--percentage-options` | Format: `<percentage>:<value>`, e.g., `30:true 70:false` |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting percentage](configcat-flag-v2-targeting-percentage.md) | Manage percentage-only rules |

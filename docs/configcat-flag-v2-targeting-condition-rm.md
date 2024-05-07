# configcat flag-v2 targeting condition rm
Remove condition
## Usage
```
configcat flag-v2 targeting condition rm [options]
```
## Example
```
configcat flag-v2 targeting condition rm -i <flag-id> -e <environment-id> -rp 1
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the condition must be removed |
| `--rule-position`, `-rp` | The position of the targeting rule |
| `--condition-position`, `-cp` | The position of the condition to remove |
| `--reason`, `-r` | The reason note for the Audit Log if the Product's 'Config changes require a reason' preference is turned on |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting condition](configcat-flag-v2-targeting-condition.md) | Manage conditions |

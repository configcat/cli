# configcat flag targeting rm
Delete targeting rule
## Usage
```
configcat flag targeting rm [options]
```
## Example
```
configcat flag targeting rm -i <flag-id> -e <environment-id> -p 1
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment from where the rule must be deleted |
| `--position`, `-p` | The position of the targeting rule to delete |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag targeting](configcat-flag-targeting.md) | Manage targeting rules |

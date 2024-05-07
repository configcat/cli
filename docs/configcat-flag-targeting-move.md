# configcat flag targeting move
Move a targeting rule into a different position
## Aliases
`mv`
## Usage
```
configcat flag targeting move [options]
```
## Example
```
configcat flag targeting move -i <flag-id> -e <environment-id> --from 0 --to 1
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the move must be applied |
| `--from` | The position of the targeting rule to move |
| `--to` | The desired position of the targeting rule |
| `--reason`, `-r` | The reason note for the Audit Log if the Product's 'Config changes require a reason' preference is turned on |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag targeting](configcat-flag-targeting.md) | Manage targeting rules |

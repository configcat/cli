# configcat flag-v2 targeting rule move
Move targeting rule
## Aliases
`mv`
## Usage
```
configcat flag-v2 targeting rule move [options]
```
## Example
```
configcat flag-v2 targeting rule mv -i <flag-id> -e <environment-id> --from 1 --to 2
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the rule should be moved |
| `--from` | The position of the targeting rule to move |
| `--to` | The desired position of the targeting rule |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting rule](configcat-flag-v2-targeting-rule.md) | Manage targeting rules |

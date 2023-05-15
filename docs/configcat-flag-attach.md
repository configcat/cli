# configcat flag attach
Attach Tag(s) to a Feature Flag or Setting
## Aliases
`at`
## Usage
```
configcat flag attach [options]
```
## Example
```
configcat flag attach -i <flag-id> -g <tag1-id> <tag2-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting to attach Tags |
| `--tag-ids`, `-g` | Tag IDs to attach |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag](configcat-flag.md) | Manage Feature Flags & Settings |

# configcat flag-v2 detach
Detach Tag(s) from a Feature Flag or Setting
## Aliases
`dt`
## Usage
```
configcat flag-v2 detach [options]
```
## Example
```
configcat flag detach -i <flag-id> -g <tag1-id> <tag2-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting to detach Tags |
| `--tag-ids`, `-g` | Tag IDs to detach |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2](configcat-flag-v2.md) | Manage V2 Feature Flags & Settings |

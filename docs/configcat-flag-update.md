# configcat flag update
Update a Feature Flag or Setting identified by the `--flag-id` option
## Aliases
`up`
## Usage
```
configcat flag update [options]
```
## Example
```
configcat flag update -i <flag-id> -n "My awesome flag" -H "This is the most awesome flag." -g <tag1-id> <tag2-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting to update |
| `--name`, `-n` | The updated name |
| `--hint`, `-H` | The updated hint |
| `--tag-ids`, `-g` | The updated Tag list |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag](configcat-flag.md) | Manage Feature Flags & Settings |

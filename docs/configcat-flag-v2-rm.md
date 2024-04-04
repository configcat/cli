# configcat flag-v2 rm
Remove a Feature Flag or Setting identified by the `--flag-id` option
## Usage
```
configcat flag-v2 rm [options]
```
## Example
```
configcat flag rm -i <flag-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting to delete |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2](configcat-flag-v2.md) | Manage V2 Feature Flags & Settings |

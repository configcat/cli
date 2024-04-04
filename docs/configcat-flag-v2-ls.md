# configcat flag-v2 ls
List all Feature Flags & Settings that belongs to the configured user
## Usage
```
configcat flag-v2 ls [options]
```
## Example
```
configcat flag ls -n my_tag
```
## Options
| Option | Description |
| ------ | ----------- |
| `--config-id`, `-c` | Show only a Config's Flags & Settings |
| `--tag-name`, `-n` | Filter by a Tag's name |
| `--tag-id`, `-t` | Filter by a Tag's ID |
| `--json` | Format the output in JSON |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2](configcat-flag-v2.md) | Manage V2 Feature Flags & Settings |

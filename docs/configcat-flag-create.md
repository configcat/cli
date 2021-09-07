# configcat flag create
Create a new Feature Flag or Setting in a specified Config identified by the `--config-id` option
## Usage
```
configcat [options] flag create
```
## Aliases
`cr`
## Options
| Option | Description |
| ------ | ----------- |
| `--config-id`, `-c` | ID of the Config where the Flag must be created |
| `--name`, `-n` | Name of the new Flag or Setting |
| `--key`, `-k` | Key of the new Flag or Setting (must be unique within the given Config) |
| `--hint`, `-H` | Hint of the new Flag or Setting |
| `--type`, `-t` | Type of the new Flag or Setting |
| `--tag-ids`, `-g` | Tags to attach |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--json` | Format the output in JSON |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag](configcat-flag.md) | Manage Feature Flags & Settings |

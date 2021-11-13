# configcat flag value update
Update the value of a Feature Flag or Setting
## Usage
```
configcat [options] flag value update
```
## Aliases
`up`
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the update must be applied |
| `--flag-value`, `-f` | The value to serve, it must respect the setting type |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features. |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag value](configcat-flag-value.md) | Manage Feature Flag & Setting values in different Environments |

# configcat config update
Update a Config identified by the `--config-id` option
## Aliases
`up`
## Usage
```
configcat config update [options]
```
## Example
```
configcat config update -i <config-id> -n "NewConfig" -d "Config description"
```
## Options
| Option | Description |
| ------ | ----------- |
| `--config-id`, `-i` | ID of the Config to update |
| `--name`, `-n` | The updated name |
| `--description`, `-d` | The updated description |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat config](configcat-config.md) | Manage Configs |

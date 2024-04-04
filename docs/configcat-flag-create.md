# configcat flag create
Create a new Feature Flag or Setting in a specified Config identified by the `--config-id` option
## Aliases
`cr`
## Usage
```
configcat flag create [options]
```
## Example
```
configcat flag create -c <config-id> -n "My awesome flag" -k myAwesomeFlag -t boolean -H "This is the most awesome flag." -ive <env1-id>:true <env2-id>:false -g <tag1-id> <tag2-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--config-id`, `-c` | ID of the Config where the Flag must be created |
| `--name`, `-n` | Name of the new Flag or Setting |
| `--key`, `-k` | Key of the new Flag or Setting (must be unique within the given Config) |
| `--hint`, `-H` | Hint of the new Flag or Setting |
| `--init-value`, `-iv` | Initial value for each Environment |
| `--init-values-per-environment`, `-ive` | Initial value for specific Environments. Format: `<environment-id>:<initial-value>` |
| `--type`, `-t` | Type of the new Flag or Setting<br/><br/>*Possible values*: `boolean`, `double`, `int`, `string` |
| `--tag-ids`, `-g` | Tags to attach |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag](configcat-flag.md) | Manage Feature Flags & Settings |

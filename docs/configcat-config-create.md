# configcat config create
Create a new Config in a specified Product identified by the `--product-id` option
## Usage
```
configcat config create [options]
```
## Example
```
configcat config create -p <product-id> -n "NewConfig" -d "Config description"
```
## Aliases
`cr`
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-p` | ID of the Product where the Config must be created |
| `--name`, `-n` | Name of the new Config |
| `--description`, `-d` | Description of the new Config |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat config](configcat-config.md) | Manage Configs |

# configcat config create
Create a new Config in a specified Product identified by the `--product-id` option
## Aliases
`cr`
## Usage
```
configcat config create [options]
```
## Example
```
configcat config create -p <product-id> -n "NewConfig" -d "Config description"
```
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-p` | ID of the Product where the Config must be created |
| `--name`, `-n` | Name of the new Config |
| `--eval-version`, `-e` | Determines the evaluation version of the Config. Using `v2` enables the new features of Config V2<br/><br/>*Possible values*: `v1`, `v2` |
| `--description`, `-d` | Description of the new Config |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat config](configcat-config.md) | Manage Configs |

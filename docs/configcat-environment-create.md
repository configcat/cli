# configcat environment create
Create a new Environment in a specified Product identified by the `--product-id` option
## Aliases
`cr`
## Usage
```
configcat environment create [options]
```
## Example
```
configcat environment create -p <product-id> -n Test -d "Test Environment" -c #7D3C98
```
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-p` | ID of the Product where the Environment must be created |
| `--name`, `-n` | Name of the new Environment |
| `--description`, `-d` | Description of the new Environment |
| `--color`, `-c` | Color of the new Environment |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat environment](configcat-environment.md) | Manage Environments |

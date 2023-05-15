# configcat environment update
Update environment
## Aliases
`up`
## Usage
```
configcat environment update [options]
```
## Example
```
configcat environment update -i <environment-id> -n Test -d "Test Environment" -c #7D3C98
```
## Options
| Option | Description |
| ------ | ----------- |
| `--environment-id`, `-i` | ID of the environment to update |
| `--name`, `-n` | The updated name |
| `--description`, `-d` | The updated description |
| `--color`, `-c` | The updated color |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat environment](configcat-environment.md) | Manage Environments |

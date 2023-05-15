# configcat tag create
Create a new Tag in a specified Product identified by the `--product-id` option
## Usage
```
configcat tag create [options]
```
## Example
```
configcat tag create -n "temp_tag"
```
## Aliases
`cr`
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-p` | ID of the Product where the Tag must be created |
| `--name`, `-n` | The name of the new Tag |
| `--color`, `-c` | The color of the new Tag |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat tag](configcat-tag.md) | Manage Tags |

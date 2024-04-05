# configcat product create
Create a new Product in a specified Organization identified by the `--organization-id` option
## Aliases
`cr`
## Usage
```
configcat product create [options]
```
## Example
```
configcat product create -o <organization-id> -n "My Product" -d "Product Description"
```
## Options
| Option | Description |
| ------ | ----------- |
| `--organization-id`, `-o` | The Organization's ID where the Product must be created |
| `--name`, `-n` | Name of the new Product |
| `--description`, `-d` | Description of the new Product |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat product](configcat-product.md) | Manage Products |

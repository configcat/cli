# configcat product update
Update a Product identified by the `--product-id` option
## Aliases
`up`
## Usage
```
configcat product update [options]
```
## Example
```
configcat product update -i <product-id> -n "My Product" -d "Product Description"
```
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-i` | ID of the Product to update |
| `--name`, `-n` | The updated name |
| `--description`, `-d` | The updated description |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat product](configcat-product.md) | Manage Products |

# configcat product preferences update env
Update per-environment required reason
## Aliases
`e`
## Usage
```
configcat product preferences update env [options]
```
## Example
```
configcat product preferences update env -i <product-id> -ei <environment-id>:true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-i` | ID of the Product |
| `--environments`, `-ei` | Format: `<environment-id>:<reason-required>`. |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat product preferences update](configcat-product-preferences-update.md) | Update a Product's preferences |

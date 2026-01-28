# configcat flag-v2 variation update
Update a Predefined Variation
## Aliases
`up`
## Usage
```
configcat flag-v2 variation update [options]
```
## Example
```
configcat flag-v2 variation up -i <flag-id> -pvi <predefined-variation-id> -sv <served-value>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--predefined-variation-id`, `-pvi` | ID of the Predefined Variation to update |
| `--name`, `-n` | Name of the new Predefined Variation |
| `--hint`, `-H` | Hint of the new Predefined Variation |
| `--served-value`, `-sv` | The value associated with the Predefined Variation. It must respect the setting type |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 variation](configcat-flag-v2-variation.md) | Manage Predefined Variations |

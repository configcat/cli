# configcat flag-v2 targeting percentage attribute
Set the percentage evaluation attribute
## Aliases
`at`
## Usage
```
configcat flag-v2 targeting percentage attribute [options]
```
## Example
```
configcat flag-v2 targeting % attribute -i <flag-id> -e <environment-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment from where the rules must be deleted |
| `--attribute-name`, `-n` | The User Object attribute which serves as the basis of percentage options evaluation |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting percentage](configcat-flag-v2-targeting-percentage.md) | Manage percentage-only rules |

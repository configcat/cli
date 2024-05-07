# configcat flag-v2 targeting condition add prerequisite
Add new prerequisite flag based condition
## Aliases
`pr`
## Usage
```
configcat flag-v2 targeting condition add prerequisite [options]
```
## Example
```
configcat flag-v2 targeting condition add prerequisite -i <flag-id> -e <environment-id> -rp 1 -c equals -pi <prerequisite-id> -pv true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the condition must be created |
| `--rule-position`, `-rp` | The position of the targeting rule to which the condition is added |
| `--comparator`, `-c` | The operator which defines the relation between the evaluated value of the prerequisite flag and the comparison value<br/><br/>*Possible values*: `doesNotEqual`, `equals` |
| `--prerequisite-id`, `-pi` | ID of the prerequisite flag that the condition is based on |
| `--prerequisite-value`, `-pv` | The evaluated value of the prerequisite flag is compared to. It must respect the prerequisite flag's setting type |
| `--reason`, `-r` | The reason note for the Audit Log if the Product's 'Config changes require a reason' preference is turned on |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting condition add](configcat-flag-v2-targeting-condition-add.md) | Add new condition |

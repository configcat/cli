# configcat flag-v2 targeting rule create prerequisite
Create prerequisite flag based targeting rule
## Aliases
`pr`
## Usage
```
configcat flag-v2 targeting rule create prerequisite [options]
```
## Example
```
configcat flag-v2 targeting rule create prerequisite -i <flag-id> -e <environment-id> -c equals -pi <prerequisite-id> -pv true -sv true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the rule must be created |
| `--comparator`, `-c` | The operator which defines the relation between the evaluated value of the prerequisite flag and the comparison value<br/><br/>*Possible values*: `doesNotEqual`, `equals` |
| `--prerequisite-id`, `-pi` | ID of the prerequisite flag that the condition is based on |
| `--prerequisite-value`, `-pv` | The evaluated value of the prerequisite flag is compared to. It must respect the prerequisite flag's setting type |
| `--served-value`, `-sv` | The value associated with the targeting rule. Leave it empty if the targeting rule has percentage options. It must respect the setting type |
| `-po`, `--percentage-options` | Format: `<percentage>:<value>`, e.g., `30:true 70:false` |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting rule create](configcat-flag-v2-targeting-rule-create.md) | Create new targeting rule |

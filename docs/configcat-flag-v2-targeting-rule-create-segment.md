# configcat flag-v2 targeting rule create segment
Create segment based targeting rule
## Aliases
`sg`
## Usage
```
configcat flag-v2 targeting rule create segment [options]
```
## Example
```
configcat flag-v2 targeting rule create segment -i <flag-id> -e <environment-id> -c isIn -si <segment-id> -sv true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the rule must be created |
| `--comparator`, `-c` | The operator which defines the expected result of the evaluation of the segment<br/><br/>*Possible values*: `isIn`, `isNotIn` |
| `--segment-id`, `-si` | ID of the segment that the condition is based on |
| `--served-value`, `-sv` | The value associated with the targeting rule. Leave it empty if the targeting rule has percentage options. It must respect the setting type |
| `-po`, `--percentage-options` | Format: `<percentage>:<value>`, e.g., `30:true 70:false` |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting rule create](configcat-flag-v2-targeting-rule-create.md) | Create new targeting rule |
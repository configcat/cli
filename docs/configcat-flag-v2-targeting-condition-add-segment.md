# configcat flag-v2 targeting condition add segment
Add new segment based condition
## Aliases
`sg`
## Usage
```
configcat flag-v2 targeting condition add segment [options]
```
## Example
```
configcat flag-v2 targeting condition add segment -i <flag-id> -e <environment-id> -rp 1 -c isIn -si <segment-id> -f true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the condition must be created |
| `--rule-position`, `-rp` | The position of the targeting rule to which the condition is added |
| `--comparator`, `-c` | The operator which defines the expected result of the evaluation of the segment<br/><br/>*Possible values*: `isIn`, `isNotIn` |
| `--segment-id`, `-si` | ID of the segment that the condition is based on |
| `--reason`, `-r` | The reason note for the Audit Log if the Product's 'Config changes require a reason' preference is turned on |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting condition add](configcat-flag-v2-targeting-condition-add.md) | Add new condition |

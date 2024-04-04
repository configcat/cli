# configcat flag-v2 targeting rule update-served-value
Update a targeting rule's served value
## Aliases
`usv`
## Usage
```
configcat flag-v2 targeting rule update-served-value [options]
```
## Example
```
configcat flag-v2 targeting rule usv -i <flag-id> -e <environment-id> -rp 1 -sv true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the rule should be moved |
| `--rule-position`, `-rp` | The position of the targeting rule |
| `--served-value`, `-sv` | The value associated with the targeting rule. Leave it empty if the targeting rule has percentage options. It must respect the setting type |
| `-po`, `--percentage-options` | Format: `<percentage>:<value>`, e.g., `30:true 70:false` |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting rule](configcat-flag-v2-targeting-rule.md) | Manage targeting rules |

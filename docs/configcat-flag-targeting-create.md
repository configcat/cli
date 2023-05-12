# configcat flag targeting create
Create new targeting rule
## Usage
```
configcat [options] flag targeting create
```
## Aliases
`cr`
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the rule must be created |
| `--attribute`, `-a` | The user attribute to compare |
| `--comparator`, `-c` `<contains|doesNotContain|isNotOneOf|isOneOf|numberDoesNotEqual|numberEquals|numberGreater|numberGreaterOrEquals|numberLess|numberLessOrEquals|semVerGreater|semVerGreaterOrEquals|semVerIsNotOneOf|semVerIsOneOf|semVerLess|semVerLessOrEquals|sensitiveIsNotOneOf|sensitiveIsOneOf>` | The comparison operator |
| `--compare-to`, `-t` | The value to compare against |
| `--flag-value`, `-f` | The value to serve when the comparison matches, it must respect the setting type |
| `--segment-id`, `-si` | ID of the Segment used in the rule |
| `--segment-comparator`, `-sc` `<isIn|isNotIn>` | The segment comparison operator |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag targeting](configcat-flag-targeting.md) | Manage targeting rules |

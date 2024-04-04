# configcat flag targeting create
Create new targeting rule
## Aliases
`cr`
## Usage
```
configcat flag targeting create [options]
```
## Example
```
configcat flag targeting create -i <flag-id> -e <environment-id> -a Email -c contains -t @example.com -f true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the rule must be created |
| `--attribute`, `-a` | The user attribute to compare |
| `--comparator`, `-c` | The comparison operator<br/><br/>*Possible values*: `contains`, `doesNotContain`, `isNotOneOf`, `isOneOf`, `numberDoesNotEqual`, `numberEquals`, `numberGreater`, `numberGreaterOrEquals`, `numberLess`, `numberLessOrEquals`, `semVerGreater`, `semVerGreaterOrEquals`, `semVerIsNotOneOf`, `semVerIsOneOf`, `semVerLess`, `semVerLessOrEquals`, `sensitiveIsNotOneOf`, `sensitiveIsOneOf` |
| `--compare-to`, `-t` | The value to compare against |
| `--flag-value`, `-f` | The value to serve when the comparison matches, it must respect the setting type |
| `--segment-id`, `-si` | ID of the Segment used in the rule |
| `--segment-comparator`, `-sc` | The segment comparison operator<br/><br/>*Possible values*: `isIn`, `isNotIn` |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag targeting](configcat-flag-targeting.md) | Manage targeting rules |

# configcat flag targeting update
Update targeting rule
## Aliases
`up`
## Usage
```
configcat flag targeting update [options]
```
## Example
```
configcat flag targeting update -i <flag-id> -e <environment-id> -p 1 -a Email -c contains -t @example.com -f true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the update must be applied |
| `--position`, `-p` | The position of the updating targeting rule |
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

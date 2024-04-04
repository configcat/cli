# configcat flag-v2 targeting condition add user
Add new user based condition
## Aliases
`u`
## Usage
```
configcat flag-v2 targeting condition add user [options]
```
## Example
```
configcat flag-v2 targeting condition add user -i <flag-id> -e <environment-id> -rp 1 -a Email -c contains -cv @example.com -f true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the condition must be created |
| `--rule-position`, `-rp` | The position of the targeting rule to which the condition is added |
| `--attribute`, `-a` | The User Object attribute that the condition is based on |
| `--comparator`, `-c` | The operator which defines the relation between the comparison attribute and the comparison value<br/><br/>*Possible values*: `arrayContainsAnyOf`, `arrayDoesNotContainAnyOf`, `containsAnyOf`, `dateTimeAfter`, `dateTimeBefore`, `doesNotContainAnyOf`, `isNotOneOf`, `isOneOf`, `numberDoesNotEqual`, `numberEquals`, `numberGreater`, `numberLess`, `numberLessOrEquals`, `semVerGreater`, `semVerGreaterOrEquals`, `semVerIsNotOneOf`, `semVerIsOneOf`, `semVerLess`, `semVerLessOrEquals`, `sensitiveArrayContainsAnyOf`, `sensitiveArrayDoesNotContainAnyOf`, `sensitiveIsNotOneOf`, `sensitiveIsOneOf`, `sensitiveTextDoesNotEqual`, `sensitiveTextEndsWithAnyOf`, `sensitiveTextEquals`, `sensitiveTextNotEndsWithAnyOf`, `sensitiveTextNotStartsWithAnyOf`, `sensitiveTextStartsWithAnyOf`, `textDoesNotEqual`, `textEndsWithAnyOf`, `textEquals`, `textNotEndsWithAnyOf`, `textNotStartsWithAnyOf`, `textStartsWithAnyOf` |
| `--comparison-value`, `-cv` | The value that the User Object attribute is compared to. Can be a double, string, or value-hint list in the format: `<value>:<hint>` |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting condition add](configcat-flag-v2-targeting-condition-add.md) | Add new condition |

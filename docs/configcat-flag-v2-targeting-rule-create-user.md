# configcat flag-v2 targeting rule create user
Create user based targeting rule
## Aliases
`u`
## Usage
```
configcat flag-v2 targeting rule create user [options]
```
## Example
```
configcat flag-v2 targeting rule create user -i <flag-id> -e <environment-id> -a Email -c contains -cv @example.com -sv true
```
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the rule must be created |
| `--attribute`, `-a` | The User Object attribute that the condition is based on |
| `--comparator`, `-c` | The operator which defines the relation between the comparison attribute and the comparison value<br/><br/>*Possible values*: `arrayContainsAnyOf`, `arrayDoesNotContainAnyOf`, `containsAnyOf`, `dateTimeAfter`, `dateTimeBefore`, `doesNotContainAnyOf`, `isNotOneOf`, `isOneOf`, `numberDoesNotEqual`, `numberEquals`, `numberGreater`, `numberLess`, `numberLessOrEquals`, `semVerGreater`, `semVerGreaterOrEquals`, `semVerIsNotOneOf`, `semVerIsOneOf`, `semVerLess`, `semVerLessOrEquals`, `sensitiveArrayContainsAnyOf`, `sensitiveArrayDoesNotContainAnyOf`, `sensitiveIsNotOneOf`, `sensitiveIsOneOf`, `sensitiveTextDoesNotEqual`, `sensitiveTextEndsWithAnyOf`, `sensitiveTextEquals`, `sensitiveTextNotEndsWithAnyOf`, `sensitiveTextNotStartsWithAnyOf`, `sensitiveTextStartsWithAnyOf`, `textDoesNotEqual`, `textEndsWithAnyOf`, `textEquals`, `textNotEndsWithAnyOf`, `textNotStartsWithAnyOf`, `textStartsWithAnyOf` |
| `--comparison-value`, `-cv` | The value that the User Object attribute is compared to. Can be a double, string, or value-hint list in the format: `<value>:<hint>` |
| `--served-value`, `-sv` | The value associated with the targeting rule. Leave it empty if the targeting rule has percentage options. It must respect the setting type |
| `-po`, `--percentage-options` | Format: `<percentage>:<value>`, e.g., `30:true 70:false` |
| `--reason`, `-r` | The reason note for the Audit Log if the Product's 'Config changes require a reason' preference is turned on |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag-v2 targeting rule create](configcat-flag-v2-targeting-rule-create.md) | Create new targeting rule |

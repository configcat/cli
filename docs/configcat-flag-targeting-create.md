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
| `--flag-id`, `-i`, `--setting-id` | ID of the flag or setting |
| `--environment-id`, `-e` | ID of the environment where the rule must be created |
| `--attribute`, `-a` | The user attribute to compare |
| `--comparator`, `-c` | The comparison operator |
| `--compare-to`, `-t` | The value to compare against |
| `--flag-value`, `-f` | The value to serve when the comparison matches, it must respect the setting type |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat flag targeting](configcat-flag-targeting.md) | Manage targeting rules |

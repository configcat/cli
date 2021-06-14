# configcat flag targeting update
[GitHub](https://github.com/configcat/cli) | [Documentation](https://configcat.com/docs/advanced/cli)

Update targeting rule
## Usage
```
configcat [options] flag targeting update
```
## Aliases
`up`
## Options
| Option | Description |
| ------ | ----------- |
| `--flag-id`, `-i`, `--setting-id` | ID of the Feature Flag or Setting |
| `--environment-id`, `-e` | ID of the Environment where the update must be applied |
| `--position`, `-p` | The position of the updating targeting rule |
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

# configcat flag
Manage Feature Flags & Settings
## Usage
```
configcat flag [command]
```
## Aliases
`setting`, `f`, `s`
## Options
| Option | Description |
| ------ | ----------- |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--json` | Format the output in JSON |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat](index.md) | This is the Command Line Tool of ConfigCat.<br/>ConfigCat is a hosted feature flag service: https://configcat.com<br/>For more information, see the documentation here: https://configcat.com/docs/advanced/cli |
## Subcommands
| Command | Description |
| ------ | ----------- |
| [configcat flag ls](configcat-flag-ls.md) | List all Feature Flags & Settings that belongs to the configured user |
| [configcat flag create](configcat-flag-create.md) | Create a new Feature Flag or Setting in a specified Config identified by the `--config-id` option |
| [configcat flag rm](configcat-flag-rm.md) | Remove a Feature Flag or Setting identified by the `--flag-id` option |
| [configcat flag update](configcat-flag-update.md) | Update a Feature Flag or Setting identified by the `--flag-id` option |
| [configcat flag attach](configcat-flag-attach.md) | Attach Tag(s) to a Feature Flag or Setting |
| [configcat flag detach](configcat-flag-detach.md) | Detach Tag(s) from a Feature Flag or Setting |
| [configcat flag value](configcat-flag-value.md) | Show, and update Feature Flag or Setting values in different Environments |
| [configcat flag targeting](configcat-flag-targeting.md) | Manage targeting rules |
| [configcat flag percentage](configcat-flag-percentage.md) | Manage percentage rules |

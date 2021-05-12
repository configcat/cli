# configcat tag
Manage Tags. Tags are stored under a Product. You can attach a Tag to a Feature Flag or Setting.
## Usage
```
configcat tag [command]
```
## Aliases
`t`
## Options
| Option | Description |
| ------ | ----------- |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat](index.md) | This is the Command Line Tool of ConfigCat.<br/>ConfigCat is a hosted feature flag service: https://configcat.com<br/>For more information, see the documentation here: https://configcat.com/docs/ |
## Subcommands
| Command | Description |
| ------ | ----------- |
| [configcat tag ls](configcat-tag-ls.md) | List all Tags that belongs to the configured user |
| [configcat tag create](configcat-tag-create.md) | Create a new Tag in a specified Product identified by the `--product-id` option |
| [configcat tag rm](configcat-tag-rm.md) | Remove a Tag identified by the `--tag-id` option |
| [configcat tag update](configcat-tag-update.md) | Update a Tag identified by the `--tag-id` option |

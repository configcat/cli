# configcat permission-group
Manage Permission Groups
## Aliases
`pg`
## Usage
```
configcat permission-group [command]
```
## Options
| Option | Description |
| ------ | ----------- |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat](index.md) | This is the Command Line Tool of ConfigCat.<br/>ConfigCat is a hosted feature flag service: https://configcat.com<br/>For more information, see the documentation here: https://configcat.com/docs/advanced/cli |
## Subcommands
| Command | Description |
| ------ | ----------- |
| [configcat permission-group ls](configcat-permission-group-ls.md) | List all Permission Groups that manageable by the configured user |
| [configcat permission-group create](configcat-permission-group-create.md) | Create a new Permission Group in a specified Product identified by the `--product-id` option |
| [configcat permission-group rm](configcat-permission-group-rm.md) | Remove a Permission Group identified by the `--permission-group-id` option |
| [configcat permission-group update](configcat-permission-group-update.md) | Update a Permission Group identified by the `--permission-group-id` option |
| [configcat permission-group show](configcat-permission-group-show.md) | Show details of a Permission Group identified by the `--permission-group-id` option |
| [configcat permission-group env](configcat-permission-group-env.md) | Update the environment specific permissions of a Permission Group |

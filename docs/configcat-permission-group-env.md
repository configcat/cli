# configcat permission-group env
Update the environment specific permissions of a Permission Group
## Usage
```
configcat [options] permission-group env
```
## Options
| Option | Description |
| ------ | ----------- |
| `--permission-group-id`, `-i` | ID of the Permission Group |
| `--access-type`, `-a` | Access configuration for all environments<br/>**Available options**: `custom`, `full`, `readOnly` |
| `--new-environment-access-type`, `-na` | Access configuration for newly created environments. Interpreted only when the --access-type option is `custom` which translates to `Environment specific`<br/>**Available options**: `full`, `none`, `readOnly` |
| `--environment-specific-access-types`, `-esat` | Format: `<environment-id>:<access-type>`. Interpreted only when the --access-type is `custom` which translates to `Environment specific` |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat permission-group](configcat-permission-group.md) | Manage Permission Groups |

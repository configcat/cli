# configcat permission-group env
Update the environment specific permissions of a Permission Group
## Usage
```
configcat permission-group env [options]
```
## Example
```
configcat permission-group env -i <permission-group-id> -a custom -na readOnly -esat <environment-id>:full -def readOnly
```
## Options
| Option | Description |
| ------ | ----------- |
| `--permission-group-id`, `-i` | ID of the Permission Group |
| `--access-type`, `-a` | Access configuration for all environments<br/><br/>*Possible values*: `custom`, `full`, `readOnly` |
| `--new-environment-access-type`, `-na` | Access configuration for newly created environments. Interpreted only when the --access-type option is `custom` which translates to `Environment specific`<br/><br/>*Possible values*: `full`, `none`, `readOnly` |
| `--environment-specific-access-types`, `-esat` | Format: `<environment-id>:<access-type>`. Interpreted only when the --access-type is `custom` which translates to `Environment specific` |
| `--default-access-type-when-not-set`, `-def` | Access configuration for each environment not specified with --environment-specific-access-types. Interpreted only when the --access-type option is `custom` which translates to `Environment specific`<br/><br/>*Possible values*: `full`, `none`, `readOnly` |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat permission-group](configcat-permission-group.md) | Manage Permission Groups |

# configcat member add-permission
Add Member to Permission Groups
## Aliases
`a`
## Usage
```
configcat member add-permission [options]
```
## Example
```
configcat member add-permission -o <organization-id> -i <user-id> -pgi <permission-group-id1> <permission-group-id2>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--organization-id`, `-o` | ID of the Organization |
| `--user-id`, `-i` | ID of the Member to add |
| `--permission-group-ids`, `-pgi` | Permission Group IDs the Member must be put into |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat member](configcat-member.md) | Manage Members |

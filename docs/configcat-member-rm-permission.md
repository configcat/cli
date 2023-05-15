# configcat member rm-permission
Remove Member from Permission Groups
## Usage
```
configcat member rm-permission [options]
```
## Example
```
configcat member rm-permission -o <organization-id> -i <user-id> -pgi <permission-group-id1> <permission-group-id2>
```
## Aliases
`rmp`
## Options
| Option | Description |
| ------ | ----------- |
| `--organization-id`, `-o` | ID of the Organization |
| `--user-id`, `-i` | ID of the Member to remove |
| `--permission-group-ids`, `-pgi` | Permission Group IDs the Member must be removed from |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat member](configcat-member.md) | Manage Members |

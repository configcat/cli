# configcat member invite
Invite Member(s) into a Product
## Aliases
`inv`
## Usage
```
configcat member invite [<emails>...] [options]
```
## Example
```
configcat member invite user1@example.com user2@example.com -p <product-id> -pgi <permission-group-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-p` | The Product's ID to where the Members will be invited |
| `--permission-group-id`, `-pgi` | The Permission Group's ID to where the invited Members will join |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Arguments
| Argument | Description |
| ------ | ----------- |
| `<emails>` | List of email addresses to invite |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat member](configcat-member.md) | Manage Members |

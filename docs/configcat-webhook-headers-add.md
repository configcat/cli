# configcat webhook headers add
Add new header
## Aliases
`a`
## Usage
```
configcat webhook headers add [options]
```
## Example
```
configcat webhook headers add -i <webhook-id> -k Authorization -val "Bearer ..." --secure
```
## Options
| Option | Description |
| ------ | ----------- |
| `--webhook-id`, `-i` | ID of the Webhook to update |
| `--key`, `-k` | The Webhook header's key |
| `--value`, `-val` | The Webhook header's value |
| `--secure`, `-s` | If it's true, the Webhook header's value will kept as a secret |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat webhook headers](configcat-webhook-headers.md) | Manage Webhook headers |

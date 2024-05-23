# configcat webhook headers rm
Remove header
## Usage
```
configcat webhook headers rm [options]
```
## Example
```
configcat webhook headers rm -i <webhook-id> -k Authorization
```
## Options
| Option | Description |
| ------ | ----------- |
| `--webhook-id`, `-i` | ID of the Webhook to update |
| `--key`, `-k` | The Webhook header's key |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat webhook headers](configcat-webhook-headers.md) | Manage Webhook headers |

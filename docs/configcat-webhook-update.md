# configcat webhook update
Update a Webhook identified by the `--webhook-id` option
## Aliases
`up`
## Usage
```
configcat webhook update [options]
```
## Example
```
configcat webhook update -i <webhook-id> -u "https://example.com/hook" -m get
```
## Options
| Option | Description |
| ------ | ----------- |
| `--webhook-id`, `-i` | ID of the Webhook to update |
| `--url`, `-u` | The Webhook's URL |
| `--http-method`, `-m` | The Webhook's HTTP method<br/><br/>*Possible values*: `get`, `post` |
| `--content`, `-co` | The Webhook's HTTP body |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat webhook](configcat-webhook.md) | Manage Webhooks |

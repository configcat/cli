# configcat webhook create
Create a new Webhook
## Aliases
`cr`
## Usage
```
configcat webhook create [options]
```
## Example
```
configcat webhook create -c <config-id> -e <environment-id> -u "https://example.com/hook" -m get
```
## Options
| Option | Description |
| ------ | ----------- |
| `--config-id`, `-c` | ID of the Config |
| `--environment-id`, `-e` | ID of the Environment |
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

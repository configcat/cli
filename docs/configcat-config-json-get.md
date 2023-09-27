# configcat config-json get
Download a config JSON from the CDN servers.
## Usage
```
configcat config-json get <sdk-key> [options]
```
## Example
```
configcat config-json get -f v6 PKDVCLf-Hq-h-kCzMp-L7Q/HhOWfwVtZ0mb30i9wi17GQ > config.json
```
## Options
| Option | Description |
| ------ | ----------- |
| `-f`, `--format` | The config JSON format version.<br/><br/>*Possible values*: `v5`, `v6` |
| `--eu` | Use the ConfigCat CDN servers located in the EU. Specify this option if you enabled EU Only data governance. |
| `--base-url`, `-u` | Use the server accessible at the specified URL. Specify this option if you set up a proxy server. |
| `--pretty`, `-p` | Pretty print the downloaded JSON. |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Arguments
| Argument | Description |
| ------ | ----------- |
| `<sdk-key>` | The SDK key identifying the config to download. |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat config-json](configcat-config-json.md) | Config JSON-related utilities |

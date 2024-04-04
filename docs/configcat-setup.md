# configcat setup
Setup the CLI with Public Management API host and credentials
## Usage
```
configcat setup [options]
```
## Example
```
configcat setup -H api.configcat.com -u <user-name> -p <password>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--api-host`, `-H` | The Management API host, also used from CONFIGCAT_API_HOST. (default 'api.configcat.com') |
| `--username`, `-u` | The Management API basic authentication username, also used from CONFIGCAT_API_USER |
| `--password`, `-p` | The Management API basic authentication password, also used from CONFIGCAT_API_PASS |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat](index.md) | This is the Command Line Tool of ConfigCat.<br/>ConfigCat is a hosted feature flag service: https://configcat.com<br/>For more information, see the documentation here: https://configcat.com/docs/advanced/cli |

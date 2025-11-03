# configcat eval
Evaluate feature flags
## Usage
```
configcat eval [options]
```
## Example
```
configcat eval -sk <sdk-key> -fk <flag-keys> -ua id:<user-id>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--sdk-key`, `-sk` | SDK key identifying the config to download, also loaded from CONFIGCAT_SDK_KEY |
| `--flag-keys`, `-fk` | Feature flag keys to evaluate |
| `-user-attributes`, `-ua` | User attributes for flag evaluation. Format: `<key>:<value>`. Dedicated User Object attributes are mapped like the following: Identifier => id, Email => email, Country => country |
| `--base-url`, `-u` | The CDN base url from where the CLI will download the config JSON. Defaults to ConfigCat CDN servers |
| `--data-governance`, `-dg` | Describes the location of your feature flag and setting data within the ConfigCat CDN<br/><br/>*Possible values*: `eu`, `global` |
| `--json` | Format the output in JSON |
| `--map` | Format the output in semicolon delimited map: <key1>=<value1>;<key2>=<value2> |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat](index.md) | This is the Command Line Tool of ConfigCat.<br/>ConfigCat is a hosted feature flag service: https://configcat.com<br/>For more information, see the documentation here: https://configcat.com/docs/advanced/cli |

# configcat config-json convert
Convert between config JSON versions
## Usage
```
configcat config-json convert [<conversion>] [options]
```
## Example
```
configcat config-json convert v5-to-v6 < config_v5.json
```
## Options
| Option | Description |
| ------ | ----------- |
| `--hash-map`, `-h` | A JSON file containing the original values of hashed comparison valuesin the following format: '{"<hash>":"<original-value>"}'. |
| `--skip-salt`, `-s` | Skip writing salt into the converted JSON if it would be unused. |
| `--pretty`, `-p` | Pretty print the converted JSON. |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Arguments
| Argument | Description |
| ------ | ----------- |
| `<test-v5|test-v6|v5-to-v6>` | The conversion to perform. [default: v5-to-v6] |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat config-json](configcat-config-json.md) | Config JSON-related utilities |

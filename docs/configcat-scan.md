# configcat scan
Scans files for feature flag or setting usages
## Usage
```
configcat [options] scan <directory>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--config-id`, `-c` | ID of the config to scan against |
| `--line-count`, `-l` | Context line count before and after the reference line |
| `--print`, `-p` | Print found references |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Arguments
| Argument | Description |
| ------ | ----------- |
| `<directory>` | Directory to scan |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat](index.md) | This is the Command Line Tool of ConfigCat.<br/>ConfigCat is a hosted feature flag service: https://configcat.com<br/>For more information, see the documentation here: https://configcat.com/docs/ |

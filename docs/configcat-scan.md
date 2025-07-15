# configcat scan
Scan files for Feature Flag & Setting usages
## Usage
```
configcat scan <directory> [options]
```
## Example
```
configcat scan ./dir -c <config-id> -l 5 --print
```
## Options
| Option | Description |
| ------ | ----------- |
| `--config-id`, `-c` | ID of the Config to scan against |
| `--line-count`, `-l` | Context line count before and after the reference line (min: 1, max: 10) |
| `--timeout`, `-to` | Scan timeout in seconds (default: 1800, min: 60) |
| `--print`, `-p` | Print found references to output |
| `--upload`, `-u` | Upload references to ConfigCat |
| `--repo`, `-r` | Repository name. Mandatory for code reference upload |
| `--branch`, `-b` | Branch name. When the scanned folder is a Git repo, it's determined automatically, otherwise, it must be set manually |
| `--commit-hash`, `-cm` | Commit hash. When the scanned folder is a Git repo, it's determined automatically, otherwise, it must be set manually |
| `--file-url-template`, `-f` | Template url used to generate VCS file links. Available template parameters: `commitHash`, `filePath`, `lineNumber`. Example: https://github.com/my/repo/blob/{commitHash}/{filePath}#L{lineNumber} |
| `--commit-url-template`, `-ct` | Template url used to generate VCS commit links. Available template parameters: `commitHash`. Example: https://github.com/my/repo/commit/{commitHash} |
| `--runner`, `-ru` | Overrides the default `ConfigCat CLI {version}` executor label on the ConfigCat dashboard |
| `--exclude-flag-keys`, `-ex` | Exclude the given Feature Flag keys from scanning. E.g.: `-ex flag1 flag2` or `-ex 'flag1,flag2'` |
| `--alias-patterns`, `-ap` | List of custom regex patterns used to search for additional aliases |
| `--usage-patterns`, `-up` | List of custom regex patterns that describe additional feature flag key usages |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Arguments
| Argument | Description |
| ------ | ----------- |
| `<directory>` | Directory to scan |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat](index.md) | This is the Command Line Tool of ConfigCat.<br/>ConfigCat is a hosted feature flag service: https://configcat.com<br/>For more information, see the documentation here: https://configcat.com/docs/advanced/cli |

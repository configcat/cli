# configcat scan
Scan files for Feature Flag & Setting usages
## Usage
```
configcat [options] scan <directory>
```
## Options
| Option | Description |
| ------ | ----------- |
| `--config-id`, `-c` | ID of the Config to scan against |
| `--line-count`, `-l` | Context line count before and after the reference line (min: 1, max: 10) |
| `--print`, `-p` | Print found references to output |
| `--upload`, `-u` | Upload references to ConfigCat |
| `--repo`, `-r` | Repository name. Mandatory for code reference upload |
| `--branch`, `-b` | Branch name. When the scanned folder is a Git repo, it's determined automatically, otherwise, it must be set manually |
| `--commit-hash`, `-cm` | Commit hash. When the scanned folder is a Git repo, it's determined automatically, otherwise, it must be set manually |
| `--file-url-template`, `-f` | Template url used to generate VCS file links. Available template parameters: `commitHash`, `filePath`, `lineNumber`. Example: https://github.com/my/repo/blob/{commitHash}/{filePath}#L{lineNumber} |
| `--commit-url-template`, `-ct` | Template url used to generate VCS commit links. Available template parameters: `commitHash`. Example: https://github.com/my/repo/commit/{commitHash} |
| `--runner`, `-ru` | Overrides the default `ConfigCat CLI {version}` executor label on the ConfigCat dashboard |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features. |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Arguments
| Argument | Description |
| ------ | ----------- |
| `<directory>` | Directory to scan |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat](index.md) | This is the Command Line Tool of ConfigCat.<br/>ConfigCat is a hosted feature flag service: https://configcat.com<br/>For more information, see the documentation here: https://configcat.com/docs/advanced/cli |

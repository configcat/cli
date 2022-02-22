# configcat segment create
Create a new Segment in a specified Product identified by the `--product-id` option
## Usage
```
configcat [options] segment create
```
## Aliases
`cr`
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-p` | ID of the Product where the Segment must be created |
| `--name`, `-n` | Name of the new Segment |
| `--description`, `-d` | Description of the new Segment |
| `--attribute`, `-a` | The user attribute to compare |
| `--comparator`, `-c` | The comparison operator |
| `--compare-to`, `-t` | The value to compare against |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat segment](configcat-segment.md) | Manage Segments |

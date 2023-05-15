# configcat segment create
Create a new Segment in a specified Product identified by the `--product-id` option
## Aliases
`cr`
## Usage
```
configcat segment create [options]
```
## Example
```
configcat segment create -p <product-id> -n "Beta users" -d "Beta users" -a Email -c contains -t @example.com
```
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-p` | ID of the Product where the Segment must be created |
| `--name`, `-n` | Name of the new Segment |
| `--description`, `-d` | Description of the new Segment |
| `--attribute`, `-a` | The user attribute to compare |
| `--comparator`, `-c` | The comparison operator<br/><br/>*Possible values*: `contains`, `doesNotContain`, `isNotOneOf`, `isOneOf`, `numberDoesNotEqual`, `numberEquals`, `numberGreater`, `numberGreaterOrEquals`, `numberLess`, `numberLessOrEquals`, `semVerGreater`, `semVerGreaterOrEquals`, `semVerIsNotOneOf`, `semVerIsOneOf`, `semVerLess`, `semVerLessOrEquals`, `sensitiveIsNotOneOf`, `sensitiveIsOneOf` |
| `--compare-to`, `-t` | The value to compare against |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat segment](configcat-segment.md) | Manage Segments |

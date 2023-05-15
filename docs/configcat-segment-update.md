# configcat segment update
Update a Segment identified by the `--segment-id` option
## Aliases
`up`
## Usage
```
configcat segment update [options]
```
## Example
```
configcat segment update -i <segment-id> -n "Beta users" -d "Beta users" -a Email -c contains -t @example.com
```
## Options
| Option | Description |
| ------ | ----------- |
| `--segment-id`, `-i` | ID of the Segment to update |
| `--name`, `-n` | The updated name |
| `--description`, `-d` | The updated description |
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

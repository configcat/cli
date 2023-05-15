# configcat permission-group update
Update a Permission Group identified by the `--permission-group-id` option
## Usage
```
configcat permission-group update [options]
```
## Example
```
configcat permission-group update -i <permission-group-id> -n Developers --can-view-product-audit-log
```
## Aliases
`up`
## Options
| Option | Description |
| ------ | ----------- |
| `--permission-group-id`, `-i` | ID of the Permission Group to update |
| `--name`, `-n` | The updated name |
| `--can-manage-members` | Manage Members and Permission Groups |
| `--can-create-or-update-config` | Create, edit, and reorder Configs |
| `--can-delete-config` | Delete Configs |
| `--can-create-or-update-environment` | Create, edit and reorder Environments |
| `--can-delete-environment` | Delete Environments |
| `--can-create-or-update-setting` | Create, rename, reorder Feature Flags and change their description |
| `--can-tag-setting` | Add, and remove Tags from Feature Flags |
| `--can-delete-setting` | Delete Feature Flags |
| `--can-create-or-update-tag` | Create, rename Tags and change their color |
| `--can-delete-tag` | Delete Tags |
| `--can-manage-webhook` | Create, update, and delete Webhooks |
| `--can-use-export-import` | Export (download), and import (upload) Configs, Environments, and Feature Flags |
| `--can-manage-product-preferences` | Access, and change Product preferences |
| `--can-manage-integrations` | Connect, and disconnect 3rd party integrations |
| `--can-view-sdk-key` | View the SDK key, and the code examples |
| `--can-rotate-sdk-key` | Add, and remove SDK keys |
| `--can-view-product-statistics` | View the config.json download statistics |
| `--can-view-product-audit-log` | View the Product level Audit Log about who changed what in the Product |
| `--can-create-or-update-segments` | Create, and edit Segments |
| `--can-delete-segments` | Delete Segments |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat permission-group](configcat-permission-group.md) | Manage Permission Groups |

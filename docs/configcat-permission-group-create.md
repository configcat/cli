# configcat permission-group create
Create a new Permission Group in a specified Product identified by the `--product-id` option
## Aliases
`cr`
## Usage
```
configcat permission-group create [options]
```
## Example
```
configcat permission-group create -p <product-id> -n Developers --can-view-sdk-key --can-view-product-statistics --default-when-not-set false
```
## Options
| Option | Description |
| ------ | ----------- |
| `--product-id`, `-p` | ID of the Product where the Permission Group must be created |
| `--name`, `-n` | Name of the new Permission Group |
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
| `--default-when-not-set`, `-def` | Indicates whether each unspecified permission should be enabled or disabled by default |
| `--verbose`, `-v`, `/v` | Print detailed execution information |
| `--non-interactive`, `-ni` | Turn off progress rendering and interactive features |
| `-h`, `/h`, `--help`, `-?`, `/?` | Show help and usage information |
## Parent Command
| Command | Description |
| ------ | ----------- |
| [configcat permission-group](configcat-permission-group.md) | Manage Permission Groups |

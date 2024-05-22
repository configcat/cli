#!/usr/bin/pwsh

$cliPath = $args[0]
$scanPath = $args[1]

function Invoke-ConfigCat {
	param(
		[Parameter(Mandatory)]
		[string[]]$invokeArgs
	)

    if($PSVersionTable.Platform -eq "Unix") {
        $output = sh -c "$cliPath $invokeArgs 2>&1"
    } else {
        $output = cmd /c $cliPath $invokeArgs '2>&1'
    }

	return $output -join "`n"
}

BeforeAll {
    $organizationId = "08d8f29c-65fc-40d7-852e-4605da10d03c"
    $productName = "product_$([guid]::NewGuid().ToString())"
    $productId = Invoke-ConfigCat "product", "create", "-o", $organizationId, "-n", $productName
    Invoke-ConfigCat "product", "ls" | Should -Match ([regex]::Escape($productName))

    $configName = "CLI-IntegTest-Config"
    $configId = Invoke-ConfigCat "config", "create", "-p", $productId, "-n", $configName
    Invoke-ConfigCat "config", "ls", "-p", $productId | Should -Match ([regex]::Escape($configName))

    $environmentName = "CLI-IntegTest-Env"
    $environmentId = Invoke-ConfigCat "environment", "create", "-p", $productId, "-n", $environmentName
    Invoke-ConfigCat "environment", "ls", "-p", $productId | Should -Match ([regex]::Escape($environmentName))

    $segmentName = "CLI-IntegTest-Segment"
    $segmentId = Invoke-ConfigCat "segment", "create", "-p", $productId, "-n", $segmentName, "-a", "Identifier", "-c", "doesnotcontain", "-t", "sample.com"
    Invoke-ConfigCat "segment", "ls", "-p", $productId | Should -Match ([regex]::Escape($segmentName))
    $details = Invoke-ConfigCat "segment", "sh", "-i", $segmentId
    $details | Should -Match ([regex]::Escape("when Identifier DOES NOT CONTAIN sample.com"))
}

AfterAll {
    Invoke-ConfigCat "environment", "rm", "-i", $environmentId
    Invoke-ConfigCat "environment", "ls", "-p", $productId | Should -Not -Match ([regex]::Escape($environmentId))

    Invoke-ConfigCat "config", "rm", "-i", $configId
    Invoke-ConfigCat "config", "ls", "-p", $productId | Should -Not -Match ([regex]::Escape($configId))

    Invoke-ConfigCat "segment", "rm", "-i", $segmentId
    Invoke-ConfigCat "segment", "ls", "-p", $productId | Should -Not -Match ([regex]::Escape($segmentId))

    Invoke-ConfigCat "product", "rm", "-i", $productId
    Invoke-ConfigCat "product", "ls" | Should -Not -Match ([regex]::Escape($productId))
}

Describe "Setup Tests" {
    It "Setup" {
       Invoke-ConfigCat "setup", "-H", $Env:CONFIGCAT_API_HOST, "-u", $Env:CONFIGCAT_API_USER, "-p", $Env:CONFIGCAT_API_PASS | Should -Match ([regex]::Escape("Setup complete."))
    }
}

Describe "Product / Config / Environment Tests" {
    It "Rename Product" {
        $newProductName = "product_$([guid]::NewGuid().ToString())"
        Invoke-ConfigCat "product", "update", "-i", $productId, "-n", $newProductName
        Invoke-ConfigCat "product", "ls" | Should -Match ([regex]::Escape($newProductName))
    }

    It "Rename Config" {
        $newConfigName = "CLI-IntegTest-Config-Updated"
        Invoke-ConfigCat "config", "update", "-i", $configId, "-n", $newConfigName
        Invoke-ConfigCat "config", "ls", "-p", $productId | Should -Match ([regex]::Escape($newConfigName))
    }

    It "Rename Environment" {
        $newEnvironmentName = "CLI-IntegTest-Env-Updated"
        Invoke-ConfigCat "environment", "update", "-i", $environmentId, "-n", $newEnvironmentName
        Invoke-ConfigCat "environment", "ls", "-p", $productId | Should -Match ([regex]::Escape($newEnvironmentName))
    }

    It "Ensure SDK Keys generated" {
        $sdkKeys = Invoke-ConfigCat "sdk-key"
        $sdkKeys | Should -Match ([regex]::Escape($newProductName))
        $sdkKeys | Should -Match ([regex]::Escape($newConfigName))
        $sdkKeys | Should -Match ([regex]::Escape($newEnvironmentName))
    }
}

Describe "Permission Group Tests" {
    It "Create / Update / Delete" {
        $groupName = "permgroup_$([guid]::NewGuid().ToString())"
        $permissionGroupId = Invoke-ConfigCat "permission-group", "create", "-p", $productId, "-n", $groupName, "--can-delete-config", "false", "--can-use-export-import", "false"
        Invoke-ConfigCat "permission-group", "ls", "-p", $productId | Should -Match ([regex]::Escape($groupName))
        $printed1 = Invoke-ConfigCat "permission-group", "show", "-i", $permissionGroupId
        $printed1 | Should -Match ([regex]::Escape("[*] Manage Members and Permission Groups"))
        $printed1 | Should -Match ([regex]::Escape("[*] Create, edit, and reorder Configs"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Delete Configs"))
        $printed1 | Should -Match ([regex]::Escape("[*] Create, edit and reorder Environments"))
        $printed1 | Should -Match ([regex]::Escape("[*] Delete Environments"))
        $printed1 | Should -Match ([regex]::Escape("[*] Create, rename, reorder Feature Flags and change their description"))
        $printed1 | Should -Match ([regex]::Escape("[*] Add, and remove Tags from Feature Flags"))
        $printed1 | Should -Match ([regex]::Escape("[*] Delete Feature Flags"))
        $printed1 | Should -Match ([regex]::Escape("[*] Create, rename Tags and change their color"))
        $printed1 | Should -Match ([regex]::Escape("[*] Delete Tags"))
        $printed1 | Should -Match ([regex]::Escape("[*] Create, update, and delete Webhooks"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Export (download), and import (upload) Configs, Environments, and Feature Flags"))
        $printed1 | Should -Match ([regex]::Escape("[*] Access, and change Product preferences"))
        $printed1 | Should -Match ([regex]::Escape("[*] Connect, and disconnect 3rd party integrations"))
        $printed1 | Should -Match ([regex]::Escape("[*] View the SDK key, and the code examples"))
        $printed1 | Should -Match ([regex]::Escape("[*] Add, and remove SDK keys"))
        $printed1 | Should -Match ([regex]::Escape("[*] View the config.json download statistics"))
        $printed1 | Should -Match ([regex]::Escape("[*] View the Product level Audit Log about who changed what in the Product"))
        $printed1 | Should -Match ([regex]::Escape("[*] Create, and edit Segments"))
        $printed1 | Should -Match ([regex]::Escape("[*] Delete Segments"))
        $printed1 | Should -Match "Read/Write access in all environments"
        $printed1 | Should -Match "Read/Write access in new environments"

        $newGroupName = "permgroup_$([guid]::NewGuid().ToString())"
        Invoke-ConfigCat "permission-group", "update", "-i", $permissionGroupId, "-n", $newGroupName
        Invoke-ConfigCat "permission-group", "ls", "-p", $productId | Should -Match ([regex]::Escape($newGroupName))

        Invoke-ConfigCat "permission-group", "update", "-i", $permissionGroupId, "--can-manage-members", "false", "--can-delete-tag", "false", "--can-delete-segments", "false", "--can-view-sdk-key", "false", "--can-rotate-sdk-key", "false"
        $printed2 = Invoke-ConfigCat "permission-group", "show", "-i", $permissionGroupId
        $printed2 | Should -Match ([regex]::Escape("[ ] Manage Members and Permission Groups"))
        $printed2 | Should -Match ([regex]::Escape("[*] Create, edit, and reorder Configs"))
        $printed2 | Should -Match ([regex]::Escape("[ ] Delete Configs"))
        $printed2 | Should -Match ([regex]::Escape("[*] Create, edit and reorder Environments"))
        $printed2 | Should -Match ([regex]::Escape("[*] Delete Environments"))
        $printed2 | Should -Match ([regex]::Escape("[*] Create, rename, reorder Feature Flags and change their description"))
        $printed2 | Should -Match ([regex]::Escape("[*] Add, and remove Tags from Feature Flags"))
        $printed2 | Should -Match ([regex]::Escape("[*] Delete Feature Flags"))
        $printed2 | Should -Match ([regex]::Escape("[*] Create, rename Tags and change their color"))
        $printed2 | Should -Match ([regex]::Escape("[ ] Delete Tags"))
        $printed2 | Should -Match ([regex]::Escape("[*] Create, update, and delete Webhooks"))
        $printed2 | Should -Match ([regex]::Escape("[ ] Export (download), and import (upload) Configs, Environments, and Feature Flags"))
        $printed2 | Should -Match ([regex]::Escape("[*] Access, and change Product preferences"))
        $printed2 | Should -Match ([regex]::Escape("[*] Connect, and disconnect 3rd party integrations"))
        $printed2 | Should -Match ([regex]::Escape("[ ] View the SDK key, and the code examples"))
        $printed2 | Should -Match ([regex]::Escape("[ ] Add, and remove SDK keys"))
        $printed2 | Should -Match ([regex]::Escape("[*] View the config.json download statistics"))
        $printed2 | Should -Match ([regex]::Escape("[*] View the Product level Audit Log about who changed what in the Product"))
        $printed2 | Should -Match ([regex]::Escape("[*] Create, and edit Segments"))
        $printed2 | Should -Match ([regex]::Escape("[ ] Delete Segments"))
        $printed2 | Should -Match "Read/Write access in all environments"
        $printed2 | Should -Match "Read/Write access in new environments"

        Invoke-ConfigCat "permission-group", "env", "-i", $permissionGroupId, "--access-type", "custom", "--new-environment-access-type", "readonly", "--environment-specific-access-types", "${environmentId}:none"
        $printed3 = Invoke-ConfigCat "permission-group", "show", "-i", $permissionGroupId
        $printed3 | Should -Match ([regex]::Escape("[ ] Manage Members and Permission Groups"))
        $printed3 | Should -Match ([regex]::Escape("[*] Create, edit, and reorder Configs"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Delete Configs"))
        $printed3 | Should -Match ([regex]::Escape("[*] Create, edit and reorder Environments"))
        $printed3 | Should -Match ([regex]::Escape("[*] Delete Environments"))
        $printed3 | Should -Match ([regex]::Escape("[*] Create, rename, reorder Feature Flags and change their description"))
        $printed3 | Should -Match ([regex]::Escape("[*] Add, and remove Tags from Feature Flags"))
        $printed3 | Should -Match ([regex]::Escape("[*] Delete Feature Flags"))
        $printed3 | Should -Match ([regex]::Escape("[*] Create, rename Tags and change their color"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Delete Tags"))
        $printed3 | Should -Match ([regex]::Escape("[*] Create, update, and delete Webhooks"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Export (download), and import (upload) Configs, Environments, and Feature Flags"))
        $printed3 | Should -Match ([regex]::Escape("[*] Access, and change Product preferences"))
        $printed3 | Should -Match ([regex]::Escape("[*] Connect, and disconnect 3rd party integrations"))
        $printed3 | Should -Match ([regex]::Escape("[ ] View the SDK key, and the code examples"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Add, and remove SDK keys"))
        $printed3 | Should -Match ([regex]::Escape("[*] View the config.json download statistics"))
        $printed3 | Should -Match ([regex]::Escape("[*] View the Product level Audit Log about who changed what in the Product"))
        $printed3 | Should -Match ([regex]::Escape("[*] Create, and edit Segments"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Delete Segments"))
        $printed3 | Should -Match "Environment specific access in all environments"
        $printed3 | Should -Match "Read-only access in new environments"
        $printed3 | Should -Match "No access in ${environmentName}"

        Invoke-ConfigCat "permission-group", "rm", "-i", $permissionGroupId
        Invoke-ConfigCat "permission-group", "ls", "-p", $productId | Should -Not -Match ([regex]::Escape($newGroupName))
    }

    It "Create with defaults" {
        $group2Name = "permgroup2_$([guid]::NewGuid().ToString())"
        $permissionGroup2Id = Invoke-ConfigCat "permission-group", "create", "-p", $productId, "-n", $group2Name, "--can-view-sdk-key", "--can-view-product-statistics", "--can-view-product-audit-log", "--default-when-not-set", "false"
        Invoke-ConfigCat "permission-group", "ls", "-p", $productId | Should -Match ([regex]::Escape($group2Name))
        $printed1 = Invoke-ConfigCat "permission-group", "show", "-i", $permissionGroup2Id
        $printed1 | Should -Match ([regex]::Escape("[ ] Manage Members and Permission Groups"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Create, edit, and reorder Configs"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Delete Configs"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Create, edit and reorder Environments"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Delete Environments"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Create, rename, reorder Feature Flags and change their description"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Add, and remove Tags from Feature Flags"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Delete Feature Flags"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Create, rename Tags and change their color"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Delete Tags"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Create, update, and delete Webhooks"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Export (download), and import (upload) Configs, Environments, and Feature Flags"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Access, and change Product preferences"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Connect, and disconnect 3rd party integrations"))
        $printed1 | Should -Match ([regex]::Escape("[*] View the SDK key, and the code examples"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Add, and remove SDK keys"))
        $printed1 | Should -Match ([regex]::Escape("[*] View the config.json download statistics"))
        $printed1 | Should -Match ([regex]::Escape("[*] View the Product level Audit Log about who changed what in the Product"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Create, and edit Segments"))
        $printed1 | Should -Match ([regex]::Escape("[ ] Delete Segments"))
        $printed1 | Should -Match "Read/Write access in all environments"
        $printed1 | Should -Match "Read/Write access in new environments"

        Invoke-ConfigCat "permission-group", "env", "-i", $permissionGroup2Id, "--access-type", "custom", "--new-environment-access-type", "readonly", "--default-access-type-when-not-set", "full"
        $printed3 = Invoke-ConfigCat "permission-group", "show", "-i", $permissionGroup2Id
        $printed3 | Should -Match ([regex]::Escape("[ ] Manage Members and Permission Groups"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Create, edit, and reorder Configs"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Delete Configs"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Create, edit and reorder Environments"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Delete Environments"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Create, rename, reorder Feature Flags and change their description"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Add, and remove Tags from Feature Flags"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Delete Feature Flags"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Create, rename Tags and change their color"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Delete Tags"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Create, update, and delete Webhooks"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Export (download), and import (upload) Configs, Environments, and Feature Flags"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Access, and change Product preferences"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Connect, and disconnect 3rd party integrations"))
        $printed3 | Should -Match ([regex]::Escape("[*] View the SDK key, and the code examples"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Add, and remove SDK keys"))
        $printed3 | Should -Match ([regex]::Escape("[*] View the config.json download statistics"))
        $printed3 | Should -Match ([regex]::Escape("[*] View the Product level Audit Log about who changed what in the Product"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Create, and edit Segments"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Delete Segments"))
        $printed3 | Should -Match "Environment specific access in all environments"
        $printed3 | Should -Match "Read-only access in new environments"
        $printed3 | Should -Match "Read/Write access in ${environmentName}"

        Invoke-ConfigCat "permission-group", "rm", "-i", $permissionGroup2Id
    }
}

Describe "Member tests" {
    It "Add / Remove permissions" {
        $userId = "d21346be-45c9-421f-b9ec-33093ef0464c"
        $tempGroupName = "permgroup_$([guid]::NewGuid().ToString())"
        $memberTestPermGroupId = Invoke-ConfigCat "permission-group", "create", "-p", $productId, "-n", $tempGroupName
        Invoke-ConfigCat "member", "add-permission", "-o", $organizationId, "-i", $userId, "--permission-group-ids", $memberTestPermGroupId
        Invoke-ConfigCat "member", "lsp", "-p", $productId | Should -Match ([regex]::Escape($tempGroupName))

        Invoke-ConfigCat "member", "rm-permission", "-o", $organizationId, "-i", $userId, "--permission-group-ids", $memberTestPermGroupId
        Invoke-ConfigCat "member", "lsp", "-p", $productId | Should -Not -Match ([regex]::Escape($tempGroupName))
    }
}

Describe "Webhook tests" {
    BeforeAll {
        $webhookId = Invoke-ConfigCat "webhook", "create", "-c", $configId, "-e", $environmentId, "-u", "https://example.com/hook", "-m", "get"
        $tableResult = Invoke-ConfigCat "webhook", "ls", "-p", $productId
        $tableResult | Should -Match ([regex]::Escape($webhookId))
        $tableResult | Should -Match ([regex]::Escape("https://example.com/hook"))
        $tableResult | Should -Match ([regex]::Escape("GET"))
    }

    AfterAll {
        Invoke-ConfigCat "webhook", "rm", "-i", $webhookId
    }

    It "Update webhook" {
        Invoke-ConfigCat "webhook", "up", "-i", $webhookId, "-u", "https://example.com/hook2", "-m", "post", "-co", "example body"
        $hookResult = Invoke-ConfigCat "webhook", "sh", "-i", $webhookId
        $hookResult | Should -Match ([regex]::Escape("https://example.com/hook2"))
        $hookResult | Should -Match ([regex]::Escape("POST"))
        $hookResult | Should -Match ([regex]::Escape("example body"))
    }

    It "Add header" {
        Invoke-ConfigCat "webhook", "headers", "add", "-i", $webhookId, "-k", "Header1", "-val", "header-val"
        $hookResult = Invoke-ConfigCat "webhook", "sh", "-i", $webhookId
        $hookResult | Should -Match ([regex]::Escape("Header1"))
        $hookResult | Should -Match ([regex]::Escape("header-val"))

        Invoke-ConfigCat "webhook", "headers", "rm", "-i", $webhookId, "-k", "Header1"
        $hookResult = Invoke-ConfigCat "webhook", "sh", "-i", $webhookId
        $hookResult | Should -Not -Match ([regex]::Escape("Header1"))
        $hookResult | Should -Not -Match ([regex]::Escape("header-val"))
    }

    It "Add secure header" {
        Invoke-ConfigCat "webhook", "headers", "add", "-i", $webhookId, "-k", "Header2", "-val", "secure-header-val", "--secure"
        $hookResult = Invoke-ConfigCat "webhook", "sh", "-i", $webhookId
        $hookResult | Should -Match ([regex]::Escape("Header2"))
        $hookResult | Should -Not -Match ([regex]::Escape("secure-header-val"))
        $hookResult | Should -Match ([regex]::Escape("<secure>"))
    }
}

Describe "Product preferences tests" {
    AfterAll {
        Invoke-ConfigCat "product", "preferences", "update", "env", "-i", $productId, "-ei", "${environmentId}:false"
    }
    
    It "Update preferences" {
        Invoke-ConfigCat "product", "preferences", "update", "-i", $productId, "-rr", "true", "-kg", "pascalCase", "-vi", "true"
        $prefResult = Invoke-ConfigCat "product", "preferences", "sh", "-i", $productId
        $prefResult | Should -Match ([regex]::Escape("Reason required: True"))
        $prefResult | Should -Match ([regex]::Escape("Key generation mode: pascalCase"))
        $prefResult | Should -Match ([regex]::Escape("Show variation ID: True"))

        Invoke-ConfigCat "product", "preferences", "update", "-i", $productId, "-rr", "false"
        Invoke-ConfigCat "product", "preferences", "update", "env", "-i", $productId, "-ei", "${environmentId}:true"
        $prefResult = Invoke-ConfigCat "product", "preferences", "sh", "-i", $productId
        $prefResult | Should -Match ([regex]::Escape("Reason required: False"))
        $prefResult | Should -Match ([regex]::Escape("$newEnvironmentName  True"))
    }
}

Describe "Tag / Flag Tests" {
    BeforeAll {
        $tag1Id = Invoke-ConfigCat "tag", "create", "-p", $productId, "-n", "tag1", "-c", "panther"
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Match ([regex]::Escape("tag1"))

        $tag2Id = Invoke-ConfigCat "tag", "create", "-p", $productId, "-n", "tag2", "-c", "whale"
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Match ([regex]::Escape("tag2"))

        $flagId = Invoke-ConfigCat "flag", "create", "-c", $configId, "-n", "Bool-Flag", "-k", "bool_flag", "-H", "hint", "-t", "boolean", "-g", $tag1Id
        $flagResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $flagResult | Should -Match ([regex]::Escape("Bool-Flag"))
        $flagResult | Should -Match ([regex]::Escape("bool_flag"))
        $flagResult | Should -Match ([regex]::Escape("hint"))
        $flagResult | Should -Match ([regex]::Escape("boolean"))
        $flagResult | Should -Match ([regex]::Escape($tag1Id))
    }

    AfterAll {
        Invoke-ConfigCat "tag", "rm", "-i", $tag1Id
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Not -Match ([regex]::Escape($tag1Id))

        Invoke-ConfigCat "tag", "rm", "-i", $tag2Id
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Not -Match ([regex]::Escape($tag2Id))

        Invoke-ConfigCat "flag", "rm", "-i", $flagId
        Invoke-ConfigCat "flag", "ls", "-c", $configId | Should -Not -Match ([regex]::Escape($flagId))
    }

    It "Create with initial values" {
        $flagWithInitId = Invoke-ConfigCat "flag", "create", "-c", $configId, "-n", "Bool-With-Init-Flag", "-k", "bool_with_init_flag", "-H", "hint", "-t", "boolean", "-iv", "true"
        $showWithInitResult = Invoke-ConfigCat "flag", "value", "show", "-i", $flagWithInitId
        $showWithInitResult | Should -Match ([regex]::Escape("Default: True"))
    }

    It "Create with initial values" {
        $flagWithEnvInitId = Invoke-ConfigCat "flag", "create", "-c", $configId, "-n", "Bool-With-Env-Init-Flag", "-k", "bool_with_env_init_flag", "-H", "hint", "-t", "boolean", "-ive", "${environmentId}:true"
        $showWithEnvInitResult = Invoke-ConfigCat "flag", "value", "show", "-i", $flagWithEnvInitId
        $showWithEnvInitResult | Should -Match ([regex]::Escape("Default: True"))
    }

    It "Update Tag" {
        $newTagName = "newTag1"
        $newTagColor = "salmon"
        Invoke-ConfigCat "tag", "update", "-i", $tag1Id, "-n", $newTagName, "-c", $newTagColor
        $tagResult = Invoke-ConfigCat "tag", "ls", "-p", $productId
        $tagResult | Should -Match ([regex]::Escape($newTagName))
        $tagResult | Should -Match ([regex]::Escape($newTagColor))
    }

    It "Update Flag" {
        Invoke-ConfigCat "flag", "update", "-i", $flagId, "-n", "Bool-Flag-Updated", "-H", "hint-updated", "-g", $tag2Id
        $updateResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $updateResult | Should -Match ([regex]::Escape("Bool-Flag-Updated"))
        $updateResult | Should -Match ([regex]::Escape("bool_flag"))
        $updateResult | Should -Match ([regex]::Escape("hint-updated"))
        $updateResult | Should -Match ([regex]::Escape("boolean"))
        $updateResult | Should -Match ([regex]::Escape($tag2Id))
        $updateResult | Should -Not -Match ([regex]::Escape($tag1Id))
    }

    It "Attach Tag" {
        Invoke-ConfigCat "flag", "attach", "-i", $flagId, "-g", $tag1Id
        $attachResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $attachResult | Should -Match ([regex]::Escape($tag2Id))
        $attachResult | Should -Match ([regex]::Escape($tag1Id))
    }

    It "Detach Tag" {
        Invoke-ConfigCat "flag", "detach", "-i", $flagId, "-g", $tag1Id, $tag2Id
        $attachResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $attachResult | Should -Not -Match ([regex]::Escape($tag2Id))
        $attachResult | Should -Not -Match ([regex]::Escape($tag1Id))
    }
}

Describe "Flag value / Rule Tests" {
    BeforeAll {
        $flagId = Invoke-ConfigCat "flag", "create", "-c", $configId, "-n", "Bool-Flag", "-k", "bool_flag", "-H", "hint", "-t", "boolean"
    }

    AfterAll {
        Invoke-ConfigCat "flag", "rm", "-i", $flagId
    }


    It "Update Value" {
        Invoke-ConfigCat "flag", "value", "update", "-i", $flagId, "-e", $environmentId, "-f", "true"
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match ([regex]::Escape("Default: True"))
    }

    It "Create targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "create", "-i", $flagId, "-e", $environmentId, "-a", "ID", "-c", "isoneof", "-t", "SAMPLEID,SOMEID", "-f", "true"
        Invoke-ConfigCat "flag", "targeting", "create", "-i", $flagId, "-e", $environmentId, "-a", "EMAIL", "-c", "contains", "-t", "example.com", "-f", "true"
        Invoke-ConfigCat "flag", "targeting", "create", "-i", $flagId, "-e", $environmentId, "-a", "VERSION", "-c", "isNotOneOf", "-t", "1.2.6,1.2.8", "-f", "true"
        Invoke-ConfigCat "flag", "targeting", "create", "-i", $flagId, "-e", $environmentId, "-si", $segmentId, "-sc", "isNotIn", "-f", "true"
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match ([regex]::Escape("1. When ID IS ONE OF SAMPLEID,SOMEID then True"))
        $result | Should -Match ([regex]::Escape("2. When EMAIL CONTAINS example.com then True"))
        $result | Should -Match ([regex]::Escape("3. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"))
        $result | Should -Match ([regex]::Escape("4. When IS NOT IN SEGMENT $segmentName then True"))
    }

    It "Update targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "update", "-i", $flagId, "-e", $environmentId, "-p", 2, "-a", "EMAIL", "-c", "doesnotcontain", "-t", "sample.com", "-f", "false"
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match ([regex]::Escape("1. When ID IS ONE OF SAMPLEID,SOMEID then True"))
        $result | Should -Match ([regex]::Escape("2. When EMAIL DOES NOT CONTAIN sample.com then False"))
        $result | Should -Match ([regex]::Escape("3. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"))
        $result | Should -Match ([regex]::Escape("4. When IS NOT IN SEGMENT $segmentName then True"))
    }

    It "Update segment rule" {
        Invoke-ConfigCat "flag", "targeting", "update", "-i", $flagId, "-e", $environmentId, "-p", 4, "-si", $segmentId, "-sc", "isIn", "-f", "false"
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match ([regex]::Escape("1. When ID IS ONE OF SAMPLEID,SOMEID then True"))
        $result | Should -Match ([regex]::Escape("2. When EMAIL DOES NOT CONTAIN sample.com then False"))
        $result | Should -Match ([regex]::Escape("3. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"))
        $result | Should -Match ([regex]::Escape("4. When IS IN SEGMENT $segmentName then False"))
    }

    It "Move targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "move", "-i", $flagId, "-e", $environmentId, "--from", 3, "--to", 1 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match ([regex]::Escape("1. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"))
        $result | Should -Match ([regex]::Escape("2. When ID IS ONE OF SAMPLEID,SOMEID then True"))
        $result | Should -Match ([regex]::Escape("3. When EMAIL DOES NOT CONTAIN sample.com then False"))
        $result | Should -Match ([regex]::Escape("4. When IS IN SEGMENT $segmentName then False"))
    }

    It "Move segment rule" {
        Invoke-ConfigCat "flag", "targeting", "move", "-i", $flagId, "-e", $environmentId, "--from", 4, "--to", 2 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match ([regex]::Escape("1. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"))
        $result | Should -Match ([regex]::Escape("2. When IS IN SEGMENT $segmentName then False"))
        $result | Should -Match ([regex]::Escape("3. When ID IS ONE OF SAMPLEID,SOMEID then True"))
        $result | Should -Match ([regex]::Escape("4. When EMAIL DOES NOT CONTAIN sample.com then False"))
    }

    It "Delete targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "rm", "-i", $flagId, "-e", $environmentId, "-p", 2 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match ([regex]::Escape("1. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"))
        $result | Should -Match ([regex]::Escape("2. When ID IS ONE OF SAMPLEID,SOMEID then True"))
        $result | Should -Match ([regex]::Escape("3. When EMAIL DOES NOT CONTAIN sample.com then False"))
    }

    It "Add percentage rules" {
        Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "30:true", "70:false" 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match ([regex]::Escape("30% -> True"))
        $result | Should -Match ([regex]::Escape("70% -> False"))
    }

    It "Update percentage rules" {
        Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "60:true", "40:false" 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match ([regex]::Escape("60% -> True"))
        $result | Should -Match ([regex]::Escape("40% -> False"))
    }

    It "Clear percentage rules" {
        Invoke-ConfigCat "flag", "percentage", "clear", "-i", $flagId, "-e", $environmentId
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Not -Match ([regex]::Escape("60% -> True"))
        $result | Should -Not -Match ([regex]::Escape("40% -> False"))
    }

    It "Percentage must be integer" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "text:true", "70:false" 
        $result | Should -Match ([regex]::Escape("is not a number"))
    }

    It "Percentage sum must be 100" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "50:true", "70:false" 
        $result | Should -Match ([regex]::Escape("must be 100"))
    }

    It "Percentage can't be negative" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "-100:true", "200:false" 
        $result | Should -Match ([regex]::Escape("must be a non-negative number"))
    }

    It "Bool can have 2 rules only" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "20:true", "30:false", "50:false" 
        $result | Should -Match ([regex]::Escape("only have 2 percentage rules"))
    }
}

Describe "Flag value / Rule Tests V2" {
    BeforeAll {
        $configV2Name = "CLI-IntegTest-Config-V2"
        $configV2Id = Invoke-ConfigCat "config", "create", "-p", $productId, "-n", $configV2Name, "-e", "v2"
        Invoke-ConfigCat "config", "ls", "-p", $productId | Should -Match ([regex]::Escape($configV2Name))
    }
    
    AfterAll {
        Invoke-ConfigCat "config", "rm", "-i", $configV2Id
        Invoke-ConfigCat "config", "ls", "-p", $productId | Should -Not -Match ([regex]::Escape($configV2Id))
    }
    
    BeforeEach {
        $flagV2Id = Invoke-ConfigCat "flag-v2", "create", "-c", $configV2Id, "-n", "Bool-Flag", "-k", "bool_flag", "-H", "hint", "-t", "boolean"
        $flagV2Id2 = Invoke-ConfigCat "flag-v2", "create", "-c", $configV2Id, "-n", "Bool-Flag", "-k", "bool_flag2", "-H", "hint", "-t", "boolean"
    }

    AfterEach {
        Invoke-ConfigCat "flag-v2", "rm", "-i", $flagV2Id
        Invoke-ConfigCat "flag-v2", "rm", "-i", $flagV2Id2
    }

    It "Update Value" {
        Invoke-ConfigCat "flag-v2", "value", "update", "-i", $flagV2Id, "-e", $environmentId, "-f", "true"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("Default: True"))
    }

    It "Create user targeting rule" {
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "u", "-i", $flagV2Id, "-e", $environmentId, "-a", "ID", "-c", "isOneOf", "-cv", "id1:user1", "id2:user2", "-sv", "true"
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "u", "-i", $flagV2Id, "-e", $environmentId, "-a", "EMAIL", "-c", "textEquals", "-cv", "example.com", "-sv", "true"
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "u", "-i", $flagV2Id, "-e", $environmentId, "-a", "VERSION", "-c", "isNotOneOf", "-cv", "1.2.6:", "1.2.8:", "-sv", "true"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
        $result | Should -Match ([regex]::Escape("2. If EMAIL EQUALS example.com"))
        $result | Should -Match ([regex]::Escape("3. If VERSION IS NOT ONE OF [2 items]"))
    }
    
    It "Create segment targeting rule" {
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "sg", "-i", $flagV2Id, "-e", $environmentId, "-si", $segmentId, "-c", "isNotIn", "-sv", "true"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If IS NOT IN SEGMENT CLI-IntegTest-Segment"))
        $result | Should -Match ([regex]::Escape("Then: True"))
    }
    
    It "Create prerequisite targeting rule" {
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "pr", "-i", $flagV2Id, "-e", $environmentId, "-c", "equals", "-pi", $flagV2Id2, "-pv", "true", "-sv", "true"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If bool_flag2 EQUALS True"))
        $result | Should -Match ([regex]::Escape("Then: True"))
    }

    It "Remove targeting rule" {
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "u", "-i", $flagV2Id, "-e", $environmentId, "-a", "ID", "-c", "isOneOf", "-cv", "id1:user1", "id2:user2", "-sv", "true"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "rm", "-i", $flagV2Id, "-e", $environmentId, "-rp", "1", "-v"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Not -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
    }
    
    It "Move targeting rule" {
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "u", "-i", $flagV2Id, "-e", $environmentId, "-a", "ID", "-c", "isOneOf", "-cv", "id1:user1", "id2:user2", "-sv", "true"
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "u", "-i", $flagV2Id, "-e", $environmentId, "-a", "EMAIL", "-c", "textEquals", "-cv", "example.com", "-sv", "true"
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "u", "-i", $flagV2Id, "-e", $environmentId, "-a", "VERSION", "-c", "isNotOneOf", "-cv", "1.2.6:", "1.2.8:", "-sv", "true"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
        $result | Should -Match ([regex]::Escape("2. If EMAIL EQUALS example.com"))
        $result | Should -Match ([regex]::Escape("3. If VERSION IS NOT ONE OF [2 items]"))
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "mv", "-i", $flagV2Id, "-e", $environmentId, "--from", "1", "--to", "3"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If EMAIL EQUALS example.com"))
        $result | Should -Match ([regex]::Escape("2. If VERSION IS NOT ONE OF [2 items]"))
        $result | Should -Match ([regex]::Escape("3. If ID IS ONE OF [2 items]"))
    }

    It "Update targeting rule's served value" {
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "u", "-i", $flagV2Id, "-e", $environmentId, "-a", "ID", "-c", "isOneOf", "-cv", "id1:user1", "id2:user2", "-sv", "true"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
        $result | Should -Match ([regex]::Escape("Then: True"))
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "usv", "-i", $flagV2Id, "-e", $environmentId, "-rp", "1", "-po", "30:true", "70:false"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
        $result | Should -Match ([regex]::Escape("30% -> True"))
        $result | Should -Match ([regex]::Escape("70% -> False"))
    }

    It "Add/remove conditions" {
        Invoke-ConfigCat "flag-v2", "targeting", "rule", "cr", "u", "-i", $flagV2Id, "-e", $environmentId, "-a", "ID", "-c", "isOneOf", "-cv", "id1:user1", "id2:user2", "-sv", "true"
        Invoke-ConfigCat "flag-v2", "targeting", "c", "a", "u", "-i", $flagV2Id, "-e", $environmentId, "-rp", "1", "-a", "EMAIL", "-c", "textEquals", "-cv", "test@example.com", "-sv", "true"
        Invoke-ConfigCat "flag-v2", "targeting", "c", "a", "sg", "-i", $flagV2Id, "-e", $environmentId, "-rp", "1", "-si", $segmentId, "-c", "isIn"
        Invoke-ConfigCat "flag-v2", "targeting", "c", "a", "pr", "-i", $flagV2Id, "-e", $environmentId, "-rp", "1", "-c", "equals", "-pi", $flagV2Id2, "-pv", "true"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
        $result | Should -Match ([regex]::Escape("&& EMAIL EQUALS test@example.com"))
        $result | Should -Match ([regex]::Escape("&& IS IN SEGMENT $segmentName"))
        $result | Should -Match ([regex]::Escape("&& bool_flag2 EQUALS True"))
        Invoke-ConfigCat "flag-v2", "targeting", "c", "rm", "-i", $flagV2Id, "-e", $environmentId, "-rp", "1", "-cp", "4"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
        $result | Should -Match ([regex]::Escape("&& EMAIL EQUALS test@example.com"))
        $result | Should -Match ([regex]::Escape("&& IS IN SEGMENT $segmentName"))
        $result | Should -Not -Match ([regex]::Escape("&& bool_flag2 EQUALS True"))
        Invoke-ConfigCat "flag-v2", "targeting", "c", "rm", "-i", $flagV2Id, "-e", $environmentId, "-rp", "1", "-cp", "3"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
        $result | Should -Match ([regex]::Escape("&& EMAIL EQUALS test@example.com"))
        $result | Should -Not -Match ([regex]::Escape("&& IS IN SEGMENT $segmentName"))
        Invoke-ConfigCat "flag-v2", "targeting", "c", "rm", "-i", $flagV2Id, "-e", $environmentId, "-rp", "1", "-cp", "2"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. If ID IS ONE OF [2 items]"))
        $result | Should -Not -Match ([regex]::Escape("&& EMAIL EQUALS test@example.com"))
    }

    It "Update percentage options" {
        Invoke-ConfigCat "flag-v2", "targeting", "%", "up", "-i", $flagV2Id, "-e", $environmentId, "-po", "40:true", "60:false"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("40% -> True"))
        $result | Should -Match ([regex]::Escape("60% -> False"))
        Invoke-ConfigCat "flag-v2", "targeting", "%", "clr", "-i", $flagV2Id, "-e", $environmentId
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Not -Match ([regex]::Escape("40% -> True"))
        $result | Should -Not -Match ([regex]::Escape("60% -> False"))
    }

    It "Update percentage attribute" {
        Invoke-ConfigCat "flag-v2", "targeting", "%", "at", "-i", $flagV2Id, "-e", $environmentId, "-n", "Custom1"
        Invoke-ConfigCat "flag-v2", "targeting", "%", "up", "-i", $flagV2Id, "-e", $environmentId, "-po", "40:true", "60:false"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("40% of Custom1 attribute -> True"))
        $result | Should -Match ([regex]::Escape("60% of Custom1 attribute -> False"))
        Invoke-ConfigCat "flag-v2", "targeting", "c", "a", "sg", "-i", $flagV2Id, "-e", $environmentId, "-rp", "1", "-si", $segmentId, "-c", "isIn"
        $result = Invoke-ConfigCat "flag-v2", "value", "show", "-i", $flagV2Id
        $result | Should -Match ([regex]::Escape("1. IF IS IN SEGMENT $segmentName"))
        $result | Should -Match ([regex]::Escape("40% of Custom1 attribute -> True"))
        $result | Should -Match ([regex]::Escape("60% of Custom1 attribute -> False"))
    }
}

Describe "Scan Tests" {
    BeforeAll {
        $flagIdToScan1 = Invoke-ConfigCat "flag", "create", "-c", $configId, "-n", "Flag-To-Scan", "-k", "flag_to_scan", "-H", "hint", "-t", "boolean"
        $flagIdToScan2 = Invoke-ConfigCat "flag", "create", "-c", $configId, "-n", "Flag-To-Scan-2", "-k", "flag_to_scan_2", "-H", "hint", "-t", "boolean"
    }

    AfterAll {
        Invoke-ConfigCat "flag", "rm", "-i", $flagIdToScan1
        Invoke-ConfigCat "flag", "rm", "-i", $flagIdToScan2
    }

    It "Scan" {
        $result = Invoke-ConfigCat "scan", $scanPath, "-c", $configId, "-r", "cli", "--upload", "--print"
        $result | Should -Match ([regex]::Escape("Repository: cli"))
        $result | Should -Match ([regex]::Escape("Uploading code references... Ok."))
        $result | Should -Match ([regex]::Escape("'flag_to_scan'"))
        $result | Should -Match ([regex]::Escape("'flag_to_scan_2'"))
        $result | Should -Match ([regex]::Escape("deleted feature flag/setting reference(s) found in"))
    }

    It "Scan exclude" {
        $result = Invoke-ConfigCat "scan", $scanPath, "-c", $configId, "-r", "cli", "-ex", "flag_to_scan", "flag_to_scan_2"
        $result | Should -Not -Match ([regex]::Escape("'flag_to_scan'"))
        $result | Should -Not -Match ([regex]::Escape("'flag_to_scan_2'"))
        $result | Should -Match ([regex]::Escape("deleted feature flag/setting reference(s) found in"))
    }

    It "Scan exclude comma" {
        $result = Invoke-ConfigCat "scan", $scanPath, "-c", $configId, "-r", "cli", "-ex", "flag_to_scan, flag_to_scan_2"
        $result | Should -Not -Match ([regex]::Escape("'flag_to_scan'"))
        $result | Should -Not -Match ([regex]::Escape("'flag_to_scan_2'"))
        $result | Should -Match ([regex]::Escape("deleted feature flag/setting reference(s) found in"))
    }
    
    It "Scan custom pattern" {
        $result = Invoke-ConfigCat "scan", $scanPath, "-c", $configId, "-r", "cli", "-ap", "(\w+) = flags!(CC_KEY)", "--print"
        $result | Should -Match ([regex]::Escape("custom_alias"))
    }
}
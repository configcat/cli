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
    $productName = [guid]::NewGuid().ToString()
    $productId = Invoke-ConfigCat "product", "create", "-o", $organizationId, "-n", $productName
    Invoke-ConfigCat "product", "ls" | Should -Match $productName

    $configName = "CLI-IntegTest-Config"
    $configId = Invoke-ConfigCat "config", "create", "-p", $productId, "-n", $configName
    Invoke-ConfigCat "config", "ls" | Should -Match $configName

    $environmentName = "CLI-IntegTest-Env"
    $environmentId = Invoke-ConfigCat "environment", "create", "-p", $productId, "-n", $environmentName
    Invoke-ConfigCat "environment", "ls" | Should -Match $environmentName

    $segmentName = "CLI-IntegTest-Segment"
    $segmentId = Invoke-ConfigCat "segment", "create", "-p", $productId, "-n", $segmentName, "-a", "Identifier", "-c", "doesnotcontain", "-t", "sample.com"
    Invoke-ConfigCat "segment", "ls" | Should -Match $segmentName
    $details = Invoke-ConfigCat "segment", "sh", "-i", $segmentId
    $details | Should -Match "when Identifier DOES NOT CONTAIN sample.com"
}

AfterAll {
    Invoke-ConfigCat "environment", "rm", "-i", $environmentId
    Invoke-ConfigCat "environment", "ls" | Should -Not -Match $environmentId

    Invoke-ConfigCat "config", "rm", "-i", $configId
    Invoke-ConfigCat "config", "ls" | Should -Not -Match $configId

    Invoke-ConfigCat "product", "rm", "-i", $productId
    Invoke-ConfigCat "product", "ls" | Should -Not -Match $productId

    Invoke-ConfigCat "segment", "rm", "-i", $segmentId
    Invoke-ConfigCat "segment", "ls" | Should -Not -Match $segmentId
}

Describe "Setup Tests" {
    It "Setup" {
       Invoke-ConfigCat "setup", "-H", $Env:CONFIGCAT_API_HOST, "-u", $Env:CONFIGCAT_API_USER, "-p", $Env:CONFIGCAT_API_PASS | Should -Match "Setup complete."
    }
}

Describe "Product / Config / Environment Tests" {
    It "Rename Product" {
        $newProductName = [guid]::NewGuid().ToString()
        Invoke-ConfigCat "product", "update", "-i", $productId, "-n", $newProductName
        Invoke-ConfigCat "product", "ls" | Should -Match $newProductName
    }

    It "Rename Config" {
        $newConfigName = "CLI-IntegTest-Config-Updated"
        Invoke-ConfigCat "config", "update", "-i", $configId, "-n", $newConfigName
        Invoke-ConfigCat "config", "ls" | Should -Match $newConfigName
    }

    It "Rename Environment" {
        $newEnvironmentName = "CLI-IntegTest-Env-Updated"
        Invoke-ConfigCat "environment", "update", "-i", $environmentId, "-n", $newEnvironmentName
        Invoke-ConfigCat "environment", "ls" | Should -Match $newEnvironmentName
    }

    It "Ensure SDK Keys generated" {
        $sdkKeys = Invoke-ConfigCat "sdk-key"
        $sdkKeys | Should -Match $newProductName
        $sdkKeys | Should -Match $newConfigName
        $sdkKeys | Should -Match $newEnvironmentName
    }
}

Describe "Permission Group Tests" {
    It "Create / Update / Delete" {
        $groupName = [guid]::NewGuid().ToString()
        $permissionGroupId = Invoke-ConfigCat "permission-group", "create", "-p", $productId, "-n", $groupName, "--can-delete-config", "false", "--can-use-export-import", "false"
        Invoke-ConfigCat "permission-group", "ls" | Should -Match $groupName
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

        $newGroupName = [guid]::NewGuid().ToString()
        Invoke-ConfigCat "permission-group", "update", "-i", $permissionGroupId, "-n", $newGroupName
        Invoke-ConfigCat "permission-group", "ls" | Should -Match $newGroupName

        Invoke-ConfigCat "permission-group", "update", "-i", $permissionGroupId, "--can-manage-members", "false", "--can-delete-tag", "false", "--can-delete-segments", "false", "--can-view-sdk-key", "false"
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
        $printed2 | Should -Match ([regex]::Escape("[*] Add, and remove SDK keys"))
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
        $printed3 | Should -Match ([regex]::Escape("[*] Add, and remove SDK keys"))
        $printed3 | Should -Match ([regex]::Escape("[*] View the config.json download statistics"))
        $printed3 | Should -Match ([regex]::Escape("[*] View the Product level Audit Log about who changed what in the Product"))
        $printed3 | Should -Match ([regex]::Escape("[*] Create, and edit Segments"))
        $printed3 | Should -Match ([regex]::Escape("[ ] Delete Segments"))
        $printed3 | Should -Match "Environment specific access in all environments"
        $printed3 | Should -Match "Read-only access in new environments"
        $printed3 | Should -Match "No access in ${environmentName}"

        Invoke-ConfigCat "permission-group", "rm", "-i", $permissionGroupId
        Invoke-ConfigCat "permission-group", "ls" | Should -Not -Match $newGroupName
    }
}

Describe "Member tests" {
    It "Add / Remove permissions" {
        $userId = "d21346be-45c9-421f-b9ec-33093ef0464c"
        Invoke-ConfigCat "member", "lsp", "-p", $productId | Should -Not -Match $userId

        $tempGroupName = [guid]::NewGuid().ToString()
        $permissionGroupId = Invoke-ConfigCat "permission-group", "create", "-p", $productId, "-n", $tempGroupName
        Invoke-ConfigCat "member", "add-permission", "-o", $organizationId, "-i", $userId, "--permission-group-ids", $permissionGroupId
        Invoke-ConfigCat "member", "lsp", "-p", $productId | Should -Match $userId

        Invoke-ConfigCat "member", "rm-permission", "-o", $organizationId, "-i", $userId, "--permission-group-ids", $permissionGroupId
        Invoke-ConfigCat "member", "lsp", "-p", $productId | Should -Not -Match $userId
    }
}

Describe "Tag / Flag Tests" {
    BeforeAll {
        $tag1Id = Invoke-ConfigCat "tag", "create", "-p", $productId, "-n", "tag1", "-c", "panther"
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Match "tag1"
        
        $tag2Id = Invoke-ConfigCat "tag", "create", "-p", $productId, "-n", "tag2", "-c", "whale"
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Match "tag2"    

        $flagId = Invoke-ConfigCat "flag", "create", "-c", $configId, "-n", "Bool-Flag", "-k", "bool_flag", "-H", "hint", "-t", "boolean", "-g", $tag1Id
        $flagResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $flagResult | Should -Match "Bool-Flag"
        $flagResult | Should -Match "bool_flag"
        $flagResult | Should -Match "hint"
        $flagResult | Should -Match "boolean"
        $flagResult | Should -Match $tag1Id
    }

    AfterAll {
        Invoke-ConfigCat "tag", "rm", "-i", $tag1Id
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Not -Match $tag1Id

        Invoke-ConfigCat "tag", "rm", "-i", $tag2Id
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Not -Match $tag2Id

        Invoke-ConfigCat "flag", "rm", "-i", $flagId
        Invoke-ConfigCat "flag", "ls", "-c", $configId | Should -Not -Match $flagId
    }


    It "Update Tag" {
        $newTagName = "newTag1"
        $newTagColor = "salmon"
        Invoke-ConfigCat "tag", "update", "-i", $tag1Id, "-n", $newTagName, "-c", $newTagColor
        $tagResult = Invoke-ConfigCat "tag", "ls", "-p", $productId
        $tagResult | Should -Match $newTagName
        $tagResult | Should -Match $newTagColor
    }
    
    It "Update Flag" {
        Invoke-ConfigCat "flag", "update", "-i", $flagId, "-n", "Bool-Flag-Updated", "-H", "hint-updated", "-g", $tag2Id
        $updateResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $updateResult | Should -Match "Bool-Flag-Updated"
        $updateResult | Should -Match "bool_flag"
        $updateResult | Should -Match "hint-updated"
        $updateResult | Should -Match "boolean"
        $updateResult | Should -Match $tag2Id        
        $updateResult | Should -Not -Match $tag1Id
    }

    It "Attach Tag" {
        Invoke-ConfigCat "flag", "attach", "-i", $flagId, "-g", $tag1Id
        $attachResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $attachResult | Should -Match $tag2Id
        $attachResult | Should -Match $tag1Id
    }

    It "Detach Tag" {
        Invoke-ConfigCat "flag", "detach", "-i", $flagId, "-g", $tag1Id, $tag2Id
        $attachResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $attachResult | Should -Not -Match $tag2Id
        $attachResult | Should -Not -Match $tag1Id
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
        $result | Should -Match "Default: True"
    }

    It "Create targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "create", "-i", $flagId, "-e", $environmentId, "-a", "ID", "-c", "isoneof", "-t", "SAMPLEID,SOMEID", "-f", "true"
        Invoke-ConfigCat "flag", "targeting", "create", "-i", $flagId, "-e", $environmentId, "-a", "EMAIL", "-c", "contains", "-t", "example.com", "-f", "true"
        Invoke-ConfigCat "flag", "targeting", "create", "-i", $flagId, "-e", $environmentId, "-a", "VERSION", "-c", "isNotOneOf", "-t", "1.2.6,1.2.8", "-f", "true"
        Invoke-ConfigCat "flag", "targeting", "create", "-i", $flagId, "-e", $environmentId, "-si", $segmentId, "-sc", "isNotIn", "-f", "true"
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match "1. When ID IS ONE OF SAMPLEID,SOMEID then True"
        $result | Should -Match "2. When EMAIL CONTAINS example.com then True"
        $result | Should -Match "3. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
        $result | Should -Match "4. When IS NOT IN SEGMENT $segmentName then True"
    }

    It "Update targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "update", "-i", $flagId, "-e", $environmentId, "-p", 2, "-a", "EMAIL", "-c", "doesnotcontain", "-t", "sample.com", "-f", "false"
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match "1. When ID IS ONE OF SAMPLEID,SOMEID then True"
        $result | Should -Match "2. When EMAIL DOES NOT CONTAIN sample.com then False"
        $result | Should -Match "3. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
        $result | Should -Match "4. When IS NOT IN SEGMENT $segmentName then True"
    }

    It "Update segment rule" {
        Invoke-ConfigCat "flag", "targeting", "update", "-i", $flagId, "-e", $environmentId, "-p", 4, "-si", $segmentId, "-sc", "isIn", "-f", "false"
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match "1. When ID IS ONE OF SAMPLEID,SOMEID then True"
        $result | Should -Match "2. When EMAIL DOES NOT CONTAIN sample.com then False"
        $result | Should -Match "3. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
        $result | Should -Match "4. When IS IN SEGMENT $segmentName then False"
    }

    It "Move targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "move", "-i", $flagId, "-e", $environmentId, "--from", 3, "--to", 1 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match "1. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
        $result | Should -Match "2. When ID IS ONE OF SAMPLEID,SOMEID then True"
        $result | Should -Match "3. When EMAIL DOES NOT CONTAIN sample.com then False"
        $result | Should -Match "4. When IS IN SEGMENT $segmentName then False"
    }

    It "Move segment rule" {
        Invoke-ConfigCat "flag", "targeting", "move", "-i", $flagId, "-e", $environmentId, "--from", 4, "--to", 2 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match "1. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
        $result | Should -Match "2. When IS IN SEGMENT $segmentName then False"
        $result | Should -Match "3. When ID IS ONE OF SAMPLEID,SOMEID then True"
        $result | Should -Match "4. When EMAIL DOES NOT CONTAIN sample.com then False"
    }

    It "Delete targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "rm", "-i", $flagId, "-e", $environmentId, "-p", 2 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match "1. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
        $result | Should -Match "2. When ID IS ONE OF SAMPLEID,SOMEID then True"
        $result | Should -Match "3. When EMAIL DOES NOT CONTAIN sample.com then False"
    }

    It "Add percentage rules" {
        Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "30:true", "70:false" 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match "30% -> True"
        $result | Should -Match "70% -> False"
    }

    It "Update percentage rules" {
        Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "60:true", "40:false" 
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Match "60% -> True"
        $result | Should -Match "40% -> False"
    }

    It "Clear percentage rules" {
        Invoke-ConfigCat "flag", "percentage", "clear", "-i", $flagId, "-e", $environmentId
        $result = Invoke-ConfigCat "flag", "value", "show", "-i", $flagId
        $result | Should -Not -Match "60% -> True"
        $result | Should -Not -Match "40% -> False"
    }

    It "Percentage must be integer" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "text:true", "70:false" 
        $result | Should -Match "is not a number"
    }

    It "Percentage sum must be 100" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "50:true", "70:false" 
        $result | Should -Match "must be 100"
    }

    It "Percentage can't be negative" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "-100:true", "200:false" 
        $result | Should -Match "must be a non-negative number"
    }

    It "Bool can have 2 rules only" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", "-i", $flagId, "-e", $environmentId, "20:true", "30:false", "50:false" 
        $result | Should -Match "only have 2 percentage rules"
    }
}

Describe "Scan Tests" {
    BeforeAll {
        $flagIdToScan = Invoke-ConfigCat "flag", "create", "-c", $configId, "-n", "Flag-To-Scan", "-k", "flag_to_scan", "-H", "hint", "-t", "boolean"
    }

    AfterAll {
        Invoke-ConfigCat "flag", "rm", "-i", $flagIdToScan
    }

    It "Scan" {
        $result = Invoke-ConfigCat "scan", $scanPath, "-c", $configId, "-r", "cli", "--upload", "--print"
        $result | Should -Match "Repository: cli"
        $result | Should -Match "Uploading code references... Ok."
        $result | Should -Match "'flag_to_scan'"
        $result | Should -Match "'bool_flag'"
    }
}
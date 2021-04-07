function Invoke-ConfigCat {
	param(
		[Parameter(Mandatory)]
		[string[]]$invokeArgs
	)

	return (./configcat $invokeArgs 2>&1) -join "`r`n"
}

BeforeAll {
    $organizationId = "08d8f29c-65fc-40d7-852e-4605da10d03c"
    $productName = [guid]::NewGuid().ToString()
    $productId = Invoke-ConfigCat "product", "create", $organizationId, "-n", $productName
    Invoke-ConfigCat "product", "ls" | Should -Match $productName

    $configName = "CLI IntegTest Config"
    $configId = Invoke-ConfigCat "config", "create", $productId, "-n", $configName
    Invoke-ConfigCat "config", "ls" | Should -Match $configName

    $environmentName = "CLI IntegTest Env"
    $environmentId = Invoke-ConfigCat "environment", "create", $productId, "-n", $environmentName
    Invoke-ConfigCat "environment", "ls" | Should -Match $environmentName
}

AfterAll {
    Invoke-ConfigCat "environment", "rm", $environmentId
    Invoke-ConfigCat "environment", "ls" | Should -Not -Match $environmentId

    Invoke-ConfigCat "config", "rm", $configId
    Invoke-ConfigCat "config", "ls" | Should -Not -Match $configId

    Invoke-ConfigCat "product", "rm", $productId
    Invoke-ConfigCat "product", "ls" | Should -Not -Match $productId
}

Describe "Product / Config / Environment Tests" {
    It "Rename Product" {
        $newProductName = [guid]::NewGuid().ToString()
        Invoke-ConfigCat "product", "update", $productId, "-n", $newProductName
        Invoke-ConfigCat "product", "ls" | Should -Match $newProductName
    }

    It "Rename Config" {
        $newConfigName = "CLI IntegTest Config Updated"
        Invoke-ConfigCat "config", "update", $configId, "-n", $newConfigName
        Invoke-ConfigCat "config", "ls" | Should -Match $newConfigName
    }

    It "Rename Environment" {
        $newEnvironmentName = "CLI IntegTest Env Updated"
        Invoke-ConfigCat "environment", "update", $environmentId, "-n", $newEnvironmentName
        Invoke-ConfigCat "environment", "ls" | Should -Match $newEnvironmentName
    }

    It "Ensure SDK Keys generated" {
        $sdkKeys = Invoke-ConfigCat "sdk-key"
        $sdkKeys | Should -Match $newProductName
        $sdkKeys | Should -Match $newConfigName
        $sdkKeys | Should -Match $newEnvironmentName
    }
}

Describe "Tag / Flag Tests" {
    BeforeAll {
        $tag1Id = Invoke-ConfigCat "tag", "create", $productId, "-n", "tag1", "-c", "panther"
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Match "tag1"
        
        $tag2Id = Invoke-ConfigCat "tag", "create", $productId, "-n", "tag2", "-c", "whale"
        Invoke-ConfigCat "tag", "ls", "-p", $productId | Should -Match "tag2"    

        $flagId = Invoke-ConfigCat "flag", "create", $configId, "-n", "Bool Flag", "-k", "bool_flag", "-i", "hint", "-t", "boolean", "-g", $tag1Id
        $flagResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $flagResult | Should -Match "Bool Flag"
        $flagResult | Should -Match "bool_flag"
        $flagResult | Should -Match "hint"
        $flagResult | Should -Match "boolean"
        $flagResult | Should -Match $tag1Id
    }

    AfterAll {
        Invoke-ConfigCat "tag", "rm", $tag1Id
        Invoke-ConfigCat "tag", "ls" | Should -Not -Match $tag1Id

        Invoke-ConfigCat "tag", "rm", $tag2Id
        Invoke-ConfigCat "tag", "ls" | Should -Not -Match $tag2Id

        Invoke-ConfigCat "flag", "rm", $flagId
        Invoke-ConfigCat "flag", "ls", "-c", $configId | Should -Not -Match $flagId
    }


    It "Update Tag" {
        $newTagName = "newTag1"
        $newTagColor = "salmon"
        Invoke-ConfigCat "tag", "update", $tag1Id, "-n", $newTagName, "-c", $newTagColor
        $tagResult = Invoke-ConfigCat "tag", "ls", "-p", $productId
        $tagResult | Should -Match $newTagName
        $tagResult | Should -Match $newTagColor
    }
    
    It "Update Flag" {
        Invoke-ConfigCat "flag", "update", $flagId, "-n", "Bool Flag Updated", "-i", "hint updated", "-g", $tag2Id
        $updateResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $updateResult | Should -Match "Bool Flag Updated"
        $updateResult | Should -Match "bool_flag"
        $updateResult | Should -Match "hint updated"
        $updateResult | Should -Match "boolean"
        $updateResult | Should -Match $tag2Id        
        $updateResult | Should -Not -Match $tag1Id
    }

    It "Attach Tag" {
        Invoke-ConfigCat "flag", "attach", $flagId, $tag1Id
        $attachResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $attachResult | Should -Match $tag2Id
        $attachResult | Should -Match $tag1Id
    }

    It "Detach Tag" {
        Invoke-ConfigCat "flag", "detach", $flagId, $tag1Id, $tag2Id
        $attachResult = Invoke-ConfigCat "flag", "ls", "-c", $configId
        $attachResult | Should -Not -Match $tag2Id
        $attachResult | Should -Not -Match $tag1Id
    }
}

Describe "Flag value / Rule Tests" {
    BeforeAll {
        $flagId = Invoke-ConfigCat "flag", "create", $configId, "-n", "Bool Flag", "-k", "bool_flag", "-i", "hint", "-t", "boolean"
    }

    AfterAll {
        Invoke-ConfigCat "flag", "rm", $flagId
    }


    It "Update Value" {
        Invoke-ConfigCat "flag", "value", "update", $flagId, $environmentId, "-f", "true"
        $result = Invoke-ConfigCat "flag", "value", "print", $flagId
        $result | Should -Match "Default: True"
    }

    It "Add targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "add", $flagId, $environmentId, "-a", "ID", "-c", "isoneof", "-t", "SAMPLEID,SOMEID", "-f", "true"
        Invoke-ConfigCat "flag", "targeting", "add", $flagId, $environmentId, "-a", "EMAIL", "-c", "contains", "-t", "example.com", "-f", "true"
        Invoke-ConfigCat "flag", "targeting", "add", $flagId, $environmentId, "-a", "VERSION", "-c", "isNotOneOf", "-t", "1.2.6,1.2.8", "-f", "true"
        $result = Invoke-ConfigCat "flag", "value", "print", $flagId
        $result | Should -Match "1. When ID IS ONE OF SAMPLEID,SOMEID then True"
        $result | Should -Match "2. When EMAIL CONTAINS example.com then True"
        $result | Should -Match "3. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
    }

    It "Update targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "update", $flagId, $environmentId, 2, "-a", "EMAIL", "-c", "doesnotcontain", "-t", "sample.com", "-f", "false"
        $result = Invoke-ConfigCat "flag", "value", "print", $flagId
        $result | Should -Match "1. When ID IS ONE OF SAMPLEID,SOMEID then True"
        $result | Should -Match "2. When EMAIL DOES NOT CONTAIN sample.com then False"
        $result | Should -Match "3. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
    }

    It "Move targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "move", $flagId, $environmentId, 3, 1 
        $result = Invoke-ConfigCat "flag", "value", "print", $flagId
        $result | Should -Match "1. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
        $result | Should -Match "2. When ID IS ONE OF SAMPLEID,SOMEID then True"
        $result | Should -Match "3. When EMAIL DOES NOT CONTAIN sample.com then False"
    }

    It "Delete targeting rule" {
        Invoke-ConfigCat "flag", "targeting", "rm", $flagId, $environmentId, 2 
        $result = Invoke-ConfigCat "flag", "value", "print", $flagId
        $result | Should -Match "1. When VERSION IS NOT ONE OF 1.2.6,1.2.8 then True"
        $result | Should -Match "2. When EMAIL DOES NOT CONTAIN sample.com then False"
    }

    It "Add percentage rules" {
        Invoke-ConfigCat "flag", "percentage", "update", $flagId, $environmentId, "30:true", "70:false" 
        $result = Invoke-ConfigCat "flag", "value", "print", $flagId
        $result | Should -Match "30% -> True"
        $result | Should -Match "70% -> False"
    }

    It "Update percentage rules" {
        Invoke-ConfigCat "flag", "percentage", "update", $flagId, $environmentId, "60:true", "40:false" 
        $result = Invoke-ConfigCat "flag", "value", "print", $flagId
        $result | Should -Match "60% -> True"
        $result | Should -Match "40% -> False"
    }

    It "Clear percentage rules" {
        Invoke-ConfigCat "flag", "percentage", "clear", $flagId, $environmentId
        $result = Invoke-ConfigCat "flag", "value", "print", $flagId
        $result | Should -Not -Match "60% -> True"
        $result | Should -Not -Match "40% -> False"
    }

    It "Percentage must be integer" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", $flagId, $environmentId, "text:true", "70:false" 
        $result | Should -Match "is not a number"
    }

    It "Percentage sum must be 100" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", $flagId, $environmentId, "50:true", "70:false" 
        $result | Should -Match "must be 100"
    }

    It "Percentage can't be negative" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", $flagId, $environmentId, "-100:true", "200:false" 
        $result | Should -Match "must be a non-negative number"
    }

    It "Bool can have 2 rules only" {
        $result = Invoke-ConfigCat "flag", "percentage", "update", $flagId, $environmentId, "20:true", "30:false", "50:false" 
        $result | Should -Match "only have 2 percentage rules"
    }
}
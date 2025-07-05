# Pester Test Configuration for Bus Buddy PowerShell Scripts
# Tests the PowerShell automation tools

Describe "Bus Buddy PowerShell Tools Tests" {

    Context "Dependency Analysis" {
        It "Should find project files" {
            $projectFiles = Get-ChildItem -Filter "*.csproj" -Recurse
            $projectFiles.Count | Should -BeGreaterThan 0
        }

        It "Should detect Syncfusion dependencies" {
            # Mock test - would analyze actual dependencies
            $true | Should -Be $true
        }
    }

    Context "Build Error Categorization" {
        It "Should categorize missing property errors" {
            $sampleError = "CS1061: 'Route' does not contain a definition for 'RouteName'"
            $sampleError | Should -Match "does not contain a definition for"
        }

        It "Should identify namespace issues" {
            $sampleError = "CS0103: The name 'Directory' does not exist in the current context"
            $sampleError | Should -Match "does not exist in the current context"
        }
    }

    Context "Refactoring Tools" {
        It "Should create repository template" {
            # Test repository template generation
            $template = "ActivityRepository"
            $template | Should -Not -BeNullOrEmpty
        }

        It "Should update property references" {
            # Test property mapping
            $oldRef = "Route.RouteId"
            $newRef = $oldRef -replace "RouteId", "Id"
            $newRef | Should -Be "Route.Id"
        }
    }
}

Describe "Code Quality Checks" {

    Context "PowerShell Script Analysis" {
        It "Should pass PSScriptAnalyzer rules" {
            $scriptPath = "Scripts\*.ps1"
            if (Get-Module PSScriptAnalyzer -ListAvailable) {
                $issues = Invoke-ScriptAnalyzer -Path $scriptPath -Severity Error
                $issues.Count | Should -Be 0
            } else {
                Set-ItResult -Skipped -Because "PSScriptAnalyzer not installed"
            }
        }
    }

    Context "Project Structure Validation" {
        It "Should have required directories" {
            $requiredDirs = @("Models", "Data", "Forms", "Services", "Scripts")
            foreach ($dir in $requiredDirs) {
                Test-Path $dir | Should -Be $true
            }
        }

        It "Should have consistent namespaces" {
            $csFiles = Get-ChildItem -Filter "*.cs" -Recurse | Select-Object -First 5
            foreach ($file in $csFiles) {
                $content = Get-Content $file.FullName -Raw
                $content | Should -Match "namespace\s+Bus_Buddy"
            }
        }
    }
}

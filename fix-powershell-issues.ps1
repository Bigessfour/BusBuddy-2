#Requires -Version 7.0
<#
.SYNOPSIS
    Systematically fix PowerShell code issues identified by PSScriptAnalyzer

.DESCRIPTION
    This script addresses the major categories of PowerShell issues:
    1. PSUseConsistentWhitespace - Operator spacing
    2. PSAvoidUsingCmdletAliases - Replace aliases with full cmdlet names
    3. PSUseBOMForUnicodeEncodedFile - Add UTF-8 BOM encoding
    4. PSAvoidAssignmentToAutomaticVariable - Fix automatic variable assignments

.PARAMETER WhatIf
    Show what would be changed without making actual changes

.PARAMETER Category
    Specify which category of issues to fix: Whitespace, Aliases, Encoding, Variables, All

.EXAMPLE
    .\fix-powershell-issues.ps1 -Category All
    Fix all categories of issues

.EXAMPLE
    .\fix-powershell-issues.ps1 -Category Aliases -WhatIf
    Show what alias changes would be made
#>

[CmdletBinding()]
param(
    [switch]$WhatIf,
    [ValidateSet('Whitespace', 'Aliases', 'Encoding', 'Variables', 'All')]
    [string]$Category = 'All'
)

# Initialize counters
$script:FixedFiles = 0
$script:TotalChanges = 0

function Write-FixLog {
    param([string]$Message, [string]$Color = 'White')
    Write-Host $Message -ForegroundColor $Color
}

function Fix-OperatorSpacing {
    param([string]$FilePath, [string]$Content)

    $originalContent = $Content

    # Fix common operator spacing issues
    $patterns = @{
        # Assignment operators
        '([a-zA-Z0-9_\$\]])(=)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(\+=)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(-=)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'

        # Comparison operators
        '([a-zA-Z0-9_\$\]])(==)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(!=)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(-eq)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(-ne)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(-gt)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(-lt)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(-ge)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(-le)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'

        # Arithmetic operators (be careful with negative numbers)
        '([a-zA-Z0-9_\$\]])(\+)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
        '([a-zA-Z0-9_\$\]])(-)([a-zA-Z0-9_\$\[])' = '$1 $2 $3'
    }

    foreach ($pattern in $patterns.Keys) {
        $Content = $Content -replace $pattern, $patterns[$pattern]
    }

    if ($Content -ne $originalContent) {
        return $Content
    }
    return $null
}

function Fix-CmdletAliases {
    param([string]$FilePath, [string]$Content)

    $originalContent = $Content

    # Common aliases to replace
    $aliasMap = @{
        '\bWrite\b(?!\-)'          = 'Write-Output'
        '\bSort\b(?!\-)'           = 'Sort-Object'
        '\bSelect\b(?!\-)'         = 'Select-Object'
        '\bWhere\b(?!\-)'          = 'Where-Object'
        '\bForEach\b(?!\-)'        = 'ForEach-Object'
        '\bGroup\b(?!\-)'          = 'Group-Object'
        '\bMeasure\b(?!\-)'        = 'Measure-Object'
        '\bCompare\b(?!\-)'        = 'Compare-Object'
        '\bTee\b(?!\-)'            = 'Tee-Object'
        '\bft\b'                   = 'Format-Table'
        '\bfl\b'                   = 'Format-List'
        '\bfw\b'                   = 'Format-Wide'
        '\bgci\b'                  = 'Get-ChildItem'
        '\bgcm\b'                  = 'Get-Command'
        '\bgm\b'                   = 'Get-Member'
        '\bgi\b'                   = 'Get-Item'
        '\bgl\b'                   = 'Get-Location'
        '\bps\b'                   = 'Get-Process'
        '\bgsv\b'                  = 'Get-Service'
        '\bsv\b'                   = 'Set-Variable'
        '\bgv\b'                   = 'Get-Variable'
        '\bni\b'                   = 'New-Item'
        '\bsi\b'                   = 'Set-Item'
        '\bri\b'                   = 'Remove-Item'
        '\bmi\b'                   = 'Move-Item'
        '\bci\b'                   = 'Copy-Item'
        '\bclc\b'                  = 'Clear-Content'
        '\bgc\b'                   = 'Get-Content'
        '\bsc\b'                   = 'Set-Content'
        '\bac\b'                   = 'Add-Content'
    }

    foreach ($alias in $aliasMap.Keys) {
        $replacement = $aliasMap[$alias]
        $Content = $Content -replace $alias, $replacement
    }

    if ($Content -ne $originalContent) {
        return $Content
    }
    return $null
}

function Fix-AutomaticVariables {
    param([string]$FilePath, [string]$Content)

    $originalContent = $Content

    # Fix common automatic variable assignments
    $variableMap = @{
        '\$profile\s*='            = '$userProfile ='
        '\$host\s*='               = '$psHost ='
        '\$error\s*='              = '$errorList ='
        '\$input\s*='              = '$inputData ='
        '\$matches\s*='            = '$matchResults ='
        '\$myinvocation\s*='       = '$invocationInfo ='
        '\$pid\s*='                = '$processId ='
        '\$pwd\s*='                = '$currentPath ='
        '\$shellid\s*='            = '$shellIdentifier ='
    }

    foreach ($variable in $variableMap.Keys) {
        $replacement = $variableMap[$variable]
        $Content = $Content -ireplace $variable, $replacement
    }

    if ($Content -ne $originalContent) {
        return $Content
    }
    return $null
}

function Add-UTF8BOM {
    param([string]$FilePath)

    try {
        $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
        if ($content) {
            # Add UTF-8 BOM
            $utf8BOM = [System.Text.Encoding]::UTF8.GetPreamble()
            $contentBytes = [System.Text.Encoding]::UTF8.GetBytes($content)
            $fullContent = $utf8BOM + $contentBytes
            [System.IO.File]::WriteAllBytes($FilePath, $fullContent)
            return $true
        }
    }
    catch {
        Write-FixLog "Error adding BOM to $FilePath`: $_" -Color Red
    }
    return $false
}

function Process-PowerShellFile {
    param([string]$FilePath)

    Write-FixLog "Processing: $($FilePath | Split-Path -Leaf)" -Color Cyan

    try {
        $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
        if (-not $content) {
            Write-FixLog "  Skipping empty file" -Color Yellow
            return
        }

        $originalContent = $content
        $changesMade = $false
        $changeList = @()

        # Fix whitespace issues
        if ($Category -in @('Whitespace', 'All')) {
            $fixedContent = Fix-OperatorSpacing -FilePath $FilePath -Content $content
            if ($fixedContent) {
                $content = $fixedContent
                $changesMade = $true
                $changeList += "operator spacing"
            }
        }

        # Fix cmdlet aliases
        if ($Category -in @('Aliases', 'All')) {
            $fixedContent = Fix-CmdletAliases -FilePath $FilePath -Content $content
            if ($fixedContent) {
                $content = $fixedContent
                $changesMade = $true
                $changeList += "cmdlet aliases"
            }
        }

        # Fix automatic variables
        if ($Category -in @('Variables', 'All')) {
            $fixedContent = Fix-AutomaticVariables -FilePath $FilePath -Content $content
            if ($fixedContent) {
                $content = $fixedContent
                $changesMade = $true
                $changeList += "automatic variables"
            }
        }

        # Write-Output changes
        if ($changesMade) {
            if ($WhatIf) {
                Write-FixLog "  Would fix: $($changeList -join ', ')" -Color Green
            } else {
                [System.IO.File]::WriteAllText($FilePath, $content, [System.Text.Encoding]::UTF8)
                Write-FixLog "  Fixed: $($changeList -join ', ')" -Color Green
                $script:TotalChanges++
            }
            $script:FixedFiles++
        }

        # Add UTF-8 BOM if needed
        if ($Category -in @('Encoding', 'All')) {
            if (-not $WhatIf -and $changesMade) {
                if (Add-UTF8BOM -FilePath $FilePath) {
                    Write-FixLog "  Added UTF-8 BOM" -Color Green
                }
            }
        }
    }
    catch {
        Write-FixLog "Error processing $FilePath`: $_" -Color Red
    }
}

# Main execution
Write-FixLog "=== PowerShell Code Issue Fixer ===" -Color Magenta
Write-FixLog "Category: $Category" -Color Cyan
if ($WhatIf) {
    Write-FixLog "Mode: WhatIf (no changes will be made)" -Color Yellow
}
Write-FixLog ""

# Get all PowerShell files
$psFiles = Get-ChildItem -Path '.' -Filter '*.ps1' -Recurse | Where-Object {
    $_.FullName -notlike '*\.git\*' -and
    $_.FullName -notlike '*\bin\*' -and
    $_.FullName -notlike '*\obj\*'
}

Write-FixLog "Found $($psFiles.Count) PowerShell files to process" -Color Cyan
Write-FixLog ""

# Process each file
foreach ($file in $psFiles) {
    Process-PowerShellFile -FilePath $file.FullName
}

# Summary
Write-FixLog ""
Write-FixLog "=== Summary ===" -Color Magenta
Write-FixLog "Files processed: $($psFiles.Count)" -Color Cyan
Write-FixLog "Files with changes: $script:FixedFiles" -Color Green
if (-not $WhatIf) {
    Write-FixLog "Total changes made: $script:TotalChanges" -Color Green

    # Run final analysis
    Write-FixLog ""
    Write-FixLog "Running final analysis..." -Color Cyan
    try {
        $remainingIssues = Invoke-ScriptAnalyzer -Path '.' -Recurse | Where-Object { $_.Severity -in @('Error', 'Warning') }
        Write-FixLog "Remaining issues: $($remainingIssues.Count)" -Color $(if($remainingIssues.Count -eq 0) {'Green'} else {'Yellow'})

        if ($remainingIssues.Count -gt 0) {
            $remainingIssues | Group-Object RuleName | Sort-Object Count -Descending | Select-Object -First 10 | ForEach-Object {
                Write-FixLog "  $($_.Name): $($_.Count)" -Color White
            }
        }
    }
    catch {
        Write-FixLog "Could not run final analysis: $_" -Color Red
    }
}

Write-FixLog ""
Write-FixLog "=== Fix Complete ===" -Color Magenta

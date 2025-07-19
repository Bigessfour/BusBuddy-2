# BusBuddy XAML Manipulation Tools
# Structure-aware XAML editing functions

function Edit-XamlElement {
    <#
    .SYNOPSIS
    Safely edit XAML elements using XPath

    .PARAMETER FilePath
    Path to the XAML file

    .PARAMETER XPath
    XPath selector for the element to edit

    .PARAMETER NewContent
    New XML content for the element
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [Parameter(Mandatory)]
        [string]$XPath,

        [Parameter(Mandatory)]
        [string]$NewContent
    )

    try {
        # Load XAML as XML document
        [xml]$xaml = Get-Content $FilePath -Raw

        # Find target element
        $element = $xaml.SelectSingleNode($XPath)
        if (-not $element) {
            throw "Element not found with XPath: $XPath"
        }

        # Parse new content
        [xml]$newXml = "<root>$NewContent</root>"
        $newElement = $xaml.ImportNode($newXml.DocumentElement.FirstChild, $true)

        # Replace element
        $element.ParentNode.ReplaceChild($newElement, $element)

        # Save with proper formatting
        $xaml.Save($FilePath)

        Write-Host '✅ Successfully edited XAML element' -ForegroundColor Green
    } catch {
        Write-Error "❌ XAML edit failed: $($_.Exception.Message)"
    }
}

function Add-XamlElement {
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [Parameter(Mandatory)]
        [string]$ParentXPath,

        [Parameter(Mandatory)]
        [string]$ElementXml,

        [string]$Position = 'LastChild'
    )

    try {
        [xml]$xaml = Get-Content $FilePath -Raw
        $parent = $xaml.SelectSingleNode($ParentXPath)

        if (-not $parent) {
            throw "Parent element not found: $ParentXPath"
        }

        [xml]$newXml = "<root>$ElementXml</root>"
        $newElement = $xaml.ImportNode($newXml.DocumentElement.FirstChild, $true)

        switch ($Position) {
            'FirstChild' { $parent.PrependChild($newElement) }
            'LastChild' { $parent.AppendChild($newElement) }
            default { $parent.AppendChild($newElement) }
        }

        $xaml.Save($FilePath)
        Write-Host '✅ Successfully added XAML element' -ForegroundColor Green
    } catch {
        Write-Error "❌ XAML add failed: $($_.Exception.Message)"
    }
}

function Set-XamlAttribute {
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [Parameter(Mandatory)]
        [string]$ElementXPath,

        [Parameter(Mandatory)]
        [hashtable]$Attributes
    )

    try {
        [xml]$xaml = Get-Content $FilePath -Raw
        $element = $xaml.SelectSingleNode($ElementXPath)

        if (-not $element) {
            throw "Element not found: $ElementXPath"
        }

        foreach ($attr in $Attributes.GetEnumerator()) {
            $element.SetAttribute($attr.Key, $attr.Value)
        }

        $xaml.Save($FilePath)
        Write-Host '✅ Successfully set XAML attributes' -ForegroundColor Green
    } catch {
        Write-Error "❌ XAML attribute edit failed: $($_.Exception.Message)"
    }
}

function Test-XamlValidity {
    param(
        [Parameter(Mandatory)]
        [string]$FilePath
    )

    try {
        [xml]$xaml = Get-Content $FilePath -Raw
        if ($xaml -and $xaml.DocumentElement) {
            Write-Host '✅ XAML is valid XML with document element' -ForegroundColor Green
            return $true
        } else {
            Write-Host '❌ XAML lacks proper document structure' -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Error "❌ XAML validation failed: $($_.Exception.Message)"
        return $false
    }
}

function Format-XamlFile {
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [int]$IndentSize = 4
    )

    try {
        [xml]$xaml = Get-Content $FilePath -Raw

        # Create formatted XML writer
        $settings = New-Object System.Xml.XmlWriterSettings
        $settings.Indent = $true
        $settings.IndentChars = ' ' * $IndentSize
        $settings.NewLineChars = "`r`n"
        $settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace

        $writer = [System.Xml.XmlWriter]::Create($FilePath, $settings)
        $xaml.Save($writer)
        $writer.Close()

        Write-Host '✅ XAML formatted successfully' -ForegroundColor Green
    } catch {
        Write-Error "❌ XAML formatting failed: $($_.Exception.Message)"
    }
}

# ============================================================================
# PowerShell Syntax Validation Tools
# ============================================================================

function Test-PowerShellSyntax {
    param(
        [string]$FilePath,
        [string]$ScriptContent
    )

    try {
        if ($FilePath) {
            $ScriptContent = Get-Content $FilePath -Raw
        }

        # Parse the script to check for syntax errors
        $tokens = $null
        $errors = $null
        [System.Management.Automation.Language.Parser]::ParseInput($ScriptContent, [ref]$tokens, [ref]$errors)

        $result = @{
            IsValid  = $errors.Count -eq 0
            Errors   = $errors
            Tokens   = $tokens
            Warnings = @()
        }

        # Additional validation checks
        $result.Warnings += Test-PowerShellBestPractices -Content $ScriptContent

        return $result
    } catch {
        return @{
            IsValid  = $false
            Errors   = @($_)
            Tokens   = @()
            Warnings = @()
        }
    }
}

function Test-PowerShellBestPractices {
    param([string]$Content)

    $warnings = @()

    # Check for common issues
    if ($Content -match '\$\w+\s*=\s*\$null') {
        $warnings += 'Consider using [string]::Empty or proper null checks'
    }

    if ($Content -match 'Write-Host') {
        $warnings += 'Consider using Write-Output or Write-Information instead of Write-Host'
    }

    if ($Content -match 'Invoke-Expression') {
        $warnings += 'Avoid Invoke-Expression for security reasons'
    }

    # Check parameter validation
    if ($Content -match 'param\s*\(' -and $Content -notmatch '\[Parameter\(') {
        $warnings += 'Consider adding parameter attributes for better validation'
    }

    return $warnings
}

function Format-PowerShellCode {
    param(
        [string]$FilePath,
        [string]$Content
    )

    try {
        if ($FilePath) {
            $Content = Get-Content $FilePath -Raw
        }

        # Basic formatting rules
        $formatted = $Content

        # Fix indentation (basic)
        $lines = $formatted -split "`n"
        $indentLevel = 0
        $formattedLines = @()

        foreach ($line in $lines) {
            $trimmedLine = $line.Trim()

            # Decrease indent for closing braces
            if ($trimmedLine -match '^}') {
                $indentLevel = [Math]::Max(0, $indentLevel - 1)
            }

            # Add proper indentation
            $formattedLines += ('    ' * $indentLevel) + $trimmedLine

            # Increase indent for opening braces
            if ($trimmedLine -match '{$') {
                $indentLevel++
            }
        }

        return $formattedLines -join "`n"
    } catch {
        Write-Error "Failed to format PowerShell code: $_"
        return $Content
    }
}

function New-PowerShellFunction {
    param(
        [string]$FunctionName,
        [string[]]$Parameters,
        [string]$Description,
        [switch]$AdvancedFunction
    )

    $template = @"
<#
.SYNOPSIS
    $Description

.DESCRIPTION
    Long description

.PARAMETER ParameterName
    Description of parameter

.EXAMPLE
    $FunctionName -Parameter Value

.NOTES
    Author: Generated by XAML-Tools
    Date: $(Get-Date -Format 'yyyy-MM-dd')
#>
function $FunctionName {
"@

    if ($AdvancedFunction) {
        $template += @'

    [CmdletBinding()]
'@
    }

    $template += @'

    param(
'@

    foreach ($param in $Parameters) {
        $template += @"

        [Parameter(Mandatory=`$false)]
        [string]`$$param
"@
        if ($param -ne $Parameters[-1]) {
            $template += ','
        }
    }

    $template += @"

    )

    begin {
        Write-Verbose "Starting `$FunctionName"
    }

    process {
        try {
            # Function implementation here

        }
        catch {
            Write-Error "Error in `$FunctionName: `$(`$_.Exception.Message)"
            throw
        }
    }

    end {
        Write-Verbose "Completed `$FunctionName"
    }
}
"@

    return $template
}

function Test-PowerShellParameterSyntax {
    param(
        [string]$ParameterBlock
    )

    $issues = @()

    # Check for proper parameter attributes
    if ($ParameterBlock -notmatch '\[Parameter\(') {
        $issues += 'Missing Parameter attributes'
    }

    # Check for type annotations
    if ($ParameterBlock -notmatch '\[string\]|\[int\]|\[bool\]|\[object\]') {
        $issues += 'Consider adding type annotations'
    }

    # Check for mandatory parameters without validation
    if ($ParameterBlock -match 'Mandatory=\$true' -and $ParameterBlock -notmatch 'ValidateNotNullOrEmpty') {
        $issues += 'Mandatory parameters should have validation'
    }

    return @{
        IsValid = $issues.Count -eq 0
        Issues  = $issues
    }
}

function New-PowerShellParameterBlock {
    param(
        [hashtable[]]$Parameters
    )

    $paramBlock = "param(`n"

    for ($i = 0; $i -lt $Parameters.Count; $i++) {
        $param = $Parameters[$i]

        $paramBlock += '    [Parameter('

        if ($param.Mandatory) {
            $paramBlock += "Mandatory=`$true"
        }

        if ($null -ne $param.Position) {
            if ($param.Mandatory) { $paramBlock += ', ' }
            $paramBlock += "Position=$($param.Position)"
        }

        $paramBlock += ")]`n"

        if ($param.ValidateSet) {
            $paramBlock += "    [ValidateSet($($param.ValidateSet -join ', '))]`n"
        }

        if ($param.ValidateNotNullOrEmpty) {
            $paramBlock += "    [ValidateNotNullOrEmpty()]`n"
        }

        $paramBlock += "    [$($param.Type)]`$$($param.Name)"

        if ($param.DefaultValue) {
            $paramBlock += " = $($param.DefaultValue)"
        }

        if ($i -lt $Parameters.Count - 1) {
            $paramBlock += ','
        }

        $paramBlock += "`n`n"
    }

    $paramBlock += ')'

    return $paramBlock
}

# Export functions
Export-ModuleMember -Function Edit-XamlElement, Add-XamlElement, Set-XamlAttribute, Test-XamlValidity, Format-XamlFile, Test-PowerShellSyntax, Test-PowerShellBestPractices, Format-PowerShellCode, New-PowerShellFunction, Test-PowerShellParameterSyntax, New-PowerShellParameterBlock

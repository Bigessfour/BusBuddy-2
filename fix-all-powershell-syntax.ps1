
<##
.SYNOPSIS
    Comprehensive PowerShell Syntax Fix Script with robust logging and production readiness.
.DESCRIPTION
    Fixes common PowerShell syntax errors across all .ps1 files in the project. Supports robust logging, exit codes, parameter validation, and JSON output for CI/CD.
.PARAMETER WhatIf
    Show what would be changed without making actual changes
.PARAMETER LogFile
    Path to a log file for output. If not specified, logs only to console.
.PARAMETER Production
    Enables production mode: stricter error handling, disables interactive prompts.
.PARAMETER OutputFormat
    Output format: 'Text' (default) or 'Json'.
.EXAMPLE
    .\fix-all-powershell-syntax.ps1 -LogFile './logs/fix-syntax.log' -OutputFormat Json
.EXAMPLE
    .\fix-all-powershell-syntax.ps1 -WhatIf -Production
.NOTES
    Exits with code 0 on success, 1 on error. Designed for use in CI/CD and production environments.
##>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf,
    [string]$LogFile,
    [switch]$Production,
    [ValidateSet('Text', 'Json')][string]$OutputFormat = 'Text'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = if ($Production) { 'Stop' } else { 'Continue' }

#region Logging
function Write-Log {
    param([string]$Message, [string]$Color = 'White', [string]$Level = 'INFO', [switch]$ToConsole = $true)
    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $logLine = "[$timestamp][$Level] $Message"
    if ($ToConsole) { Write-Host $logLine -ForegroundColor $Color }
    if ($LogFile) { Add-Content -Path $LogFile -Value $logLine }
}
#endregion

$fixCount = 0
$errorCount = 0
$result = [ordered]@{
    Success        = $false
    Errors         = @()
    FilesProcessed = 0
    FilesFixed     = 0
    ErrorsCount    = 0
    Details        = @()
}

Write-Log "🔧 Starting comprehensive PowerShell syntax fixes..." 'Cyan'

try {
    # Get all PowerShell files
    $psFiles = Get-ChildItem -Path "." -Filter "*.ps1" -Recurse -ErrorAction SilentlyContinue
    $result.FilesProcessed = $psFiles.Count
    Write-Log "📁 Found $($psFiles.Count) PowerShell files" 'Yellow'

    foreach ($file in $psFiles) {
        Write-Log "🔍 Processing: $($file.Name)" 'Gray'
        try {
            $content = Get-Content -Path $file.FullName -Raw -ErrorAction Stop
            $originalContent = $content

            # Fix 1: ForEach-Object syntax errors
            $content = $content -replace 'ForEach-Object\s*\(\s*\$(\w+)\s+in\s+([^)]+)\)\s*\{', 'foreach ($$1 in $2) {'

            # Fix 2: Replace Group-Object with GroupBy or proper syntax
            $content = $content -replace '\$Group-Object\.Count', '($_ | Group-Object).Count'

            # Fix 3: Fix hyphenated variable names
            $content = $content -replace '\$([a-zA-Z]+)-([a-zA-Z]+)', '$${1}${2}'

            # Fix 4: Fix Write-Output with hyphens
            $content = $content -replace 'Write-Output\s*-\s*Host', 'Write-Host'
            $content = $content -replace 'Write-Output\s*-\s*Warning', 'Write-Warning'
            $content = $content -replace 'Write-Output\s*-\s*Error', 'Write-Error'
            $content = $content -replace 'Write-Output\s*-\s*Verbose', 'Write-Verbose'
            $content = $content -replace 'Write-Output\s*-\s*Information', 'Write-Information'
            $content = $content -replace 'Write-Output\s*-\s*Output', 'Write-Output'

            # Fix 5: Fix Set-Variable -Content to Set-Content
            $content = $content -replace 'Set-Variable\s*-\s*Content', 'Set-Content'

            # Fix 6: Fix Copy-Item with hyphens
            $content = $content -replace 'Copy-Item\s*-\s*Item', 'Copy-Item'

            # Fix 7: Fix Join-Path with spaces
            $content = $content -replace 'Join\s*-\s*Path', 'Join-Path'

            # Fix 8: Fix Test-Path with spaces
            $content = $content -replace 'Test\s*-\s*Path', 'Test-Path'

            # Fix 9: Fix Get-Command with spaces
            $content = $content -replace 'Get\s*-\s*Command', 'Get-Command'

            # Fix 10: Fix missing CmdletBinding brackets
            $content = $content -replace '\[CmdletBinding\(\)\s*$', '[CmdletBinding()]'

            # Only write if content changed
            if ($content -ne $originalContent) {
                if (-not $WhatIf) {
                    $content | Set-Content -Path $file.FullName -Force
                    Write-Log "✅ Fixed: $($file.Name)" 'Green'
                    $fixCount++
                } else {
                    Write-Log "🔍 Would fix: $($file.Name)" 'Yellow'
                    $fixCount++
                }
            }
        } catch {
            Write-Log "❌ Error processing $($file.Name): $_" 'Red' 'ERROR'
            $errorCount++
            $result.Errors += $_.Exception.Message
        }
    }
    $result.FilesFixed = $fixCount
    $result.ErrorsCount = $errorCount

    Write-Log "`n📊 Summary:" 'Cyan'
    Write-Log "   Fixed files: $fixCount" 'Green'
    Write-Log "   Errors: $errorCount" 'Red'
    Write-Log "   Total files: $($psFiles.Count)" 'Yellow'

    if ($WhatIf) {
        Write-Log "`n💡 Run without -WhatIf to apply fixes" 'Yellow'
    } else {
        Write-Log "`n🎉 PowerShell syntax fixes complete!" 'Green'
    }
    $result.Success = $true
    if ($OutputFormat -eq 'Json') { $result | ConvertTo-Json -Depth 5 | Write-Output }
    exit 0
} catch {
    $msg = "❌ Error during PowerShell syntax fixing: $($_.Exception.Message)"
    Write-Log $msg 'Red' 'ERROR'
    $result.Errors += $msg
    if ($OutputFormat -eq 'Json') { $result | ConvertTo-Json -Depth 5 | Write-Output }
    exit 1
}

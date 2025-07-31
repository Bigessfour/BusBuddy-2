# Script to check PowerShell syntax
param(
    [string]$FilePath = "c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy-DevTools.ps1"
)

function Test-PowerShellSyntax {
    param([string]$Path)

    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($Path, [ref]$null, [ref]$errors)

    if ($errors.Count -gt 0) {
        Write-Host "Syntax errors found in $Path:" -ForegroundColor Red
        foreach ($error in $errors) {
            Write-Host ("Line {0}, Column {1}: {2}" -f $error.Extent.StartLineNumber,
                                                       $error.Extent.StartColumnNumber,
                                                       $error.Message) -ForegroundColor Red
        }
        return $false
    } else {
        Write-Host "No syntax errors found in $Path" -ForegroundColor Green
        return $true
    }
}

Test-PowerShellSyntax -Path $FilePath

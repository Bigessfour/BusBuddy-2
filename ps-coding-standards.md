### 📝 **PowerShell Coding Standards**

BusBuddy enforces strict PowerShell coding standards through PSScriptAnalyzer:

#### 🔄 **Approved PowerShell Verbs**

All PowerShell functions MUST use approved verbs as defined in Microsoft's documentation:
[PowerShell Approved Verbs](https://learn.microsoft.com/en-us/powershell/utility-modules/psscriptanalyzer/rules/useapprovedverbs)

```powershell
# Approved verb groups and examples:
# Common: Get, Set, New, Remove, Enable, Disable, Start, Stop, Read, Write
# Data: Backup, Restore, Import, Export, Sync, Update, Convert
# Lifecycle: Install, Uninstall, Register, Unregister, Request, Resume
# Security: Grant, Revoke, Block, Unblock, Protect, Unprotect
```

❌ **AVOID these non-approved verbs:**
- Display/Show (use `Write-*` or `Format-*` instead)
- Build (use `New-*` or `ConvertTo-*` instead)
- Execute (use `Invoke-*` instead)
- Parse (use `ConvertFrom-*` instead)

✅ **CORRECT function naming:**
```powershell
function Write-DependencySummary { ... }  # NOT Show-DependencySummary
function New-ProjectTemplate { ... }      # NOT Build-ProjectTemplate
function Invoke-BuildProcess { ... }      # NOT Execute-BuildProcess
function Get-SystemHealth { ... }         # NOT Check-SystemHealth
```

#### 🔧 **PSScriptAnalyzer Enforcement**

BusBuddy uses PSScriptAnalyzer to enforce these standards:

```powershell
# Validate script with BusBuddy PSScriptAnalyzer settings
Invoke-ScriptAnalyzer -Path "YourScript.ps1" -Settings ".vscode/PSScriptAnalyzerSettings.psd1"
```

The VS Code task `� BB: Mandatory PowerShell 7.5.2 Syntax Check` will verify all scripts against these standards.

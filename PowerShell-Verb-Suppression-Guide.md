# PowerShell Verb Suppression Guide for BusBuddy

This document demonstrates how to suppress `PSUseApprovedVerbs` warnings when renaming functions isn't feasible.

## ✅ **PREFERRED APPROACH: Use Approved Verbs**

Always prefer using approved PowerShell verbs when possible:
- `Fix-` → `Repair-` (Diagnostic group)
- `Fix-` → `Resolve-` (Diagnostic group) 
- `Fix-` → `Update-` (Data group)
- `Fix-` → `Set-` (Common group)

## Method 1: Function-Level Suppression

Use `[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute]` for individual functions:

```powershell
# Suppress verb warning for legacy function
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', '')]
function Fix-LegacyIssue {
    param([string]$IssueDescription)
    Write-Host "Legacy function that can't be renamed due to external dependencies"
}

# Multiple suppressions
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', '')]
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '')]
function Fix-MultipleWarnings {
    Write-Host "Function with multiple suppressions"
}

# Suppression with specific scope
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', '', Scope='Function', Target='Fix-SpecificFunction')]
function Fix-SpecificFunction {
    param([string]$InputData)
    return $InputData
}
```

## Method 2: VS Code Settings.json Suppression

Add to `.vscode/settings.json` (globally suppresses for workspace):

```json
{
  "powershell.scriptAnalysis.excludeRules": [
    "PSUseApprovedVerbs"
  ]
}
```

**Current BusBuddy settings.json already includes this setting.**

## Method 3: PSScriptAnalyzerSettings.psd1 Suppression

Update `PowerShell/Scripts/Utilities/PSScriptAnalyzerSettings.psd1`:

```powershell
@{
    # Include default PowerShell best practice rules
    IncludeDefaultRules = $true
    
    # Exclude specific rules
    ExcludeRules = @(
        'PSUseApprovedVerbs'  # Add this to suppress verb warnings
    )
}
```

## Method 4: Script-Level Suppression

At the top of a script file:

```powershell
# Suppress specific rules for entire script
[Diagnostics.CodeAnalysis.SuppressMessage('PSUseApprovedVerbs', '')]
param()

# Rest of script...
function Fix-Something { }
```

## Method 5: Inline Suppression

For individual lines:

```powershell
function Fix-TemporaryIssue {  # [SuppressMessage('PSUseApprovedVerbs', '')]
    # Function implementation
}
```

## BusBuddy Implementation Status

### ✅ **COMPLETED: All Functions Use Approved Verbs**

The following functions were successfully renamed:
- `Fix-JsonParsingIssues` → `Repair-JsonParsingIssues`
- `Fix-WorkflowTimeouts` → `Repair-WorkflowTimeouts`  
- `Fix-DotNetCompatibility` → `Repair-DotNetCompatibility`
- `Fix-StandardsValidation` → `Repair-StandardsValidation`

### 🔧 **Current VS Code Configuration**

```json
{
  "powershell.scriptAnalysis.enable": true,
  "powershell.scriptAnalysis.settingsPath": "PSScriptAnalyzerSettings.psd1",
  "powershell.scriptAnalysis.enableDiagnosticSuppression": true,
  "powershell.scriptAnalysis.excludeRules": [
    "PSUseApprovedVerbs"
  ]
}
```

### 📋 **Available Approved Verbs**

Query approved verbs in PowerShell:
```powershell
Get-Verb | Where-Object { $_.Verb -like '*pair*' -or $_.Verb -like '*solve*' -or $_.Verb -like '*date*' }
Get-Verb | Group-Object Group | Sort-Object Name
```

## Best Practices

1. **Always prefer approved verbs** over suppression
2. **Use function-level suppression** for legacy functions that can't be renamed
3. **Use workspace-level suppression** only when absolutely necessary
4. **Document why suppression is needed** in comments
5. **Regularly review suppressed functions** for renaming opportunities

## When to Use Each Method

| Scenario | Recommended Method |
|----------|-------------------|
| Single legacy function | Function-level suppression |
| Multiple related functions | PSScriptAnalyzerSettings.psd1 |
| Entire workspace exception | VS Code settings.json |
| Temporary development | Inline suppression |
| New functions | **Use approved verbs instead** |

## GitHub Actions YAML Parsing Fix

### ✅ **COMPLETED: build-reusable.yml Secrets Fix**

Fixed VS Code GitHub Actions extension warning "Unrecognized named-value: 'secrets'" by:

1. **Explicitly defined all secrets** in `workflow_call.secrets` section:
   ```yaml
   secrets:
     SYNC_LICENSE_KEY:
       description: 'Syncfusion License Key for CI/CD environments'
       required: false
     CODECOV_TOKEN:
       description: 'Codecov token for code coverage uploads'
       required: false
     GITHUB_TOKEN:
       description: 'GitHub token for API access and artifact uploads'
       required: false
   ```

2. **Added fallback patterns** for token usage:
   ```yaml
   env:
     GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN || github.token }}
   ```

3. **Created usage documentation** in `.github/workflows/REUSABLE-WORKFLOW-USAGE.md`

**Result**: VS Code GitHub Actions extension no longer shows parsing warnings for secrets context in reusable workflows.

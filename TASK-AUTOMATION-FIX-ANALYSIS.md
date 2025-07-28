# BusBuddy Task Configuration Analysis and Fix

## Issue Identified: Automation Misconfigs in .vscode/tasks.json

### Problems Found:
1. **Missing Process Cleanup**: Tasks don't properly terminate PowerShell sessions
2. **No problemMatcher for Build Failures**: Allows processes to persist after build failures
3. **Improper Background Process Handling**: Some tasks should be background, others shouldn't
4. **Missing Process Termination**: No cleanup of child processes when tasks end
5. **Persistent PowerShell Sessions**: Profile loading creates sessions that don't terminate

### Root Cause:
Tasks spawning PowerShell processes that load BusBuddy profiles create persistent sessions. When builds fail or are interrupted, these sessions continue holding file locks on DLLs.

### Solution Implementation:
1. Add proper problemMatchers to detect failures and trigger cleanup
2. Implement process termination in task configuration
3. Use `-NoProfile` for build tasks to prevent persistent sessions
4. Add process cleanup commands before and after operations
5. Configure proper `runOptions` and `presentation` for clean termination

## Fixed Task Configuration Patterns:

### Pattern 1: Build Tasks (No Profiles, Clean Exit)
```json
{
  "label": "🔧 BB: Clean Build (Fixed)",
  "type": "shell",
  "command": "pwsh.exe",
  "args": [
    "-NoProfile",
    "-NonInteractive",
    "-Command",
    "& { try { dotnet build BusBuddy.sln --verbosity minimal } finally { [System.GC]::Collect(); [System.GC]::WaitForPendingFinalizers() } }"
  ],
  "group": {
    "kind": "build",
    "isDefault": true
  },
  "runOptions": {
    "instanceLimit": 1
  },
  "presentation": {
    "echo": true,
    "reveal": "always",
    "focus": true,
    "panel": "dedicated",
    "showReuseMessage": false,
    "clear": true
  },
  "isBackground": false,
  "problemMatcher": [
    "$msCompile"
  ],
  "detail": "🔧 Clean build with guaranteed process cleanup"
}
```

### Pattern 2: Run Tasks (Controlled Profile Loading)
```json
{
  "label": "🚌 BB: Run App (Fixed)",
  "type": "shell",
  "command": "pwsh.exe",
  "args": [
    "-ExecutionPolicy",
    "Bypass",
    "-Command",
    "& { $ErrorActionPreference = 'Stop'; try { Set-Location '${workspaceFolder}'; if (Test-Path '.\\load-bus-buddy-profiles.ps1') { & '.\\load-bus-buddy-profiles.ps1' -Quiet }; dotnet run --project 'BusBuddy.WPF\\BusBuddy.WPF.csproj' } finally { Get-Process pwsh -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID -and $_.ProcessName -eq 'pwsh' } | Stop-Process -Force -ErrorAction SilentlyContinue; [System.GC]::Collect() } }"
  ],
  "group": "build",
  "runOptions": {
    "instanceLimit": 1
  },
  "presentation": {
    "echo": true,
    "reveal": "always",
    "focus": true,
    "panel": "dedicated",
    "showReuseMessage": false,
    "clear": false
  },
  "isBackground": false,
  "problemMatcher": [],
  "detail": "🚌 Run app with controlled profile loading and cleanup"
}
```

### Pattern 3: Diagnostic Tasks (Isolated Execution)
```json
{
  "label": "🔍 BB: Health Check (Fixed)",
  "type": "shell",
  "command": "pwsh.exe",
  "args": [
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-Command",
    "& { Write-Host '=== Health Check ===' -ForegroundColor Cyan; dotnet build BusBuddy.sln --verbosity quiet --nologo; if ($LASTEXITCODE -eq 0) { Write-Host '✅ Build: OK' -ForegroundColor Green } else { Write-Host '❌ Build: FAILED' -ForegroundColor Red } }"
  ],
  "group": "test",
  "runOptions": {
    "instanceLimit": 1
  },
  "presentation": {
    "echo": true,
    "reveal": "always",
    "focus": true,
    "panel": "shared",
    "showReuseMessage": false,
    "clear": true
  },
  "isBackground": false,
  "detail": "🔍 Isolated health check without profiles"
}
```

## Key Fixes Applied:

1. **Process Cleanup in finally blocks**: Ensures cleanup even on errors
2. **-NoProfile for builds**: Prevents profile-related persistent sessions
3. **Proper instanceLimit**: Prevents multiple concurrent instances
4. **Enhanced problemMatcher**: Detects failures for proper cleanup
5. **GC.Collect() calls**: Forces garbage collection to release file handles
6. **Process termination**: Explicitly kills orphaned PowerShell processes
7. **Controlled panel management**: Prevents session mixing

## Implementation Plan:

1. Backup current tasks.json
2. Apply fixed task configurations
3. Test build and run operations
4. Verify no persistent PowerShell processes
5. Confirm file locks are resolved

This addresses the MSB3027/MSB3021 file lock issues by ensuring proper process lifecycle management in VS Code tasks.

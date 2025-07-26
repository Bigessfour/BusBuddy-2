#Requires -Version 7.0
<#
.SYNOPSIS
Enhanced Terminal Persistence for BusBuddy Development Environment

.DESCRIPTION
Provides persistent terminal session management with state preservation,
automatic profile loading, and enhanced debugging capabilities for the
BusBuddy WPF application development workflow.

.NOTES
- Requires PowerShell 7.0+
- Compatible with VS Code integrated terminal
- Supports multiple concurrent terminal sessions
- Integrates with BusBuddy PowerShell profiles

.EXAMPLE
.\Enhanced-Terminal-Persistence.ps1 -Initialize
Initialize persistent terminal environment

.EXAMPLE
.\Enhanced-Terminal-Persistence.ps1 -RestoreSession "DevSession1"
Restore a previously saved terminal session
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$SessionName = "BusBuddy-Dev",

    [Parameter(Mandatory = $false)]
    [switch]$Initialize,

    [Parameter(Mandatory = $false)]
    [switch]$RestoreSession,

    [Parameter(Mandatory = $false)]
    [switch]$SaveSession,

    [Parameter(Mandatory = $false)]
    [switch]$ListSessions,

    [Parameter(Mandatory = $false)]
    [string]$WorkspaceRoot = $PSScriptRoot,

    [Parameter(Mandatory = $false)]
    [switch]$Verbose
)

# Enhanced Terminal Persistence Module for BusBuddy
class TerminalPersistenceManager {
    [string]$SessionsPath
    [string]$WorkspaceRoot
    [hashtable]$SessionData
    [string]$CurrentSessionId

    TerminalPersistenceManager([string]$workspaceRoot) {
        $this.WorkspaceRoot = $workspaceRoot
        $this.SessionsPath = Join-Path $workspaceRoot ".vscode\terminal-sessions"
        $this.SessionData = @{}
        $this.CurrentSessionId = [System.Guid]::NewGuid().ToString()

        # Ensure sessions directory exists
        if (-not (Test-Path $this.SessionsPath)) {
            New-Item -Path $this.SessionsPath -ItemType Directory -Force | Out-Null
        }
    }

    [void]InitializeSession([string]$sessionName) {
        Write-Host "🚀 Initializing enhanced terminal session: $sessionName" -ForegroundColor Cyan

        # Set up session environment
        $sessionConfig = @{
            SessionName   = $sessionName
            SessionId     = $this.CurrentSessionId
            WorkspaceRoot = $this.WorkspaceRoot
            StartTime     = Get-Date
            Environment   = @{
                PSVersion = $PSVersionTable.PSVersion.ToString()
                OS        = $PSVersionTable.OS
                Platform  = $PSVersionTable.Platform
                Edition   = $PSVersionTable.PSEdition
            }
            History       = @()
            Variables     = @{}
            Modules       = @()
            Functions     = @()
        }

        # Load BusBuddy profiles if available
        $this.LoadBusBuddyProfiles()

        # Set up environment variables
        $env:BUSBUDDY_SESSION_ID = $this.CurrentSessionId
        $env:BUSBUDDY_SESSION_NAME = $sessionName
        $env:BUSBUDDY_WORKSPACE = $this.WorkspaceRoot

        # Configure PowerShell for enhanced development
        $this.ConfigureEnhancedEnvironment()

        # Save initial session state
        $this.SaveSessionState($sessionName, $sessionConfig)

        Write-Host "✅ Terminal session initialized successfully" -ForegroundColor Green
        Write-Host "   Session ID: $($this.CurrentSessionId)" -ForegroundColor Gray
        Write-Host "   Workspace: $($this.WorkspaceRoot)" -ForegroundColor Gray
    }

    [void]LoadBusBuddyProfiles() {
        $profilePath = Join-Path $this.WorkspaceRoot "load-bus-buddy-profiles.ps1"

        if (Test-Path $profilePath) {
            Write-Host "📋 Loading BusBuddy profiles..." -ForegroundColor Yellow
            try {
                & $profilePath -Quiet
                Write-Host "✅ BusBuddy profiles loaded" -ForegroundColor Green
            }
            catch {
                Write-Host "⚠️ Failed to load BusBuddy profiles: $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
        else {
            Write-Host "ℹ️ BusBuddy profiles not found, using basic configuration" -ForegroundColor Blue
        }
    }

    [void]ConfigureEnhancedEnvironment() {
        # Set up enhanced prompt with session info
        function global:prompt {
            $sessionInfo = if ($env:BUSBUDDY_SESSION_NAME) {
                "[$($env:BUSBUDDY_SESSION_NAME)]"
            }
            else {
                "[BusBuddy]"
            }

            $location = Split-Path -Leaf (Get-Location)
            $gitBranch = ""

            try {
                $gitStatus = git branch --show-current 2>$null
                if ($gitStatus) {
                    $gitBranch = " (git:$gitStatus)"
                }
            }
            catch { }

            Write-Host "$sessionInfo " -NoNewline -ForegroundColor Cyan
            Write-Host "$location" -NoNewline -ForegroundColor Yellow
            Write-Host "$gitBranch" -NoNewline -ForegroundColor Green
            Write-Host " PS> " -NoNewline -ForegroundColor White
            return " "
        }

        # Set up enhanced history
        Set-PSReadLineOption -PredictionSource History
        Set-PSReadLineOption -HistorySearchCursorMovesToEnd
        Set-PSReadLineKeyHandler -Key UpArrow -Function HistorySearchBackward
        Set-PSReadLineKeyHandler -Key DownArrow -Function HistorySearchForward

        # Set up tab completion
        Set-PSReadLineKeyHandler -Key Tab -Function Complete

        # Enhanced error handling
        $ErrorActionPreference = "Continue"
    }

    [void]SaveSessionState([string]$sessionName, [hashtable]$config) {
        $sessionFile = Join-Path $this.SessionsPath "$sessionName.json"

        try {
            # Capture current state
            $config.Environment.CurrentLocation = Get-Location
            $config.Environment.LoadedModules = Get-Module | Select-Object Name, Version
            $config.Variables.EnvironmentVariables = Get-ChildItem env: | Where-Object { $_.Name -like "BUSBUDDY*" }

            $config | ConvertTo-Json -Depth 10 | Set-Content -Path $sessionFile -Encoding UTF8
            Write-Verbose "Session state saved to: $sessionFile"
        }
        catch {
            Write-Warning "Failed to save session state: $($_.Exception.Message)"
        }
    }

    [hashtable]RestoreSessionState([string]$sessionName) {
        $sessionFile = Join-Path $this.SessionsPath "$sessionName.json"

        if (-not (Test-Path $sessionFile)) {
            throw "Session '$sessionName' not found"
        }

        try {
            $sessionData = Get-Content -Path $sessionFile -Raw | ConvertFrom-Json -AsHashtable

            # Restore environment variables
            if ($sessionData.Variables.EnvironmentVariables) {
                foreach ($envVar in $sessionData.Variables.EnvironmentVariables) {
                    Set-Item -Path "env:$($envVar.Name)" -Value $envVar.Value
                }
            }

            # Restore location
            if ($sessionData.Environment.CurrentLocation) {
                Set-Location -Path $sessionData.Environment.CurrentLocation
            }

            Write-Host "🔄 Session '$sessionName' restored successfully" -ForegroundColor Green
            return $sessionData
        }
        catch {
            throw "Failed to restore session: $($_.Exception.Message)"
        }
    }

    [array]ListAvailableSessions() {
        $sessionFiles = Get-ChildItem -Path $this.SessionsPath -Filter "*.json" -ErrorAction SilentlyContinue

        $sessions = @()
        foreach ($file in $sessionFiles) {
            try {
                $sessionData = Get-Content -Path $file.FullName -Raw | ConvertFrom-Json
                $sessions += [PSCustomObject]@{
                    Name      = $sessionData.SessionName
                    StartTime = $sessionData.StartTime
                    SessionId = $sessionData.SessionId
                    File      = $file.Name
                }
            }
            catch {
                Write-Warning "Failed to read session file: $($file.Name)"
            }
        }

        return $sessions
    }

    [void]CleanupOldSessions([int]$daysOld = 7) {
        $cutoffDate = (Get-Date).AddDays(-$daysOld)
        $sessionFiles = Get-ChildItem -Path $this.SessionsPath -Filter "*.json" -ErrorAction SilentlyContinue

        foreach ($file in $sessionFiles) {
            if ($file.LastWriteTime -lt $cutoffDate) {
                Remove-Item -Path $file.FullName -Force
                Write-Verbose "Removed old session file: $($file.Name)"
            }
        }
    }
}

# Main execution logic
function Main {
    $persistenceManager = [TerminalPersistenceManager]::new($WorkspaceRoot)

    if ($Initialize) {
        $persistenceManager.InitializeSession($SessionName)
        return
    }

    if ($RestoreSession) {
        try {
            $sessionData = $persistenceManager.RestoreSessionState($SessionName)
            Write-Host "Session Details:" -ForegroundColor Cyan
            Write-Host "  Name: $($sessionData.SessionName)" -ForegroundColor Gray
            Write-Host "  Started: $($sessionData.StartTime)" -ForegroundColor Gray
            Write-Host "  Session ID: $($sessionData.SessionId)" -ForegroundColor Gray
        }
        catch {
            Write-Error "Failed to restore session: $($_.Exception.Message)"
            exit 1
        }
        return
    }

    if ($SaveSession) {
        $config = @{
            SessionName = $SessionName
            SessionId   = $env:BUSBUDDY_SESSION_ID
            SaveTime    = Get-Date
        }
        $persistenceManager.SaveSessionState($SessionName, $config)
        Write-Host "✅ Session '$SessionName' saved" -ForegroundColor Green
        return
    }

    if ($ListSessions) {
        $sessions = $persistenceManager.ListAvailableSessions()

        if ($sessions.Count -eq 0) {
            Write-Host "No saved sessions found" -ForegroundColor Yellow
        }
        else {
            Write-Host "Available Sessions:" -ForegroundColor Cyan
            $sessions | Format-Table Name, StartTime, SessionId -AutoSize
        }
        return
    }

    # Default behavior: initialize session
    Write-Host "Enhanced Terminal Persistence for BusBuddy" -ForegroundColor Cyan
    Write-Host "Use -Initialize to start a new session" -ForegroundColor Yellow
    Write-Host "Use -RestoreSession to restore a saved session" -ForegroundColor Yellow
    Write-Host "Use -ListSessions to see available sessions" -ForegroundColor Yellow
}

# Execute main function if script is run directly
if ($MyInvocation.InvocationName -ne '.') {
    Main
}

# Export functions for module use
Export-ModuleMember -Function @('Main')

# Install Professional Developer Tools for Bus Buddy Project
# PowerShell 7.x Compatible Version with Administrator Support

# Check if running as Administrator
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

$isAdmin = Test-Administrator
$installScope = $isAdmin ? 'AllUsers' : 'CurrentUser'

Write-Host 'Installing Professional Developer Tools for Bus Buddy...' -ForegroundColor Green
Write-Host "Administrator Status: $($isAdmin ? 'YES' : 'NO')" -ForegroundColor ($isAdmin ? 'Green' : 'Yellow')
Write-Host "Installation Scope: $installScope" -ForegroundColor Cyan

# 1. PSScriptAnalyzer - Code analysis and linting
Write-Host 'Installing PSScriptAnalyzer...' -ForegroundColor Yellow
Install-Module -Name PSScriptAnalyzer -Force -AllowClobber -Scope $installScope

# 2. Pester - Testing framework
Write-Host 'Installing Pester...' -ForegroundColor Yellow
Install-Module -Name Pester -Force -AllowClobber -Scope $installScope

# 3. EF Core Tools
Write-Host 'Installing Entity Framework Core Tools...' -ForegroundColor Yellow
dotnet tool install --global dotnet-ef

# 4. SQL Server PowerShell Module
Write-Host 'Installing SQL Server PowerShell Module...' -ForegroundColor Yellow
Install-Module -Name SqlServer -Force -AllowClobber -Scope $installScope

# 5. Invoke-Build - Advanced build automation
Write-Host 'Installing Invoke-Build...' -ForegroundColor Yellow
Install-Module -Name InvokeBuild -Force -AllowClobber -Scope $installScope

# 6. PowerShell Pro Tools (if available)
Write-Host 'Installing PowerShell Pro Tools...' -ForegroundColor Yellow
try {
    Install-Module -Name PowerShellProTools -Force -AllowClobber -Scope $installScope
} catch {
    Write-Warning 'PowerShell Pro Tools not available in PowerShell Gallery'
}

# 7. PackageManagement enhancements
Write-Host 'Installing PowerShellGet...' -ForegroundColor Yellow
Install-Module -Name PowerShellGet -Force -AllowClobber -Scope $installScope

# 8. NUnit Testing Tools
Write-Host 'Installing NUnit testing tools...' -ForegroundColor Yellow
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet tool install --global coverlet.console

# 9. Test result parsers
Write-Host 'Installing test result analyzers...' -ForegroundColor Yellow
try {
    Install-Module -Name PSTestReport -Force -AllowClobber -Scope $installScope
} catch {
    Write-Warning 'PSTestReport not available in PowerShell Gallery'
}

# 10. PowerShell 7.x Enhanced Tools (Admin gets additional tools)
if ($isAdmin) {
    Write-Host 'Installing Administrator-Enhanced Tools...' -ForegroundColor Magenta

    # Terminal-Icons
    try {
        Install-Module -Name Terminal-Icons -Force -AllowClobber -Scope AllUsers
        Write-Host '✓ Terminal-Icons (system-wide)' -ForegroundColor Green
    } catch {
        Write-Warning 'Failed to install Terminal-Icons system-wide'
    }

    # Console GUI Tools
    try {
        Install-Module -Name Microsoft.PowerShell.ConsoleGuiTools -Force -AllowClobber -Scope AllUsers
        Write-Host '✓ ConsoleGuiTools (system-wide)' -ForegroundColor Green
    } catch {
        Write-Warning 'Failed to install ConsoleGuiTools system-wide'
    }

    # PowerShell-Yaml
    try {
        Install-Module -Name powershell-yaml -Force -AllowClobber -Scope AllUsers
        Write-Host '✓ PowerShell-Yaml (system-wide)' -ForegroundColor Green
    } catch {
        Write-Warning 'Failed to install PowerShell-Yaml system-wide'
    }

    # Admin-only tools
    try {
        Install-Module -Name PSWindowsUpdate -Force -AllowClobber -Scope AllUsers
        Write-Host '✓ PSWindowsUpdate (Admin-only)' -ForegroundColor Green
    } catch {
        Write-Warning 'Failed to install PSWindowsUpdate'
    }

    try {
        Install-Module -Name PowerShellForGitHub -Force -AllowClobber -Scope AllUsers
        Write-Host '✓ PowerShellForGitHub (system-wide)' -ForegroundColor Green
    } catch {
        Write-Warning 'Failed to install PowerShellForGitHub'
    }
}

Write-Host 'All developer tools installed successfully!' -ForegroundColor Green
Write-Host "Run 'Get-Module -ListAvailable' to see installed modules" -ForegroundColor Cyan
Write-Host 'NUnit tools: dotnet-reportgenerator-globaltool, coverlet.console' -ForegroundColor Cyan

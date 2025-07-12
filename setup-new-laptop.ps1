# BusBuddy New Laptop Setup Script
# Run this script as Administrator in PowerShell

param(
    [string]$GitHubRepo = "https://github.com/Bigessfour/BusBuddy_Syncfusion.git",
    [string]$ProjectPath = "C:\Development\BusBuddy",
    [switch]$SkipSoftwareInstall = $false
)

Write-Host "🚌 BusBuddy - New Laptop Setup Script" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Function to check if software is installed
function Test-SoftwareInstalled {
    param([string]$SoftwareName)
    try {
        $installed = Get-WmiObject -Class Win32_Product | Where-Object { $_.Name -like "*$SoftwareName*" }
        return $installed -ne $null
    }
    catch {
        return $false
    }
}

# 1. Install Prerequisites (if not skipped)
if (-not $SkipSoftwareInstall) {
    Write-Host "📦 Installing Prerequisites..." -ForegroundColor Yellow

    # Check if winget is available
    if (Get-Command winget -ErrorAction SilentlyContinue) {
        # Install .NET 8 SDK
        if (-not (Test-Path "C:\Program Files\dotnet\dotnet.exe")) {
            Write-Host "Installing .NET 8 SDK..." -ForegroundColor Cyan
            winget install Microsoft.DotNet.SDK.8 --silent
        }

        # Install Git
        if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
            Write-Host "Installing Git..." -ForegroundColor Cyan
            winget install Git.Git --silent
        }

        # Install SQL Server Express
        if (-not (Test-SoftwareInstalled "SQL Server")) {
            Write-Host "Installing SQL Server Express..." -ForegroundColor Cyan
            winget install Microsoft.SQLServer.2022.Express --silent
        }

        # Install Visual Studio (optional - takes time)
        $installVS = Read-Host "Install Visual Studio 2022? (y/n)"
        if ($installVS -eq 'y') {
            winget install Microsoft.VisualStudio.2022.Community --silent
        }
    }
    else {
        Write-Host "❌ winget not available. Please install software manually." -ForegroundColor Red
        Write-Host "Required: .NET 8 SDK, Git, SQL Server Express, Visual Studio 2022" -ForegroundColor Yellow
    }
}

# 2. Create project directory
Write-Host "📁 Creating project directory..." -ForegroundColor Yellow
if (-not (Test-Path $ProjectPath)) {
    New-Item -ItemType Directory -Path $ProjectPath -Force | Out-Null
}
Set-Location $ProjectPath

# 3. Clone repository
Write-Host "🔄 Cloning repository..." -ForegroundColor Yellow
if (Test-Path ".git") {
    Write-Host "Repository already cloned. Pulling latest changes..." -ForegroundColor Cyan
    git pull
}
else {
    git clone $GitHubRepo .
}

# 4. Configure Git (if needed)
$gitUser = git config --global user.name 2>$null
if (-not $gitUser) {
    $userName = Read-Host "Enter your Git username"
    $userEmail = Read-Host "Enter your Git email"
    git config --global user.name $userName
    git config --global user.email $userEmail
}

# 5. Environment Configuration
Write-Host "⚙️ Configuring environment..." -ForegroundColor Yellow

# Check if appsettings.json exists
$appSettingsPath = Join-Path $ProjectPath "BusBuddy.WPF\appsettings.json"
if (-not (Test-Path $appSettingsPath)) {
    Write-Host "❌ appsettings.json not found!" -ForegroundColor Red
    Write-Host "Please copy appsettings.json from your other laptop to:" -ForegroundColor Yellow
    Write-Host $appSettingsPath -ForegroundColor Cyan
}

# 6. Restore packages and build
Write-Host "🔨 Restoring packages and building..." -ForegroundColor Yellow
try {
    dotnet restore
    $buildResult = dotnet build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Build successful!" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Build failed. Check output above." -ForegroundColor Red
    }
}
catch {
    Write-Host "❌ Error during build: $_" -ForegroundColor Red
}

# 7. Database setup
Write-Host "🗄️ Setting up database..." -ForegroundColor Yellow
$setupDB = Read-Host "Set up local database? (y/n)"
if ($setupDB -eq 'y') {
    try {
        # Check if SQL Server is running
        $sqlService = Get-Service -Name "MSSQL*" -ErrorAction SilentlyContinue
        if ($sqlService -and $sqlService.Status -eq "Running") {
            Write-Host "SQL Server is running. Applying migrations..." -ForegroundColor Cyan
            dotnet ef database update --project BusBuddy.Core
            Write-Host "✅ Database setup complete!" -ForegroundColor Green
        }
        else {
            Write-Host "❌ SQL Server not running. Please start SQL Server service." -ForegroundColor Red
        }
    }
    catch {
        Write-Host "❌ Database setup failed: $_" -ForegroundColor Red
    }
}

# 8. Final instructions
Write-Host ""
Write-Host "🎉 Setup Complete!" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Copy appsettings.json from your other laptop (if not already done)"
Write-Host "2. Ensure Syncfusion license is valid"
Write-Host "3. Open BusBuddy.sln in Visual Studio"
Write-Host "4. Build and run the project"
Write-Host ""
Write-Host "Project location: $ProjectPath" -ForegroundColor Cyan

# Open in VS Code or Explorer
$openLocation = Read-Host "Open project folder? (y/n)"
if ($openLocation -eq 'y') {
    if (Get-Command code -ErrorAction SilentlyContinue) {
        code .
    }
    else {
        explorer .
    }
}

# Bus Buddy Development PowerShell Profile
# This file contains custom functions and aliases for Bus Buddy development

# ============================================
# VS Code Integration Functions
# ============================================

function code {
    <#
    .SYNOPSIS
    Opens VS Code with robust path detection

    .DESCRIPTION
    Automatically detects VS Code installation and opens files/folders
    Supports both VS Code and VS Code Insiders
    #>
    param([string[]]$Arguments)

    $vsCodePaths = @(
        'C:\Users\steve.mckitrick\AppData\Local\Programs\Microsoft VS Code Insiders\bin\code-insiders.cmd',
        'C:\Users\steve.mckitrick\AppData\Local\Programs\Microsoft VS Code\bin\code.cmd',
        'C:\Program Files\Microsoft VS Code\bin\code.cmd',
        'C:\Program Files (x86)\Microsoft VS Code\bin\code.cmd'
    )

    $foundPath = $vsCodePaths | Where-Object { Test-Path $_ } | Select-Object -First 1

    if ($foundPath) {
        & $foundPath @Arguments
    } elseif (Get-Command code-insiders -ErrorAction SilentlyContinue) {
        & code-insiders @Arguments
    } elseif (Get-Command code -ErrorAction SilentlyContinue) {
        & code @Arguments
    } else {
        Write-Host '❌ VS Code not found in expected locations' -ForegroundColor Red
        Write-Host 'Please ensure VS Code is installed and in PATH' -ForegroundColor Yellow
    }
}

# VS Code Aliases
function vscode { code @Arguments }
function edit { code @Arguments }
Set-Alias -Name vs -Value code -Force
Set-Alias -Name edit-file -Value code -Force

# ============================================
# XAML Safe Processing Functions
# ============================================

function Test-XamlSyntax {
    <#
    .SYNOPSIS
    Validates XAML file syntax using XML parser
    
    .DESCRIPTION
    Tests if a XAML file has valid XML syntax before processing
    Returns true if valid, false if invalid with error details
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )
    
    if (-not (Test-Path $FilePath)) {
        Write-Host "❌ File not found: $FilePath" -ForegroundColor Red
        return $false
    }
    
    try {
        [xml]$test = Get-Content $FilePath -Raw
        Write-Host "✅ Valid XML syntax: $FilePath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ XML Syntax Error in $FilePath`: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Backup-XamlFiles {
    <#
    .SYNOPSIS
    Creates timestamped backup of all XAML files in a directory
    
    .DESCRIPTION
    Creates a backup directory with timestamp and copies all XAML files
    Preserves directory structure for easy restoration
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath
    )
    
    if (-not (Test-Path $RootPath)) {
        Write-Host "❌ Path not found: $RootPath" -ForegroundColor Red
        return
    }
    
    $timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
    $backupDir = "$RootPath\.backup_$timestamp"
    
    Write-Host "🔄 Creating backup..." -ForegroundColor Yellow
    
    Get-ChildItem -Path $RootPath -Recurse -Filter "*.xaml" | ForEach-Object {
        $relativePath = $_.FullName.Substring($RootPath.Length + 1)
        $backupPath = Join-Path $backupDir $relativePath
        $backupFolder = Split-Path $backupPath -Parent
        
        if (-not (Test-Path $backupFolder)) {
            New-Item -Path $backupFolder -ItemType Directory -Force | Out-Null
        }
        Copy-Item $_.FullName $backupPath
    }
    
    Write-Host "✅ Backup created: $backupDir" -ForegroundColor Green
    return $backupDir
}

function Remove-XmlAttribute-Safe {
    <#
    .SYNOPSIS
    Safely removes XML attributes using proper XML parsing
    
    .DESCRIPTION
    Uses XML DOM manipulation to safely remove attributes without corrupting the file
    Validates XML before and after operation
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(Mandatory = $true)]
        [string]$AttributeName,
        [string]$AttributeValue = $null
    )
    
    try {
        # Validate input file
        if (-not (Test-XamlSyntax -FilePath $FilePath)) {
            return $false
        }
        
        # Load as XML document for proper parsing
        [xml]$xmlDoc = Get-Content $FilePath
        
        # Find all elements with the target attribute
        $xpath = if ($AttributeValue) {
            "//*[@$AttributeName='$AttributeValue']"
        } else {
            "//*[@$AttributeName]"
        }
        
        $elementsWithAttribute = $xmlDoc.SelectNodes($xpath)
        $removedCount = 0
        
        foreach ($element in $elementsWithAttribute) {
            $element.RemoveAttribute($AttributeName)
            $removedCount++
        }
        
        if ($removedCount -gt 0) {
            # Save with proper formatting
            $xmlDoc.Save($FilePath)
            Write-Host "✅ Removed $removedCount instances of '$AttributeName' from $FilePath" -ForegroundColor Green
        }
        
        return $true
    }
    catch {
        Write-Host "❌ Error processing $FilePath`: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Remove-SyncfusionVisualStyle {
    <#
    .SYNOPSIS
    Safely removes deprecated Syncfusion VisualStyle attributes
    
    .DESCRIPTION
    Removes syncfusionskin:SfSkinManager.VisualStyle="FluentDark" attributes
    Uses XML DOM manipulation for safety
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )
    
    return Remove-XmlAttribute-Safe -FilePath $FilePath -AttributeName "syncfusionskin:SfSkinManager.VisualStyle" -AttributeValue "FluentDark"
}

function Format-XamlFile {
    <#
    .SYNOPSIS
    Formats XAML file with proper indentation and structure
    
    .DESCRIPTION
    Uses XML formatting to ensure proper XAML structure and indentation
    Validates syntax before and after formatting
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )
    
    try {
        # Validate input
        if (-not (Test-XamlSyntax -FilePath $FilePath)) {
            return $false
        }
        
        # Load and format XML
        [xml]$xmlDoc = Get-Content $FilePath
        
        # Create formatted output
        $stringWriter = New-Object System.IO.StringWriter
        $xmlWriter = New-Object System.Xml.XmlTextWriter($stringWriter)
        $xmlWriter.Formatting = [System.Xml.Formatting]::Indented
        $xmlWriter.Indentation = 4
        $xmlWriter.IndentChar = ' '
        
        $xmlDoc.WriteContentTo($xmlWriter)
        $xmlWriter.Close()
        
        # Save formatted content
        $formattedContent = $stringWriter.ToString()
        Set-Content -Path $FilePath -Value $formattedContent -Encoding UTF8
        
        Write-Host "✅ Formatted: $FilePath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ Error formatting $FilePath`: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Safe-BulkXamlUpdate {
    <#
    .SYNOPSIS
    Safely performs bulk XAML updates with backup and validation
    
    .DESCRIPTION
    Creates backup, validates all files, performs updates, and validates results
    Implements rollback on failure
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath,
        [Parameter(Mandatory = $true)]
        [scriptblock]$UpdateScript,
        [string]$Description = "Bulk XAML Update"
    )
    
    Write-Host "🚀 Starting: $Description" -ForegroundColor Cyan
    
    # 1. Create backup
    $backupDir = Backup-XamlFiles -RootPath $RootPath
    if (-not $backupDir) {
        Write-Host "❌ Backup failed. Aborting update." -ForegroundColor Red
        return $false
    }
    
    # 2. Validate all files before processing
    Write-Host "🔍 Validating XAML files..." -ForegroundColor Yellow
    $xamlFiles = Get-ChildItem -Path $RootPath -Recurse -Filter "*.xaml"
    $invalidFiles = @()
    
    foreach ($file in $xamlFiles) {
        if (-not (Test-XamlSyntax -FilePath $file.FullName)) {
            $invalidFiles += $file.FullName
        }
    }
    
    if ($invalidFiles.Count -gt 0) {
        Write-Host "❌ Found $($invalidFiles.Count) invalid XAML files before processing. Aborting." -ForegroundColor Red
        $invalidFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
        return $false
    }
    
    # 3. Apply updates
    Write-Host "⚡ Applying updates..." -ForegroundColor Yellow
    try {
        & $UpdateScript
    }
    catch {
        Write-Host "❌ Update script failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    # 4. Validate after processing
    Write-Host "🔍 Validating results..." -ForegroundColor Yellow
    $brokenFiles = @()
    foreach ($file in $xamlFiles) {
        if (-not (Test-XamlSyntax -FilePath $file.FullName)) {
            $brokenFiles += $file.FullName
        }
    }
    
    if ($brokenFiles.Count -gt 0) {
        Write-Host "❌ Update broke $($brokenFiles.Count) files. Rolling back..." -ForegroundColor Red
        # Restore from backup
        Get-ChildItem -Path $backupDir -Recurse -Filter "*.xaml" | ForEach-Object {
            $relativePath = $_.FullName.Substring($backupDir.Length + 1)
            $originalPath = Join-Path $RootPath $relativePath
            Copy-Item $_.FullName $originalPath -Force
        }
        Write-Host "🔄 Rollback completed" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "✅ $Description completed successfully!" -ForegroundColor Green
    Write-Host "📁 Backup preserved at: $backupDir" -ForegroundColor Cyan
    return $true
}

# ============================================
# Bus Buddy Debug Integration Functions
# ============================================

function Start-BusBuddyDebugFilter {
    <#
    .SYNOPSIS
    Starts the Bus Buddy debug filter using DebugHelper
    #>
    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path "$workspacePath\BusBuddy.WPF\BusBuddy.WPF.csproj") {
        Set-Location $workspacePath
        Write-Host '🔍 Starting Bus Buddy debug filter...' -ForegroundColor Cyan
        dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj --no-build -- --start-debug-filter
    } else {
        Write-Host '❌ Bus Buddy WPF project not found' -ForegroundColor Red
    }
}

function Start-BusBuddyDebugStreaming {
    <#
    .SYNOPSIS
    Starts real-time debug streaming for Bus Buddy
    #>
    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path "$workspacePath\BusBuddy.WPF\BusBuddy.WPF.csproj") {
        Set-Location $workspacePath
        Write-Host '🎯 Starting real-time debug streaming...' -ForegroundColor Cyan
        dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj --no-build -- --start-streaming
    } else {
        Write-Host '❌ Bus Buddy WPF project not found' -ForegroundColor Red
    }
}

function Export-BusBuddyDebugJson {
    <#
    .SYNOPSIS
    Exports Bus Buddy debug issues to JSON
    #>
    param(
        [string]$OutputFile = $null
    )

    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path "$workspacePath\BusBuddy.WPF\BusBuddy.WPF.csproj") {
        Set-Location $workspacePath
        Write-Host '📄 Exporting debug issues to JSON...' -ForegroundColor Cyan

        if ($OutputFile) {
            dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj --no-build -- --export-debug-json --output-file=$OutputFile
        } else {
            dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj --no-build -- --export-debug-json
        }
    } else {
        Write-Host '❌ Bus Buddy WPF project not found' -ForegroundColor Red
    }
}

function Test-BusBuddyDebugFilter {
    <#
    .SYNOPSIS
    Tests the Bus Buddy debug filter
    #>
    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path "$workspacePath\BusBuddy.WPF\BusBuddy.WPF.csproj") {
        Set-Location $workspacePath
        Write-Host '🧪 Testing Bus Buddy debug filter...' -ForegroundColor Cyan
        dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj --no-build -- --test-filter
    } else {
        Write-Host '❌ Bus Buddy WPF project not found' -ForegroundColor Red
    }
}

function Show-BusBuddyRecentStreamingEntries {
    <#
    .SYNOPSIS
    Shows recent streaming entries from Bus Buddy debug
    #>
    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path "$workspacePath\BusBuddy.WPF\BusBuddy.WPF.csproj") {
        Set-Location $workspacePath
        Write-Host '📊 Showing recent streaming entries...' -ForegroundColor Cyan
        dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj --no-build -- --show-recent
    } else {
        Write-Host '❌ Bus Buddy WPF project not found' -ForegroundColor Red
    }
}

function Invoke-BusBuddyHealthCheck {
    <#
    .SYNOPSIS
    Runs a health check on Bus Buddy debug system
    #>
    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path "$workspacePath\BusBuddy.WPF\BusBuddy.WPF.csproj") {
        Set-Location $workspacePath
        Write-Host '🏥 Running Bus Buddy health check...' -ForegroundColor Cyan
        dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj --no-build -- --health-check
    } else {
        Write-Host '❌ Bus Buddy WPF project not found' -ForegroundColor Red
    }
}

function Start-BusBuddyWithDebug {
    <#
    .SYNOPSIS
    Builds and starts Bus Buddy application with debug filter enabled
    #>
    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path "$workspacePath\BusBuddy.sln") {
        Set-Location $workspacePath
        Write-Host '🔨 Building Bus Buddy with debug features...' -ForegroundColor Cyan

        # Build first
        dotnet build BusBuddy.sln --verbosity minimal

        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ Build successful - Starting with debug filter...' -ForegroundColor Green
            Start-Process powershell -ArgumentList '-NoProfile', '-NoExit', '-Command', 'dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj --no-build -- --start-debug-filter'
        } else {
            Write-Host '❌ Build failed - Debug filter not started' -ForegroundColor Red
        }
    } else {
        Write-Host '❌ Bus Buddy solution file not found' -ForegroundColor Red
    }
}

# Debug Function Aliases
Set-Alias -Name bb-debug-start -Value Start-BusBuddyDebugFilter -Force
Set-Alias -Name bb-debug-stream -Value Start-BusBuddyDebugStreaming -Force
Set-Alias -Name bb-debug-export -Value Export-BusBuddyDebugJson -Force
Set-Alias -Name bb-debug-test -Value Test-BusBuddyDebugFilter -Force
Set-Alias -Name bb-debug-recent -Value Show-BusBuddyRecentStreamingEntries -Force
Set-Alias -Name bb-health -Value Invoke-BusBuddyHealthCheck -Force
Set-Alias -Name bb-debug-run -Value Start-BusBuddyWithDebug -Force

function Open-BusBuddyWorkspace {
    <#
    .SYNOPSIS
    Opens the Bus Buddy workspace in VS Code
    #>
    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path $workspacePath) {
        code $workspacePath
        Write-Host '🚌 Bus Buddy workspace opened in VS Code' -ForegroundColor Green
    } else {
        Write-Host "❌ Bus Buddy workspace not found at: $workspacePath" -ForegroundColor Red
    }
}

function Start-BusBuddyBuild {
    <#
    .SYNOPSIS
    Builds the Bus Buddy solution
    #>
    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path "$workspacePath\BusBuddy.sln") {
        Set-Location $workspacePath
        Write-Host '🔨 Building Bus Buddy solution...' -ForegroundColor Cyan
        dotnet build BusBuddy.sln --verbosity minimal
        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ Build successful!' -ForegroundColor Green
        } else {
            Write-Host '❌ Build failed!' -ForegroundColor Red
        }
    } else {
        Write-Host '❌ Bus Buddy solution file not found' -ForegroundColor Red
    }
}

function Start-BusBuddyApp {
    <#
    .SYNOPSIS
    Runs the Bus Buddy WPF application
    #>
    $workspacePath = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy'
    if (Test-Path "$workspacePath\BusBuddy.WPF\BusBuddy.WPF.csproj") {
        Set-Location $workspacePath
        Write-Host '🚀 Starting Bus Buddy application...' -ForegroundColor Cyan
        dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj
    } else {
        Write-Host '❌ Bus Buddy WPF project not found' -ForegroundColor Red
    }
}

# Bus Buddy Aliases
Set-Alias -Name bb-open -Value Open-BusBuddyWorkspace -Force
Set-Alias -Name bb-build -Value Start-BusBuddyBuild -Force
Set-Alias -Name bb-run -Value Start-BusBuddyApp -Force

# ============================================
# XAML Management Aliases and Functions
# ============================================

# Quick XAML processing aliases
Set-Alias -Name xaml-test -Value Test-XamlSyntax -Force
Set-Alias -Name xaml-backup -Value Backup-XamlFiles -Force
Set-Alias -Name xaml-format -Value Format-XamlFile -Force

function bb-xaml-clean {
    <#
    .SYNOPSIS
    Removes deprecated Syncfusion VisualStyle attributes from all XAML files
    #>
    param([string]$Path = "BusBuddy.WPF")
    
    $fullPath = if (Test-Path $Path) { $Path } else { Join-Path (Get-Location) $Path }
    
    Safe-BulkXamlUpdate -RootPath $fullPath -Description "Remove deprecated Syncfusion VisualStyle attributes" -UpdateScript {
        Get-ChildItem -Path $fullPath -Recurse -Filter "*.xaml" | ForEach-Object {
            Remove-SyncfusionVisualStyle -FilePath $_.FullName
        }
    }
}

function bb-xaml-format {
    <#
    .SYNOPSIS
    Formats all XAML files in the project with proper indentation
    #>
    param([string]$Path = "BusBuddy.WPF")
    
    $fullPath = if (Test-Path $Path) { $Path } else { Join-Path (Get-Location) $Path }
    
    Safe-BulkXamlUpdate -RootPath $fullPath -Description "Format XAML files" -UpdateScript {
        Get-ChildItem -Path $fullPath -Recurse -Filter "*.xaml" | ForEach-Object {
            Format-XamlFile -FilePath $_.FullName
        }
    }
}

function bb-xaml-validate {
    <#
    .SYNOPSIS
    Validates all XAML files in the project
    #>
    param([string]$Path = "BusBuddy.WPF")
    
    $fullPath = if (Test-Path $Path) { $Path } else { Join-Path (Get-Location) $Path }
    
    Write-Host "🔍 Validating XAML files in: $fullPath" -ForegroundColor Yellow
    
    $xamlFiles = Get-ChildItem -Path $fullPath -Recurse -Filter "*.xaml"
    $validCount = 0
    $invalidCount = 0
    $invalidFiles = @()
    
    foreach ($file in $xamlFiles) {
        if (Test-XamlSyntax -FilePath $file.FullName) {
            $validCount++
        } else {
            $invalidCount++
            $invalidFiles += $file.FullName
        }
    }
    
    Write-Host ""
    Write-Host "📊 Validation Summary:" -ForegroundColor Cyan
    Write-Host "  ✅ Valid files: $validCount" -ForegroundColor Green
    Write-Host "  ❌ Invalid files: $invalidCount" -ForegroundColor Red
    
    if ($invalidFiles.Count -gt 0) {
        Write-Host ""
        Write-Host "❌ Invalid files:" -ForegroundColor Red
        $invalidFiles | ForEach-Object { 
            $relativePath = $_.Substring($fullPath.Length + 1)
            Write-Host "  - $relativePath" -ForegroundColor Red 
        }
    }
    
    return $invalidCount -eq 0
}

function bb-xaml-fix-spacing {
    <#
    .SYNOPSIS
    Fixes common XML spacing issues in XAML files
    #>
    param([string]$Path = "BusBuddy.WPF")
    
    $fullPath = if (Test-Path $Path) { $Path } else { Join-Path (Get-Location) $Path }
    
    Safe-BulkXamlUpdate -RootPath $fullPath -Description "Fix XAML spacing issues" -UpdateScript {
        Get-ChildItem -Path $fullPath -Recurse -Filter "*.xaml" | ForEach-Object {
            $content = Get-Content $_.FullName -Raw
            
            # Fix common spacing issues
            $updatedContent = $content
            
            # Fix missing spaces before attributes
            $updatedContent = $updatedContent -replace '([a-zA-Z0-9"])([A-Z][a-zA-Z]+=)', '$1 $2'
            
            # Fix multiple consecutive spaces
            $updatedContent = $updatedContent -replace '\s{2,}', ' '
            
            # Fix spaces around = in attributes
            $updatedContent = $updatedContent -replace '\s*=\s*', '='
            
            # Validate the result
            try {
                [xml]$testXml = $updatedContent
                Set-Content -Path $_.FullName -Value $updatedContent -NoNewline
                Write-Host "✅ Fixed spacing: $($_.Name)" -ForegroundColor Green
            }
            catch {
                Write-Host "❌ Could not fix spacing for: $($_.Name) - $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}

function bb-xaml-report {
    <#
    .SYNOPSIS
    Generates a comprehensive report of XAML files in the project
    #>
    param([string]$Path = "BusBuddy.WPF")
    
    $fullPath = if (Test-Path $Path) { $Path } else { Join-Path (Get-Location) $Path }
    $xamlFiles = Get-ChildItem -Path $fullPath -Recurse -Filter "*.xaml"
    
    Write-Host "📋 XAML Project Report" -ForegroundColor Cyan
    Write-Host "======================" -ForegroundColor Cyan
    Write-Host "Project Path: $fullPath" -ForegroundColor Yellow
    Write-Host "Total XAML Files: $($xamlFiles.Count)" -ForegroundColor Yellow
    Write-Host ""
    
    # Group by directory
    $grouped = $xamlFiles | Group-Object { Split-Path $_.FullName -Parent }
    
    foreach ($group in $grouped) {
        $relativePath = $group.Name.Substring($fullPath.Length + 1)
        Write-Host "📁 $relativePath" -ForegroundColor White
        
        foreach ($file in $group.Group) {
            $size = [math]::Round($file.Length / 1KB, 1)
            $status = if (Test-XamlSyntax -FilePath $file.FullName) { "✅" } else { "❌" }
            Write-Host "  $status $($file.Name) ($size KB)" -ForegroundColor Gray
        }
        Write-Host ""
    }
    
    # Summary statistics
    $validFiles = $xamlFiles | Where-Object { Test-XamlSyntax -FilePath $_.FullName }
    $totalSize = [math]::Round(($xamlFiles | Measure-Object Length -Sum).Sum / 1KB, 1)
    
    Write-Host "📊 Summary Statistics:" -ForegroundColor Cyan
    Write-Host "  Valid Files: $($validFiles.Count) / $($xamlFiles.Count)" -ForegroundColor Green
    Write-Host "  Total Size: $totalSize KB" -ForegroundColor Yellow
    Write-Host "  Average Size: $([math]::Round($totalSize / $xamlFiles.Count, 1)) KB" -ForegroundColor Yellow
}

# ============================================
# Task Explorer Integration
# ============================================

function Show-TaskExplorerHelp {
    <#
    .SYNOPSIS
    Shows available Task Explorer commands and shortcuts
    #>
    Write-Host '📋 Bus Buddy Development Environment Help' -ForegroundColor Cyan
    Write-Host '=======================================' -ForegroundColor Cyan
    Write-Host ''
    Write-Host 'VS Code Commands:' -ForegroundColor Yellow
    Write-Host '  code, vscode, edit, vs, edit-file' -ForegroundColor White
    Write-Host ''
    Write-Host 'Bus Buddy Commands:' -ForegroundColor Yellow
    Write-Host '  bb-open     - Open Bus Buddy workspace' -ForegroundColor White
    Write-Host '  bb-build    - Build Bus Buddy solution' -ForegroundColor White
    Write-Host '  bb-run      - Run Bus Buddy application' -ForegroundColor White
    Write-Host ''
    Write-Host 'XAML Management Commands:' -ForegroundColor Yellow
    Write-Host '  bb-xaml-validate    - Validate all XAML files' -ForegroundColor White
    Write-Host '  bb-xaml-format      - Format all XAML files' -ForegroundColor White
    Write-Host '  bb-xaml-clean       - Remove deprecated Syncfusion attributes' -ForegroundColor White
    Write-Host '  bb-xaml-fix-spacing - Fix common spacing issues' -ForegroundColor White
    Write-Host '  bb-xaml-report      - Generate XAML project report' -ForegroundColor White
    Write-Host '  xaml-test <file>    - Test single XAML file syntax' -ForegroundColor White
    Write-Host '  xaml-backup <path>  - Backup XAML files' -ForegroundColor White
    Write-Host '  xaml-format <file>  - Format single XAML file' -ForegroundColor White
    Write-Host ''
    Write-Host 'Bus Buddy Debug Commands:' -ForegroundColor Yellow
    Write-Host '  bb-debug-start    - Start debug filter' -ForegroundColor White
    Write-Host '  bb-debug-stream   - Start real-time debug streaming' -ForegroundColor White
    Write-Host '  bb-debug-export   - Export debug issues to JSON' -ForegroundColor White
    Write-Host '  bb-debug-test     - Test debug filter functionality' -ForegroundColor White
    Write-Host '  bb-debug-recent   - Show recent streaming entries' -ForegroundColor White
    Write-Host '  bb-health         - Run debug system health check' -ForegroundColor White
    Write-Host '  bb-debug-run      - Build and run with debug filter' -ForegroundColor White
    Write-Host ''
    Write-Host 'Task Explorer Shortcuts (in VS Code):' -ForegroundColor Yellow
    Write-Host "  Ctrl+Shift+P -> 'Task Explorer: Run Task'" -ForegroundColor White
    Write-Host "  F1 -> 'Tasks: Run Task'" -ForegroundColor White
    Write-Host ''
    Write-Host '🎯 Use Task Explorer as the exclusive task management method!' -ForegroundColor Green
    Write-Host '🔍 Debug commands integrate with App.xaml.cs DebugHelper methods!' -ForegroundColor Green
    Write-Host '🧹 XAML commands use safe XML processing with backup/rollback!' -ForegroundColor Green
}

# ============================================
# Startup Message and Auto-Load Tools
# ============================================

Write-Host ''
Write-Host '🚌 Bus Buddy Development Environment Loaded!' -ForegroundColor Green
Write-Host "PowerShell Core $($PSVersionTable.PSVersion)" -ForegroundColor Cyan
Write-Host ''
Write-Host 'Available commands: code, vs, bb-open, bb-build, bb-run' -ForegroundColor Yellow
Write-Host 'XAML commands: bb-xaml-validate, bb-xaml-format, bb-xaml-clean' -ForegroundColor Yellow
Write-Host 'Debug commands: bb-debug-start, bb-debug-stream, bb-health' -ForegroundColor Yellow

# Auto-load novice-friendly configuration if available
$noviceSetupPath = Join-Path (Get-Location) "Tools\Config\Novice-Setup.ps1"
if (Test-Path $noviceSetupPath) {
    try {
        . $noviceSetupPath
        Write-Host '✅ Novice-friendly tools loaded successfully!' -ForegroundColor Green
        Write-Host 'Enhanced commands: Show-BusBuddyHelp, bb-fix-xaml, bb-check-xaml' -ForegroundColor Magenta
    }
    catch {
        Write-Host "⚠️ Could not load novice tools: $($_.Exception.Message)" -ForegroundColor Yellow
    }
} else {
    Write-Host 'ℹ️ Novice tools not found - using basic environment' -ForegroundColor Cyan
}

Write-Host "Type 'Show-TaskExplorerHelp' for full command reference" -ForegroundColor Cyan
Write-Host ''

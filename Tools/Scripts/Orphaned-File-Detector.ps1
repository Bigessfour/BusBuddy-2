#Requires -Version 7.5

<#
.SYNOPSIS
    Comprehensive orphaned file detection and cleanup system for Bus Buddy project
.DESCRIPTION
    Detects, analyzes, and optionally removes orphaned files in the Bus Buddy project.
    Includes dependency analysis and file system reorganization suggestions.
.PARAMETER WorkspaceRoot
    Root directory of the Bus Buddy workspace
.PARAMETER Action
    Action to perform: 'Analyze', 'Delete', 'Reorganize', or 'All'
.PARAMETER DryRun
    Show what would be done without actually making changes
.PARAMETER UpdateDependencies
    Update project references after reorganization
.EXAMPLE
    .\Orphaned-File-Detector.ps1 -WorkspaceRoot "." -Action Analyze -DryRun
.EXAMPLE
    .\Orphaned-File-Detector.ps1 -WorkspaceRoot "." -Action All -UpdateDependencies
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$WorkspaceRoot = ".",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Analyze", "Delete", "Reorganize", "All")]
    [string]$Action = "Analyze",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$UpdateDependencies
)

# Enhanced error handling and logging
$ErrorActionPreference = 'Stop'
$VerbosePreference = 'Continue'

# Color-coded output functions
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "⚠️ $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "ℹ️ $Message" -ForegroundColor Cyan }
function Write-Progress { param($Message) Write-Host "🔄 $Message" -ForegroundColor Blue }

# Define project structure standards based on coding instructions
$ProjectStructure = @{
    'BusBuddy.Core' = @{
        'Configuration' = @('*Settings.cs', '*Configuration.cs', '*Helper.cs')
        'Data' = @('*Context.cs', '*Factory.cs', '*Migration.cs', 'Interfaces/*.cs', 'Repositories/*.cs', 'UnitOfWork/*.cs')
        'Extensions' = @('*Extensions.cs')
        'Interceptors' = @('*Interceptor.cs')
        'Logging' = @('*Logger*.cs', '*Logging*.cs')
        'Models' = @('*.cs')
        'Services' = @('*Service.cs', 'Interfaces/*Service.cs')
        'Utilities' = @('*Utility.cs', '*Helper.cs', '*Manager.cs')
    }
    'BusBuddy.WPF' = @{
        'Assets' = @('*.png', '*.jpg', '*.ico', '*.ttf', '*.woff', '*.svg')
        'Controls' = @('*Control.xaml', '*Control.xaml.cs', '*Template.xaml')
        'Converters' = @('*Converter.cs', '*Converter.xaml')
        'Extensions' = @('*Extensions.cs')
        'Logging' = @('*Logger*.cs', '*Logging*.cs', '*Enricher*.cs')
        'Models' = @('*Model.cs', '*DTO.cs', '*ViewModel.cs')
        'Resources' = @('*.xaml', '*.resx')
        'Services' = @('*Service.cs', 'Interfaces/*Service.cs')
        'Utilities' = @('*Utility.cs', '*Helper.cs', '*Manager.cs', '*Optimizer.cs')
        'ViewModels' = @('*ViewModel.cs', '*/*ViewModel.cs')
        'Views' = @('*View.xaml', '*View.xaml.cs', '*/*View.xaml', '*/*View.xaml.cs')
    }
    'BusBuddy.Tests' = @{
        'ViewModels' = @('*Tests.cs', '*Test.cs')
        'Services' = @('*Tests.cs', '*Test.cs')
        'Models' = @('*Tests.cs', '*Test.cs')
    }
}

# Known orphaned file patterns
$OrphanedPatterns = @(
    # Temporary and backup files
    '*.tmp', '*.temp', '*.bak', '*.backup', '*.old', '*~'
    # Visual Studio artifacts
    '*.user', '*.suo', '*.cache', '*.ide', '*.ide-*'
    # Build artifacts in wrong locations
    '*.dll', '*.exe', '*.pdb', '*.xml' # when not in bin/obj
    # Old/abandoned files
    '*_old.*', '*_backup.*', '*_temp.*', '*_copy.*'
    # Duplicate files
    '*Copy.*', '*copy.*', '* - Copy.*'
    # Log files in wrong locations
    '*.log' # when not in logs/ directory
    # Package artifacts
    'packages.config', '*.nupkg'
)

# Files that should be moved to proper locations
$FileMoveRules = @{
    # Configuration files
    '*.json' = @{
        'appsettings*.json' = 'Configuration/'
        'global.json' = 'Root'
        'nuget.config' = 'Root'
    }
    # Documentation
    '*.md' = 'Documentation/'
    # Scripts
    '*.ps1' = 'Tools/Scripts/'
    # License and legal
    'LICENSE*' = 'Root'
    'SECURITY*' = 'Root'
    # Git and CI/CD
    '.git*' = 'Root'
    '.vscode/*' = 'Root'
    '.github/*' = 'Root'
}

class OrphanedFile {
    [string]$Path
    [string]$Name
    [string]$Reason
    [string]$RecommendedAction
    [string]$SuggestedLocation
    [long]$Size
    [DateTime]$LastModified
    [bool]$IsReferenced
    [string[]]$References
}

class ProjectAnalysis {
    [OrphanedFile[]]$OrphanedFiles
    [hashtable]$FileReferences
    [hashtable]$DependencyMap
    [string[]]$Errors
    [string[]]$Warnings
    [hashtable]$Statistics
}

function Get-ProjectFiles {
    param([string]$RootPath)

    Write-Progress "Scanning project files..."

    $allFiles = Get-ChildItem -Path $RootPath -Recurse -File | Where-Object {
        $_.FullName -notmatch '\\(bin|obj|packages|node_modules|\\.git|\\.vs)\\' -and
        $_.Name -notmatch '^(\\.git|thumbs\\.db|desktop\\.ini)$'
    }

    return $allFiles
}

function Find-FileReferences {
    param(
        [System.IO.FileInfo[]]$Files,
        [string]$TargetFile
    )

    $references = @()
    $targetFileName = [System.IO.Path]::GetFileName($TargetFile)
    $targetFileNameWithoutExt = [System.IO.Path]::GetFileNameWithoutExtension($TargetFile)

    # Search in project files
    $projectFiles = $Files | Where-Object { $_.Extension -match '\.(cs|xaml|csproj|sln|json|config)$' }

    foreach ($file in $projectFiles) {
        try {
            $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
            if ($null -eq $content) { continue }

            # Check for various reference patterns
            $patterns = @(
                $targetFileName,
                $targetFileNameWithoutExt,
                $TargetFile.Replace('\', '/'),
                $TargetFile.Replace('/', '\')
            )

            foreach ($pattern in $patterns) {
                if ($content -match [regex]::Escape($pattern)) {
                    $references += $file.FullName
                    break
                }
            }
        }
        catch {
            # Skip files that can't be read
        }
    }

    return $references
}

function Test-IsOrphaned {
    param(
        [System.IO.FileInfo]$File,
        [System.IO.FileInfo[]]$AllFiles
    )

    $relativePath = $File.FullName.Replace($WorkspaceRoot, '').TrimStart('\', '/')
    $fileName = $File.Name
    $extension = $File.Extension.ToLower()

    # Check if file matches known orphaned patterns
    foreach ($pattern in $OrphanedPatterns) {
        if ($fileName -like $pattern) {
            return @{
                IsOrphaned = $true
                Reason = "Matches orphaned pattern: $pattern"
                Action = "Delete"
            }
        }
    }

    # Check if file is in wrong location based on project structure
    $projectName = $relativePath.Split('\')[0]
    if ($ProjectStructure.ContainsKey($projectName)) {
        $isInCorrectLocation = $false
        $suggestedLocation = ""

        foreach ($folder in $ProjectStructure[$projectName].Keys) {
            $patterns = $ProjectStructure[$projectName][$folder]
            foreach ($pattern in $patterns) {
                if ($fileName -like $pattern) {
                    $expectedPath = "$projectName\$folder"
                    if ($relativePath -like "$expectedPath\*") {
                        $isInCorrectLocation = $true
                        break
                    } else {
                        $suggestedLocation = $expectedPath
                    }
                }
            }
            if ($isInCorrectLocation) { break }
        }

        if (-not $isInCorrectLocation -and $suggestedLocation) {
            return @{
                IsOrphaned = $true
                Reason = "File in wrong location for project structure"
                Action = "Move"
                SuggestedLocation = $suggestedLocation
            }
        }
    }

    # Check for duplicate files
    $sameNameFiles = $AllFiles | Where-Object { $_.Name -eq $fileName -and $_.FullName -ne $File.FullName }
    if ($sameNameFiles.Count -gt 0) {
        # Check if this appears to be a backup/copy
        if ($fileName -match '(copy|backup|old|temp|\d+)' -or $File.Directory.Name -match '(backup|old|temp)') {
            return @{
                IsOrphaned = $true
                Reason = "Appears to be duplicate/backup file"
                Action = "Delete"
            }
        }
    }

    # Check if file has no references (for code files)
    if ($extension -in @('.cs', '.xaml') -and $fileName -notmatch '(App|AssemblyInfo|GlobalSuppressions)') {
        $references = Find-FileReferences -Files $AllFiles -TargetFile $relativePath
        if ($references.Count -eq 0) {
            return @{
                IsOrphaned = $true
                Reason = "No references found in project"
                Action = "Review" # Don't auto-delete, needs manual review
            }
        }
    }

    return @{
        IsOrphaned = $false
        Reason = "File appears to be properly placed and referenced"
        Action = "Keep"
    }
}

function Analyze-OrphanedFiles {
    param([string]$RootPath)

    Write-Info "Starting comprehensive orphaned file analysis..."

    $analysis = [ProjectAnalysis]::new()
    $analysis.OrphanedFiles = @()
    $analysis.FileReferences = @{}
    $analysis.DependencyMap = @{}
    $analysis.Errors = @()
    $analysis.Warnings = @()
    $analysis.Statistics = @{}

    try {
        # Get all project files
        $allFiles = Get-ProjectFiles -RootPath $RootPath
        Write-Info "Found $($allFiles.Count) files to analyze"

        # Analyze each file
        $orphanedCount = 0
        $processedCount = 0

        foreach ($file in $allFiles) {
            $processedCount++
            if ($processedCount % 50 -eq 0) {
                Write-Progress "Analyzing file $processedCount of $($allFiles.Count)..."
            }

            try {
                $orphanedCheck = Test-IsOrphaned -File $file -AllFiles $allFiles

                if ($orphanedCheck.IsOrphaned) {
                    $orphanedCount++

                    $orphanedFile = [OrphanedFile]::new()
                    $orphanedFile.Path = $file.FullName
                    $orphanedFile.Name = $file.Name
                    $orphanedFile.Reason = $orphanedCheck.Reason
                    $orphanedFile.RecommendedAction = $orphanedCheck.Action
                    $orphanedFile.SuggestedLocation = $orphanedCheck.SuggestedLocation
                    $orphanedFile.Size = $file.Length
                    $orphanedFile.LastModified = $file.LastWriteTime

                    # Find references for this file
                    $relativePath = $file.FullName.Replace($RootPath, '').TrimStart('\', '/')
                    $references = Find-FileReferences -Files $allFiles -TargetFile $relativePath
                    $orphanedFile.References = $references
                    $orphanedFile.IsReferenced = $references.Count -gt 0

                    $analysis.OrphanedFiles += $orphanedFile
                }
            }
            catch {
                $analysis.Errors += "Error analyzing file $($file.FullName): $($_.Exception.Message)"
            }
        }

        # Generate statistics
        $analysis.Statistics = @{
            TotalFiles = $allFiles.Count
            OrphanedFiles = $orphanedCount
            FilesToDelete = ($analysis.OrphanedFiles | Where-Object { $_.RecommendedAction -eq 'Delete' }).Count
            FilesToMove = ($analysis.OrphanedFiles | Where-Object { $_.RecommendedAction -eq 'Move' }).Count
            FilesToReview = ($analysis.OrphanedFiles | Where-Object { $_.RecommendedAction -eq 'Review' }).Count
            TotalSizeToClean = ($analysis.OrphanedFiles | Measure-Object -Property Size -Sum).Sum
        }

        Write-Success "Analysis completed: Found $orphanedCount orphaned files out of $($allFiles.Count) total files"

    }
    catch {
        $analysis.Errors += "Critical error during analysis: $($_.Exception.Message)"
        Write-Error "Analysis failed: $($_.Exception.Message)"
    }

    return $analysis
}

function Show-AnalysisResults {
    param([ProjectAnalysis]$Analysis)

    Write-Info "=== ORPHANED FILE ANALYSIS RESULTS ==="
    Write-Host ""

    # Statistics
    Write-Info "📊 STATISTICS:"
    Write-Host "  Total Files Analyzed: $($Analysis.Statistics.TotalFiles)" -ForegroundColor White
    Write-Host "  Orphaned Files Found: $($Analysis.Statistics.OrphanedFiles)" -ForegroundColor Yellow
    Write-Host "  Files to Delete: $($Analysis.Statistics.FilesToDelete)" -ForegroundColor Red
    Write-Host "  Files to Move: $($Analysis.Statistics.FilesToMove)" -ForegroundColor Blue
    Write-Host "  Files to Review: $($Analysis.Statistics.FilesToReview)" -ForegroundColor Magenta
    Write-Host "  Total Size to Clean: $([math]::Round($Analysis.Statistics.TotalSizeToClean / 1KB, 2)) KB" -ForegroundColor Green
    Write-Host ""

    # Group by action
    $byAction = $Analysis.OrphanedFiles | Group-Object RecommendedAction

    foreach ($group in $byAction) {
        $actionColor = switch ($group.Name) {
            'Delete' { 'Red' }
            'Move' { 'Blue' }
            'Review' { 'Magenta' }
            default { 'White' }
        }

        Write-Host "🔍 FILES TO $($group.Name.ToUpper()):" -ForegroundColor $actionColor
        foreach ($file in $group.Group) {
            $relPath = $file.Path.Replace($WorkspaceRoot, '.').Replace('\', '/')
            Write-Host "  $relPath" -ForegroundColor Gray
            Write-Host "    Reason: $($file.Reason)" -ForegroundColor DarkGray
            if ($file.SuggestedLocation) {
                Write-Host "    Suggested: $($file.SuggestedLocation)" -ForegroundColor DarkCyan
            }
            if ($file.IsReferenced) {
                Write-Host "    ⚠️ Has $($file.References.Count) reference(s)" -ForegroundColor Yellow
            }
        }
        Write-Host ""
    }

    # Show errors and warnings
    if ($Analysis.Errors.Count -gt 0) {
        Write-Error "❌ ERRORS ENCOUNTERED:"
        foreach ($error in $Analysis.Errors) {
            Write-Host "  $error" -ForegroundColor Red
        }
        Write-Host ""
    }

    if ($Analysis.Warnings.Count -gt 0) {
        Write-Warning "⚠️ WARNINGS:"
        foreach ($warning in $Analysis.Warnings) {
            Write-Host "  $warning" -ForegroundColor Yellow
        }
        Write-Host ""
    }
}

function Remove-OrphanedFiles {
    param(
        [ProjectAnalysis]$Analysis,
        [bool]$DryRunMode = $true
    )

    $filesToDelete = $Analysis.OrphanedFiles | Where-Object { $_.RecommendedAction -eq 'Delete' -and -not $_.IsReferenced }

    if ($filesToDelete.Count -eq 0) {
        Write-Info "No files safe to delete automatically"
        return
    }

    Write-Info "=== DELETING ORPHANED FILES ==="
    Write-Host ""

    $deletedCount = 0
    $freedSpace = 0

    foreach ($file in $filesToDelete) {
        $relPath = $file.Path.Replace($WorkspaceRoot, '.').Replace('\', '/')

        if ($DryRunMode) {
            Write-Host "[DRY RUN] Would delete: $relPath" -ForegroundColor Yellow
        } else {
            try {
                Remove-Item -Path $file.Path -Force
                Write-Success "Deleted: $relPath"
                $deletedCount++
                $freedSpace += $file.Size
            }
            catch {
                Write-Error "Failed to delete $relPath : $($_.Exception.Message)"
            }
        }
    }

    if (-not $DryRunMode) {
        Write-Success "Deleted $deletedCount files, freed $([math]::Round($freedSpace / 1KB, 2)) KB"
    } else {
        Write-Info "DRY RUN: Would delete $($filesToDelete.Count) files, freeing $([math]::Round(($filesToDelete | Measure-Object -Property Size -Sum).Sum / 1KB, 2)) KB"
    }
}

function Move-FilesToCorrectLocations {
    param(
        [ProjectAnalysis]$Analysis,
        [bool]$DryRunMode = $true
    )

    $filesToMove = $Analysis.OrphanedFiles | Where-Object { $_.RecommendedAction -eq 'Move' -and $_.SuggestedLocation }

    if ($filesToMove.Count -eq 0) {
        Write-Info "No files need to be moved"
        return
    }

    Write-Info "=== REORGANIZING FILE STRUCTURE ==="
    Write-Host ""

    $movedCount = 0

    foreach ($file in $filesToMove) {
        $sourcePath = $file.Path
        $targetDir = Join-Path $WorkspaceRoot $file.SuggestedLocation
        $targetPath = Join-Path $targetDir $file.Name

        $relSource = $sourcePath.Replace($WorkspaceRoot, '.').Replace('\', '/')
        $relTarget = $targetPath.Replace($WorkspaceRoot, '.').Replace('\', '/')

        if ($DryRunMode) {
            Write-Host "[DRY RUN] Would move: $relSource" -ForegroundColor Yellow
            Write-Host "                  to: $relTarget" -ForegroundColor Cyan
        } else {
            try {
                # Create target directory if it doesn't exist
                if (-not (Test-Path $targetDir)) {
                    New-Item -Path $targetDir -ItemType Directory -Force | Out-Null
                }

                # Move the file
                Move-Item -Path $sourcePath -Destination $targetPath -Force
                Write-Success "Moved: $relSource"
                Write-Host "    to: $relTarget" -ForegroundColor Green
                $movedCount++

                # Track the move for dependency updates
                $Analysis.DependencyMap[$sourcePath] = $targetPath
            }
            catch {
                Write-Error "Failed to move $relSource : $($_.Exception.Message)"
            }
        }
    }

    if (-not $DryRunMode) {
        Write-Success "Moved $movedCount files to correct locations"
    } else {
        Write-Info "DRY RUN: Would move $($filesToMove.Count) files"
    }
}

function Update-ProjectDependencies {
    param(
        [ProjectAnalysis]$Analysis,
        [bool]$DryRunMode = $true
    )

    if ($Analysis.DependencyMap.Count -eq 0) {
        Write-Info "No dependency updates needed"
        return
    }

    Write-Info "=== UPDATING PROJECT DEPENDENCIES ==="
    Write-Host ""

    # Get project files that might contain references
    $projectFiles = Get-ChildItem -Path $WorkspaceRoot -Recurse -Include "*.csproj", "*.sln", "*.cs", "*.xaml", "*.json", "*.config" |
        Where-Object { $_.FullName -notmatch '\\(bin|obj|packages)\\' }

    $updatedFiles = 0

    foreach ($projectFile in $projectFiles) {
        try {
            $content = Get-Content $projectFile.FullName -Raw
            $originalContent = $content
            $hasChanges = $false

            foreach ($oldPath in $Analysis.DependencyMap.Keys) {
                $newPath = $Analysis.DependencyMap[$oldPath]

                $oldRelative = $oldPath.Replace($WorkspaceRoot, '').TrimStart('\', '/').Replace('\', '/')
                $newRelative = $newPath.Replace($WorkspaceRoot, '').TrimStart('\', '/').Replace('\', '/')

                # Update various reference patterns
                if ($content -match [regex]::Escape($oldRelative)) {
                    $content = $content -replace [regex]::Escape($oldRelative), $newRelative
                    $hasChanges = $true
                }

                if ($content -match [regex]::Escape($oldRelative.Replace('/', '\'))) {
                    $content = $content -replace [regex]::Escape($oldRelative.Replace('/', '\')), $newRelative.Replace('/', '\')
                    $hasChanges = $true
                }
            }

            if ($hasChanges) {
                $relProjectFile = $projectFile.FullName.Replace($WorkspaceRoot, '.').Replace('\', '/')

                if ($DryRunMode) {
                    Write-Host "[DRY RUN] Would update: $relProjectFile" -ForegroundColor Yellow
                } else {
                    Set-Content -Path $projectFile.FullName -Value $content -Encoding UTF8
                    Write-Success "Updated: $relProjectFile"
                    $updatedFiles++
                }
            }
        }
        catch {
            Write-Error "Failed to update $($projectFile.FullName): $($_.Exception.Message)"
        }
    }

    if (-not $DryRunMode) {
        Write-Success "Updated $updatedFiles project files"
    } else {
        Write-Info "DRY RUN: Would update dependencies in project files"
    }
}

function Export-AnalysisReport {
    param(
        [ProjectAnalysis]$Analysis,
        [string]$OutputPath
    )

    $report = @{
        Timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
        Statistics = $Analysis.Statistics
        OrphanedFiles = $Analysis.OrphanedFiles | ForEach-Object {
            @{
                Path = $_.Path.Replace($WorkspaceRoot, '.')
                Name = $_.Name
                Reason = $_.Reason
                Action = $_.RecommendedAction
                SuggestedLocation = $_.SuggestedLocation
                Size = $_.Size
                LastModified = $_.LastModified.ToString('yyyy-MM-dd HH:mm:ss')
                IsReferenced = $_.IsReferenced
                ReferenceCount = $_.References.Count
            }
        }
        Errors = $Analysis.Errors
        Warnings = $Analysis.Warnings
    }

    $report | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputPath -Encoding UTF8
    Write-Success "Analysis report exported to: $OutputPath"
}

# Main execution
try {
    $WorkspaceRoot = Resolve-Path $WorkspaceRoot
    Write-Info "Bus Buddy Orphaned File Detector"
    Write-Info "Workspace: $WorkspaceRoot"
    Write-Info "Action: $Action"
    if ($DryRun) { Write-Warning "DRY RUN MODE - No changes will be made" }
    Write-Host ""

    # Perform analysis
    $analysis = Analyze-OrphanedFiles -RootPath $WorkspaceRoot

    # Show results
    Show-AnalysisResults -Analysis $analysis

    # Export report
    $reportPath = Join-Path $WorkspaceRoot "orphaned-files-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    Export-AnalysisReport -Analysis $analysis -OutputPath $reportPath

    # Perform actions based on parameters
    switch ($Action) {
        'Delete' {
            Remove-OrphanedFiles -Analysis $analysis -DryRunMode $DryRun
        }
        'Reorganize' {
            Move-FilesToCorrectLocations -Analysis $analysis -DryRunMode $DryRun
            if ($UpdateDependencies) {
                Update-ProjectDependencies -Analysis $analysis -DryRunMode $DryRun
            }
        }
        'All' {
            Remove-OrphanedFiles -Analysis $analysis -DryRunMode $DryRun
            Move-FilesToCorrectLocations -Analysis $analysis -DryRunMode $DryRun
            if ($UpdateDependencies) {
                Update-ProjectDependencies -Analysis $analysis -DryRunMode $DryRun
            }
        }
    }

    Write-Success "Orphaned file detection completed successfully"

    if ($analysis.Statistics.OrphanedFiles -gt 0) {
        Write-Host ""
        Write-Info "NEXT STEPS:"
        Write-Host "1. Review the analysis report: $reportPath" -ForegroundColor White
        if ($DryRun) {
            Write-Host "2. Run without -DryRun to execute changes" -ForegroundColor White
        }
        Write-Host "3. Test build after file reorganization" -ForegroundColor White
        Write-Host "4. Update any remaining manual references" -ForegroundColor White
    }
}
catch {
    Write-Error "Script execution failed: $($_.Exception.Message)"
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}

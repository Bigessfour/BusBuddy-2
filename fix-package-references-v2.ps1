$ErrorActionPreference = 'Stop'

# Variables
$solutionPath = "BusBuddy.sln"
$projectFiles = @(
    "BusBuddy.Core\BusBuddy.Core.csproj",
    "BusBuddy.WPF\BusBuddy.WPF.csproj",
    "BusBuddy.Tests\BusBuddy.Tests.csproj",
    "BusBuddy.UITests\BusBuddy.UITests.csproj"
)

function ProcessProjectFile {
    param (
        [string]$projectPath,
        [bool]$createBackup = $true
    )

    Write-Host "Processing $projectPath..."

    if (-not (Test-Path $projectPath)) {
        Write-Host "  ❌ Project file not found: $projectPath" -ForegroundColor Red
        return
    }

    $backupPath = "$projectPath.bak"
    if ($createBackup) {
        Copy-Item -Path $projectPath -Destination $backupPath -Force
        Write-Host "  📄 Created backup at $backupPath" -ForegroundColor Blue
    }

    # Load XML with preserving whitespace
    $xml = New-Object System.Xml.XmlDocument
    $xml.PreserveWhitespace = $true
    $xml.Load((Resolve-Path $projectPath))

    # Find all PackageReference nodes
    $packageRefs = $xml.SelectNodes("//PackageReference")

    # Group by Include attribute to find duplicates
    $packages = @{}
    $duplicatesFound = $false

    foreach ($ref in $packageRefs) {
        $packageName = $ref.Include
        $packageVersion = $ref.Version

        if (-not $packages.ContainsKey($packageName)) {
            $packages[$packageName] = @{
                Version = $packageVersion
                Node = $ref
                Count = 1
            }
        } else {
            $duplicatesFound = $true
            $packages[$packageName].Count++

            # Keep the first occurrence and mark others for removal
            $ref.ParentNode.RemoveChild($ref) | Out-Null
            Write-Host "  🔄 Removed duplicate: $packageName v$packageVersion" -ForegroundColor Yellow
        }
    }

    # Save the changes if duplicates were found
    if ($duplicatesFound) {
        $xml.Save($projectPath)
        Write-Host "  ✅ Saved changes to $projectPath" -ForegroundColor Green
    } else {
        Write-Host "  ✅ No duplicates found in $projectPath" -ForegroundColor Green
    }

    return $duplicatesFound
}

function Main {
    Write-Host "📦 BusBuddy Package Reference Fixer v2.0" -ForegroundColor Cyan
    Write-Host "========================================"
    Write-Host "Working directory: $PWD"

    $modified = 0
    $processed = 0

    foreach ($project in $projectFiles) {
        $wasModified = ProcessProjectFile -projectPath $project -createBackup $true
        $processed++
        if ($wasModified) {
            $modified++
        }
    }

    Write-Host ""
    Write-Host "📊 Summary:"
    Write-Host "  Files processed: $processed"
    Write-Host "  Files modified: $modified"

    if ($modified -gt 0) {
        Write-Host "✅ Duplicate references removed successfully." -ForegroundColor Green
    } else {
        Write-Host "✅ No duplicate references found." -ForegroundColor Green
    }
}

# Run the main function
Main

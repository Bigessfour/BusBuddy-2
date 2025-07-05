# Bus Buddy Dependency Analyzer
# Analyzes project dependencies and identifies potential issues

param(
    [string]$ProjectPath = ".",
    [switch]$ShowDetails,
    [switch]$ExportReport
)

function Analyze-ProjectDependencies {
    param([string]$Path)

    Write-Host "Analyzing Bus Buddy Project Dependencies..." -ForegroundColor Green

    # Find all C# project files
    $projectFiles = Get-ChildItem -Path $Path -Filter "*.csproj" -Recurse

    $dependencies = @{}
    $issues = @()

    foreach ($project in $projectFiles) {
        Write-Host "Analyzing $($project.Name)..." -ForegroundColor Yellow

        $content = Get-Content $project.FullName -Raw

        # Extract package references
        $packageRefs = [regex]::Matches($content, '<PackageReference Include="([^"]+)"\s+Version="([^"]+)"')

        foreach ($match in $packageRefs) {
            $packageName = $match.Groups[1].Value
            $version = $match.Groups[2].Value

            if (-not $dependencies.ContainsKey($packageName)) {
                $dependencies[$packageName] = @()
            }

            $dependencies[$packageName] += @{
                Project = $project.Name
                Version = $version
            }
        }
    }

    # Check for version conflicts
    Write-Host "`nChecking for version conflicts..." -ForegroundColor Cyan

    foreach ($package in $dependencies.Keys) {
        $versions = $dependencies[$package] | ForEach-Object { $_.Version } | Sort-Object -Unique

        if ($versions.Count -gt 1) {
            $issues += "Version conflict for $package`: $($versions -join ', ')"
            Write-Warning "Version conflict detected: $package has versions $($versions -join ', ')"
        }
    }

    # Analyze Syncfusion dependencies specifically
    Write-Host "`nAnalyzing Syncfusion dependencies..." -ForegroundColor Cyan

    $syncfusionPackages = $dependencies.Keys | Where-Object { $_ -like "*Syncfusion*" }

    foreach ($package in $syncfusionPackages) {
        Write-Host "  $package`: $($dependencies[$package][0].Version)" -ForegroundColor White
    }

    # Generate report
    if ($ExportReport) {
        $reportPath = Join-Path $Path "DependencyAnalysisReport.txt"
        $report = @"
Bus Buddy Dependency Analysis Report
Generated: $(Get-Date)

=== PACKAGE DEPENDENCIES ===
$($dependencies.Keys | ForEach-Object { "$_`: $($dependencies[$_][0].Version)" } | Out-String)

=== ISSUES FOUND ===
$($issues | Out-String)

=== SYNCFUSION PACKAGES ===
$($syncfusionPackages | ForEach-Object { "$_`: $($dependencies[$_][0].Version)" } | Out-String)
"@

        $report | Out-File -FilePath $reportPath -Encoding UTF8
        Write-Host "Report exported to: $reportPath" -ForegroundColor Green
    }

    return @{
        Dependencies = $dependencies
        Issues = $issues
        SyncfusionPackages = $syncfusionPackages
    }
}

# Run the analysis
$result = Analyze-ProjectDependencies -Path $ProjectPath

if ($ShowDetails) {
    Write-Host "`nDETAILED DEPENDENCY REPORT:" -ForegroundColor Magenta
    $result.Dependencies | Format-Table -AutoSize
}

Write-Host "`nAnalysis complete. Found $($result.Issues.Count) potential issues." -ForegroundColor Green

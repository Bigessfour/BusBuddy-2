# Bus Buddy Code Refactoring Tools
# Smart refactoring scripts for common Bus Buddy maintenance tasks

function New-RepositoryFromTemplate {
    param(
        [string]$EntityName,
        [string]$OutputPath = "Data\Repositories"
    )

    $template = @"
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Data.Interfaces;
using Bus_Buddy.Models;

namespace Bus_Buddy.Data.Repositories;

/// <summary>
/// $EntityName-specific repository implementation
/// Extends generic repository with $EntityName-specific operations
/// </summary>
public class $($EntityName)Repository : Repository<$EntityName>, I$($EntityName)Repository
{
    public $($EntityName)Repository(BusBuddyDbContext context) : base(context)
    {
    }

    #region Async $EntityName-Specific Operations

    public async Task<IEnumerable<$EntityName>> GetActive$($EntityName)sAsync()
    {
        return await Query()
            .Where(x => x.IsActive) // Adjust property as needed
            .OrderBy(x => x.Id) // Adjust property as needed
            .ToListAsync();
    }

    // Add more $EntityName-specific methods here

    #endregion

    #region Synchronous Methods for Syncfusion Compatibility

    public IEnumerable<$EntityName> GetActive$($EntityName)s()
    {
        return GetActive$($EntityName)sAsync().GetAwaiter().GetResult();
    }

    #endregion
}
"@

    $fileName = "$($EntityName)Repository.cs"
    $fullPath = Join-Path $OutputPath $fileName

    # Create directory if it doesn't exist
    $directory = Split-Path $fullPath -Parent
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force
    }

    $template | Out-File -FilePath $fullPath -Encoding UTF8
    Write-Host "Created repository: $fullPath" -ForegroundColor Green
}

function Update-RoutePropertyReferences {
    param(
        [string]$ProjectPath = "."
    )

    Write-Host "Updating Route property references..." -ForegroundColor Yellow

    # Property mapping for Route model corrections
    $propertyMappings = @{
        'RouteId' = 'Id'
        'RouteName' = 'Name'  # Update based on actual Route model
        'Date' = 'RouteDate'  # Update based on actual Route model
        'IsActive' = 'Active' # Update based on actual Route model
    }

    $files = Get-ChildItem -Path $ProjectPath -Filter "*.cs" -Recurse |
             Where-Object { $_.Name -notlike "*Designer*" }

    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw
        $updated = $false

        foreach ($oldProp in $propertyMappings.Keys) {
            $newProp = $propertyMappings[$oldProp]
            if ($content -match "Route\.$oldProp") {
                $content = $content -replace "Route\.$oldProp", "Route.$newProp"
                $updated = $true
                Write-Host "  Updated $($file.Name): Route.$oldProp -> Route.$newProp" -ForegroundColor White
            }
        }

        if ($updated) {
            $content | Out-File -FilePath $file.FullName -Encoding UTF8 -NoNewline
        }
    }

    Write-Host "Route property update complete." -ForegroundColor Green
}

function Test-SyncfusionReferences {
    param([string]$ProjectPath = ".")

    Write-Host "Testing Syncfusion references..." -ForegroundColor Yellow

    $projectFiles = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -Recurse

    foreach ($project in $projectFiles) {
        $content = Get-Content $project.FullName -Raw

        # Check for Syncfusion package references
        $syncfusionRefs = [regex]::Matches($content, '<PackageReference Include="(Syncfusion[^"]+)"\s+Version="([^"]+)"')

        Write-Host "`nSyncfusion packages in $($project.Name):"
        foreach ($ref in $syncfusionRefs) {
            $package = $ref.Groups[1].Value
            $version = $ref.Groups[2].Value
            Write-Host "  $package`: $version" -ForegroundColor Green
        }

        # Check if version matches expected 30.1.37
        $wrongVersions = $syncfusionRefs | Where-Object { $_.Groups[2].Value -ne "30.1.37" }
        if ($wrongVersions) {
            Write-Warning "Version mismatch detected in $($project.Name)"
        }
    }
}

# Export functions for use in other scripts
Export-ModuleMember -Function New-RepositoryFromTemplate, Update-RoutePropertyReferences, Test-SyncfusionReferences

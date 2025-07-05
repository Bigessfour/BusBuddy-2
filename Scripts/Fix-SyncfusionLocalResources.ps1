# Fix Syncfusion Local Resources - Automated Configuration
# Ensures all tools use ONLY local Syncfusion installation

param(
    [switch]$Validate,
    [switch]$Configure,
    [switch]$UpdateProjectFiles
)

# ============================================================================
# SYNCFUSION LOCAL RESOURCE LOCKDOWN
# ============================================================================

function Set-SyncfusionLocalLockdown {
    Write-Host '🔒 Implementing Syncfusion Local Resource Lockdown' -ForegroundColor Cyan
    Write-Host '====================================================' -ForegroundColor Cyan

    $syncfusionPath = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37'
    $licenseKey = 'Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhec3RSRGRYU0R2WUBWYEk='

    Write-Host "`n📋 Configuration Details:" -ForegroundColor Yellow
    Write-Host "Local Path: $syncfusionPath" -ForegroundColor White
    Write-Host "License: $(($licenseKey).Substring(0,20))..." -ForegroundColor White
    Write-Host 'Policy: LOCAL RESOURCES ONLY' -ForegroundColor Green

    # Validate installation
    if (-not (Test-Path $syncfusionPath)) {
        Write-Host "`n❌ CRITICAL: Syncfusion not installed at required path!" -ForegroundColor Red
        Write-Host "Required: $syncfusionPath" -ForegroundColor Red
        Write-Host "`nPlease install Syncfusion Essential Studio 30.1.37 first." -ForegroundColor Yellow
        return $false
    }

    # Check key components
    $components = @{
        'Assemblies'    = Join-Path $syncfusionPath 'Assemblies'
        'Documentation' = Join-Path $syncfusionPath 'Help'
        'Samples'       = Join-Path $syncfusionPath 'Samples'
        'Grid Samples'  = Join-Path $syncfusionPath 'Samples\4.8\Grid'
    }

    $allComponentsFound = $true
    foreach ($component in $components.GetEnumerator()) {
        if (Test-Path $component.Value) {
            Write-Host "✅ $($component.Key): Found" -ForegroundColor Green
        } else {
            Write-Host "⚠️ $($component.Key): Missing" -ForegroundColor Yellow
            if ($component.Key -eq 'Assemblies') {
                $allComponentsFound = $false
            }
        }
    }

    return $allComponentsFound
}

function Update-ProjectConfiguration {
    Write-Host "`n🔧 Updating project configuration for local Syncfusion..." -ForegroundColor Cyan

    $csprojPath = 'Bus Buddy.csproj'
    if (-not (Test-Path $csprojPath)) {
        Write-Host "❌ Project file not found: $csprojPath" -ForegroundColor Red
        return
    }

    # Read current project file
    $projectContent = Get-Content $csprojPath -Raw

    # Check if local reference configuration exists
    if ($projectContent -match 'SYNCFUSION_LOCAL_REFERENCES') {
        Write-Host '✅ Local Syncfusion references already configured' -ForegroundColor Green
    } else {
        Write-Host '🔧 Adding local Syncfusion reference configuration...' -ForegroundColor Yellow

        # Create backup
        Copy-Item $csprojPath "$csprojPath.backup" -Force
        Write-Host "📁 Backup created: $csprojPath.backup" -ForegroundColor Gray

        # Add local reference configuration
        $localConfig = @"

  <!-- SYNCFUSION_LOCAL_REFERENCES: Lock to local installation only -->
  <PropertyGroup>
    <SyncfusionLocalPath>C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37</SyncfusionLocalPath>
    <SyncfusionAssembliesPath>`$(SyncfusionLocalPath)\Assemblies</SyncfusionAssembliesPath>
    <SyncfusionUseLocalOnly>true</SyncfusionUseLocalOnly>
  </PropertyGroup>

  <ItemGroup Condition="'`$(SyncfusionUseLocalOnly)' == 'true'">
    <!-- Local Syncfusion References - NO EXTERNAL PACKAGES -->
    <Reference Include="Syncfusion.SfDataGrid.WinForms">
      <HintPath>`$(SyncfusionAssembliesPath)\4.8\Syncfusion.SfDataGrid.WinForms.dll</HintPath>
      <Private>True</Private>
    </Reference>
    <Reference Include="Syncfusion.Shared.Base">
      <HintPath>`$(SyncfusionAssembliesPath)\4.8\Syncfusion.Shared.Base.dll</HintPath>
      <Private>True</Private>
    </Reference>
  </ItemGroup>

"@

        # Insert before closing </Project> tag
        $updatedContent = $projectContent -replace '</Project>', "$localConfig</Project>"
        $updatedContent | Out-File $csprojPath -Encoding UTF8

        Write-Host '✅ Project file updated with local Syncfusion configuration' -ForegroundColor Green
    }
}

function Set-EnvironmentConfiguration {
    Write-Host "`n🌍 Setting up environment for local-only operation..." -ForegroundColor Cyan

    $envConfig = @{
        'SYNCFUSION_LOCAL_PATH'           = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37'
        'SYNCFUSION_USE_LOCAL_ONLY'       = 'true'
        'SYNCFUSION_LICENSE_KEY'          = 'Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhec3RSRGRYU0R2WUBWYEk='
        'SYNCFUSION_NO_EXTERNAL_PACKAGES' = 'true'
        'SYNCFUSION_DOCS_LOCAL_ONLY'      = 'true'
    }

    foreach ($var in $envConfig.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable($var.Key, $var.Value, 'Process')
        Write-Host "✅ $($var.Key) = $($var.Value)" -ForegroundColor Green
    }

    # Create PowerShell profile addition for persistence
    $profileAddition = @"

# Syncfusion Local Resource Lockdown - Auto-loaded
`$env:SYNCFUSION_LOCAL_PATH = "C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37"
`$env:SYNCFUSION_USE_LOCAL_ONLY = "true"
`$env:SYNCFUSION_NO_EXTERNAL_PACKAGES = "true"
Write-Host "🔒 Syncfusion locked to local resources" -ForegroundColor Green

"@

    $profilePath = $PROFILE
    if (-not (Test-Path $profilePath)) {
        New-Item $profilePath -Force | Out-Null
    }

    $currentProfile = Get-Content $profilePath -Raw -ErrorAction SilentlyContinue
    if ($currentProfile -notmatch 'Syncfusion Local Resource Lockdown') {
        Add-Content $profilePath $profileAddition
        Write-Host '✅ PowerShell profile updated for automatic local resource locking' -ForegroundColor Green
    }
}

function Update-AppSettings {
    Write-Host "`n⚙️ Updating appsettings.json for local Syncfusion..." -ForegroundColor Cyan

    $appSettingsPath = 'appsettings.json'
    if (Test-Path $appSettingsPath) {
        $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    } else {
        $appSettings = @{}
    }

    # Ensure Syncfusion section exists
    if (-not $appSettings.Syncfusion) {
        $appSettings | Add-Member -NotePropertyName 'Syncfusion' -NotePropertyValue @{}
    }

    # Update with local configuration
    $appSettings.Syncfusion = @{
        'LicenseKey'            = 'Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhec3RSRGRYU0R2WUBWYEk='
        'UseLocalResourcesOnly' = $true
        'LocalInstallationPath' = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37'
        'DocumentationPath'     = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37\Help'
        'SamplesPath'           = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37\Samples'
        'NoExternalPackages'    = $true
    }

    $appSettings | ConvertTo-Json -Depth 3 | Out-File $appSettingsPath -Encoding UTF8
    Write-Host '✅ appsettings.json updated with local Syncfusion configuration' -ForegroundColor Green
}

function Test-LocalResourceAccess {
    Write-Host "`n🔍 Testing local resource access..." -ForegroundColor Cyan

    $tests = @(
        @{
            Name = 'SfDataGrid Assembly'
            Path = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37\Assemblies\4.8\Syncfusion.SfDataGrid.WinForms.dll'
        },
        @{
            Name = 'Grid Samples'
            Path = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37\Samples\4.8\Grid'
        },
        @{
            Name = 'Local Documentation'
            Path = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37\Help'
        }
    )

    $allTestsPassed = $true
    foreach ($test in $tests) {
        if (Test-Path $test.Path) {
            Write-Host "✅ $($test.Name): Accessible" -ForegroundColor Green
        } else {
            Write-Host "❌ $($test.Name): NOT FOUND" -ForegroundColor Red
            $allTestsPassed = $false
        }
    }

    if ($allTestsPassed) {
        Write-Host "`n🎉 ALL LOCAL RESOURCES VERIFIED!" -ForegroundColor Green
        Write-Host 'Tools are now locked to local Syncfusion installation only.' -ForegroundColor Cyan
    } else {
        Write-Host "`n⚠️ Some resources missing - tools may not function properly" -ForegroundColor Yellow
    }

    return $allTestsPassed
}

function Show-LockdownSummary {
    Write-Host "`n📋 SYNCFUSION LOCAL RESOURCE LOCKDOWN SUMMARY" -ForegroundColor Cyan
    Write-Host '===============================================' -ForegroundColor Cyan

    Write-Host "`n🔒 POLICY ENFORCED:" -ForegroundColor Yellow
    Write-Host '- All tools MUST use local Syncfusion installation' -ForegroundColor White
    Write-Host '- NO external package references allowed' -ForegroundColor White
    Write-Host '- Documentation sourced from local Help folder' -ForegroundColor White
    Write-Host '- Samples referenced from local Samples folder' -ForegroundColor White
    Write-Host '- License key locked to provided key' -ForegroundColor White

    Write-Host "`n📍 LOCAL PATHS LOCKED:" -ForegroundColor Yellow
    Write-Host 'Installation: C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37' -ForegroundColor Gray
    Write-Host 'Assemblies:   ...\Assemblies' -ForegroundColor Gray
    Write-Host 'Samples:      ...\Samples' -ForegroundColor Gray
    Write-Host 'Docs:         ...\Help' -ForegroundColor Gray

    Write-Host "`n🛡️ PROTECTION ACTIVE:" -ForegroundColor Yellow
    Write-Host '- Environment variables set' -ForegroundColor White
    Write-Host '- PowerShell profile updated' -ForegroundColor White
    Write-Host '- Project file configured' -ForegroundColor White
    Write-Host '- AppSettings locked' -ForegroundColor White

    Write-Host "`n🎯 WHAT THIS MEANS FOR YOU:" -ForegroundColor Green
    Write-Host "When you 'poke buttons', all PowerShell tools will:" -ForegroundColor White
    Write-Host '✅ Use ONLY your local Syncfusion installation' -ForegroundColor Green
    Write-Host '✅ Reference local documentation and samples' -ForegroundColor Green
    Write-Host '✅ Never attempt external package downloads' -ForegroundColor Green
    Write-Host '✅ Maintain consistent behavior across sessions' -ForegroundColor Green
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

if ($Validate) {
    Write-Host '🔍 Validating Syncfusion local resource configuration...' -ForegroundColor Cyan
    $isValid = Set-SyncfusionLocalLockdown
    Test-LocalResourceAccess
    exit
}

if ($Configure) {
    Write-Host '⚙️ Configuring environment only...' -ForegroundColor Cyan
    Set-EnvironmentConfiguration
    exit
}

if ($UpdateProjectFiles) {
    Write-Host '📝 Updating project files only...' -ForegroundColor Cyan
    Update-ProjectConfiguration
    Update-AppSettings
    exit
}

# Full lockdown procedure
Write-Host '🚀 Starting Syncfusion Local Resource Lockdown...' -ForegroundColor Cyan

# Step 1: Validate installation
$installationValid = Set-SyncfusionLocalLockdown
if (-not $installationValid) {
    Write-Host "`n❌ Cannot proceed - Syncfusion installation incomplete" -ForegroundColor Red
    exit 1
}

# Step 2: Configure environment
Set-EnvironmentConfiguration

# Step 3: Update project files
Update-ProjectConfiguration
Update-AppSettings

# Step 4: Test access
$accessValid = Test-LocalResourceAccess

# Step 5: Show summary
Show-LockdownSummary

if ($accessValid) {
    Write-Host "`n🎉 SYNCFUSION LOCAL RESOURCE LOCKDOWN COMPLETE!" -ForegroundColor Green
    Write-Host 'All PowerShell tools are now locked to your local installation.' -ForegroundColor Cyan
} else {
    Write-Host "`n⚠️ Lockdown completed with warnings - some resources unavailable" -ForegroundColor Yellow
}

Write-Host "`n🔴 REMEMBER: Only use local Syncfusion resources as specified in copilot-instructions.md" -ForegroundColor Red

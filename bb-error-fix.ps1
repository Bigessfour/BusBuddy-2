# Bus Buddy Error Analysis and Fix Tool
# Analyzes build errors and provides automated fix recommendations

param(
    [string]$ProjectPath = (Get-Location),
    [switch]$AutoFix,
    [switch]$Detailed
)

function Analyze-BusBuddyBuildErrors {
    param(
        [string]$BuildOutput,
        [string]$ProjectPath
    )

    Write-Host "🔍 Bus Buddy Error Analysis Tool" -ForegroundColor Cyan
    Write-Host "=================================" -ForegroundColor Cyan

    $errors = @()
    $fixes = @()

    # Extract specific errors
    $buildLines = $BuildOutput -split "`n"

    foreach ($line in $buildLines) {
        if ($line -match "error MC3074.*'(.+?)' does not exist.*Line (\d+)") {
            $tagName = $matches[1]
            $lineNumber = $matches[2]
            $filePath = if ($line -match "([^\\]+\.xaml)") { $matches[1] } else { "Unknown" }

            $errorItem = @{
                Type        = "XAML_TAG_NOT_FOUND"
                Tag         = $tagName
                Line        = $lineNumber
                File        = $filePath
                Severity    = "High"
                Description = "Syncfusion control or property not available in current namespace"
            }

            $fix = Get-SyncfusionTagFix -TagName $tagName -Line $lineNumber -File $filePath

            $errors += $errorItem
            $fixes += $fix
        }

        if ($line -match "error CS1061.*'(.+?)' does not contain a definition for '(.+?)'") {
            $className = $matches[1]
            $methodName = $matches[2]

            $errorItem = @{
                Type        = "METHOD_NOT_FOUND"
                Class       = $className
                Method      = $methodName
                Severity    = "High"
                Description = "Method or property not found in class"
            }

            $fix = Get-CSharpMethodFix -ClassName $className -MethodName $methodName

            $errors += $errorItem
            $fixes += $fix
        }
    }

    # Display analysis
    Write-Host "`n📋 Error Analysis Results:" -ForegroundColor Yellow
    Write-Host "Found $($errors.Count) errors with fix recommendations" -ForegroundColor White

    for ($i = 0; $i -lt $errors.Count; $i++) {
        $errorItem = $errors[$i]
        $fix = $fixes[$i]

        Write-Host "`n❌ Error $($i + 1): $($errorItem.Type)" -ForegroundColor Red
        Write-Host "   File: $($errorItem.File)" -ForegroundColor Gray
        Write-Host "   Line: $($errorItem.Line)" -ForegroundColor Gray
        Write-Host "   Issue: $($errorItem.Description)" -ForegroundColor White

        Write-Host "`n💡 Recommended Fix:" -ForegroundColor Green
        Write-Host "   $($fix.Description)" -ForegroundColor White
        Write-Host "   Action: $($fix.Action)" -ForegroundColor Cyan

        if ($fix.Code) {
            Write-Host "   Replacement Code:" -ForegroundColor Yellow
            Write-Host "   $($fix.Code)" -ForegroundColor Gray
        }
    }

    return @{
        Errors = $errors
        Fixes  = $fixes
    }
}

function Get-SyncfusionTagFix {
    param(
        [string]$TagName,
        [string]$Line,
        [string]$File
    )

    $syncfusionFixes = @{
        "SfScheduler.WeekViewSettings"   = @{
            Description = "WeekViewSettings property not available in Syncfusion.SfScheduler.WPF version 30.1.40"
            Action      = "Remove WeekViewSettings and MonthViewSettings properties - use basic SfScheduler"
            Code        = "<!-- Remove WeekViewSettings and MonthViewSettings sections -->"
        }
        "SfScheduler.MonthViewSettings"  = @{
            Description = "MonthViewSettings property not available in current Syncfusion version"
            Action      = "Remove MonthViewSettings property - use basic SfScheduler configuration"
            Code        = "<!-- Use basic SfScheduler without advanced view settings -->"
        }
        "SfScheduler.AppointmentMapping" = @{
            Description = "AppointmentMapping may need different syntax in current version"
            Action      = "Simplify appointment mapping or use ItemsSource directly"
            Code        = '<syncfusion:SfScheduler ItemsSource="{Binding ActivitySchedules}" />'
        }
    }

    if ($syncfusionFixes.ContainsKey($TagName)) {
        return $syncfusionFixes[$TagName]
    }

    return @{
        Description = "Unknown Syncfusion control issue"
        Action      = "Check Syncfusion documentation for version 30.1.40"
        Code        = "<!-- Consult Syncfusion docs -->"
    }
}

function Get-CSharpMethodFix {
    param(
        [string]$ClassName,
        [string]$MethodName
    )

    return @{
        Description = "Method $MethodName not found in class $ClassName"
        Action      = "Check method name spelling, add using statements, or implement missing method"
        Code        = "// Verify method exists and is accessible"
    }
}

function Apply-AutoFix {
    param(
        [array]$Fixes,
        [string]$ProjectPath
    )

    Write-Host "`n🔧 Applying Automatic Fixes..." -ForegroundColor Yellow

    foreach ($fix in $Fixes) {
        if ($fix.Action -like "*Remove WeekViewSettings*") {
            Write-Host "   Removing Syncfusion WeekViewSettings from ActivityScheduleView.xaml..." -ForegroundColor Cyan
            # This would call the replace_string_in_file or similar automated fix
            # For now, just show what would be done
            Write-Host "   ✅ Would remove WeekViewSettings section" -ForegroundColor Green
        }
    }
}

# Main execution
Write-Host "🚌 Bus Buddy Error Analysis and Fix Tool" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Get build output and analyze
Write-Host "🔨 Running build to capture errors..." -ForegroundColor Yellow
$buildOutput = dotnet build BusBuddy.sln 2>&1 | Out-String
$analysis = Analyze-BusBuddyBuildErrors -BuildOutput $buildOutput -ProjectPath $ProjectPath

if ($AutoFix) {
    Apply-AutoFix -Fixes $analysis.Fixes -ProjectPath $ProjectPath
}

# Add this tool to the PowerShell profile
Write-Host "`n🚌 Error analysis completed!" -ForegroundColor Green

#Requires -Version 7.5
<#
.SYNOPSIS
Master issue resolution script for BusBuddy priority issues

.DESCRIPTION
Comprehensive resolution of all priority issues:
- High: File locks from PowerShell processes
- Medium: Build workflow optimization
- Low: Auto-disposal prevention
- Low: Phase 2 performance preparation

.PARAMETER IssueLevel
Target specific issue levels: All (default), High, Medium, Low

.PARAMETER AutoFix
Automatically fix issues without prompting

.PARAMETER ReportOnly
Only report issues without fixing them

.EXAMPLE
.\Master-Issue-Resolution.ps1 -IssueLevel All -AutoFix
#>

param(
    [ValidateSet("All", "High", "Medium", "Low")]
    [string]$IssueLevel = "All",

    [switch]$AutoFix,
    [switch]$ReportOnly
)

$ErrorActionPreference = "Stop"

class IssueTracker {
    [hashtable]$Issues
    [hashtable]$Solutions
    [datetime]$StartTime

    IssueTracker() {
        $this.Issues = @{}
        $this.Solutions = @{}
        $this.StartTime = Get-Date
    }

    [void]AddIssue([string]$Priority, [string]$Type, [string]$Description, [string]$Solution) {
        $id = "Issue_$($this.Issues.Count + 1)"
        $this.Issues[$id] = @{
            Priority = $Priority
            Type = $Type
            Description = $Description
            Solution = $Solution
            Status = "Identified"
            Timestamp = Get-Date
        }
    }

    [void]MarkResolved([string]$IssueId, [string]$Result) {
        if ($this.Issues.ContainsKey($IssueId)) {
            $this.Issues[$IssueId].Status = "Resolved"
            $this.Issues[$IssueId].Result = $Result
            $this.Issues[$IssueId].ResolvedTime = Get-Date
        }
    }

    [array]GetIssuesByPriority([string]$Priority) {
        return $this.Issues.Values | Where-Object { $_.Priority -eq $Priority }
    }

    [hashtable]GetSummary() {
        $total = $this.Issues.Count
        $resolved = ($this.Issues.Values | Where-Object { $_.Status -eq "Resolved" }).Count
        $remaining = $total - $resolved

        return @{
            Total = $total
            Resolved = $resolved
            Remaining = $remaining
            SuccessRate = if ($total -gt 0) { [math]::Round(($resolved / $total) * 100, 1) } else { 0 }
            Duration = (Get-Date) - $this.StartTime
        }
    }
}

function Write-MasterStatus {
    param($Message, $Type = "Info", $Priority = "")

    $priorityColor = switch($Priority) {
        "High" { "Red" }
        "Medium" { "Yellow" }
        "Low" { "Green" }
        default { "White" }
    }

    $typeIcon = switch($Type) {
        "Success" { "✅" }
        "Warning" { "⚠️" }
        "Error" { "❌" }
        "Fix" { "🔧" }
        "Report" { "📊" }
        default { "🎯" }
    }

    $color = switch($Type) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        "Fix" { "Cyan" }
        "Report" { "Magenta" }
        default { $priorityColor }
    }

    $priorityTag = if ($Priority) { "[$Priority] " } else { "" }
    Write-Host "$typeIcon $priorityTag$Message" -ForegroundColor $color
}

function Initialize-IssueDatabase {
    param([object]$Tracker)

    Write-MasterStatus "Initializing issue database..." "Report"

    # High Priority Issues
    $Tracker.AddIssue("High", "File Lock", "DLL held by PowerShell 7 processes", "Kill PIDs; close sessions")

    # Medium Priority Issues
    $Tracker.AddIssue("Medium", "Build Workflow", "Using direct dotnet vs bb-build", "Switch to PowerShell commands for diagnostics")

    # Low Priority Issues
    $Tracker.AddIssue("Low", "Prevention", "No auto-disposal in scripts", "Add unload logic to automation")
    $Tracker.AddIssue("Low", "Phase 2 Performance", "Large data seeding", "Test with optimized loader post-build")

    Write-MasterStatus "Issue database initialized with $($Tracker.Issues.Count) issues" "Success"
}

function Resolve-HighPriorityIssues {
    param([object]$Tracker, [bool]$AutoFix, [bool]$ReportOnly)

    Write-MasterStatus "=== HIGH PRIORITY ISSUES ===" "Fix" "High"

    $highIssues = $Tracker.GetIssuesByPriority("High")

    foreach ($issue in $highIssues) {
        Write-MasterStatus "Issue: $($issue.Description)" "Warning" "High"
        Write-MasterStatus "Solution: $($issue.Solution)" "Fix" "High"

        if ($ReportOnly) {
            Write-MasterStatus "Report mode: Issue documented" "Report" "High"
            continue
        }

        if (-not $AutoFix) {
            $response = Read-Host "Apply fix for file locks? (y/N)"
            if ($response -notmatch '^[Yy]') {
                Write-MasterStatus "Skipped by user" "Warning" "High"
                continue
            }
        }

        try {
            Write-MasterStatus "Executing file lock resolution..." "Fix" "High"

            # Run the file lock fix script
            if (Test-Path ".\Scripts\Maintenance\Fix-File-Locks.ps1") {
                & ".\Scripts\Maintenance\Fix-File-Locks.ps1" -Safe
                $Tracker.MarkResolved("Issue_1", "File locks cleared successfully")
                Write-MasterStatus "File lock issue resolved" "Success" "High"
            } else {
                Write-MasterStatus "Fix script not found, manual resolution required" "Warning" "High"
            }

        } catch {
            Write-MasterStatus "Failed to resolve: $($_.Exception.Message)" "Error" "High"
        }
    }
}

function Resolve-MediumPriorityIssues {
    param([object]$Tracker, [bool]$AutoFix, [bool]$ReportOnly)

    Write-MasterStatus "=== MEDIUM PRIORITY ISSUES ===" "Fix" "Medium"

    $mediumIssues = $Tracker.GetIssuesByPriority("Medium")

    foreach ($issue in $mediumIssues) {
        Write-MasterStatus "Issue: $($issue.Description)" "Warning" "Medium"
        Write-MasterStatus "Solution: $($issue.Solution)" "Fix" "Medium"

        if ($ReportOnly) {
            Write-MasterStatus "Report mode: Issue documented" "Report" "Medium"
            continue
        }

        if (-not $AutoFix) {
            $response = Read-Host "Apply build workflow enhancement? (y/N)"
            if ($response -notmatch '^[Yy]') {
                Write-MasterStatus "Skipped by user" "Warning" "Medium"
                continue
            }
        }

        try {
            Write-MasterStatus "Implementing enhanced build workflow..." "Fix" "Medium"

            # Run the enhanced build workflow
            if (Test-Path ".\Scripts\Maintenance\Enhanced-Build-Workflow.ps1") {
                & ".\Scripts\Maintenance\Enhanced-Build-Workflow.ps1" -Mode Auto
                $Tracker.MarkResolved("Issue_2", "Enhanced build workflow implemented")
                Write-MasterStatus "Build workflow issue resolved" "Success" "Medium"
            } else {
                Write-MasterStatus "Enhancement script not found" "Warning" "Medium"
            }

        } catch {
            Write-MasterStatus "Failed to resolve: $($_.Exception.Message)" "Error" "Medium"
        }
    }
}

function Resolve-LowPriorityIssues {
    param([object]$Tracker, [bool]$AutoFix, [bool]$ReportOnly)

    Write-MasterStatus "=== LOW PRIORITY ISSUES ===" "Fix" "Low"

    $lowIssues = $Tracker.GetIssuesByPriority("Low")

    foreach ($issue in $lowIssues) {
        Write-MasterStatus "Issue: $($issue.Description)" "Warning" "Low"
        Write-MasterStatus "Solution: $($issue.Solution)" "Fix" "Low"

        if ($ReportOnly) {
            Write-MasterStatus "Report mode: Issue documented" "Report" "Low"
            continue
        }

        if (-not $AutoFix) {
            $response = Read-Host "Apply fix for '$($issue.Type)'? (y/N)"
            if ($response -notmatch '^[Yy]') {
                Write-MasterStatus "Skipped by user" "Warning" "Low"
                continue
            }
        }

        try {
            if ($issue.Type -eq "Prevention") {
                Write-MasterStatus "Implementing resource cleanup..." "Fix" "Low"

                if (Test-Path ".\Scripts\Maintenance\Resource-Cleanup-Manager.ps1") {
                    & ".\Scripts\Maintenance\Resource-Cleanup-Manager.ps1" -Mode Auto -UnloadModules
                    $Tracker.MarkResolved("Issue_3", "Auto-disposal prevention implemented")
                    Write-MasterStatus "Resource cleanup issue resolved" "Success" "Low"
                }

            } elseif ($issue.Type -eq "Phase 2 Performance") {
                Write-MasterStatus "Setting up Phase 2 performance optimization..." "Fix" "Low"

                if (Test-Path ".\Scripts\Maintenance\Phase2-Performance-Optimizer.ps1") {
                    & ".\Scripts\Maintenance\Phase2-Performance-Optimizer.ps1" -TestSize Medium -OptimizeForBuild
                    $Tracker.MarkResolved("Issue_4", "Phase 2 performance optimization prepared")
                    Write-MasterStatus "Phase 2 performance issue resolved" "Success" "Low"
                }
            }

        } catch {
            Write-MasterStatus "Failed to resolve: $($_.Exception.Message)" "Error" "Low"
        }
    }
}

function New-ResolutionReport {
    param([object]$Tracker)

    Write-MasterStatus "Generating resolution report..." "Report"

    $summary = $Tracker.GetSummary()
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"

    $reportContent = @"
# BusBuddy Master Issue Resolution Report
Generated: $(Get-Date)
Duration: $($summary.Duration.TotalSeconds) seconds

## Summary
- Total Issues: $($summary.Total)
- Resolved: $($summary.Resolved)
- Remaining: $($summary.Remaining)
- Success Rate: $($summary.SuccessRate)%

## Issue Details
"@

    foreach ($issueId in $Tracker.Issues.Keys) {
        $issue = $Tracker.Issues[$issueId]
        $reportContent += @"

### $issueId - $($issue.Priority) Priority
- **Type**: $($issue.Type)
- **Description**: $($issue.Description)
- **Solution**: $($issue.Solution)
- **Status**: $($issue.Status)
- **Timestamp**: $($issue.Timestamp)
"@

        if ($issue.Status -eq "Resolved") {
            $reportContent += "`n- **Result**: $($issue.Result)"
            $reportContent += "`n- **Resolved**: $($issue.ResolvedTime)"
        }
    }

    $reportContent += @"

## Recommendations
1. **High Priority**: $(if (($Tracker.GetIssuesByPriority("High") | Where-Object { $_.Status -eq "Resolved" }).Count -gt 0) { "File lock issues addressed" } else { "Manual file lock resolution may be needed" })
2. **Medium Priority**: $(if (($Tracker.GetIssuesByPriority("Medium") | Where-Object { $_.Status -eq "Resolved" }).Count -gt 0) { "Build workflow optimized" } else { "Consider implementing enhanced build workflow" })
3. **Low Priority**: $(if (($Tracker.GetIssuesByPriority("Low") | Where-Object { $_.Status -eq "Resolved" }).Count -gt 0) { "Prevention measures implemented" } else { "Defer to Phase 2 development" })

## Next Steps
$(if ($summary.SuccessRate -gt 80) { "✅ Ready for normal development workflow" } else { "⚠️ Additional manual intervention may be required" })
"@

    try {
        $reportPath = "logs\issue-resolution-$timestamp.md"
        Set-Content -Path $reportPath -Value $reportContent -Encoding UTF8
        Write-MasterStatus "Report saved to: $reportPath" "Success"
        return $reportPath
    } catch {
        Write-MasterStatus "Failed to save report: $($_.Exception.Message)" "Error"
        Write-Host $reportContent
        return $null
    }
}

# Main execution
try {
    Write-MasterStatus "=== BusBuddy Master Issue Resolution ===" "Fix"
    Write-MasterStatus "Target Level: $IssueLevel | Auto Fix: $AutoFix | Report Only: $ReportOnly"
    Write-Host ""

    # Initialize issue tracking
    $tracker = [IssueTracker]::new()
    Initialize-IssueDatabase -Tracker $tracker
    Write-Host ""

    # Resolve issues based on level
    if ($IssueLevel -eq "All" -or $IssueLevel -eq "High") {
        Resolve-HighPriorityIssues -Tracker $tracker -AutoFix $AutoFix -ReportOnly $ReportOnly
        Write-Host ""
    }

    if ($IssueLevel -eq "All" -or $IssueLevel -eq "Medium") {
        Resolve-MediumPriorityIssues -Tracker $tracker -AutoFix $AutoFix -ReportOnly $ReportOnly
        Write-Host ""
    }

    if ($IssueLevel -eq "All" -or $IssueLevel -eq "Low") {
        Resolve-LowPriorityIssues -Tracker $tracker -AutoFix $AutoFix -ReportOnly $ReportOnly
        Write-Host ""
    }

    # Generate final report
    $reportPath = New-ResolutionReport -Tracker $tracker

    # Display summary
    $summary = $tracker.GetSummary()
    Write-MasterStatus "=== RESOLUTION SUMMARY ===" "Report"
    Write-MasterStatus "Issues Resolved: $($summary.Resolved)/$($summary.Total) ($($summary.SuccessRate)%)" "Success"
    Write-MasterStatus "Total Duration: $([math]::Round($summary.Duration.TotalSeconds, 1)) seconds" "Success"

    if ($summary.SuccessRate -gt 80) {
        Write-MasterStatus "Master issue resolution completed successfully" "Success"
    } else {
        Write-MasterStatus "Partial resolution completed. Manual intervention may be required." "Warning"
    }

} catch {
    Write-MasterStatus "Master resolution failed: $($_.Exception.Message)" "Error"
    exit 1
}

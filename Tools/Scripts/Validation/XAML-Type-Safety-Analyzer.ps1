#Requires -Version 7.0
<#
.SYNOPSIS
    XAML Type Safety Analyzer for Bus Buddy

.DESCRIPTION
    Detects type mismatches in XAML bindings that cause 30% of application crashes.
    Validates converter usage and suggests type-safe alternatives.

.NOTES
    Addresses the industry statistic that 30% of crashes come from type mismatch errors
#>

class XamlTypeSafetyIssue {
    [string]$FilePath
    [int]$LineNumber
    [string]$IssueType
    [string]$Severity
    [string]$PropertyName
    [string]$ExpectedType
    [string]$ActualBinding
    [string]$Recommendation
}

function Test-XamlTypeSafety {
    <#
    .SYNOPSIS
        Analyze XAML bindings for type safety issues
    #>
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $issues = @()
    $xamlFiles = Get-ChildItem $Path -Filter "*.xaml" -Recurse

    # Common WPF property type mappings
    $propertyTypes = @{
        'Width' = 'Double'
        'Height' = 'Double'
        'Margin' = 'Thickness'
        'Padding' = 'Thickness'
        'Background' = 'Brush'
        'Foreground' = 'Brush'
        'FontSize' = 'Double'
        'Opacity' = 'Double'
        'Visibility' = 'Visibility'
        'IsEnabled' = 'Boolean'
        'IsVisible' = 'Boolean'
        'Content' = 'Object'
        'Text' = 'String'
        'ItemsSource' = 'IEnumerable'
    }

    foreach ($file in $xamlFiles) {
        $lines = Get-Content $file.FullName

        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]

            # Check for numeric properties with string bindings
            if ($line -match '(Width|Height|FontSize|Opacity)="\{Binding\s+(\w+)') {
                $property = $matches[1]
                $bindingPath = $matches[2]

                # Check if binding path suggests non-numeric data
                if ($bindingPath -match '(Name|Title|Description|Text)$') {
                    $issue = [XamlTypeSafetyIssue]::new()
                    $issue.FilePath = $file.FullName
                    $issue.LineNumber = $i + 1
                    $issue.IssueType = "TypeMismatch"
                    $issue.Severity = "High"
                    $issue.PropertyName = $property
                    $issue.ExpectedType = $propertyTypes[$property]
                    $issue.ActualBinding = $bindingPath
                    $issue.Recommendation = "Add converter or ensure ViewModel property returns $($propertyTypes[$property])"
                    $issues += $issue
                }
            }

            # Check for boolean properties with non-boolean bindings
            if ($line -match '(IsEnabled|IsVisible|IsChecked)="\{Binding\s+(\w+)') {
                $property = $matches[1]
                $bindingPath = $matches[2]

                if ($bindingPath -notmatch '(Is|Can|Has|Should)' -and $line -notmatch 'Converter') {
                    $issue = [XamlTypeSafetyIssue]::new()
                    $issue.FilePath = $file.FullName
                    $issue.LineNumber = $i + 1
                    $issue.IssueType = "BooleanMismatch"
                    $issue.Severity = "Medium"
                    $issue.PropertyName = $property
                    $issue.ExpectedType = "Boolean"
                    $issue.ActualBinding = $bindingPath
                    $issue.Recommendation = "Add BooleanConverter or use boolean property naming (Is*, Can*, Has*)"
                    $issues += $issue
                }
            }

            # Check for Visibility bindings without converter
            if ($line -match 'Visibility="\{Binding\s+(\w+)"' -and $line -notmatch 'Converter') {
                $bindingPath = $matches[1]

                if ($bindingPath -notmatch 'Visibility$') {
                    $issue = [XamlTypeSafetyIssue]::new()
                    $issue.FilePath = $file.FullName
                    $issue.LineNumber = $i + 1
                    $issue.IssueType = "VisibilityMismatch"
                    $issue.Severity = "High"
                    $issue.PropertyName = "Visibility"
                    $issue.ExpectedType = "Visibility"
                    $issue.ActualBinding = $bindingPath
                    $issue.Recommendation = "Add BooleanToVisibilityConverter or return Visibility enum from ViewModel"
                    $issues += $issue
                }
            }

            # Check for missing StringFormat on non-string properties
            if ($line -match 'Text="\{Binding\s+(\w+)"' -and $line -notmatch 'StringFormat') {
                $bindingPath = $matches[1]

                if ($bindingPath -match '(Date|Time|Count|Amount|Price|Number)') {
                    $issue = [XamlTypeSafetyIssue]::new()
                    $issue.FilePath = $file.FullName
                    $issue.LineNumber = $i + 1
                    $issue.IssueType = "FormattingMissing"
                    $issue.Severity = "Low"
                    $issue.PropertyName = "Text"
                    $issue.ExpectedType = "String"
                    $issue.ActualBinding = $bindingPath
                    $issue.Recommendation = "Add StringFormat for proper display formatting"
                    $issues += $issue
                }
            }

            # Check for ItemsSource type safety
            if ($line -match 'ItemsSource="\{Binding\s+(\w+)"') {
                $bindingPath = $matches[1]

                if ($bindingPath -notmatch '(Items|Collection|List)$' -and $bindingPath -notmatch 's$') {
                    $issue = [XamlTypeSafetyIssue]::new()
                    $issue.FilePath = $file.FullName
                    $issue.LineNumber = $i + 1
                    $issue.IssueType = "CollectionMismatch"
                    $issue.Severity = "Medium"
                    $issue.PropertyName = "ItemsSource"
                    $issue.ExpectedType = "IEnumerable"
                    $issue.ActualBinding = $bindingPath
                    $issue.Recommendation = "Ensure property returns IEnumerable/ObservableCollection"
                    $issues += $issue
                }
            }
        }
    }

    return $issues
}

function Invoke-XamlTypeSafetyCheck {
    <#
    .SYNOPSIS
        Bus Buddy XAML type safety validator
    .EXAMPLE
        Invoke-XamlTypeSafetyCheck
        Invoke-XamlTypeSafetyCheck -Path "Views\Dashboard" -GenerateReport
    #>
    param(
        [string]$Path = "BusBuddy.WPF\Views",
        [switch]$GenerateReport,
        [switch]$ShowSuggestions
    )

    Write-Host "🔒 Bus Buddy Type Safety Validator" -ForegroundColor Cyan

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Host "❌ Bus Buddy project root not found" -ForegroundColor Red
        return
    }

    $targetPath = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $projectRoot $Path }

    $issues = Test-XamlTypeSafety -Path $targetPath

    if ($issues.Count -eq 0) {
        Write-Host "✅ No type safety issues found!" -ForegroundColor Green
        return
    }

    # Statistics
    $highIssues = ($issues | Where-Object { $_.Severity -eq "High" }).Count
    $mediumIssues = ($issues | Where-Object { $_.Severity -eq "Medium" }).Count
    $lowIssues = ($issues | Where-Object { $_.Severity -eq "Low" }).Count

    Write-Host "`n📊 Type Safety Analysis:" -ForegroundColor Yellow
    Write-Host "   Total Issues: $($issues.Count)" -ForegroundColor White
    Write-Host "   High Priority: $highIssues (potential crashes)" -ForegroundColor Red
    Write-Host "   Medium Priority: $mediumIssues (runtime errors)" -ForegroundColor Yellow
    Write-Host "   Low Priority: $lowIssues (formatting issues)" -ForegroundColor Green

    # Group-Object by issue type
    $groupedIssues = $issues | Group-Object IssueType | Sort-Object {
        switch ($_.Name) {
            "TypeMismatch" { 1 }
            "VisibilityMismatch" { 2 }
            "BooleanMismatch" { 3 }
            "CollectionMismatch" { 4 }
            "FormattingMissing" { 5 }
            default { 6 }
        }
    }

    foreach ($group in $groupedIssues) {
        Write-Host "`n🔍 $($group.Name) ($($group.Count) occurrences):" -ForegroundColor Magenta

        $priorityIssues = $group.Group | Sort-Object {
            switch ($_.Severity) {
                "High" { 1 }
                "Medium" { 2 }
                "Low" { 3 }
            }
        } | Select-Object -First 3

        foreach ($issue in $priorityIssues) {
            $fileName = Split-Path $issue.FilePath -Leaf
            $severityColor = switch ($issue.Severity) {
                "High" { "Red" }
                "Medium" { "Yellow" }
                "Low" { "Green" }
            }

            Write-Host "   📄 $fileName (Line $($issue.LineNumber)) " -ForegroundColor White -NoNewline
            Write-Host "[$($issue.Severity)]" -ForegroundColor $severityColor
            Write-Host "      Property: $($issue.PropertyName) (expects $($issue.ExpectedType))" -ForegroundColor Gray
            Write-Host "      Binding: $($issue.ActualBinding)" -ForegroundColor Gray
            Write-Host "      💡 $($issue.Recommendation)" -ForegroundColor Green
        }

        if ($group.Count -gt 3) {
            Write-Host "      ... and $($group.Count - 3) more similar issues" -ForegroundColor Gray
        }
    }

    if ($ShowSuggestions) {
        Write-Host "`n💡 General Type Safety Tips:" -ForegroundColor Yellow
        Write-Host "   • Use strongly-typed ViewModels with proper property types" -ForegroundColor Gray
        Write-Host "   • Implement IValueConverter for complex type conversions" -ForegroundColor Gray
        Write-Host "   • Use StringFormat for display formatting instead of converters when possible" -ForegroundColor Gray
        Write-Host "   • Name boolean properties with Is*, Can*, Has* prefixes" -ForegroundColor Gray
        Write-Host "   • Return ObservableCollection<T> for ItemsSource properties" -ForegroundColor Gray
    }

    if ($GenerateReport) {
        $reportPath = Join-Path $projectRoot "logs\type-safety-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
        $report = @{
            GeneratedAt = Get-Date
            ProjectPath = $targetPath
            Summary = @{
                TotalIssues = $issues.Count
                HighPriority = $highIssues
                MediumPriority = $mediumIssues
                LowPriority = $lowIssues
            }
            Issues = $issues
        }
        $report | ConvertTo-Json -Depth 4 | Out-File $reportPath -Encoding UTF8
        Write-Host "`n💾 Report exported to: $reportPath" -ForegroundColor Green
    }
}

# Import Bus Buddy project helper if available
if (-not (Get-Command Get-BusBuddyProjectRoot -ErrorAction SilentlyContinue)) {
    function Get-BusBuddyProjectRoot {
        $currentPath = $PWD.Path
        while ($currentPath) {
            if (Test-Path (Join-Path $currentPath 'BusBuddy.sln')) {
                return $currentPath
            }
            $parentPath = Split-Path $currentPath -Parent
            if ($parentPath -eq $currentPath) { break }
            $currentPath = $parentPath
        }
        return $null
    }
}

# Alias for easier use
Set-Alias -Name "bb-types" -Value "Invoke-XamlTypeSafetyCheck"

#Requires -Version 7.5
<#
.SYNOPSIS
    GitHub Actions Workflow Validator and Fetchability Enhancer

.DESCRIPTION
    Validates GitHub Actions workflow files for syntax errors and common issues.
    Fixes encoding problems, YAML syntax errors, and workflow configuration issues.
    Ensures files are fetchable via GitHub raw URLs and APIs.

.EXAMPLE
    .\Fix-GitHub-Workflows.ps1 -ValidateOnly
    .\Fix-GitHub-Workflows.ps1 -FixIssues
    .\Fix-GitHub-Workflows.ps1 -ValidateFetchability
    .\Fix-GitHub-Workflows.ps1 -FixIssues -ValidateFetchability
#>

param(
    [switch]$ValidateOnly,
    [switch]$FixIssues,
    [switch]$ValidateFetchability,
    [string]$GitHubRepo = "Bigessfour/BusBuddy-2",
    [string]$Branch = "main"
)

Write-Host "🔍 GitHub Actions Workflow Validator & Fetchability Enhancer" -ForegroundColor Cyan
Write-Host ""

$WorkflowPath = ".github\workflows"
$IssuesFound = @()
$FetchabilityResults = @()

# Function to test file fetchability
function Test-FileFetchability {
    param(
        [string]$FilePath,
        [string]$Repo,
        [string]$Branch
    )

    $RelativePath = $FilePath -replace [regex]::Escape($PWD.Path + "\"), "" -replace "\\", "/"
    $RawUrl = "https://raw.githubusercontent.com/$Repo/$Branch/$RelativePath"

    try {
        $Response = Invoke-WebRequest -Uri $RawUrl -Method Head -TimeoutSec 10 -ErrorAction Stop
        return [PSCustomObject]@{
            File        = $FilePath
            RawUrl      = $RawUrl
            Status      = "✅ Fetchable"
            StatusCode  = $Response.StatusCode
            ContentType = $Response.Headers['Content-Type']
            Size        = $Response.Headers['Content-Length']
            Issues      = @()
        }
    }
    catch {
        $Issues = @()

        if ($_.Exception.Response.StatusCode -eq 404) {
            $Issues += "File not found (404) - may be uncommitted or private repo"
        }
        elseif ($_.Exception.Response.StatusCode -eq 403) {
            $Issues += "Access forbidden (403) - check repo visibility or authentication"
        }
        else {
            $Issues += "Network error: $($_.Exception.Message)"
        }

        return [PSCustomObject]@{
            File       = $FilePath
            RawUrl     = $RawUrl
            Status     = "❌ Not Fetchable"
            StatusCode = $_.Exception.Response.StatusCode
            Issues     = $Issues
        }
    }
}

# Function to validate repository structure for fetchability
function Test-RepoFetchability {
    param([string]$Repo, [string]$Branch)

    Write-Host "🌐 Testing repository fetchability..." -ForegroundColor Yellow

    $TestResults = @()

    # Test README fetchability
    $ReadmeUrl = "https://raw.githubusercontent.com/$Repo/$Branch/README.md"
    try {
        $Response = Invoke-WebRequest -Uri $ReadmeUrl -Method Head -TimeoutSec 10
        $TestResults += "✅ README.md is fetchable"
    }
    catch {
        $TestResults += "❌ README.md not fetchable - repo may be private or file missing"
    }

    # Check if .gitattributes exists for line ending management
    if (Test-Path ".gitattributes") {
        $TestResults += "✅ .gitattributes found - line ending handling configured"
    }
    else {
        $TestResults += "⚠️  .gitattributes missing - consider adding for consistent line endings"
    }

    # Check for large files that might not be fetchable
    $LargeFiles = Get-ChildItem -Recurse -File | Where-Object { $_.Length -gt 100MB }
    if ($LargeFiles.Count -gt 0) {
        $TestResults += "⚠️  Found $($LargeFiles.Count) files >100MB - consider Git LFS"
        foreach ($File in $LargeFiles) {
            $TestResults += "   - $($File.FullName) ($([math]::Round($File.Length/1MB, 2))MB)"
        }
    }
    else {
        $TestResults += "✅ No files >100MB detected"
    }

    return $TestResults
}

if (-not (Test-Path $WorkflowPath)) {
    Write-Host "❌ Workflows directory not found: $WorkflowPath" -ForegroundColor Red
    if ($ValidateFetchability) {
        Write-Host "📁 Creating .github/workflows directory for future use..." -ForegroundColor Yellow
        New-Item -Path $WorkflowPath -ItemType Directory -Force | Out-Null
        Write-Host "✅ Directory created" -ForegroundColor Green
    }
    else {
        return
    }
}

# Validate repository fetchability if requested
if ($ValidateFetchability) {
    $RepoResults = Test-RepoFetchability -Repo $GitHubRepo -Branch $Branch
    Write-Host ""
    Write-Host "📊 Repository Fetchability Results:" -ForegroundColor Cyan
    foreach ($Result in $RepoResults) {
        Write-Host "  $Result" -ForegroundColor White
    }
    Write-Host ""
}

# Get all workflow files
$WorkflowFiles = Get-ChildItem -Path $WorkflowPath -Filter "*.yml" -File -ErrorAction SilentlyContinue

if ($WorkflowFiles.Count -eq 0) {
    Write-Host "📄 No workflow files found in $WorkflowPath" -ForegroundColor Yellow
    if ($ValidateFetchability) {
        Write-Host "📝 Fetchability can still be validated for other project files" -ForegroundColor Blue
    }
}
else {
    Write-Host "📄 Found $($WorkflowFiles.Count) workflow files to validate..." -ForegroundColor Yellow
}
Write-Host ""

foreach ($File in $WorkflowFiles) {
    Write-Host "🔍 Validating: $($File.Name)" -ForegroundColor Blue

    try {
        $Content = Get-Content -Path $File.FullName -Raw -Encoding UTF8
        $Lines = Get-Content -Path $File.FullName -Encoding UTF8

        # Check for encoding issues
        $EncodingIssues = @()
        if ($Content -match '[^\x00-\x7F]') {
            # Check for problematic characters
            for ($i = 0; $i -lt $Lines.Count; $i++) {
                $line = $Lines[$i]
                if ($line -match '[^\x00-\x7F]') {
                    $EncodingIssues += [PSCustomObject]@{
                        LineNumber       = $i + 1
                        Issue            = "Non-ASCII characters detected"
                        Line             = $line
                        ProblematicChars = [regex]::Matches($line, '[^\x00-\x7F]').Value -join ', '
                    }
                }
            }
        }

        # Check for YAML syntax issues
        $YamlIssues = @()

        # Check for common YAML issues
        for ($i = 0; $i -lt $Lines.Count; $i++) {
            $line = $Lines[$i]
            $lineNumber = $i + 1

            # Check for tab characters (YAML should use spaces)
            if ($line -match "`t") {
                $YamlIssues += [PSCustomObject]@{
                    LineNumber = $lineNumber
                    Issue      = "Tab characters detected (YAML requires spaces)"
                    Line       = $line
                    Fix        = $line -replace "`t", "  "
                }
            }

            # Check for inconsistent indentation
            if ($line -match '^(\s+)' -and $line -notmatch '^(\s*#)') {
                $indentation = $Matches[1]
                if ($indentation.Length % 2 -ne 0) {
                    $YamlIssues += [PSCustomObject]@{
                        LineNumber        = $lineNumber
                        Issue             = "Inconsistent indentation (should be multiples of 2)"
                        Line              = $line
                        IndentationLength = $indentation.Length
                    }
                }
            }

            # Check for missing spaces after colons
            if ($line -match '^(\s*\w+):(\S)' -and $line -notmatch '^(\s*\w+):\|') {
                $YamlIssues += [PSCustomObject]@{
                    LineNumber = $lineNumber
                    Issue      = "Missing space after colon"
                    Line       = $line
                    Fix        = $line -replace '^(\s*\w+):(\S)', '$1: $2'
                }
            }
        }

        # Check for GitHub Actions specific issues
        $ActionsIssues = @()

        # Check for required workflow elements
        if ($Content -notmatch '^name:\s*') {
            $ActionsIssues += [PSCustomObject]@{
                LineNumber = 1
                Issue      = "Missing workflow name"
                Suggestion = "Add 'name: Your Workflow Name' at the top"
            }
        }

        if ($Content -notmatch '^on:\s*') {
            $ActionsIssues += [PSCustomObject]@{
                LineNumber = 1
                Issue      = "Missing 'on' trigger definition"
                Suggestion = "Add 'on:' section to define when workflow runs"
            }
        }

        if ($Content -notmatch '^jobs:\s*') {
            $ActionsIssues += [PSCustomObject]@{
                LineNumber = 1
                Issue      = "Missing 'jobs' section"
                Suggestion = "Add 'jobs:' section with at least one job"
            }
        }

        # Report issues for this file
        $TotalIssues = $EncodingIssues.Count + $YamlIssues.Count + $ActionsIssues.Count

        if ($TotalIssues -eq 0) {
            Write-Host "  ✅ No issues found" -ForegroundColor Green
        }
        else {
            Write-Host "  ⚠️  Found $TotalIssues issue(s)" -ForegroundColor Yellow

            # Encoding issues
            if ($EncodingIssues.Count -gt 0) {
                Write-Host "    📝 Encoding Issues:" -ForegroundColor Red
                foreach ($Issue in $EncodingIssues) {
                    Write-Host "      Line $($Issue.LineNumber): $($Issue.Issue)" -ForegroundColor Red
                    Write-Host "        Characters: $($Issue.ProblematicChars)" -ForegroundColor Gray
                }
            }

            # YAML issues
            if ($YamlIssues.Count -gt 0) {
                Write-Host "    📝 YAML Issues:" -ForegroundColor Yellow
                foreach ($Issue in $YamlIssues) {
                    Write-Host "      Line $($Issue.LineNumber): $($Issue.Issue)" -ForegroundColor Yellow
                    if ($Issue.Fix) {
                        Write-Host "        Fix: $($Issue.Fix)" -ForegroundColor Green
                    }
                }
            }

            # Actions issues
            if ($ActionsIssues.Count -gt 0) {
                Write-Host "    📝 Actions Issues:" -ForegroundColor Magenta
                foreach ($Issue in $ActionsIssues) {
                    Write-Host "      $($Issue.Issue)" -ForegroundColor Magenta
                    Write-Host "        Suggestion: $($Issue.Suggestion)" -ForegroundColor Blue
                }
            }
        }

        # Store issues for fixing
        $IssuesFound += [PSCustomObject]@{
            File           = $File.FullName
            FileName       = $File.Name
            EncodingIssues = $EncodingIssues
            YamlIssues     = $YamlIssues
            ActionsIssues  = $ActionsIssues
            TotalIssues    = $TotalIssues
        }

        # Test fetchability if requested
        if ($ValidateFetchability) {
            Write-Host "  🌐 Testing fetchability..." -ForegroundColor Blue
            $FetchResult = Test-FileFetchability -FilePath $File.FullName -Repo $GitHubRepo -Branch $Branch
            $FetchabilityResults += $FetchResult

            Write-Host "    $($FetchResult.Status)" -ForegroundColor $(if ($FetchResult.Status -like "*✅*") { "Green" } else { "Red" })
            if ($FetchResult.Issues.Count -gt 0) {
                foreach ($Issue in $FetchResult.Issues) {
                    Write-Host "      ⚠️  $Issue" -ForegroundColor Yellow
                }
            }
            Write-Host "    🔗 Raw URL: $($FetchResult.RawUrl)" -ForegroundColor Gray
        }

    }
    catch {
        Write-Host "  ❌ Error validating file: $($_.Exception.Message)" -ForegroundColor Red
        $IssuesFound += [PSCustomObject]@{
            File        = $File.FullName
            FileName    = $File.Name
            Error       = $_.Exception.Message
            TotalIssues = 1
        }
    }

    Write-Host ""
}

# Summary
$TotalFiles = $WorkflowFiles.Count
$FilesWithIssues = ($IssuesFound | Where-Object { $_.TotalIssues -gt 0 }).Count
$TotalIssues = ($IssuesFound | Measure-Object -Property TotalIssues -Sum).Sum

Write-Host "📊 Validation Summary" -ForegroundColor Cyan
Write-Host "Files Validated: $TotalFiles" -ForegroundColor White
Write-Host "Files with Issues: $FilesWithIssues" -ForegroundColor White
Write-Host "Total Issues Found: $TotalIssues" -ForegroundColor White

# Fetchability summary
if ($ValidateFetchability) {
    $FetchableFiles = ($FetchabilityResults | Where-Object { $_.Status -like "*✅*" }).Count
    $UnfetchableFiles = $FetchabilityResults.Count - $FetchableFiles

    Write-Host ""
    Write-Host "🌐 Fetchability Summary" -ForegroundColor Cyan
    Write-Host "Files Tested: $($FetchabilityResults.Count)" -ForegroundColor White
    Write-Host "Fetchable: $FetchableFiles" -ForegroundColor Green
    Write-Host "Not Fetchable: $UnfetchableFiles" -ForegroundColor Red

    if ($UnfetchableFiles -gt 0) {
        Write-Host ""
        Write-Host "💡 Fetchability Tips:" -ForegroundColor Yellow
        Write-Host "  • Ensure repository is public for external access" -ForegroundColor White
        Write-Host "  • Commit and push all files to make them available" -ForegroundColor White
        Write-Host "  • Check for typos in repository name or branch" -ForegroundColor White
        Write-Host "  • Wait 5-10 minutes after push for GitHub cache refresh" -ForegroundColor White
    }
}

if ($TotalIssues -eq 0 -and (-not $ValidateFetchability -or $UnfetchableFiles -eq 0)) {
    Write-Host ""
    Write-Host "✅ All workflow files are valid and fetchable!" -ForegroundColor Green
    return
}

# Apply fixes if requested
if ($FixIssues) {
    Write-Host ""
    Write-Host "🔧 Applying fixes..." -ForegroundColor Cyan

    foreach ($FileIssue in $IssuesFound | Where-Object { $_.TotalIssues -gt 0 -and -not $_.Error }) {
        Write-Host "🔧 Fixing: $($FileIssue.FileName)" -ForegroundColor Blue

        $Content = Get-Content -Path $FileIssue.File -Raw -Encoding UTF8
        $Lines = Get-Content -Path $FileIssue.File -Encoding UTF8
        $Modified = $false

        # Fix encoding issues
        if ($FileIssue.EncodingIssues.Count -gt 0) {
            Write-Host "  📝 Fixing encoding issues..." -ForegroundColor Yellow

            # Common emoji/character replacements for encoding issues
            $Content = $Content -replace 'ðŸ"¥', '🔥'
            $Content = $Content -replace 'ðŸ§ª', '🧪'
            $Content = $Content -replace 'ðŸ"¦', '📦'
            $Content = $Content -replace 'ðŸŽ‰', '🎉'
            $Content = $Content -replace 'âœ…', '✅'
            $Content = $Content -replace 'ðŸš€', '🚀'
            $Content = $Content -replace 'ðŸ"', '📋'
            $Content = $Content -replace 'ðŸ"Š', '📊'
            $Content = $Content -replace 'ðŸ›¡ï¸', '🛡️'

            $Modified = $true
        }

        # Fix YAML issues
        if ($FileIssue.YamlIssues.Count -gt 0) {
            Write-Host "  📝 Fixing YAML issues..." -ForegroundColor Yellow

            foreach ($Issue in $FileIssue.YamlIssues) {
                if ($Issue.Fix) {
                    $Lines[$Issue.LineNumber - 1] = $Issue.Fix
                    $Modified = $true
                }
            }

            if ($Modified) {
                $Content = $Lines -join "`n"
            }
        }

        # Save fixed content
        if ($Modified) {
            # Create backup
            $BackupPath = "$($FileIssue.File).backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Copy-Item -Path $FileIssue.File -Destination $BackupPath

            # Save fixed content
            Set-Content -Path $FileIssue.File -Value $Content -Encoding UTF8

            Write-Host "  ✅ Fixed and saved. Backup: $([System.IO.Path]::GetFileName($BackupPath))" -ForegroundColor Green
        }
        else {
            Write-Host "  ℹ️  No automatic fixes available" -ForegroundColor Blue
        }
    }

    Write-Host ""
    Write-Host "✅ Fix process complete!" -ForegroundColor Green

    # Generate .gitattributes if it doesn't exist and we're fixing issues
    if (-not (Test-Path ".gitattributes")) {
        Write-Host ""
        Write-Host "📝 Creating .gitattributes for better fetchability..." -ForegroundColor Yellow

        $GitAttributesContent = @"
# Auto detect text files and perform LF normalization
* text=auto

# Explicitly declare text files you want to always be normalized and converted
# to native line endings on checkout.
*.cs text
*.xaml text
*.xml text
*.json text
*.md text
*.yml text
*.yaml text
*.ps1 text
*.bat text eol=crlf
*.cmd text eol=crlf

# Declare files that will always have CRLF line endings on checkout.
*.sln text eol=crlf

# Declare files that will always have LF line endings on checkout.
*.sh text eol=lf

# Denote all files that are truly binary and should not be modified.
*.png binary
*.jpg binary
*.jpeg binary
*.gif binary
*.ico binary
*.pdf binary
*.zip binary
*.7z binary
*.dll binary
*.exe binary
"@

        Set-Content -Path ".gitattributes" -Value $GitAttributesContent -Encoding UTF8
        Write-Host "  ✅ .gitattributes created for consistent line endings" -ForegroundColor Green
    }
}
else {
    Write-Host ""
    Write-Host "ℹ️  Run with -FixIssues to apply automatic fixes" -ForegroundColor Blue
}

Write-Host ""
Write-Host "🔗 Additional Resources:" -ForegroundColor Cyan
Write-Host "   • Test workflows locally: act (GitHub Actions runner)" -ForegroundColor White
Write-Host "     https://github.com/nektos/act" -ForegroundColor Gray
Write-Host "   • GitHub Raw URLs format:" -ForegroundColor White
Write-Host "     https://raw.githubusercontent.com/{user}/{repo}/{branch}/{path}" -ForegroundColor Gray
Write-Host "   • Check repo visibility: https://github.com/$GitHubRepo" -ForegroundColor White

if ($ValidateFetchability) {
    Write-Host ""
    Write-Host "💡 Pro Tips for Better Fetchability:" -ForegroundColor Blue
    Write-Host "   • Use 'git status' to check uncommitted files" -ForegroundColor White
    Write-Host "   • Test fetch with: curl -s https://raw.githubusercontent.com/$GitHubRepo/$Branch/README.md" -ForegroundColor White
    Write-Host "   • PowerShell test: Invoke-WebRequest -Uri <raw-url>" -ForegroundColor White
    Write-Host "   • Add bb-fetch command to PowerShell profile for automation" -ForegroundColor White
}

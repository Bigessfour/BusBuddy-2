# Setup-CodecovToken.ps1
# Bus Buddy - Codecov Integration Setup Script

<#
.SYNOPSIS
    Helps set up Codecov integration for Bus Buddy repository

.DESCRIPTION
    This script provides guidance and automation for setting up Codecov integration
    including token configuration and initial repository setup.

.EXAMPLE
    .\Setup-CodecovToken.ps1

.EXAMPLE
    .\Setup-CodecovToken.ps1 -OpenBrowser -ShowInstructions
#>

param(
    [switch]$OpenBrowser,
    [switch]$ShowInstructions,
    [string]$RepoOwner = "Bigessfour",
    [string]$RepoName = "BusBuddy-WPF"
)

function Write-StepHeader {
    param([string]$Title)
    Write-Host "`n🔧 $Title" -ForegroundColor Cyan
    Write-Host ("=" * ($Title.Length + 4)) -ForegroundColor Cyan
}

function Write-StepInfo {
    param([string]$Message)
    Write-Host "   💡 $Message" -ForegroundColor Yellow
}

function Write-StepSuccess {
    param([string]$Message)
    Write-Host "   ✅ $Message" -ForegroundColor Green
}

function Write-StepWarning {
    param([string]$Message)
    Write-Host "   ⚠️ $Message" -ForegroundColor Yellow
}

function Test-CodecovToken {
    <#
    .SYNOPSIS
        Tests if Codecov token is configured in GitHub repository secrets
    #>

    Write-StepHeader "Testing Codecov Configuration"

    # Check if GitHub CLI is available
    $ghCli = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $ghCli) {
        Write-StepWarning "GitHub CLI not found. Cannot test token configuration automatically."
        Write-StepInfo "Install GitHub CLI: winget install GitHub.cli"
        return $false
    }

    try {
        # Test if authenticated
        $authStatus = gh auth status 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-StepWarning "GitHub CLI not authenticated. Run: gh auth login"
            return $false
        }

        Write-StepSuccess "GitHub CLI authenticated"

        # Check if we can access the repository
        $repoInfo = gh repo view "$RepoOwner/$RepoName" --json name 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-StepWarning "Cannot access repository $RepoOwner/$RepoName"
            return $false
        }

        Write-StepSuccess "Repository access confirmed"

        # Note: GitHub CLI cannot read secret values for security reasons
        Write-StepInfo "Secret values cannot be read via CLI for security reasons"
        Write-StepInfo "Check GitHub repository settings manually for CODECOV_TOKEN"

        return $true
    }
    catch {
        Write-StepWarning "Error testing configuration: $($_.Exception.Message)"
        return $false
    }
}

function Show-CodecovSetupInstructions {
    <#
    .SYNOPSIS
        Displays comprehensive setup instructions for Codecov integration
    #>

    Write-StepHeader "Codecov Setup Instructions"

    Write-Host "`n📋 Step 1: Create Codecov Account" -ForegroundColor Magenta
    Write-StepInfo "1. Visit https://codecov.io"
    Write-StepInfo "2. Click 'Sign up with GitHub'"
    Write-StepInfo "3. Authorize Codecov to access your GitHub account"

    Write-Host "`n📋 Step 2: Add Repository to Codecov" -ForegroundColor Magenta
    Write-StepInfo "1. In Codecov dashboard, click '+ Add new repository'"
    Write-StepInfo "2. Find and select '$RepoOwner/$RepoName'"
    Write-StepInfo "3. Click 'Setup repo' to configure"

    Write-Host "`n📋 Step 3: Get Repository Token" -ForegroundColor Magenta
    Write-StepInfo "1. In Codecov, go to your repository settings"
    Write-StepInfo "2. Find the 'Repository Upload Token' section"
    Write-StepInfo "3. Copy the token (format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)"

    Write-Host "`n📋 Step 4: Add Token to GitHub Secrets" -ForegroundColor Magenta
    Write-StepInfo "1. Go to https://github.com/$RepoOwner/$RepoName/settings/secrets/actions"
    Write-StepInfo "2. Click 'New repository secret'"
    Write-StepInfo "3. Name: CODECOV_TOKEN"
    Write-StepInfo "4. Value: [paste your Codecov token]"
    Write-StepInfo "5. Click 'Add secret'"

    Write-Host "`n📋 Step 5: Verify Setup" -ForegroundColor Magenta
    Write-StepInfo "1. Push a commit to trigger GitHub Actions"
    Write-StepInfo "2. Check that the workflow completes without Codecov warnings"
    Write-StepInfo "3. Verify coverage reports appear in Codecov dashboard"

    if ($OpenBrowser) {
        Write-StepHeader "Opening Setup URLs"
        try {
            Start-Process "https://codecov.io"
            Start-Sleep 2
            Start-Process "https://github.com/$RepoOwner/$RepoName/settings/secrets/actions"
            Write-StepSuccess "Setup URLs opened in browser"
        }
        catch {
            Write-StepWarning "Could not open browser: $($_.Exception.Message)"
        }
    }
}

function Test-WorkflowStatus {
    <#
    .SYNOPSIS
        Checks the status of recent GitHub Actions workflows
    #>

    Write-StepHeader "Checking Workflow Status"

    $ghCli = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $ghCli) {
        Write-StepWarning "GitHub CLI not available for workflow status check"
        return
    }

    try {
        Write-StepInfo "Fetching recent workflow runs..."
        $workflows = gh run list --repo "$RepoOwner/$RepoName" --limit 5 --json status,conclusion,name,createdAt,htmlUrl | ConvertFrom-Json

        if ($workflows.Count -eq 0) {
            Write-StepWarning "No recent workflow runs found"
            return
        }

        Write-Host "`n📊 Recent Workflow Runs:" -ForegroundColor Cyan
        foreach ($workflow in $workflows) {
            $status = $workflow.status
            $conclusion = $workflow.conclusion
            $name = $workflow.name
            $date = [DateTime]::Parse($workflow.createdAt).ToString("yyyy-MM-dd HH:mm")

            $statusIcon = switch ($conclusion) {
                "success" { "✅" }
                "failure" { "❌" }
                "cancelled" { "⏸️" }
                default { "🔄" }
            }

            Write-Host "   $statusIcon $name - $status" -ForegroundColor $(if ($conclusion -eq "success") { "Green" } else { "Red" })
            Write-Host "      Date: $date" -ForegroundColor Gray
            Write-Host "      URL: $($workflow.htmlUrl)" -ForegroundColor Blue
        }
    }
    catch {
        Write-StepWarning "Error fetching workflow status: $($_.Exception.Message)"
    }
}

function Show-CodecovBadgeInfo {
    <#
    .SYNOPSIS
        Shows information about adding Codecov badges to README
    #>

    Write-StepHeader "Codecov Badge Setup"

    $badgeUrl = "https://codecov.io/gh/$RepoOwner/$RepoName/branch/main/graph/badge.svg"
    $markdownBadge = "[![codecov](https://codecov.io/gh/$RepoOwner/$RepoName/branch/main/graph/badge.svg)](https://codecov.io/gh/$RepoOwner/$RepoName)"

    Write-StepInfo "Add this badge to your README.md:"
    Write-Host "`n$markdownBadge`n" -ForegroundColor Green

    Write-StepInfo "Badge URL: $badgeUrl"
    Write-StepInfo "This will show coverage percentage and link to detailed reports"
}

# Main execution
Write-Host "🚌 Bus Buddy - Codecov Integration Setup" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

# Test current configuration
$isConfigured = Test-CodecovToken

if ($ShowInstructions -or -not $isConfigured) {
    Show-CodecovSetupInstructions
}

# Check workflow status
Test-WorkflowStatus

# Show badge information
Show-CodecovBadgeInfo

Write-StepHeader "Next Steps"
if (-not $isConfigured) {
    Write-StepInfo "1. Follow the setup instructions above"
    Write-StepInfo "2. Add the CODECOV_TOKEN to GitHub repository secrets"
    Write-StepInfo "3. Push a commit to test the integration"
    Write-StepInfo "4. Run this script again to verify setup"
} else {
    Write-StepSuccess "Codecov appears to be configured!"
    Write-StepInfo "1. Push a commit to test coverage upload"
    Write-StepInfo "2. Check Codecov dashboard for coverage reports"
    Write-StepInfo "3. Consider adding the coverage badge to README.md"
}

Write-Host "`n✨ Setup complete! For support, check the Bus Buddy documentation." -ForegroundColor Green

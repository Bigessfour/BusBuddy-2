#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy AI workflow integration functions

.DESCRIPTION
    Provides AI-enhanced development workflows including intelligent search,
    context-aware assistance, and technology-specific guidance for BusBuddy development.
#>

function Start-BusBuddyAIWorkflow {
    <#
    .SYNOPSIS
        Start an AI-enhanced development workflow session

    .DESCRIPTION
        Combines traditional BusBuddy development commands with AI-powered search
        and guidance for enhanced productivity and learning.

    .PARAMETER WorkflowType
        Type of AI workflow to start

    .PARAMETER Technology
        Technology focus for the session

    .PARAMETER Interactive
        Enable interactive mode with prompts

    .EXAMPLE
        Start-BusBuddyAIWorkflow -WorkflowType Research -Technology WPF

    .EXAMPLE
        bb-ai-workflow -WorkflowType Troubleshooting -Interactive
    #>
    [CmdletBinding()]
    param(
        [ValidateSet('Research', 'Troubleshooting', 'Learning', 'Planning')]
        [string]$WorkflowType = 'Research',

        [ValidateSet('PowerShell', 'WPF', 'DotNet', 'Azure', 'EntityFramework', 'Syncfusion', 'MVVM', 'Testing')]
        [string]$Technology,

        [switch]$Interactive
    )

    Write-BusBuddyStatus "🤖 Starting AI-Enhanced Development Workflow: $WorkflowType" -Status Info
    Write-Host "══════════════════════════════════════════════════════════" -ForegroundColor DarkGray

    switch ($WorkflowType) {
        'Research' {
            Write-Host "🔍 Research Workflow - Find solutions and best practices" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Available commands:" -ForegroundColor Yellow
            Write-Host "  bb-tavily-search '<query>'        # Search with Tavily Expert" -ForegroundColor Green
            Write-Host "  bb-mentor -Topic <technology>     # Get learning guidance" -ForegroundColor Green
            Write-Host "  bb-docs -Technology <tech>        # Search official docs" -ForegroundColor Green

            if ($Interactive) {
                $query = Read-Host "Enter your research query"
                if ($query) {
                    Invoke-BusBuddyTavilySearch -Query $query -Technology $Technology -IncludeDetails
                }
            }
        }

        'Troubleshooting' {
            Write-Host "🔧 Troubleshooting Workflow - Diagnose and fix issues" -ForegroundColor Red
            Write-Host ""
            Write-Host "Available commands:" -ForegroundColor Yellow
            Write-Host "  bb-health                         # Check environment" -ForegroundColor Green
            Write-Host "  bb-error-fix                      # Analyze build errors" -ForegroundColor Green
            Write-Host "  bb-tavily-search 'error message'  # Search for solutions" -ForegroundColor Green
            Write-Host "  bb-warning-analysis               # Analyze warnings" -ForegroundColor Green

            if ($Interactive) {
                Write-Host ""
                Write-Host "Running environment health check..." -ForegroundColor Yellow
                Invoke-BusBuddyHealthCheck -Quick
            }
        }

        'Learning' {
            Write-Host "📚 Learning Workflow - Master BusBuddy technologies" -ForegroundColor Blue
            Write-Host ""
            Write-Host "Available commands:" -ForegroundColor Yellow
            Write-Host "  bb-mentor -Topic <technology>     # Interactive learning" -ForegroundColor Green
            Write-Host "  bb-tavily-search '<concept>'      # Research concepts" -ForegroundColor Green
            Write-Host "  bb-ref <technology>               # Quick reference" -ForegroundColor Green
            Write-Host "  bb-happiness                      # Get motivated! 😊" -ForegroundColor Green

            if ($Interactive -and $Technology) {
                Get-BusBuddyMentor -Topic $Technology -IncludeExamples
            }
        }

        'Planning' {
            Write-Host "📋 Planning Workflow - Plan features and architecture" -ForegroundColor Magenta
            Write-Host ""
            Write-Host "Available commands:" -ForegroundColor Yellow
            Write-Host "  bb-tavily-search 'architecture patterns'  # Research patterns" -ForegroundColor Green
            Write-Host "  bb-mentor -Topic Architecture             # Get guidance" -ForegroundColor Green
            Write-Host "  bb-dev-workflow                           # Full dev cycle" -ForegroundColor Green

            if ($Interactive) {
                $feature = Read-Host "What feature are you planning to implement?"
                if ($feature) {
                    $searchQuery = "$feature implementation best practices"
                    if ($Technology) {
                        $searchQuery += " $Technology"
                    }
                    Invoke-BusBuddyTavilySearch -Query $searchQuery -Technology $Technology
                }
            }
        }
    }

    Write-Host ""
    Write-Host "💡 Workflow Tips:" -ForegroundColor Blue
    Write-Host "  • Use bb-tavily-search for real-time web research" -ForegroundColor Cyan
    Write-Host "  • Combine with bb-mentor for structured learning" -ForegroundColor Cyan
    Write-Host "  • Save important results with -SaveResults flag" -ForegroundColor Cyan
    Write-Host "  • Use -OutputFormat Markdown for documentation" -ForegroundColor Cyan
}

function Get-BusBuddyAIAssistance {
    <#
    .SYNOPSIS
        Get AI assistance for specific development tasks

    .DESCRIPTION
        Provides contextual AI assistance for common BusBuddy development scenarios
        by combining search, documentation, and interactive guidance.

    .PARAMETER Task
        The development task you need help with

    .PARAMETER ErrorMessage
        Specific error message to research

    .PARAMETER Technology
        Technology context for the assistance

    .PARAMETER QuickHelp
        Get quick help without full research

    .EXAMPLE
        Get-BusBuddyAIAssistance -Task "MVVM data binding"

    .EXAMPLE
        bb-ai-help -ErrorMessage "CS8600 nullable reference" -Technology DotNet

    .EXAMPLE
        bb-ai-help -Task "Syncfusion DataGrid" -Technology WPF
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [string]$Task,

        [string]$ErrorMessage,

        [ValidateSet('PowerShell', 'WPF', 'DotNet', 'Azure', 'EntityFramework', 'Syncfusion', 'MVVM', 'Testing')]
        [string]$Technology,

        [switch]$QuickHelp
    )

    if ($ErrorMessage) {
        Write-BusBuddyStatus "🔍 Researching error: $ErrorMessage" -Status Info

        # Search for error solutions
        $searchQuery = "fix error $ErrorMessage"
        if ($Technology) {
            $searchQuery += " $Technology"
        }

        Invoke-BusBuddyTavilySearch -Query $searchQuery -Technology $Technology -MaxResults 3

        # Also run local error analysis if it's a build error
        if ($ErrorMessage -match "CS\d+|error") {
            Write-Host ""
            Write-Host "🔧 Running local error analysis..." -ForegroundColor Yellow
            Invoke-BusBuddyErrorAnalysis
        }
    }
    elseif ($Task) {
        Write-BusBuddyStatus "💡 Getting assistance for: $Task" -Status Info

        if ($QuickHelp) {
            # Quick reference lookup
            if ($Technology) {
                Get-QuickReference -Technology $Technology
            }

            # Quick search
            Invoke-BusBuddyTavilySearch -Query "$Task tutorial" -Technology $Technology -MaxResults 2
        }
        else {
            # Comprehensive assistance
            Write-Host "🔍 Searching for detailed guidance..." -ForegroundColor Cyan
            Invoke-BusBuddyTavilySearch -Query "$Task best practices examples" -Technology $Technology -IncludeDetails

            Write-Host ""
            Write-Host "📚 Related learning resources:" -ForegroundColor Yellow
            if ($Technology) {
                Get-BusBuddyMentor -Topic $Technology
            }
        }
    }
    else {
        Write-Host "🤖 BusBuddy AI Assistant" -ForegroundColor Cyan
        Write-Host "═══════════════════════════" -ForegroundColor DarkCyan
        Write-Host ""
        Write-Host "Usage examples:" -ForegroundColor Yellow
        Write-Host "  bb-ai-help 'MVVM data binding'                    # Get task help" -ForegroundColor Green
        Write-Host "  bb-ai-help -ErrorMessage 'CS8600' -Technology DotNet  # Research error" -ForegroundColor Green
        Write-Host "  bb-ai-help 'Syncfusion DataGrid' -QuickHelp       # Quick reference" -ForegroundColor Green
        Write-Host ""
        Write-Host "Available workflows:" -ForegroundColor Yellow
        Write-Host "  bb-ai-workflow -WorkflowType Research             # Research workflow" -ForegroundColor Green
        Write-Host "  bb-ai-workflow -WorkflowType Troubleshooting      # Debug workflow" -ForegroundColor Green
        Write-Host "  bb-ai-workflow -WorkflowType Learning             # Learning workflow" -ForegroundColor Green
    }
}

function Get-BusBuddyContextualSearch {
    <#
    .SYNOPSIS
        Perform contextual search based on current development state

    .DESCRIPTION
        Analyzes the current BusBuddy project state and provides contextual search suggestions
        and automated research based on recent errors, current tasks, and project status.

    .PARAMETER AutoDetect
        Automatically detect context from project state

    .PARAMETER IncludeProjectAnalysis
        Include analysis of current project files

    .EXAMPLE
        Get-BusBuddyContextualSearch -AutoDetect

    .EXAMPLE
        bb-context-search -IncludeProjectAnalysis
    #>
    [CmdletBinding()]
    param(
        [switch]$AutoDetect,
        [switch]$IncludeProjectAnalysis
    )

    Write-BusBuddyStatus "🎯 Contextual Search Analysis" -Status Info
    Write-Host ""

    $context = @{
        BuildStatus = $null
        RecentErrors = @()
        ProjectPhase = "Unknown"
        SuggestedSearches = @()
    }

    if ($AutoDetect) {
        Write-Host "🔍 Analyzing current project state..." -ForegroundColor Yellow

        # Check build status
        $projectRoot = Get-BusBuddyProjectRoot
        if ($projectRoot) {
            Push-Location $projectRoot
            try {
                $buildOutput = dotnet build BusBuddy.sln --verbosity quiet 2>&1
                $context.BuildStatus = if ($LASTEXITCODE -eq 0) { "Success" } else { "Failed" }

                if ($context.BuildStatus -eq "Failed") {
                    $context.RecentErrors = $buildOutput | Select-String -Pattern "error" | Select-Object -First 3
                }
            }
            finally {
                Pop-Location
            }
        }

        # Determine project phase
        if ($context.BuildStatus -eq "Failed") {
            $context.ProjectPhase = "Build Issues"
            $context.SuggestedSearches += "fix build errors"
            $context.SuggestedSearches += "dotnet compilation issues"
        }
        elseif ($context.BuildStatus -eq "Success") {
            $context.ProjectPhase = "Development Ready"
            $context.SuggestedSearches += "WPF MVVM best practices"
            $context.SuggestedSearches += "Entity Framework optimization"
            $context.SuggestedSearches += "Syncfusion advanced features"
        }

        # Display context analysis
        Write-Host "📊 Current Context:" -ForegroundColor Cyan
        Write-Host "  Build Status: $($context.BuildStatus)" -ForegroundColor $(if ($context.BuildStatus -eq "Success") { "Green" } else { "Red" })
        Write-Host "  Project Phase: $($context.ProjectPhase)" -ForegroundColor Yellow

        if ($context.RecentErrors.Count -gt 0) {
            Write-Host "  Recent Errors: $($context.RecentErrors.Count)" -ForegroundColor Red
            foreach ($errorItem in $context.RecentErrors) {
                Write-Host "    • $($errorItem.Line.Trim())" -ForegroundColor Gray
            }
        }

        Write-Host ""
        Write-Host "💡 Contextual Search Suggestions:" -ForegroundColor Blue
        foreach ($suggestion in $context.SuggestedSearches) {
            Write-Host "  bb-tavily-search '$suggestion'" -ForegroundColor Green
        }

        # Auto-run first suggestion if errors exist
        if ($context.RecentErrors.Count -gt 0 -and $context.SuggestedSearches.Count -gt 0) {
            Write-Host ""
            Write-Host "🚀 Auto-searching for solutions..." -ForegroundColor Magenta
            Invoke-BusBuddyTavilySearch -Query $context.SuggestedSearches[0] -MaxResults 3
        }
    }

    if ($IncludeProjectAnalysis) {
        Write-Host ""
        Write-Host "📁 Project Analysis:" -ForegroundColor Cyan

        # Analyze recent file changes
        $projectRoot = Get-BusBuddyProjectRoot
        if ($projectRoot) {
            Push-Location $projectRoot
            try {
                $recentFiles = git log --name-only --since="1 week ago" --pretty=format: |
                    Where-Object { $_ -match '\.(cs|xaml|ps1)$' } |
                    Group-Object |
                    Sort-Object Count -Descending |
                    Select-Object -First 5

                if ($recentFiles) {
                    Write-Host "  Most Modified Files (last week):" -ForegroundColor Yellow
                    foreach ($file in $recentFiles) {
                        Write-Host "    • $($file.Name) ($($file.Count) changes)" -ForegroundColor Gray

                        # Suggest searches based on file types
                        if ($file.Name -match '\.xaml$') {
                            $context.SuggestedSearches += "WPF XAML best practices"
                        }
                        elseif ($file.Name -match 'ViewModel\.cs$') {
                            $context.SuggestedSearches += "MVVM ViewModel patterns"
                        }
                        elseif ($file.Name -match '\.ps1$') {
                            $context.SuggestedSearches += "PowerShell 7.5 advanced features"
                        }
                    }
                }
            }
            catch {
                Write-Host "  Git analysis not available" -ForegroundColor Gray
            }
            finally {
                Pop-Location
            }
        }
    }

    return $context
}

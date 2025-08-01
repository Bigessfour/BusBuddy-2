#Requires -Version 7.5
<#
.SYNOPSIS
    Tavily Expert search integration for BusBuddy development workflows

.DESCRIPTION
    Integrates Tavily Expert search capabilities into BusBuddy PowerShell module.
    Provides AI-powered web search for development research, troubleshooting,
    and technology guidance using the configured Tavily Expert MCP server.

.PARAMETER Query
    Search query for Tavily Expert

.PARAMETER Technology
    Focus search on specific technology (PowerShell, WPF, Azure, etc.)

.PARAMETER MaxResults
    Maximum number of search results to return (default: 5)

.PARAMETER IncludeDetails
    Include detailed excerpts from search results

.PARAMETER OutputFormat
    Output format: Text, Json, Markdown

.PARAMETER SaveResults
    Save results to file for later reference

.PARAMETER Context
    Additional context for the search (BusBuddy-specific)

.EXAMPLE
    Invoke-BusBuddyTavilySearch -Query "WPF MVVM best practices"

.EXAMPLE
    bb-tavily-search "Entity Framework migrations" -Technology "DotNet" -IncludeDetails

.EXAMPLE
    bb-tavily-search "PowerShell 7.5 parallel processing" -OutputFormat Markdown -SaveResults
#>
function Invoke-BusBuddyTavilySearch {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory, Position = 0)]
        [string]$Query,

        [ValidateSet('PowerShell', 'WPF', 'DotNet', 'Azure', 'EntityFramework', 'Syncfusion', 'MVVM', 'Testing')]
        [string]$Technology,

        [ValidateRange(1, 20)]
        [int]$MaxResults = 5,

        [switch]$IncludeDetails,

        [ValidateSet('Text', 'Json', 'Markdown')]
        [string]$OutputFormat = 'Text',

        [switch]$SaveResults,

        [string]$Context = "BusBuddy WPF application development"
    )

    # Check if Tavily API key is available
    $apiKey = $env:TAVILY_API_KEY
    if (-not $apiKey) {
        Write-BusBuddyError -Message "TAVILY_API_KEY environment variable not set" -RecommendedAction "Set your Tavily API key: `$env:TAVILY_API_KEY = 'your-key'"
        return $null
    }

    Write-BusBuddyStatus "🔍 Searching with Tavily Expert..." -Status Info

    try {
        # Enhance query with technology context if specified
        $enhancedQuery = if ($Technology) {
            "$Query $Technology development"
        } else {
            $Query
        }

        # Add BusBuddy context
        if ($Context) {
            $enhancedQuery += " in context of $Context"
        }

        # Prepare Tavily API request
        $tavilyEndpoint = "https://api.tavily.com/search"

        $requestBody = @{
            api_key = $apiKey
            query = $enhancedQuery
            search_depth = "advanced"
            include_answer = $true
            include_raw_content = $IncludeDetails
            max_results = $MaxResults
        } | ConvertTo-Json -Depth 3

        Write-Verbose "Tavily search query: $enhancedQuery"

        # Make API request
        $headers = @{
            'Content-Type' = 'application/json'
            'User-Agent' = 'BusBuddy-PowerShell/1.0'
        }

        $response = Invoke-RestMethod -Uri $tavilyEndpoint -Method Post -Body $requestBody -Headers $headers -TimeoutSec 30

        if (-not $response -or -not $response.results) {
            Write-BusBuddyStatus "⚠️ No results found for query: $Query" -Status Warning
            return $null
        }

        # Process results
        $searchResults = @{
            Query = $Query
            Technology = $Technology
            Context = $Context
            Timestamp = Get-Date
            ResultCount = $response.results.Count
            Answer = $response.answer
            Results = @()
        }

        foreach ($result in $response.results) {
            $processedResult = @{
                Title = $result.title
                Url = $result.url
                Content = $result.content
                Score = $result.score
                PublishedDate = $result.published_date
            }

            if ($IncludeDetails -and $result.raw_content) {
                $processedResult.RawContent = $result.raw_content
            }

            $searchResults.Results += $processedResult
        }

        # Output results based on format
        switch ($OutputFormat) {
            'Json' {
                $jsonOutput = $searchResults | ConvertTo-Json -Depth 4
                Write-Host $jsonOutput

                if ($SaveResults) {
                    $filename = "tavily-search-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
                    $jsonOutput | Out-File $filename -Encoding UTF8
                    Write-BusBuddyStatus "📄 Results saved to: $filename" -Status Info
                }

                return $searchResults
            }

            'Markdown' {
                $markdownOutput = Format-TavilyResultsMarkdown -SearchResults $searchResults
                Write-Host $markdownOutput

                if ($SaveResults) {
                    $filename = "tavily-search-$(Get-Date -Format 'yyyyMMdd-HHmmss').md"
                    $markdownOutput | Out-File $filename -Encoding UTF8
                    Write-BusBuddyStatus "📄 Results saved to: $filename" -Status Info
                }

                return $searchResults
            }

            default { # Text
                Format-TavilyResultsSummary -SearchResults $searchResults

                if ($SaveResults) {
                    $filename = "tavily-search-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
                    $textOutput = Format-TavilyResultsSummary -SearchResults $searchResults -AsText
                    $textOutput | Out-File $filename -Encoding UTF8
                    Write-BusBuddyStatus "📄 Results saved to: $filename" -Status Info
                }

                return $searchResults
            }
        }
    }
    catch {
        Write-BusBuddyError -Message "Tavily search failed: $($_.Exception.Message)" -Exception $_.Exception -RecommendedAction "Check API key and internet connectivity"
        return $null
    }
}

function Format-TavilyResultsSummary {
    <#
    .SYNOPSIS
        Format Tavily search results for console display
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [hashtable]$SearchResults,

        [switch]$AsText
    )

    $output = @()

    $output += ""
    $output += "🔍 Tavily Expert Search Results"
    $output += "═══════════════════════════════════════"
    $output += "Query: $($SearchResults.Query)"
    if ($SearchResults.Technology) {
        $output += "Technology Focus: $($SearchResults.Technology)"
    }
    $output += "Results: $($SearchResults.ResultCount)"
    $output += "Timestamp: $($SearchResults.Timestamp.ToString('yyyy-MM-dd HH:mm:ss'))"
    $output += ""

    # Display AI answer if available
    if ($SearchResults.Answer) {
        $output += "🤖 AI Summary:"
        $output += "─────────────"
        $output += $SearchResults.Answer
        $output += ""
    }

    # Display top results
    $output += "📚 Top Results:"
    $output += "──────────────"

    for ($i = 0; $i -lt [Math]::Min(5, $SearchResults.Results.Count); $i++) {
        $result = $SearchResults.Results[$i]
        $output += ""
        $output += "[$($i + 1)] $($result.Title)"
        $output += "    🔗 $($result.Url)"
        if ($result.Score) {
            $output += "    📊 Relevance: $([math]::Round($result.Score * 100, 1))%"
        }
        if ($result.Content) {
            $contentPreview = $result.Content.Substring(0, [Math]::Min(200, $result.Content.Length))
            if ($result.Content.Length -gt 200) { $contentPreview += "..." }
            $output += "    📄 $contentPreview"
        }
    }

    $output += ""
    $output += "💡 Use -IncludeDetails for full content, -OutputFormat Markdown for formatted output"
    $output += ""

    if ($AsText) {
        return $output -join "`n"
    } else {
        foreach ($line in $output) {
            if ($line.StartsWith("🔍")) {
                Write-Host $line -ForegroundColor Cyan
            } elseif ($line.StartsWith("🤖")) {
                Write-Host $line -ForegroundColor Green
            } elseif ($line.StartsWith("📚")) {
                Write-Host $line -ForegroundColor Yellow
            } elseif ($line.StartsWith("[$")) {
                Write-Host $line -ForegroundColor White
            } elseif ($line.StartsWith("    🔗")) {
                Write-Host $line -ForegroundColor Blue
            } elseif ($line.StartsWith("    📊")) {
                Write-Host $line -ForegroundColor Magenta
            } elseif ($line.StartsWith("    📄")) {
                Write-Host $line -ForegroundColor Gray
            } elseif ($line.StartsWith("💡")) {
                Write-Host $line -ForegroundColor Blue
            } else {
                Write-Host $line
            }
        }
    }
}

function Format-TavilyResultsMarkdown {
    <#
    .SYNOPSIS
        Format Tavily search results as Markdown
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [hashtable]$SearchResults
    )

    $markdown = @()

    $markdown += "# 🔍 Tavily Expert Search Results"
    $markdown += ""
    $markdown += "**Query:** $($SearchResults.Query)"
    if ($SearchResults.Technology) {
        $markdown += "**Technology Focus:** $($SearchResults.Technology)"
    }
    $markdown += "**Results:** $($SearchResults.ResultCount)"
    $markdown += "**Timestamp:** $($SearchResults.Timestamp.ToString('yyyy-MM-dd HH:mm:ss'))"
    $markdown += ""

    # AI Summary
    if ($SearchResults.Answer) {
        $markdown += "## 🤖 AI Summary"
        $markdown += ""
        $markdown += $SearchResults.Answer
        $markdown += ""
    }

    # Results
    $markdown += "## 📚 Search Results"
    $markdown += ""

    for ($i = 0; $i -lt $SearchResults.Results.Count; $i++) {
        $result = $SearchResults.Results[$i]
        $markdown += "### $($i + 1). $($result.Title)"
        $markdown += ""
        $markdown += "**URL:** [$($result.Url)]($($result.Url))"
        if ($result.Score) {
            $markdown += "**Relevance:** $([math]::Round($result.Score * 100, 1))%"
        }
        if ($result.PublishedDate) {
            $markdown += "**Published:** $($result.PublishedDate)"
        }
        $markdown += ""
        if ($result.Content) {
            $markdown += "**Summary:**"
            $markdown += $result.Content
            $markdown += ""
        }
        $markdown += "---"
        $markdown += ""
    }

    $markdown += "*Generated by BusBuddy Tavily Integration*"

    return $markdown -join "`n"
}

# Export the main function
Export-ModuleMember -Function Invoke-BusBuddyTavilySearch

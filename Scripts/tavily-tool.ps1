#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy Tavily integration tool for AI-enhanced search and information retrieval.

.DESCRIPTION
    This script provides integration with Tavily API for the BusBuddy project, enabling
    real-time search, document Q&A, and code-specific intelligence within PowerShell.

.NOTES
    File Name      : tavily-tool.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2+, Tavily API Key, Internet Connection

.EXAMPLE
    .\tavily-tool.ps1 -Search "bus routing algorithms"

    Performs a search for bus routing algorithms and returns relevant results.

.EXAMPLE
    .\tavily-tool.ps1 -CodeSearch "Syncfusion DockingManager"

    Performs a code-specific search for Syncfusion DockingManager documentation and examples.
#>

param (
    [Parameter(Mandatory = $false)]
    [string]$Search,

    [Parameter(Mandatory = $false)]
    [string]$CodeSearch,

    [Parameter(Mandatory = $false)]
    [string]$DocSearch,

    [Parameter(Mandatory = $false)]
    [string]$ApiKey,

    [Parameter(Mandatory = $false)]
    [switch]$Initialize,

    [Parameter(Mandatory = $false)]
    [switch]$ValidateSetup,

    [Parameter(Mandatory = $false)]
    [int]$MaxResults = 5,

    [Parameter(Mandatory = $false)]
    [switch]$IncludeMetadata,

    [Parameter(Mandatory = $false)]
    [switch]$Detailed
)

# Module validation and setup
function Initialize-TavilyEnvironment {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$LocalApiKey = $null
    )

    Write-Host "🔍 Initializing Tavily environment for BusBuddy..." -ForegroundColor Cyan

    # Check API key (in order of precedence)
    # 1. Function parameter (LocalApiKey)
    # 2. Script parameter (global $ApiKey)
    # 3. Environment variable (TAVILY_API_KEY)

    $tavilyApiKey = $env:TAVILY_API_KEY

    if (-not [string]::IsNullOrEmpty($LocalApiKey)) {
        # Use API key provided to function
        $env:TAVILY_API_KEY = $LocalApiKey
        Write-Host "✅ Tavily API key set from function parameter" -ForegroundColor Green
    }
    elseif (-not [string]::IsNullOrEmpty($global:ApiKey)) {
        # Use API key provided to script
        $env:TAVILY_API_KEY = $global:ApiKey
        Write-Host "✅ Tavily API key set from script parameter" -ForegroundColor Green
    }
    elseif (-not [string]::IsNullOrEmpty($tavilyApiKey)) {
        # API key already exists in environment variables
        Write-Host "✅ Using Tavily API key from environment variables" -ForegroundColor Green
    }
    else {
        Write-Warning "No Tavily API key found in environment or parameters!"
        Write-Host "Please set your Tavily API key using:" -ForegroundColor Yellow
        Write-Host '$env:TAVILY_API_KEY = "tvly-YOUR_API_KEY"' -ForegroundColor Yellow
        return $false
    }

    # Validate the API key format (should start with tvly-)
    if (-not $env:TAVILY_API_KEY.StartsWith("tvly-")) {
        Write-Warning "Tavily API key should start with 'tvly-'. Please check your API key format."
    }

    # Create cache directory if it doesn't exist
    $cacheDir = Join-Path $env:USERPROFILE ".busbuddy\tavily-cache"
    if (-not (Test-Path $cacheDir)) {
        New-Item -Path $cacheDir -ItemType Directory -Force | Out-Null
        Write-Host "✅ Created Tavily cache directory: $cacheDir" -ForegroundColor Green
    }

    # Verify network connectivity to Tavily API
    try {
        # Use the proper authorization header with the API key
        $headers = @{
            "Content-Type"  = "application/json"
            "Authorization" = "Bearer $($env:TAVILY_API_KEY)"
        }

        Write-Host "🔄 Testing connection to Tavily API..." -ForegroundColor Cyan

        # Instead of using the status endpoint, let's do a simple search test
        # This is more reliable than trying to guess the status endpoint structure
        $testBody = @{
            "query" = "test connection"
            "max_results" = 1
        } | ConvertTo-Json

        # According to https://tavily.tadata.com/docs/reference
        $testEndpoint = "https://api.tavily.com/v1/search"

        try {
            # Try primary endpoint
            $testResponse = Invoke-RestMethod -Uri $testEndpoint -Method POST -Headers $headers -Body $testBody -TimeoutSec 10 -ErrorAction Stop
            $global:TavilyConnectionValid = $true
            Write-Host "✅ Tavily API connection successful!" -ForegroundColor Green

            # If response has results, connection is definitely good
            if ($testResponse.results -and $testResponse.results.Count -gt 0) {
                Write-Host "   Confirmed with successful search response" -ForegroundColor Green
            }

            # Additional information if available
            if ($testResponse.PSObject.Properties.Name -contains "search_id") {
                Write-Host "   Search ID: $($testResponse.search_id)" -ForegroundColor Cyan
            }

            # Return true for successful validation
            $global:TavilyConnectionValid = $true
        }
        catch {
            $global:TavilyConnectionValid = $false
            Write-Warning "Could not connect to Tavily API: $($_.Exception.Message)"
            Write-Host "Error Details:" -ForegroundColor Red

            if ($_.Exception.Response) {
                $statusCode = $_.Exception.Response.StatusCode.value__
                Write-Host "   Status Code: $statusCode - $($_.Exception.Response.StatusDescription)" -ForegroundColor Red

                # Specific guidance based on status code
                switch ($statusCode) {
                    401 {
                        Write-Host "`nAuthentication Error: Your API key is invalid or expired." -ForegroundColor Red
                        Write-Host "Please verify your API key is correct and properly formatted (should start with 'tvly-')." -ForegroundColor Yellow
                        Write-Host "Visit https://tavily.tadata.com/signin to check your API key." -ForegroundColor Yellow
                    }
                    403 {
                        Write-Host "`nPermission Error: Your account doesn't have permission for this operation." -ForegroundColor Red
                        Write-Host "Please check your Tavily account permissions and plan limitations." -ForegroundColor Yellow
                    }
                    429 {
                        Write-Host "`nRate Limit Exceeded: You've made too many requests." -ForegroundColor Red
                        Write-Host "Please wait before making additional requests or upgrade your plan." -ForegroundColor Yellow
                    }
                    default {
                        Write-Host "`nAPI Error: Please check the Tavily API documentation for more information." -ForegroundColor Yellow
                        Write-Host "Documentation: https://tavily.tadata.com/docs/reference" -ForegroundColor Yellow
                    }
                }
            }

            Write-Host "API connection failed, but we'll proceed anyway. Some functionality may be limited." -ForegroundColor Yellow

            # For validate setup mode, provide more details
            if ($ValidateSetup) {
                Write-Host "`nTroubleshooting steps:" -ForegroundColor Yellow
                Write-Host "1. Verify your API key starts with 'tvly-'" -ForegroundColor Yellow
                Write-Host "2. Check your internet connection" -ForegroundColor Yellow
                Write-Host "3. Ensure the Tavily API service is available" -ForegroundColor Yellow
                Write-Host "4. Try setting the API key directly in your environment variables" -ForegroundColor Yellow
                Write-Host "   $env:TAVILY_API_KEY = 'tvly-your-key-here'" -ForegroundColor Yellow
                Write-Host "5. Visit https://tavily.tadata.com/docs/reference for current API documentation" -ForegroundColor Yellow
            }
        }
    }

    Write-Host "✅ Tavily environment initialized successfully" -ForegroundColor Green

    # Return true for successful initialization, even if API validation failed
    # This allows non-API operations to continue
    return $true
}

# Make Tavily API calls
function Invoke-TavilySearch {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Query,

        [Parameter(Mandatory = $false)]
        [ValidateSet("search", "code_search", "doc_search")]
        [string]$SearchType = "search",

        [Parameter(Mandatory = $false)]
        [int]$MaxResults = 5,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeMetadata
    )

    $apiKey = $env:TAVILY_API_KEY
    if ([string]::IsNullOrEmpty($apiKey)) {
        Write-Error "Tavily API key not set. Use Initialize-TavilyEnvironment first."
        return $null
    }

    # Use the API key from environment variables
    $endpoint = "https://api.tavily.com/v1/search"

    # Fallback endpoint without v1 prefix in case API structure changes
    $fallbackEndpoint = "https://api.tavily.com/search"

    $headers = @{
        "Content-Type"  = "application/json"
        "Authorization" = "Bearer $apiKey"
    }

    $body = @{
        "query"           = $Query
        "max_results"     = $MaxResults
        "include_domains" = @()
        "exclude_domains" = @()
        "search_depth"    = "advanced"
    }

    if ($SearchType -eq "code_search") {
        $body.search_depth = "moderate"
        $body.include_answer = $true
        $body.search_context = "technical and programming context, specifically focused on coding, software development, and technical documentation"
    }
    elseif ($SearchType -eq "doc_search") {
        $body.search_depth = "advanced"
        $body.include_answer = $true
        $body.search_context = "focus on technical documentation, user manuals, and explanatory content"
    }

    $bodyJson = $body | ConvertTo-Json

    try {
        $cacheDir = Join-Path $env:USERPROFILE ".busbuddy\tavily-cache"
        $cacheKey = [System.Security.Cryptography.SHA256]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes($bodyJson)) | ForEach-Object { $_.ToString("x2") } | Join-String
        $cacheFile = Join-Path $cacheDir "$cacheKey.json"

        # Check if we have a valid cache hit (less than 2 hours old)
        if (Test-Path $cacheFile) {
            $fileAge = (Get-Date) - (Get-Item $cacheFile).LastWriteTime
            if ($fileAge.TotalHours -lt 2) {
                $cachedResult = Get-Content $cacheFile -Raw | ConvertFrom-Json
                Write-Host "🔍 Retrieved from cache (age: $([math]::Round($fileAge.TotalMinutes)) minutes)" -ForegroundColor Cyan
                return $cachedResult
            }
        }

        $response = $null
        try {
            # First try the v1 endpoint
            $response = Invoke-RestMethod -Uri $endpoint -Method POST -Headers $headers -Body $bodyJson -ErrorAction Stop
            Write-Host "✅ Search completed using primary endpoint" -ForegroundColor Green
        }
        catch {
            Write-Host "⚠️ Primary endpoint failed, trying fallback..." -ForegroundColor Yellow
            # If that fails, try the fallback endpoint
            $response = Invoke-RestMethod -Uri $fallbackEndpoint -Method POST -Headers $headers -Body $bodyJson -ErrorAction Stop
            Write-Host "✅ Search completed using fallback endpoint" -ForegroundColor Green
        }

        # Cache the result
        $response | ConvertTo-Json -Depth 20 | Set-Content -Path $cacheFile -Force

        return $response
    }
    catch {
        Write-Error "Error querying Tavily API: $($_.Exception.Message)"

        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode.value__
            $statusDescription = $_.Exception.Response.StatusDescription

            Write-Host "HTTP Status: $statusCode - $statusDescription" -ForegroundColor Red

            # Handle specific status codes
            switch ($statusCode) {
                401 { Write-Host "Authentication failed. Please check your API key." -ForegroundColor Red }
                403 { Write-Host "API key does not have permission to access this resource." -ForegroundColor Red }
                429 { Write-Host "Rate limit exceeded. Please try again later." -ForegroundColor Red }
                default { Write-Host "Please check Tavily API documentation for more information." -ForegroundColor Red }
            }
        }

        return $null
    }
}

# Format search results nicely
function Format-TavilyResults {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [PSCustomObject]$Results,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeMetadata
    )

    if ($null -eq $Results) {
        Write-Host "No results found or error occurred." -ForegroundColor Yellow
        return
    }

    # Display answer if available
    if ($Results.answer) {
        Write-Host "`n📝 ANSWER:" -ForegroundColor Cyan
        Write-Host $Results.answer -ForegroundColor White
        Write-Host "`n" -ForegroundColor White
    }

    # Display results
    Write-Host "🔍 SEARCH RESULTS:" -ForegroundColor Cyan

    foreach ($result in $Results.results) {
        Write-Host "`n$($result.title)" -ForegroundColor Green
        Write-Host $result.url -ForegroundColor Blue

        if ($result.content) {
            $snippetText = if ($result.content.Length -gt 200) {
                $result.content.Substring(0, 200) + "..."
            }
            else {
                $result.content
            }
            Write-Host $snippetText -ForegroundColor Gray
        }

        if ($IncludeMetadata) {
            Write-Host "Score: $($result.score)" -ForegroundColor Yellow
        }

        Write-Host "-----------------------------------------" -ForegroundColor DarkGray
    }

    # Display metadata
    if ($IncludeMetadata) {
        Write-Host "`n📊 SEARCH METADATA:" -ForegroundColor Magenta
        Write-Host "Query: $($Results.query)" -ForegroundColor White
        Write-Host "Search ID: $($Results.search_id)" -ForegroundColor White
        Write-Host "Time Taken: $($Results.query_time_sec) seconds" -ForegroundColor White
    }
}

# Main execution logic
if ($ValidateSetup) {
    Write-Host "`n🔍 TAVILY API SETUP VALIDATION" -ForegroundColor Cyan
    Write-Host "================================`n" -ForegroundColor Cyan

    # Run the initialization
    Initialize-TavilyEnvironment

    # Display validation results
    Write-Host "`n📋 VALIDATION SUMMARY" -ForegroundColor Yellow
    Write-Host "================================" -ForegroundColor Yellow

    Write-Host "API Key Found:        " -NoNewline
    if (-not [string]::IsNullOrEmpty($env:TAVILY_API_KEY)) {
        Write-Host "✅ YES" -ForegroundColor Green

        # Check key format
        Write-Host "API Key Format:       " -NoNewline
        if ($env:TAVILY_API_KEY.StartsWith("tvly-")) {
            Write-Host "✅ VALID (starts with tvly-)" -ForegroundColor Green
        } else {
            Write-Host "⚠️ INVALID (should start with tvly-)" -ForegroundColor Red
        }

        # Mask the API key for display
        $maskedKey = $env:TAVILY_API_KEY.Substring(0, 8) + "..." + $env:TAVILY_API_KEY.Substring($env:TAVILY_API_KEY.Length - 4)
        Write-Host "API Key:             $maskedKey" -ForegroundColor Yellow
    } else {
        Write-Host "❌ NO" -ForegroundColor Red
    }

    Write-Host "API Connection:       " -NoNewline
    if ($global:TavilyConnectionValid) {
        Write-Host "✅ SUCCESS" -ForegroundColor Green
    } else {
        Write-Host "❌ FAILED" -ForegroundColor Red
    }

    Write-Host "Cache Directory:      " -NoNewline
    $cacheDir = Join-Path $env:USERPROFILE ".busbuddy\tavily-cache"
    if (Test-Path $cacheDir) {
        Write-Host "✅ EXISTS ($cacheDir)" -ForegroundColor Green
    } else {
        Write-Host "❌ MISSING" -ForegroundColor Red
    }

    # Overall status
    Write-Host "`nOverall Status:       " -NoNewline
    if (-not [string]::IsNullOrEmpty($env:TAVILY_API_KEY) -and $global:TavilyConnectionValid) {
        Write-Host "✅ READY TO USE" -ForegroundColor Green
    } else {
        Write-Host "❌ CONFIGURATION REQUIRED" -ForegroundColor Red
    }

    Write-Host "`n================================`n" -ForegroundColor Yellow

    exit 0
}

if ($Initialize) {
    $initResult = Initialize-TavilyEnvironment
    if (-not $initResult) {
        Write-Error "Failed to initialize Tavily environment."
        exit 1
    }
    exit 0
}

# Set the global API key variable to ensure it's accessible to functions
$global:ApiKey = $ApiKey

# Initialize the global connection validation flag
$global:TavilyConnectionValid = $false
if (-not [string]::IsNullOrEmpty($env:TAVILY_API_KEY) -or -not [string]::IsNullOrEmpty($ApiKey)) {
    # API key exists in environment variables or was provided as parameter
    $tavilyReady = Initialize-TavilyEnvironment
}
else {
    Write-Host "First-time use detected, initializing Tavily environment..." -ForegroundColor Yellow
    $tavilyReady = Initialize-TavilyEnvironment
}

if (-not $tavilyReady) {
    Write-Error "Tavily environment is not ready. Please initialize with -Initialize parameter."
    exit 1
}

# Handle search types
if ($Search) {
    $results = Invoke-TavilySearch -Query $Search -SearchType "search" -MaxResults $MaxResults
    Format-TavilyResults -Results $results -IncludeMetadata:$IncludeMetadata
}
elseif ($CodeSearch) {
    $results = Invoke-TavilySearch -Query $CodeSearch -SearchType "code_search" -MaxResults $MaxResults
    Format-TavilyResults -Results $results -IncludeMetadata:$IncludeMetadata
}
elseif ($DocSearch) {
    $results = Invoke-TavilySearch -Query $DocSearch -SearchType "doc_search" -MaxResults $MaxResults
    Format-TavilyResults -Results $results -IncludeMetadata:$IncludeMetadata
}
else {
    Write-Host "Please specify a search parameter: -Search, -CodeSearch, or -DocSearch" -ForegroundColor Yellow
    Write-Host "Example: .\tavily-tool.ps1 -Search 'bus routing algorithms'" -ForegroundColor Yellow
}

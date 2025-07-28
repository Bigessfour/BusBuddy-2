# Tavily API Integration Guide for BusBuddy

This guide explains how to set up and use the Tavily API integration in the BusBuddy project.

## Setup

### 1. Getting an API Key

To use the Tavily API integration, you need a valid Tavily API key:

1. Sign up at [Tavily AI](https://tavily.com/)
2. Navigate to your account dashboard
3. Find or create an API key (should start with `tvly-`)

### 2. Setting up your Environment

The Tavily integration uses environment variables for secure API key storage. Set up your environment in one of these ways:

#### Option A: Set Environment Variable Directly

In PowerShell:

```powershell
$env:TAVILY_API_KEY = "tvly-your-api-key-here"
```

To make this persistent, you can add it to your PowerShell profile or use the Windows Environment Variables system settings.

#### Option B: Use the Script Parameter

```powershell
.\Scripts\tavily-tool.ps1 -ApiKey "tvly-your-api-key-here" -Initialize
```

### 3. Validate Your Setup

Run the validation tool to check if everything is configured correctly:

```powershell
.\Scripts\tavily-tool.ps1 -ValidateSetup
```

This will:
- Check if your API key is properly set
- Validate the API key format
- Test the connection to the Tavily API
- Verify the cache directory exists
- Provide an overall status summary

## Using the Tool

### Basic Search

```powershell
.\Scripts\tavily-tool.ps1 -Search "bus routing algorithms"
```

### Code-Focused Search

Optimized for programming and technical topics:

```powershell
.\Scripts\tavily-tool.ps1 -CodeSearch "Syncfusion DockingManager examples"
```

### Documentation Search

Optimized for finding documentation and explanatory content:

```powershell
.\Scripts\tavily-tool.ps1 -DocSearch "how to use Entity Framework with SQLite"
```

### Advanced Options

Control the number of results:

```powershell
.\Scripts\tavily-tool.ps1 -Search "bus scheduling" -MaxResults 10
```

Include metadata about the search:

```powershell
.\Scripts\tavily-tool.ps1 -Search "bus scheduling" -IncludeMetadata
```

## PowerShell Integration Commands

Once the register-tavily-commands.ps1 script is loaded, you can use these PowerShell commands:

```powershell
# Import the commands
. .\Scripts\register-tavily-commands.ps1

# Initialize (only needed once per session)
Initialize-BBTavily

# Basic search
Search-BBTavily -Query "bus scheduling algorithms"

# Code search
Search-BBTavilyCode -Query "WPF data binding examples"

# Documentation search
Search-BBTavilyDocs -Query "Entity Framework Core migrations"
```

## Troubleshooting

### API Key Issues

If you see "API key not set" or authorization errors:

1. Check that your API key starts with `tvly-`
2. Verify the environment variable is set correctly
3. Try setting the key explicitly with the `-ApiKey` parameter
4. Set the environment variable correctly in PowerShell:
   ```powershell
   $env:TAVILY_API_KEY = "tvly-your-key-here"
   ```
5. Verify the API key is accepted by running the validation tool:
   ```powershell
   .\Scripts\tavily-tool.ps1 -ValidateSetup
   ```

### Common Error Codes

If you encounter HTTP error codes when connecting to the Tavily API:

- **401 Unauthorized**: Your API key is invalid or expired
- **403 Forbidden**: Your account doesn't have permission for this operation
- **429 Too Many Requests**: You've exceeded your rate limit
- **404 Not Found**: The API endpoint has changed or doesn't exist

### Connection Issues

If you can't connect to the Tavily API:

1. Check your internet connection
2. Verify the Tavily service is operational
3. Ensure your API key has sufficient credits
4. Check for rate limiting (HTTP 429 errors)

### Cache Issues

If you're getting stale or incorrect results:

1. The cache stores results for 2 hours
2. Clear the cache by deleting files in `%USERPROFILE%\.busbuddy\tavily-cache\`

## API Version Notes

The Tavily API uses the `/v1/search` endpoint as documented at [Tavily API Reference](https://tavily.tadata.com/docs/reference).

This integration has been updated to match the current API specification and automatically handles version differences.

## API Limits and Usage

- Check your remaining API credits with `-ValidateSetup`
- Tavily API has usage limits based on your plan
- Use caching for repeated queries to save credits
- Visit [Tavily Dashboard](https://tavily.tadata.com/signin) to check your account status and API usage

## Contributing

When extending the Tavily integration:

1. Maintain the error handling patterns
2. Respect the API's rate limits
3. Update documentation with any new features
4. Follow PowerShell best practices for parameter handling

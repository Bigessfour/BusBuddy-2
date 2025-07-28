# Tavily Expert MCP Integration Guide

## How Tavily Expert Works

Tavily Expert is an MCP server that acts as an AI-assisted coding companion for VS Code. It leverages Tavily's Search API to provide real-time web data access and specialized developer tools. Key features relevant to BusBuddy include:

- **Real-Time Search**: Queries web data (e.g., transport APIs, route info) for AI agents, ideal for Phase 2's mapping and optimization
- **Documentation Access**: Fetches up-to-date documentation and best practices, preventing outdated LLM assumptions
- **Testing Capabilities**: Validates API calls (e.g., search queries for BusBuddy's JSON data), ensuring correct implementation
- **Smart Onboarding**: Tools like `tavily_start_tool` guide setup, critical for BusBuddy's PowerShell-driven automation

The workflow involves Tavily Expert running as a remote MCP server, integrating with your IDE to provide context-aware coding help. For BusBuddy, it can fetch real-world data or assist with PowerShell scripting, helping avoid parser errors.

## Configuration Status

The Tavily Expert MCP has been configured on your system:

1. ✅ The Copilot MCP extension is installed
2. ✅ Your settings.json has the correct MCP server configuration
3. ✅ The local MCP configuration file has been created at `%USERPROFILE%\.github\copilot\mcp.json`

## Next Steps

To start using the Tavily Expert MCP:

1. **Restart VS Code completely**
   - Close all VS Code windows
   - Reopen VS Code

2. **Access Copilot Chat**
   - Press `Ctrl+Shift+I` or click the Copilot Chat icon in the sidebar
   - Start typing a question

3. **Select Tavily Expert MCP**
   - Look for the MCP selector that should appear near the input field
   - It may only appear after you begin typing
   - Select "Tavily Expert" from the options

4. **Ask questions that benefit from search**
   - "What are the latest developments in quantum computing?"
   - "Summarize the current state of AI regulation"
   - "What are best practices for secure API authentication in 2025?"

## Advanced Troubleshooting

If you don't see the MCP selector:

1. **Check for MCP Server button**:
   - Look for the "MCP Servers" button in the VS Code activity bar (left sidebar)
   - This should open a panel showing your configured MCP servers

2. **Verify installation with command palette**:
   - Press `Ctrl+Shift+P` to open the command palette
   - Type "MCP" to see available MCP-related commands
   - Look for commands like "MCP: List Servers" or "MCP: Connect to Server"

3. **Examine VS Code logs**:
   - Open the Output panel (`Ctrl+Shift+U`)
   - Select "Copilot MCP" or "GitHub Copilot" from the dropdown
   - Look for any error messages related to MCP configuration

4. **Manual verification**:
   - Check that the config files exist at:
     - `%USERPROFILE%\.github\copilot\mcp.json`
     - VS Code settings.json with the `mcpManager.servers` section

5. **Alternative setup methods**:
   - Try installing Tavily via npm or Python SDK:
   ```bash
   # Python SDK installation
   pip install tavily-python

   # Verify installation
   pip show tavily-python
   ```
   - Set API key in environment variables:
   ```powershell
   $env:TAVILY_API_KEY = "tvly-YOUR_API_KEY"
   ```

6. **Reinstall extensions**:
   - Uninstall and reinstall the Copilot MCP extension
   - Make sure GitHub Copilot Chat is up to date

## Configuration Details (for reference)

**settings.json**:
```json
"mcpManager.servers": {
    "Tavily Expert": {
        "serverUrl": "https://tavily.api.tadata.com/mcp/tavily/endoderm-victory-fling-azuqd2"
    }
}
```

**mcp.json**:
```json
{
  "mcpServers": {
    "Tavily Expert": {
      "serverUrl": "https://tavily.api.tadata.com/mcp/tavily/endoderm-victory-fling-azuqd2"
    }
  }
}
```

Your Tavily Expert MCP is now ready to use after restarting VS Code!

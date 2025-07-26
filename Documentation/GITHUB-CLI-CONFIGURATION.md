# GitHub CLI Configuration & Commands - BusBuddy Project

## Current Configuration

**GitHub CLI Version:** 2.74.0 (Released: May 29, 2025)
**Authentication Status:** ✅ Authenticated as `Bigessfour`
**Repository:** `BusBuddy-2` (Bigessfour/BusBuddy-2)

## Authentication & Setup

### Current Authentication
```bash
# Check authentication status
gh auth status

# Current status:
# ✓ Logged in to github.com account Bigessfour (keyring)
# - Active account: true
# - Git operations protocol: ssh
# - Token scopes: 'admin:public_key', 'gist', 'read:org', 'repo'
```

### Authentication Commands
```bash
# Login/refresh authentication
gh auth login

# Login with specific scopes
gh auth login --scopes repo,workflow,admin:public_key,gist,read:org

# Login for GitHub Enterprise (if needed)
gh auth login --hostname your-enterprise.github.com

# Refresh token
gh auth refresh

# Switch accounts
gh auth switch
```

## Current Configuration Settings

```bash
# View current configuration
gh config list

# Updated optimized settings:
git_protocol=ssh
editor=code --wait
prompt=enabled
prefer_editor_prompt=disabled
pager=more
http_unix_socket=
browser=
color_labels=enabled
accessible_colors=enabled
accessible_prompter=disabled
spinner=enabled
```

## Recommended Configuration Updates

### Set Editor for GitHub CLI
```bash
# Set VS Code as default editor
gh config set editor "code --wait"

# Alternative editors:
gh config set editor "notepad.exe"  # Windows Notepad
gh config set editor "vim"          # Vim
gh config set editor "nano"         # Nano
```

### Enhanced Display Settings
```bash
# Enable colored labels for better visibility
gh config set color_labels enabled

# Enable accessible colors for better contrast
gh config set accessible_colors enabled

# Set a pager for long output (Windows compatible)
gh config set pager "more"

# Alternative pagers:
gh config set pager "less -R"  # If you have Git Bash or WSL
gh config set pager ""         # Disable pager

# Set default browser for opening links
gh config set browser "chrome.exe"  # or "msedge.exe", "firefox.exe"
```

## Essential GitHub CLI Commands for BusBuddy

### Repository Management
```bash
# Clone repository
gh repo clone Bigessfour/BusBuddy-2

# View repository info
gh repo view Bigessfour/BusBuddy-2

# Create new repository
gh repo create BusBuddy-New --public --description "Bus management system"

# Fork repository
gh repo fork Bigessfour/BusBuddy-2
```

### Workflow & Actions Management
```bash
# List all workflows
gh workflow list

# View specific workflow
gh workflow view "CI/CD - Build, Test & Standards Validation"

# Run a workflow manually
gh workflow run "CI/CD - Build, Test & Standards Validation"

# List recent workflow runs
gh run list --limit 10

# View specific run details
gh run view [RUN_ID]

# View failed run logs
gh run view [RUN_ID] --log-failed

# Watch a running workflow
gh run watch [RUN_ID]

# Rerun a failed workflow
gh run rerun [RUN_ID]

# Cancel a running workflow
gh run cancel [RUN_ID]

# Download workflow artifacts
gh run download [RUN_ID]
```

### Pull Requests
```bash
# List pull requests
gh pr list

# Create pull request
gh pr create --title "Feature: Add new functionality" --body "Description"

# View pull request
gh pr view [PR_NUMBER]

# Merge pull request
gh pr merge [PR_NUMBER] --squash

# Close pull request
gh pr close [PR_NUMBER]
```

### Issues Management
```bash
# List issues
gh issue list

# Create new issue
gh issue create --title "Bug: Application crash" --body "Description"

# View issue
gh issue view [ISSUE_NUMBER]

# Close issue
gh issue close [ISSUE_NUMBER]
```

### Release Management
```bash
# List releases
gh release list

# Create new release
gh release create v1.0.0 --title "Release v1.0.0" --notes "Release notes"

# View release
gh release view v1.0.0

# Download release assets
gh release download v1.0.0
```

## BusBuddy-Specific Commands

### Quick Status Check
```bash
# Check recent workflow runs for our repository
gh run list --repo Bigessfour/BusBuddy-2 --limit 5

# View latest CI/CD run
gh run view --repo Bigessfour/BusBuddy-2 $(gh run list --repo Bigessfour/BusBuddy-2 --limit 1 --json databaseId --jq '.[0].databaseId')
```

### Development Workflow
```bash
# Create feature branch and push
git checkout -b feature/new-functionality
git push -u origin feature/new-functionality

# Create pull request for current branch
gh pr create --assignee @me --reviewer @maintainer

# Merge completed feature
gh pr merge --squash --delete-branch
```

### Monitoring & Debugging
```bash
# Monitor latest workflow run
gh run watch $(gh run list --limit 1 --json databaseId --jq '.[0].databaseId')

# Get failed run details with logs
gh run view --log-failed $(gh run list --status failure --limit 1 --json databaseId --jq '.[0].databaseId')

# Download latest artifacts
gh run download $(gh run list --limit 1 --json databaseId --jq '.[0].databaseId')
```

## Environment Variables

### Optional Environment Variables
```bash
# Set default repository (in PowerShell profile or .bashrc)
$env:GH_REPO = "Bigessfour/BusBuddy-2"

# Set default editor
$env:GH_EDITOR = "code --wait"

# Set token for scripts (use with caution)
$env:GITHUB_TOKEN = "your_token_here"

# For GitHub Enterprise
$env:GH_HOST = "your-enterprise.github.com"
$env:GH_ENTERPRISE_TOKEN = "your_enterprise_token"
```

## Configured Aliases

```bash
# List current aliases
gh alias list

# Currently configured aliases:
runs: run list --limit 10
failures: run list --status failure --limit 5
bb-status: run list --repo Bigessfour/BusBuddy-2 --limit 5

# Usage examples:
gh runs        # List 10 most recent workflow runs
gh failures    # List 5 most recent failed runs
gh bb-status   # List 5 most recent runs for BusBuddy-2
```

## Aliases for Common Tasks

```bash
# Set up useful aliases
gh alias set prs 'pr list --author @me'
gh alias set issues 'issue list --assignee @me'
gh alias set runs 'run list --limit 10'
gh alias set failures 'run list --status failure --limit 5'
gh alias set latest 'run view $(gh run list --limit 1 --json databaseId --jq ".[0].databaseId")'

# Usage examples:
gh prs        # List my pull requests
gh issues     # List my assigned issues
gh runs       # List recent runs
gh failures   # List recent failed runs
gh latest     # View latest run
```

## Security Best Practices

### Token Management
```bash
# View current token scopes
gh auth status

# Refresh token if needed
gh auth refresh --scopes repo,workflow,admin:public_key,gist,read:org

# Use environment variables for automation scripts
# Never hardcode tokens in scripts
```

### Repository Access
```bash
# Always specify repository explicitly in scripts
gh run list --repo Bigessfour/BusBuddy-2

# Use SSH for git operations (already configured)
git_protocol=ssh
```

## Troubleshooting

### Common Issues & Solutions

#### Authentication Issues
```bash
# Re-authenticate
gh auth login --web

# Check token scopes
gh auth status

# Switch accounts if multiple accounts
gh auth switch
```

#### Permission Issues
```bash
# Verify repository access
gh repo view Bigessfour/BusBuddy-2

# Check if authenticated user has necessary permissions
gh api user --jq '.login'
```

#### Rate Limiting
```bash
# Check rate limit status
gh api rate_limit

# Use authentication to increase rate limits
gh auth login
```

## Integration with PowerShell Profile

Add these functions to your PowerShell profile for BusBuddy development:

```powershell
# GitHub CLI helper functions for BusBuddy
function bb-status { gh run list --repo Bigessfour/BusBuddy-2 --limit 5 }
function bb-latest { gh run view --repo Bigessfour/BusBuddy-2 $(gh run list --repo Bigessfour/BusBuddy-2 --limit 1 --json databaseId --jq '.[0].databaseId') }
function bb-failures { gh run list --repo Bigessfour/BusBuddy-2 --status failure --limit 5 }
function bb-watch { gh run watch --repo Bigessfour/BusBuddy-2 $(gh run list --repo Bigessfour/BusBuddy-2 --limit 1 --json databaseId --jq '.[0].databaseId') }
function bb-logs { gh run view --repo Bigessfour/BusBuddy-2 --log-failed $(gh run list --repo Bigessfour/BusBuddy-2 --status failure --limit 1 --json databaseId --jq '.[0].databaseId') }
```

## Documentation Links

- **GitHub CLI Manual:** https://cli.github.com/manual/
- **GitHub CLI Documentation:** https://docs.github.com/en/github-cli
- **Release Notes:** https://github.com/cli/cli/releases/tag/v2.74.0
- **Community Extensions:** https://github.com/topics/gh-extension
- **API Reference:** https://docs.github.com/en/rest

## Last Updated
- **Date:** July 25, 2025
- **GitHub CLI Version:** 2.74.0
- **Configuration Status:** ✅ Properly configured and authenticated

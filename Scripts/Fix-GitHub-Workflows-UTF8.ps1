#Requires -Version 7.5

<#
.SYNOPSIS
    GitHub Actions Workflow UTF-8 Encoding Fixer
.DESCRIPTION
    Fixes UTF-8 encoding issues in GitHub Actions workflow files by reading raw bytes
    and properly converting corrupted characters to valid UTF-8 representations.
.PARAMETER FixIssues
    Apply automatic fixes to the detected issues
.PARAMETER CreateBackups
    Create backup files before making changes (default: true)
#>

param(
    [switch]$FixIssues,
    [switch]$CreateBackups
)

# Set UTF-8 encoding for all operations
$PSDefaultParameterValues['*:Encoding'] = 'utf8'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

function Write-ColoredOutput {
    param([string]$Text, [string]$Color = "White")
    $colors = @{
        "Red" = [ConsoleColor]::Red
        "Green" = [ConsoleColor]::Green
        "Yellow" = [ConsoleColor]::Yellow
        "Cyan" = [ConsoleColor]::Cyan
        "Blue" = [ConsoleColor]::Blue
        "Magenta" = [ConsoleColor]::Magenta
        "White" = [ConsoleColor]::White
    }
    Write-Host $Text -ForegroundColor $colors[$Color]
}

function Repair-UTF8Encoding {
    param([string]$FilePath)

    try {
        Write-ColoredOutput "🔧 Fixing UTF-8 encoding for: $(Split-Path -Leaf $FilePath)" "Cyan"

        # Read file as raw bytes
        $bytes = [System.IO.File]::ReadAllBytes($FilePath)

        # Try to detect and fix encoding issues
        $content = [System.Text.Encoding]::UTF8.GetString($bytes)

        # Common emoji replacements for corrupted characters
        $emojiMappings = @{
            '�' = ''  # Remove replacement characters
            '🚌' = '🚌'  # Bus
            '📦' = '📦'  # Package
            '🔧' = '🔧'  # Wrench
            '🏗️' = '🏗️'  # Building construction
            '✅' = '✅'  # Check mark
            '🧪' = '🧪'  # Test tube
            '🔄' = '🔄'  # Counterclockwise arrows
            '🔨' = '🔨'  # Hammer
            '⚠️' = '⚠️'  # Warning
            '❌' = '❌'  # Cross mark
            '⏱️' = '⏱️'  # Stopwatch
            '⚙️' = '⚙️'  # Gear
            '📝' = '📝'  # Memo
            '🎯' = '🎯'  # Target
            '🚀' = '🚀'  # Rocket
        }

        # Apply emoji fixes
        foreach ($mapping in $emojiMappings.GetEnumerator()) {
            if ($content.Contains($mapping.Key) -and $mapping.Key -ne $mapping.Value) {
                $content = $content.Replace($mapping.Key, $mapping.Value)
                Write-ColoredOutput "  ✓ Fixed emoji: $($mapping.Key) → $($mapping.Value)" "Green"
            }
        }

        # Remove any remaining replacement characters
        $content = $content -replace '�', ''

        # Ensure proper line endings
        $content = $content -replace "`r`n", "`n" -replace "`r", "`n" -replace "`n", "`r`n"

        # Write back with explicit UTF-8 encoding (no BOM)
        $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllText($FilePath, $content, $utf8NoBom)

        Write-ColoredOutput "  ✅ UTF-8 encoding fixed successfully" "Green"
        return $true
    }
    catch {
        Write-ColoredOutput "  ❌ Failed to fix encoding: $($_.Exception.Message)" "Red"
        return $false
    }
}

function Main {
    Write-ColoredOutput "🔧 GitHub Actions UTF-8 Encoding Fixer" "Cyan"
    Write-ColoredOutput ""

    # Find workflow files
    $workflowDir = ".github\workflows"
    if (-not (Test-Path $workflowDir)) {
        Write-ColoredOutput "❌ Workflow directory not found: $workflowDir" "Red"
        return
    }

    $workflowFiles = Get-ChildItem -Path $workflowDir -Filter "*.yml" -File

    if ($workflowFiles.Count -eq 0) {
        Write-ColoredOutput "❌ No workflow files found in $workflowDir" "Red"
        return
    }

    Write-ColoredOutput "📄 Found $($workflowFiles.Count) workflow files..." "Blue"
    Write-ColoredOutput ""

    $fixedCount = 0

    foreach ($file in $workflowFiles) {
        if (-not $CreateBackups) {
            $CreateBackups = $true  # Default to true if not specified
        }

        if ($CreateBackups) {
            $backupPath = "$($file.FullName).backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Copy-Item -Path $file.FullName -Destination $backupPath -Force
            Write-ColoredOutput "💾 Backup created: $(Split-Path -Leaf $backupPath)" "Yellow"
        }

        if ($FixIssues) {
            if (Repair-UTF8Encoding -FilePath $file.FullName) {
                $fixedCount++
            }
        } else {
            Write-ColoredOutput "📋 Would fix: $($file.Name) (use -FixIssues to apply)" "Yellow"
        }
    }

    Write-ColoredOutput ""
    if ($FixIssues) {
        Write-ColoredOutput "📊 Summary: Fixed $fixedCount out of $($workflowFiles.Count) files" "Green"
    } else {
        Write-ColoredOutput "📊 Summary: Found $($workflowFiles.Count) files to fix (use -FixIssues to apply)" "Yellow"
    }
}

# Run the main function
Main

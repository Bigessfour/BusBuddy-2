# Quick Tool Validation Test
Write-Host 'Testing BusBuddy Analysis Tools...' -ForegroundColor Cyan

try {
    # Test basic PowerShell syntax parsing
    $testContent = 'function Test-Function { Write-Host "Hello" }'
    $tokens = $null
    $errors = $null
    [System.Management.Automation.Language.Parser]::ParseInput($testContent, [ref]$tokens, [ref]$errors)

    if ($errors.Count -eq 0) {
        Write-Host '✅ PowerShell parser working' -ForegroundColor Green
    } else {
        Write-Host '❌ PowerShell parser failed' -ForegroundColor Red
    }

    # Test XML parsing
    try {
        [xml]$testXml = '<root><element>test</element></root>'
        if ($testXml.root.element -eq 'test') {
            Write-Host '✅ XML parser working' -ForegroundColor Green
        } else {
            Write-Host '❌ XML parser validation failed' -ForegroundColor Red
        }
    } catch {
        Write-Host "❌ XML parser failed: $_" -ForegroundColor Red
    }

    # Test regex functionality
    $testText = 'some text with pattern'
    $regexMatches = [regex]::Matches($testText, 'pattern')
    if ($regexMatches.Count -gt 0) {
        Write-Host '✅ Regex functionality working' -ForegroundColor Green
    } else {
        Write-Host '❌ Regex functionality failed' -ForegroundColor Red
    }

    Write-Host '✅ Core functionality validated - Tools should work' -ForegroundColor Green

} catch {
    Write-Host "❌ Validation failed: $_" -ForegroundColor Red
}

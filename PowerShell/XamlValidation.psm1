function Invoke-ComprehensiveXamlValidation {
    <#
    .SYNOPSIS
        Comprehensive XAML validation for BusBuddy project files

    .DESCRIPTION
        Validates all XAML files in the BusBuddy project and provides detailed reports
        of issues that need to be fixed for proper XAML compilation.
    #>

    Write-Host "🔍 Starting Comprehensive XAML Validation..." -ForegroundColor Cyan
    Write-Host "=" * 60 -ForegroundColor DarkGray

    # Get all XAML files
    $xamlFiles = Get-ChildItem -Path "BusBuddy.WPF" -Filter "*.xaml" -Recurse

    $validFiles = @()
    $invalidFiles = @()
    $resourceDictionaries = @()

    foreach ($file in $xamlFiles) {
        Write-Host "📄 Validating: $($file.Name)" -ForegroundColor Yellow

        # Check if it's a resource dictionary
        $content = Get-Content $file.FullName -Raw
        if ($content -match '<ResourceDictionary') {
            $resourceDictionaries += $file
            Write-Host "   ✅ Resource Dictionary (no code-behind needed)" -ForegroundColor Green
            continue
        }

        # Validate XAML
        try {
            $result = Test-BusBuddyXml -FilePath $file.FullName
            if ($result.IsValid) {
                $validFiles += $file
                Write-Host "   ✅ Valid XAML" -ForegroundColor Green
            } else {
                $invalidFiles += [PSCustomObject]@{
                    File = $file
                    Errors = $result.Errors
                }
                Write-Host "   ❌ Invalid XAML" -ForegroundColor Red
                foreach ($validationError in $result.Errors) {
                    Write-Host "      Error: $($validationError.Exception.Message)" -ForegroundColor Red
                }
            }
        } catch {
            $invalidFiles += [PSCustomObject]@{
                File = $file
                Errors = @($_)
            }
            Write-Host "   ❌ Validation failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Summary report
    Write-Host ""
    Write-Host "📊 XAML Validation Summary" -ForegroundColor Cyan
    Write-Host "=" * 60 -ForegroundColor DarkGray
    Write-Host "Total XAML files: $($xamlFiles.Count)" -ForegroundColor White
    Write-Host "Resource dictionaries: $($resourceDictionaries.Count)" -ForegroundColor Green
    Write-Host "Valid XAML files: $($validFiles.Count)" -ForegroundColor Green
    Write-Host "Invalid XAML files: $($invalidFiles.Count)" -ForegroundColor Red

    if ($invalidFiles.Count -gt 0) {
        Write-Host ""
        Write-Host "🔧 Files requiring fixes:" -ForegroundColor Yellow
        foreach ($invalid in $invalidFiles) {
            Write-Host "   • $($invalid.File.Name)" -ForegroundColor Red
        }
    }

    return [PSCustomObject]@{
        TotalFiles = $xamlFiles.Count
        ValidFiles = $validFiles.Count
        InvalidFiles = $invalidFiles.Count
        ResourceDictionaries = $resourceDictionaries.Count
        InvalidFileDetails = $invalidFiles
    }
}

# Create alias
Set-Alias -Name 'bb-xaml-validate' -Value 'Invoke-ComprehensiveXamlValidation' -Description 'Comprehensive XAML validation'

Export-ModuleMember -Function Invoke-ComprehensiveXamlValidation -Alias bb-xaml-validate

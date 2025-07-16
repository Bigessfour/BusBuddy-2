# Fix Serilog migration for ScheduleManagementViewModel
$filePath = 'BusBuddy.WPF\ViewModels\Schedule\ScheduleManagementViewModel.cs'

# Read the file content
$content = Get-Content $filePath -Raw

# Replace all Microsoft.Extensions.Logging calls with Serilog format
$content = $content -replace 'Logger\?\.\LogInformation\(', 'Logger.Information('
$content = $content -replace 'Logger\?\.\LogError\(', 'Logger.Error('
$content = $content -replace 'Logger\?\.\LogWarning\(', 'Logger.Warning('
$content = $content -replace 'Logger\?\.\LogDebug\(', 'Logger.Debug('

# Write back to the file
Set-Content $filePath -Value $content

Write-Host 'Fixed all logging calls in ScheduleManagementViewModel.cs'

$filePath = 'BusBuddy.WPF\ViewModels\Schedule\ScheduleManagementViewModel.cs'
$content = Get-Content $filePath -Raw

# Replace all the logging calls
$content = $content -replace 'Logger\?\.(LogInformation|LogError|LogDebug)\(', 'Logger.$1('

Set-Content $filePath $content
Write-Host 'Fixed all logging calls in ScheduleManagementViewModel.cs'

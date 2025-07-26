# Seed real-world transportation data into BusBuddy
param(
    [string]$JsonFilePath = "../sample-realworld-data.json"
)

Write-Host "Seeding real-world data from $JsonFilePath..." -ForegroundColor Cyan

$serviceType = [BusBuddy.Core.Data.SeedDataService]
$contextFactoryType = [BusBuddy.Core.Data.BusBuddyDbContextFactory]
$contextFactory = [Activator]::CreateInstance($contextFactoryType)
$seedService = [Activator]::CreateInstance($serviceType, $contextFactory)

$result = $seedService.SeedRealWorldDataAsync($JsonFilePath).GetAwaiter().GetResult()

if ($result.Success) {
    Write-Host "✅ Data seeded successfully: $($result.DriversSeeded) drivers, $($result.VehiclesSeeded) vehicles, $($result.ActivitiesSeeded) activities" -ForegroundColor Green
}
else {
    Write-Host "❌ Data seeding failed: $($result.ErrorMessage)" -ForegroundColor Red
}

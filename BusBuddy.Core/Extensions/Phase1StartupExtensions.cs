using BusBuddy.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BusBuddy.Core.Extensions;

/// <summary>
/// Phase 1 Startup Extensions
/// Handles Phase 1 initialization including data seeding
/// </summary>
public static class Phase1StartupExtensions
{
    /// <summary>
    /// Initializes Phase 1 data and services
    /// </summary>
    public static async Task InitializePhase1Async(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Phase1DataSeedingService>>();

        try
        {
            logger.LogInformation("🚀 Starting Phase 1 initialization...");

            // Initialize database and seed data
            var dataSeeder = scope.ServiceProvider.GetRequiredService<Phase1DataSeedingService>();
            await dataSeeder.SeedPhase1DataAsync();

            // Get data summary
            var summary = await dataSeeder.GetDataSummaryAsync();
            logger.LogInformation(summary);

            logger.LogInformation("✅ Phase 1 initialization completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Phase 1 initialization failed");
            throw;
        }
    }

    /// <summary>
    /// Registers Phase 1 services
    /// </summary>
    public static IServiceCollection AddPhase1Services(this IServiceCollection services)
    {
        services.AddScoped<Phase1DataSeedingService>();
        return services;
    }
}

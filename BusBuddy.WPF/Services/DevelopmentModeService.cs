using BusBuddy.Core.Configuration;
using BusBuddy.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace BusBuddy.WPF.Services
{
    /// <summary>
    /// Service for managing development-specific features and utilities
    /// </summary>
    public class DevelopmentModeService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DevelopmentModeService> _logger;

        public DevelopmentModeService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DevelopmentModeService> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Check if development mode is enabled
        /// </summary>
        public bool IsEnabled => DevelopmentHelper.IsDevelopment(_configuration);

        /// <summary>
        /// Initialize development mode features if enabled
        /// </summary>
        public async Task InitializeAsync()
        {
            if (!IsEnabled)
            {
                _logger.LogInformation("Development mode disabled. Skipping development features.");
                return;
            }

            _logger.LogInformation("Development mode enabled. Initializing development features...");

            try
            {
                // Check if database is empty and offer to seed data
                await CheckAndOfferSeedDataAsync();
                
                // Add development menu items or buttons (if needed)
                AddDevelopmentMenuItems();
                
                _logger.LogInformation("Development mode initialization completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing development mode");
            }
        }

        /// <summary>
        /// Check if database is empty and offer to seed sample data
        /// </summary>
        private async Task CheckAndOfferSeedDataAsync()
        {
            try
            {
                var seedService = _serviceProvider.GetRequiredService<SeedDataService>();
                
                // This would typically show a dialog in development mode
                // For now, we'll just log the availability
                _logger.LogInformation("Seed data service available. Call SeedAllAsync() to populate with sample data.");
                
                // In a real implementation, you might want to:
                // - Show a dialog asking if user wants to seed data
                // - Add a menu item or button to trigger seeding
                // - Automatically seed if database is completely empty
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking seed data availability");
            }
        }

        /// <summary>
        /// Add development-specific menu items or features
        /// </summary>
        private void AddDevelopmentMenuItems()
        {
            // This would integrate with the main window to add development features
            // For example: Debug menu, seed data buttons, performance monitors, etc.
            _logger.LogDebug("Development menu items would be added here");
        }

        /// <summary>
        /// Manually trigger seed data creation
        /// </summary>
        public async Task SeedDataAsync()
        {
            if (!IsEnabled)
            {
                _logger.LogWarning("Seed data requested but development mode is disabled");
                return;
            }

            try
            {
                _logger.LogInformation("Starting manual seed data creation...");
                var seedService = _serviceProvider.GetRequiredService<SeedDataService>();
                await seedService.SeedAllAsync();
                _logger.LogInformation("Manual seed data creation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual seed data creation");
                throw;
            }
        }

        /// <summary>
        /// Clear all seed data
        /// </summary>
        public async Task ClearSeedDataAsync()
        {
            if (!IsEnabled)
            {
                _logger.LogWarning("Clear seed data requested but development mode is disabled");
                return;
            }

            try
            {
                _logger.LogInformation("Starting seed data clearing...");
                var seedService = _serviceProvider.GetRequiredService<SeedDataService>();
                await seedService.ClearSeedDataAsync();
                _logger.LogInformation("Seed data clearing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during seed data clearing");
                throw;
            }
        }

        /// <summary>
        /// Get development statistics and information
        /// </summary>
        public async Task<Dictionary<string, object>> GetDevelopmentInfoAsync()
        {
            var info = new Dictionary<string, object>
            {
                ["IsDevelopmentMode"] = IsEnabled,
                ["Environment"] = _configuration["Environment"] ?? "Unknown",
                ["CurrentMemoryMB"] = BusBuddy.WPF.Utilities.PerformanceMonitor.GetCurrentMemoryUsageMB(),
                ["OptimalLogLevel"] = DevelopmentHelper.GetOptimalLogLevel(_configuration),
                ["OptimalPageSize"] = DevelopmentHelper.GetOptimalPageSize(_configuration),
                ["MemoryWarningThreshold"] = DevelopmentHelper.GetMemoryWarningThreshold(_configuration) / (1024 * 1024),
                ["DatabaseTimeout"] = DevelopmentHelper.GetDatabaseTimeout(_configuration)
            };

            return info;
        }
    }
}

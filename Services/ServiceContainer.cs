using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Services;
using Bus_Buddy.Data;
using Bus_Buddy.Extensions;
using Syncfusion.Licensing;

namespace Bus_Buddy
{
    public static class ServiceContainer
    {
        private static ServiceProvider? _serviceProvider; public static void Initialize()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Initialize database
            InitializeDatabase();

            // Register Syncfusion license
            var configService = GetService<IConfigurationService>();
            SyncfusionLicenseProvider.RegisterLicense(configService.GetSyncfusionLicenseKey());
        }

        private static void InitializeDatabase()
        {
            try
            {
                using var scope = _serviceProvider!.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<BusBuddyDbContext>();

                // Ensure database is created
                context.Database.EnsureCreated();

                // Apply any pending migrations
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                Console.WriteLine($"Database initialization error: {ex.Message}");
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            // Use the extension methods for centralized service registration
            services.AddDataServices(configuration);
            services.AddDataExtensions(configuration);
            services.AddAIServices(configuration);

            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddConfiguration(configuration.GetSection("Logging"));
            });

            // Register BusRepository explicitly for forms that depend on it directly
            services.AddScoped<Bus_Buddy.Data.Repositories.BusRepository>();

            // Forms (as transient so they can be created multiple times if needed)
            services.AddTransient<Dashboard>();
            services.AddTransient<Bus_Buddy.Forms.BusManagementForm>();
            services.AddTransient<Bus_Buddy.Forms.BusEditForm>();
            services.AddTransient<Bus_Buddy.Forms.DriverManagementForm>();
            services.AddTransient<Bus_Buddy.Forms.DriverEditForm>();
            services.AddTransient<Bus_Buddy.Forms.RouteManagementForm>();
            services.AddTransient<Bus_Buddy.Forms.ScheduleManagementForm>();
            services.AddTransient<Bus_Buddy.Forms.ActivityEditForm>();
            services.AddTransient<Bus_Buddy.Forms.StudentManagementForm>();
            services.AddTransient<Bus_Buddy.Forms.StudentEditForm>();
            services.AddTransient<Bus_Buddy.Forms.MaintenanceManagementForm>();
            services.AddTransient<Bus_Buddy.Forms.FuelManagementForm>();
            services.AddTransient<Bus_Buddy.Forms.TicketManagementForm>();
            services.AddTransient<Bus_Buddy.Forms.TicketEditForm>();
            services.AddTransient<Bus_Buddy.Forms.VisualEnhancementShowcaseForm>();
            services.AddTransient<Bus_Buddy.Forms.PassengerManagementForm>();
            services.AddTransient<Bus_Buddy.Forms.EnhancedDashboardAnalytics>();
        }

        public static T GetService<T>() where T : notnull
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("ServiceContainer not initialized. Call Initialize() first.");

            return _serviceProvider.GetRequiredService<T>();
        }

        public static T? GetOptionalService<T>() where T : class
        {
            if (_serviceProvider == null)
                return null;

            return _serviceProvider.GetService<T>();
        }

        public static void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}

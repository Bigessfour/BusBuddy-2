using BusBuddy.Core;
using BusBuddy.Core.Data.Services;
using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusBuddy.Core.Configuration
{
    /// <summary>
    /// Production-ready dependency injection and service configuration for BusBuddy
    /// Integrates Grok's database architecture with comprehensive Serilog logging
    /// Supports multiple environments: Development, Staging, Production
    /// </summary>
    public static class ServiceConfiguration
    {
        private static readonly Serilog.ILogger Logger = Log.ForContext(typeof(ServiceConfiguration));

        /// <summary>
        /// Configure all BusBuddy services for dependency injection
        /// </summary>
        public static IServiceCollection AddBusBuddyServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(environment);

            using (LogContext.PushProperty("Operation", "ServiceConfiguration"))
            using (LogContext.PushProperty("Environment", environment.EnvironmentName))
            {
                Logger.Information("Configuring BusBuddy services for {Environment} environment", environment.EnvironmentName);

                try
                {
                    // Configure Database Context
                    services.AddBusBuddyDatabase(configuration, environment);

                    // Configure Repository Pattern
                    services.AddBusBuddyRepositories();

                    // Configure Business Services
                    services.AddBusBuddyBusinessServices();

                    // Configure Data Seeding
                    services.AddBusBuddyDataSeeding(environment);

                    Logger.Information("BusBuddy services configured successfully");
                    return services;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to configure BusBuddy services");
                    throw;
                }
            }
        }

        /// <summary>
        /// Configure Entity Framework DbContext with production-ready settings
        /// </summary>
        private static IServiceCollection AddBusBuddyDatabase(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            Logger.Debug("Configuring BusBuddy database context");

            services.AddDbContext<BusBuddyContext>((serviceProvider, options) =>
            {
                var connectionString = GetConnectionString(configuration, environment);

                if (IsAzureSqlConnection(connectionString))
                {
                    ConfigureAzureSqlServer(options, connectionString, environment);
                }
                else if (IsSqlServerConnection(connectionString))
                {
                    ConfigureSqlServer(options, connectionString, environment);
                }
                else
                {
                    ConfigureSqlite(options, connectionString, environment);
                }

                // Configure Serilog integration
                options.EnableServiceProviderCaching();
                options.EnableSensitiveDataLogging(environment.IsDevelopment());

                // Environment-specific configurations
                ConfigureEnvironmentSpecific(options, environment);

            }, ServiceLifetime.Scoped);

            Logger.Debug("Database context configured successfully");
            return services;
        }

        /// <summary>
        /// Configure repository pattern services
        /// </summary>
        private static IServiceCollection AddBusBuddyRepositories(this IServiceCollection services)
        {
            Logger.Debug("Configuring repository pattern services");

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register individual repositories (if needed separately from UoW)
            services.AddScoped(typeof(IRepository<>), typeof(Data.Repositories.Repository<>));

            Logger.Debug("Repository services configured");
            return services;
        }

        /// <summary>
        /// Configure business services
        /// </summary>
        private static IServiceCollection AddBusBuddyBusinessServices(this IServiceCollection services)
        {
            Logger.Debug("Configuring business services");

            // User context service for audit tracking
            services.AddScoped<IUserContextService, UserContextService>();

            // Add other business services here as needed
            // services.AddScoped<IDriverService, DriverService>();
            // services.AddScoped<IVehicleService, VehicleService>();
            // services.AddScoped<IActivityService, ActivityService>();

            Logger.Debug("Business services configured");
            return services;
        }

        /// <summary>
        /// Configure data seeding services
        /// </summary>
        private static IServiceCollection AddBusBuddyDataSeeding(
            this IServiceCollection services,
            IHostEnvironment environment)
        {
            Logger.Debug("Configuring data seeding services");

            // Register seed data service
            services.AddScoped<Data.Services.SeedDataService>();

            // Auto-seed data in development environment
            if (environment.IsDevelopment())
            {
                services.AddHostedService<DatabaseSeedingHostedService>();
                Logger.Debug("Auto-seeding enabled for Development environment");
            }

            return services;
        }

        #region Database Configuration Helpers

        private static string GetConnectionString(IConfiguration configuration, IHostEnvironment environment)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback based on environment
                connectionString = environment.IsDevelopment()
                    ? "Data Source=BusBuddy.db"
                    : throw new InvalidOperationException("No connection string configured for non-development environment");

                Logger.Warning("Using fallback connection string for {Environment} environment", environment.EnvironmentName);
            }
            else
            {
                Logger.Information("Using configured connection string for {Environment} environment", environment.EnvironmentName);
            }

            return connectionString;
        }

        private static void ConfigureAzureSqlServer(DbContextOptionsBuilder options, string connectionString, IHostEnvironment environment)
        {
            Logger.Debug("Configuring Azure SQL Server database options");

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);

                sqlOptions.CommandTimeout(environment.IsProduction() ? 120 : 60);
                sqlOptions.MigrationsAssembly("BusBuddy.Core");
            });
        }

        private static void ConfigureSqlServer(DbContextOptionsBuilder options, string connectionString, IHostEnvironment environment)
        {
            Logger.Debug("Configuring SQL Server database options");

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);

                sqlOptions.CommandTimeout(environment.IsProduction() ? 60 : 30);
                sqlOptions.MigrationsAssembly("BusBuddy.Core");
            });
        }

        private static void ConfigureSqlite(DbContextOptionsBuilder options, string connectionString, IHostEnvironment environment)
        {
            Logger.Debug("Configuring SQLite database options");

            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(30);
                sqliteOptions.MigrationsAssembly("BusBuddy.Core");
            });
        }

        private static void ConfigureEnvironmentSpecific(DbContextOptionsBuilder options, IHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                Logger.Debug("Development-specific database options enabled");
            }
            else if (environment.IsStaging())
            {
                options.EnableDetailedErrors();
                Logger.Debug("Staging-specific database options enabled");
            }
            // Production uses minimal logging for performance
        }

        private static bool IsAzureSqlConnection(string connectionString)
        {
            return connectionString.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSqlServerConnection(string connectionString)
        {
            return connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase)
                   && !IsAzureSqlConnection(connectionString);
        }

        #endregion

        /// <summary>
        /// Initialize database and seed data on application startup
        /// </summary>
        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            using (LogContext.PushProperty("Operation", "DatabaseInitialization"))
            {
                Logger.Information("Initializing BusBuddy database");

                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var seedService = scope.ServiceProvider.GetRequiredService<Data.Services.SeedDataService>();

                    await seedService.SeedAsync();

                    Logger.Information("Database initialization completed successfully");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Database initialization failed");
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Hosted service for automatic database seeding in development
    /// </summary>
    public class DatabaseSeedingHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly Serilog.ILogger Logger = Log.ForContext<DatabaseSeedingHostedService>();

        public DatabaseSeedingHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Starting automatic database seeding");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var seedService = scope.ServiceProvider.GetRequiredService<Data.Services.SeedDataService>();

                await seedService.SeedAsync();

                Logger.Information("Automatic database seeding completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Automatic database seeding failed");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Basic user context service for audit tracking
    /// </summary>
    public class UserContextService : IUserContextService
    {
        public string GetCurrentUserId()
        {
            // In a real application, this would get the current user from authentication context
            return Environment.UserName ?? "System";
        }

        public string GetCurrentUserName()
        {
            return Environment.UserName ?? "System";
        }
    }

    /// <summary>
    /// Interface for user context service
    /// </summary>
    public interface IUserContextService
    {
        string GetCurrentUserId();
        string GetCurrentUserName();
    }
}

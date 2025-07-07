using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Data;
using Bus_Buddy.Data.Interfaces;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Data.UnitOfWork;
using Bus_Buddy.Services;
using Bus_Buddy.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// Enhanced test base using SQLite in-memory database per Microsoft recommendations
    /// 
    /// MICROSOFT EF CORE TESTING BEST PRACTICES:
    /// - Avoid EF InMemory provider (has significant limitations)
    /// - Use SQLite in-memory for reliable test doubles
    /// - Each test gets isolated SQLite database
    /// - Better query compatibility with production SQL Server
    /// 
    /// Source: https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy
    /// </summary>
    public abstract class TestBaseWithSQLite : IDisposable, IAsyncDisposable
    {
        protected ServiceProvider ServiceProvider { get; private set; } = null!;
        protected BusBuddyDbContext DbContext { get; private set; } = null!;
        protected IConfiguration Configuration { get; private set; } = null!;

        protected TestBaseWithSQLite()
        {
            SetupServices();
            DbContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<BusBuddyDbContext>(ServiceProvider);
            InitializeDatabase();
        }

        /// <summary>
        /// Initialize SQLite in-memory database for this test instance
        /// Each test gets a completely isolated database
        /// </summary>
        private void InitializeDatabase()
        {
            DbContext.Database.EnsureCreated();
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Setup services with SQLite in-memory database per Microsoft recommendations
        /// </summary>
        private void SetupServices()
        {
            var services = new ServiceCollection();
            ConfigureSharedServices(services);
            ConfigureTestSpecificServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Configure services using SQLite in-memory database
        /// </summary>
        private void ConfigureSharedServices(ServiceCollection services)
        {
            // Configuration for SQLite testing
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["TestSettings:UseSQLiteInMemory"] = "true",
                ["TestSettings:SeedTestData"] = "false",
                ["TestSettings:EnableDetailedLogging"] = "false",
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning"
            };

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(inMemorySettings);
            Configuration = configBuilder.Build();
            services.AddSingleton(Configuration);

            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // SQLite in-memory database configuration
            services.AddDbContext<BusBuddyDbContext>(options =>
            {
                // SQLite in-memory database with unique connection per test
                var connectionString = $"Data Source=:memory:;Cache=Shared;";
                options.UseSqlite(connectionString);
                options.EnableSensitiveDataLogging();
            }, ServiceLifetime.Transient);

            // Repository Pattern Support
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IBusRepository, BusRepository>();

            // Unit of Work Pattern Support
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Business Services
            services.AddScoped<IBusService, BusService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IFuelService, FuelService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<ITicketService, TicketService>();
        }

        /// <summary>
        /// Override in derived classes for test-specific service configuration
        /// </summary>
        protected virtual void ConfigureTestSpecificServices(ServiceCollection services)
        {
            // Override in derived classes
        }

        /// <summary>
        /// Clear database and reset for next test
        /// Much more reliable with SQLite than InMemory
        /// </summary>
        protected async Task ClearDatabaseAsync()
        {
            // Keep connection alive but clear data
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Clear Entity Framework change tracker
        /// </summary>
        protected void ClearChangeTracker()
        {
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Detach all tracked entities
        /// </summary>
        protected void DetachAllEntities()
        {
            var entries = DbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }

        /// <summary>
        /// Get service with error handling
        /// </summary>
        protected T GetService<T>() where T : class
        {
            try
            {
                return Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(ServiceProvider);
            }
            catch (Exception ex)
            {
                var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<ILogger<TestBaseWithSQLite>>(ServiceProvider);
                logger?.LogError(ex, "Failed to resolve service {ServiceType}", typeof(T).Name);
                throw new InvalidOperationException($"Service resolution failed for {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Seed common test data when needed
        /// </summary>
        protected async Task SeedTestDataAsync()
        {
            var testBus = new Bus
            {
                BusNumber = "TEST001",
                Year = 2020,
                Make = "Blue Bird",
                Model = "Vision",
                SeatingCapacity = 72,
                VINNumber = "TEST001234567890",
                LicenseNumber = "TST001"
            };

            var testDriver = new Driver
            {
                DriverName = "Test Driver",
                DriverPhone = "(555) 123-4567",
                DriverEmail = "testdriver@busbuddy.com",
                DriversLicenceType = "CDL",
                TrainingComplete = true
            };

            DbContext.Vehicles.Add(testBus);
            DbContext.Drivers.Add(testDriver);

            await DbContext.SaveChangesAsync();

            var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<ILogger<TestBaseWithSQLite>>(ServiceProvider);
            logger?.LogInformation("Test data seeded: 1 bus, 1 driver");
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            if (ServiceProvider is IAsyncDisposable spAsyncDisposable)
            {
                await spAsyncDisposable.DisposeAsync();
            }
            else
            {
                ServiceProvider?.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}

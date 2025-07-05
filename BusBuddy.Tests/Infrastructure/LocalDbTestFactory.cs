using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Data;
using Bus_Buddy.Data.Interfaces;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Data.UnitOfWork;
using Bus_Buddy.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// Factory for creating isolated LocalDB test environments
    /// Each test method gets its own unique database for perfect isolation
    /// 
    /// BENEFITS:
    /// - Complete test isolation (each test = unique database)
    /// - Proper setup/teardown lifecycle management
    /// - No inheritance required - use composition instead
    /// - Can be used with any testing framework (NUnit, xUnit, MSTest)
    /// - Thread-safe for parallel test execution
    /// </summary>
    public class LocalDbTestFactory : IDisposable, IAsyncDisposable
    {
        private readonly DatabaseManager _databaseManager;
        private readonly List<LocalDbTestContext> _activeContexts;
        private readonly object _lockObject = new object();

        public LocalDbTestFactory()
        {
            _databaseManager = new DatabaseManager();
            _activeContexts = new List<LocalDbTestContext>();
        }

        /// <summary>
        /// Create a new isolated test context for a test method
        /// Each call creates a completely separate database
        /// </summary>
        public async Task<LocalDbTestContext> CreateTestContextAsync(string? testName = null)
        {
            testName ??= $"Test_{Guid.NewGuid():N}";

            var context = new LocalDbTestContext(_databaseManager, testName);
            await context.InitializeAsync();

            lock (_lockObject)
            {
                _activeContexts.Add(context);
            }

            return context;
        }

        /// <summary>
        /// Clean up all active test contexts
        /// </summary>
        public async Task CleanupAllAsync()
        {
            List<LocalDbTestContext> contextsToCleanup;

            lock (_lockObject)
            {
                contextsToCleanup = new List<LocalDbTestContext>(_activeContexts);
                _activeContexts.Clear();
            }

            foreach (var context in contextsToCleanup)
            {
                try
                {
                    await context.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to cleanup test context {context.DatabaseName}: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            CleanupAllAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            await CleanupAllAsync();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents an isolated test context with its own database and services
    /// </summary>
    public class LocalDbTestContext : IDisposable, IAsyncDisposable
    {
        public string DatabaseName { get; private set; }
        public string ConnectionString { get; private set; }
        public BusBuddyDbContext DbContext { get; private set; } = null!;
        public ServiceProvider ServiceProvider { get; private set; } = null!;
        public IConfiguration Configuration { get; private set; } = null!;

        private readonly DatabaseManager _databaseManager;
        private readonly string _testName;
        private bool _isInitialized;

        internal LocalDbTestContext(DatabaseManager databaseManager, string testName)
        {
            _databaseManager = databaseManager;
            _testName = testName;
            DatabaseName = _databaseManager.GenerateTestDatabaseName(_testName);
            ConnectionString = _databaseManager.BuildLocalDbConnectionString(DatabaseName);
        }

        /// <summary>
        /// Initialize the test context with database and services
        /// </summary>
        internal async Task InitializeAsync()
        {
            if (_isInitialized) return;

            SetupServices();
            DbContext = ServiceProvider.GetRequiredService<BusBuddyDbContext>();

            // Create fresh database for this test
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            DbContext.ChangeTracker.Clear();

            _isInitialized = true;
        }

        /// <summary>
        /// Setup dependency injection container
        /// </summary>
        private void SetupServices()
        {
            var services = new ServiceCollection();

            // Configuration
            var testSettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["ConnectionStrings:TestConnection"] = ConnectionString,
                ["TestSettings:UseInMemoryDatabase"] = "false",
                ["TestSettings:UseSqlServerTestDatabase"] = "true",
                ["TestSettings:UseLocalDb"] = "true",
                ["TestSettings:DatabaseName"] = DatabaseName,
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Error"
            };

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(testSettings);
            Configuration = configBuilder.Build();
            services.AddSingleton(Configuration);

            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);
            });

            // Database
            services.AddDbContext<BusBuddyDbContext>(options =>
            {
                options.UseSqlServer(ConnectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                });
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
            }, ServiceLifetime.Scoped);

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IBusRepository, BusRepository>();

            // Unit of Work
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
            services.AddScoped<IUserContextService, UserContextService>();

            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Get a service from the DI container
        /// </summary>
        public T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Clear all data from the database (fast cleanup)
        /// </summary>
        public async Task ClearDatabaseAsync()
        {
            await _databaseManager.TruncateAllTablesAsync(DbContext);
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Seed minimal test data
        /// </summary>
        public async Task SeedTestDataAsync()
        {
            var testBus = new Bus_Buddy.Models.Bus
            {
                BusNumber = "TEST001",
                Year = 2020,
                Make = "Blue Bird",
                Model = "Vision",
                SeatingCapacity = 72,
                VINNumber = "TEST001234567890",
                LicenseNumber = "TST001",
                CreatedDate = DateTime.UtcNow
            };

            DbContext.Vehicles.Add(testBus);
            await DbContext.SaveChangesAsync();
            DbContext.ChangeTracker.Clear();
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (DbContext != null)
                {
                    await DbContext.Database.EnsureDeletedAsync();
                    await DbContext.DisposeAsync();
                }

                if (ServiceProvider is IAsyncDisposable spAsyncDisposable)
                {
                    await spAsyncDisposable.DisposeAsync();
                }
                else
                {
                    ServiceProvider?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to cleanup test context {DatabaseName}: {ex.Message}");
            }

            GC.SuppressFinalize(this);
        }
    }
}

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
using System.Data.SqlClient;
using System.Linq;

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// TestBase using SQL Server Express LocalDB for realistic database testing
    /// 
    /// BENEFITS OF LOCALDB TESTING:
    /// - Tests run against actual SQL Server engine
    /// - Better SQL compatibility than SQLite/InMemory
    /// - Faster than full SQL Server instance
    /// - Isolated test databases per test class
    /// - Supports advanced SQL Server features
    /// 
    /// PERFORMANCE OPTIMIZATIONS:
    /// - Database name generation moved to DatabaseManager
    /// - Cached service provider when possible
    /// - Minimal logging during tests
    /// - Fast database cleanup between tests
    /// </summary>
    public abstract class LocalDbTestBase : IDisposable, IAsyncDisposable
    {
        protected ServiceProvider ServiceProvider { get; private set; } = null!;
        protected BusBuddyDbContext DbContext { get; private set; } = null!;
        protected IConfiguration Configuration { get; private set; } = null!;
        protected string DatabaseName { get; private set; } = null!;
        protected string ConnectionString { get; private set; } = null!;

        private readonly DatabaseManager _databaseManager;
        private static readonly object _lockObject = new object();

        protected LocalDbTestBase()
        {
            _databaseManager = new DatabaseManager();
            // Generate unique database name per test instance to avoid parallel execution conflicts
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            DatabaseName = _databaseManager.GenerateTestDatabaseName($"{GetType().Name}_{uniqueId}");
            ConnectionString = _databaseManager.BuildLocalDbConnectionString(DatabaseName);

            SetupServices();
            DbContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<BusBuddyDbContext>(ServiceProvider);
            InitializeDatabaseAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initialize LocalDB database for this test class
        /// Creates a fresh database with all migrations applied
        /// </summary>
        private async Task InitializeDatabaseAsync()
        {
            try
            {
                // Always start with a clean slate - delete if exists, then create
                await DbContext.Database.EnsureDeletedAsync();
                await DbContext.Database.EnsureCreatedAsync();

                // Clear change tracker for clean state
                DbContext.ChangeTracker.Clear();

                var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<ILogger<LocalDbTestBase>>(ServiceProvider);
                logger?.LogInformation("LocalDB test database initialized: {DatabaseName}", DatabaseName);
            }
            catch (Exception ex)
            {
                var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<ILogger<LocalDbTestBase>>(ServiceProvider);
                logger?.LogError(ex, "Failed to initialize LocalDB test database: {DatabaseName}", DatabaseName);

                // Try fallback approach if initial setup fails
                try
                {
                    await DbContext.Database.EnsureDeletedAsync();
                    await Task.Delay(100); // Small delay to ensure cleanup
                    await DbContext.Database.EnsureCreatedAsync();
                    DbContext.ChangeTracker.Clear();
                    logger?.LogWarning("LocalDB test database initialized using fallback approach: {DatabaseName}", DatabaseName);
                }
                catch (Exception fallbackEx)
                {
                    logger?.LogError(fallbackEx, "Fallback initialization also failed for: {DatabaseName}", DatabaseName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Setup services with SQL Server LocalDB
        /// Optimized for performance with minimal logging
        /// </summary>
        private void SetupServices()
        {
            var services = new ServiceCollection();
            ConfigureSharedServices(services);
            ConfigureTestSpecificServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Configure services using SQL Server LocalDB
        /// </summary>
        private void ConfigureSharedServices(ServiceCollection services)
        {
            // Configuration for LocalDB testing
            var testSettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["ConnectionStrings:TestConnection"] = ConnectionString,
                ["TestSettings:UseInMemoryDatabase"] = "false",
                ["TestSettings:UseSqlServerTestDatabase"] = "true",
                ["TestSettings:UseLocalDb"] = "true",
                ["TestSettings:SeedTestData"] = "false",
                ["TestSettings:EnableDetailedLogging"] = "false",
                ["TestSettings:DatabaseName"] = DatabaseName,
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Error",
                ["Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command"] = "None"
            };

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(testSettings);
            Configuration = configBuilder.Build();
            services.AddSingleton(Configuration);

            // Minimal logging for performance
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);
            });

            // SQL Server LocalDB configuration
            services.AddDbContext<BusBuddyDbContext>(options =>
            {
                options.UseSqlServer(ConnectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(30);
                    // Disable retry strategy for testing - it conflicts with user-initiated transactions
                    // sqlOptions.EnableRetryOnFailure(
                    //     maxRetryCount: 3,
                    //     maxRetryDelay: TimeSpan.FromSeconds(5),
                    //     errorNumbersToAdd: null);

                    // Configure for better concurrency handling in tests
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

                // Performance optimizations for testing
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
                options.LogTo(message => { }, LogLevel.Error); // Minimal logging

                // Disable concurrency detection for testing to avoid threading issues
                options.EnableServiceProviderCaching(false);
                options.EnableThreadSafetyChecks(false);
            }, ServiceLifetime.Scoped);

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
            services.AddScoped<IUserContextService, UserContextService>();
        }

        /// <summary>
        /// Override in derived classes for test-specific service configuration
        /// </summary>
        protected virtual void ConfigureTestSpecificServices(IServiceCollection services)
        {
            // Override in derived classes for additional test-specific services
        }

        /// <summary>
        /// Fast database cleanup - truncates all tables but keeps schema
        /// Much faster than recreating the entire database
        /// </summary>
        protected async Task ClearDatabaseAsync()
        {
            try
            {
                await _databaseManager.TruncateAllTablesAsync(DbContext);
                DbContext.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<ILogger<LocalDbTestBase>>(ServiceProvider);
                logger?.LogWarning(ex, "Fast database clear failed, falling back to recreate");

                // Fallback to full recreation if truncate fails
                await RecreateDatabase();
            }
        }

        /// <summary>
        /// Recreate database completely (slower but more thorough)
        /// </summary>
        protected async Task RecreateDatabase()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Clear Entity Framework change tracker only
        /// Fastest cleanup option when you don't need to clear data
        /// </summary>
        protected void ClearChangeTracker()
        {
            try
            {
                if (DbContext != null && !IsDisposed())
                {
                    DbContext.ChangeTracker.Clear();
                }
            }
            catch (ObjectDisposedException)
            {
                // Context is disposed, nothing to clear
            }
        }

        /// <summary>
        /// Detach all tracked entities without clearing database
        /// </summary>
        protected void DetachAllEntities()
        {
            try
            {
                if (DbContext != null && !IsDisposed())
                {
                    var entries = DbContext.ChangeTracker.Entries().ToArray();
                    foreach (var entry in entries)
                    {
                        entry.State = EntityState.Detached;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Context is disposed, nothing to detach
            }
        }

        /// <summary>
        /// Check if the LocalDbTestBase is disposed
        /// </summary>
        protected bool IsDisposed()
        {
            try
            {
                if (DbContext == null) return true;
                _ = DbContext.ChangeTracker;
                return false;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }

        /// <summary>
        /// Get service with error handling and caching
        /// </summary>
        protected T GetService<T>() where T : class
        {
            try
            {
                return Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(ServiceProvider);
            }
            catch (Exception ex)
            {
                var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<ILogger<LocalDbTestBase>>(ServiceProvider);
                logger?.LogError(ex, "Failed to resolve service {ServiceType}", typeof(T).Name);
                throw new InvalidOperationException($"Service resolution failed for {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Create a fresh scope for testing scenarios that need clean DI container
        /// </summary>
        protected IServiceScope CreateScope()
        {
            return ServiceProvider.CreateScope();
        }

        /// <summary>
        /// Seed minimal test data for performance
        /// Only adds data that's commonly needed across tests
        /// </summary>
        protected async Task SeedMinimalTestDataAsync()
        {
            var testBus = new Bus
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

            var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<ILogger<LocalDbTestBase>>(ServiceProvider);
            logger?.LogInformation("Minimal test data seeded for {DatabaseName}", DatabaseName);
        }

        /// <summary>
        /// Execute raw SQL command for advanced test scenarios
        /// </summary>
        protected async Task<int> ExecuteRawSqlAsync(string sql, params object[] parameters)
        {
            return await DbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        /// <summary>
        /// Check if database exists
        /// </summary>
        protected async Task<bool> DatabaseExistsAsync()
        {
            return await DbContext.Database.CanConnectAsync();
        }

        /// <summary>
        /// Get database statistics for performance monitoring
        /// </summary>
        protected async Task<DatabaseStats> GetDatabaseStatsAsync()
        {
            return await _databaseManager.GetDatabaseStatsAsync(DbContext);
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                // Clean up test database
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

                if (ServiceProvider != null)
                {
                    var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<ILogger<LocalDbTestBase>>(ServiceProvider);
                    logger?.LogInformation("LocalDB test database cleaned up: {DatabaseName}", DatabaseName);
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw during disposal
                Console.WriteLine($"Warning: Failed to cleanup test database {DatabaseName}: {ex.Message}");
            }

            GC.SuppressFinalize(this);
        }
    }
}

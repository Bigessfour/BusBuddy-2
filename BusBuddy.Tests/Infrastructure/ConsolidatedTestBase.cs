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
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// Consolidated test base class that follows Microsoft's best practices:
    /// - Uses SQLite in-memory for reliable test isolation (Microsoft recommended)
    /// - Supports UI component testing with DialogEventCapture
    /// - Provides consistent database setup/teardown
    /// - Includes utilities for entity tracking management
    /// </summary>
    public abstract class ConsolidatedTestBase : IDisposable, IAsyncDisposable
    {
        protected ServiceProvider ServiceProvider { get; private set; } = null!;
        protected BusBuddyDbContext DbContext { get; private set; } = null!;
        protected IConfiguration Configuration { get; private set; } = null!;
        protected DialogEventCapture DialogCapture { get; private set; } = null!;

        protected ConsolidatedTestBase()
        {
            BusBuddyDbContext.SkipGlobalSeedData = true;
            SetupServices();
            DbContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<BusBuddyDbContext>(ServiceProvider);
            DialogCapture = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<DialogEventCapture>(ServiceProvider);
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
        /// Setup services using SQLite in-memory database per Microsoft recommendations
        /// </summary>
        private void SetupServices()
        {
            var services = new ServiceCollection();
            ConfigureSharedServices(services);
            ConfigureTestSpecificServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Configures shared services common across all tests
        /// </summary>
        private void ConfigureSharedServices(ServiceCollection services)
        {
            // Test configuration
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(AppContext.BaseDirectory);

            // Try to load test config file if it exists
            try
            {
                configBuilder.AddJsonFile("appsettings.test.json", optional: true, reloadOnChange: false);
            }
            catch
            {
                // Fallback to in-memory config if file doesn't exist
            }

            // Add memory configuration for test settings
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["Syncfusion:LicenseKey"] = "Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhec3RSRGRYU0R2WUBWYEk=",
                ["TestSettings:UseInMemoryDatabase"] = "true",
                ["TestSettings:UseSQLite"] = "false",
                ["TestSettings:EnableDetailedLogging"] = "false",
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning"
            };

            configBuilder.AddInMemoryCollection(inMemorySettings);
            Configuration = configBuilder.Build();
            services.AddSingleton(Configuration);

            // Logging configuration
            services.AddLogging(builder =>
            {
                builder.AddConsole();

                var defaultLogLevel = Configuration.GetValue<LogLevel>("Logging:LogLevel:Default", LogLevel.Information);
                var efLogLevel = Configuration.GetValue<LogLevel>("Logging:LogLevel:Microsoft.EntityFrameworkCore", LogLevel.Warning);

                builder.SetMinimumLevel(defaultLogLevel);
                builder.AddFilter("Microsoft.EntityFrameworkCore", efLogLevel);
            });

            // EF Core InMemory database configuration (same as TestBase for consistency)
            services.AddDbContext<BusBuddyDbContext>(options =>
            {
                // Create unique in-memory database for each test
                options.UseInMemoryDatabase($"ConsolidatedTestDb_{Guid.NewGuid()}");
                options.EnableSensitiveDataLogging();
            }, ServiceLifetime.Transient); // Transient for proper disposal

            // Repository Pattern
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IBusRepository, BusRepository>();

            // Unit of Work Pattern
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

            // Dialog Event Capture Service for UI testing
            services.AddSingleton<DialogEventCapture>();
        }

        /// <summary>
        /// Override in derived test classes to configure test-specific services
        /// Use for mocking dependencies or adding specialized test services
        /// </summary>
        protected virtual void ConfigureTestSpecificServices(ServiceCollection services)
        {
            // Default implementation - override in derived classes for customization
        }

        /// <summary>
        /// Clears database and resets for a clean test state
        /// </summary>
        protected async Task ClearDatabaseAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Sets up a new isolated database for the test
        /// Creates a unique InMemory database to prevent test interference
        /// </summary>
        protected void SetupTestDatabase()
        {



            // Always clean up any previous test state before starting a new test
            TearDownTestDatabase();

            // Prevent global seed data for test isolation
            BusBuddyDbContext.SkipGlobalSeedData = true;

            // Rebuild DI container and all services for this test
            var services = new ServiceCollection();
            ConfigureSharedServices(services);
            ConfigureTestSpecificServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Re-resolve DbContext and DialogCapture from the new provider
            DbContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<BusBuddyDbContext>(ServiceProvider);
            DialogCapture = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<DialogEventCapture>(ServiceProvider);

            // Ensure the database is created and ready
            DbContext.Database.EnsureCreated();
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Tears down the test database
        /// Ensures proper cleanup of test data
        /// </summary>
        protected void TearDownTestDatabase()
        {
            if (DbContext != null)
            {
                DbContext.ChangeTracker.Clear();
                DbContext.Database.EnsureDeleted();
            }
        }

        /// <summary>
        /// Clears the Entity Framework change tracker to prevent entity tracking conflicts
        /// Call this between test operations to prevent tracking conflicts
        /// </summary>
        protected void ClearChangeTracker()
        {
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Detaches all tracked entities to prevent conflicts
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
        /// Registers a mocked service for unit testing with isolated dependencies
        /// </summary>
        protected void RegisterMock<TService, TImplementation>(ServiceCollection services, Mock<TImplementation> mock)
            where TService : class
            where TImplementation : class, TService
        {
            // Remove existing registration
            var existingDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TService));
            if (existingDescriptor != null)
            {
                services.Remove(existingDescriptor);
            }

            // Add mock implementation
            services.AddScoped<TService>(_ => mock.Object);
        }

        /// <summary>
        /// Seeds common test data for testing
        /// </summary>
        protected async Task SeedTestDataAsync()
        {
            // Create a test bus
            var testBus = new Bus
            {
                BusNumber = "TEST001",
                Year = 2020,
                Make = "Blue Bird",
                Model = "Vision",
                SeatingCapacity = 72,
                VINNumber = "TST001234567890",
                LicenseNumber = "TST001",
                Status = "Active"
            };

            // Create a test driver
            var testDriver = new Driver
            {
                DriverName = "Test Driver",
                DriverPhone = "(555) 123-4567",
                DriverEmail = "testdriver@busbuddy.com",
                DriversLicenceType = "CDL",
                TrainingComplete = true
            };

            // Create a test route
            var testRoute = new Route
            {
                RouteName = "Test Route 001",
                Date = DateTime.Today,
                IsActive = true
            };

            // Add entities to context
            DbContext.Vehicles.Add(testBus);
            DbContext.Drivers.Add(testDriver);
            DbContext.Routes.Add(testRoute);

            await DbContext.SaveChangesAsync();
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Creates a test Bus entity with proper data constraints
        /// </summary>
        protected Bus CreateTestBus(string? busNumber = null, string? vinNumber = null, string status = "Active")
        {
            var testId = Guid.NewGuid().ToString("N")[0..8].ToUpper();
            var defaultVin = $"{testId}123456789";

            return new Bus
            {
                BusNumber = busNumber ?? $"BUS{testId}",
                VINNumber = (vinNumber ?? defaultVin)[0..Math.Min(17, (vinNumber ?? defaultVin).Length)],
                LicenseNumber = $"LIC{testId}"[0..Math.Min(20, $"LIC{testId}".Length)],
                Make = "Blue Bird",
                Model = "Vision",
                Year = 2020,
                Status = status,
                SeatingCapacity = 72
            };
        }

        /// <summary>
        /// Creates a test Driver entity with proper data constraints
        /// </summary>
        protected Driver CreateTestDriver(string? driverName = null)
        {
            var testId = Guid.NewGuid().ToString("N")[0..8].ToUpper();

            return new Driver
            {
                DriverName = driverName ?? $"Driver{testId}",
                DriverPhone = "(555) 123-4567",
                DriverEmail = $"driver{testId}@test.com",
                DriversLicenceType = "CDL",
                TrainingComplete = true
            };
        }

        /// <summary>
        /// Get service with proper error handling
        /// </summary>
        protected T GetService<T>() where T : class
        {
            try
            {
                return Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(ServiceProvider);
            }
            catch (Exception ex)
            {
                var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<ILogger<ConsolidatedTestBase>>(ServiceProvider);
                logger?.LogError(ex, "Failed to resolve service {ServiceType}", typeof(T).Name);
                throw new InvalidOperationException($"Service resolution failed for {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Starts capturing dialog events during test execution
        /// </summary>
        protected void StartDialogCapture()
        {
            DialogCapture?.StartCapture();
        }

        /// <summary>
        /// Stops capturing dialog events and returns summary
        /// </summary>
        protected string StopDialogCaptureAndGetReport()
        {
            DialogCapture?.StopCapture();
            return DialogCapture?.GetDialogSummaryReport() ?? "No dialog capture available";
        }

        /// <summary>
        /// Gets all captured dialogs without stopping capture
        /// </summary>
        protected IReadOnlyList<DialogEvent> GetCapturedDialogs()
        {
            return DialogCapture?.GetCapturedDialogs() ?? new List<DialogEvent>().AsReadOnly();
        }

        /// <summary>
        /// Logs detailed information about all captured dialogs
        /// </summary>
        protected void LogCapturedDialogs()
        {
            DialogCapture?.LogDialogDetails();
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            if (DialogCapture != null)
            {
                DialogCapture.StopCapture();
                DialogCapture.LogDialogDetails();
                DialogCapture.Dispose();
            }

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

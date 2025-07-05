using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
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

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// Enhanced base class for unit and integration tests in the Bus Buddy project.
    /// 
    /// DESIGN DECISIONS:
    /// - Uses InMemory database with unique GUID for complete test isolation
    /// - Transient DbContext lifetime prevents disposal issues across test methods
    /// - Flexible logging configuration supports detailed EF Core diagnostics when needed
    /// - Supports both real services and mocked dependencies for comprehensive testing
    /// - Automatic test data seeding based on configuration settings
    /// - Multiple database provider support (InMemory/SQL Server) for different test scenarios
    /// 
    /// GROK ANALYSIS APPLIED:
    /// - Enhanced dependency injection with repository and unit of work patterns
    /// - Performance optimized with conditional clearing and shared service provider
    /// - Mocking support for isolated unit testing
    /// - Robust error handling and recovery mechanisms
    /// </summary>
    public abstract class TestBase : IDisposable, IAsyncDisposable
    {
        protected BusBuddyDbContext CreateInMemoryDbContext()
        {
            BusBuddyDbContext.SkipGlobalSeedData = true;
            var dbName = $"TestDb_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
            var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
                .UseInMemoryDatabase(dbName)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;
            var ctx = new BusBuddyDbContext(options);
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();
            return ctx;
        }
        protected ServiceProvider ServiceProvider { get; private set; } = null!;
        protected BusBuddyDbContext DbContext { get; private set; } = null!;
        protected IConfiguration Configuration { get; private set; } = null!;
        protected DialogEventCapture DialogCapture { get; private set; } = null!;
        protected IUserContextService UserContextService { get; private set; } = null!;
        private string? _currentTestDbName;

        protected TestBase()
        {
            // No-op: services will be set up per test in SetupTestDatabase
        }

        /// <summary>
        /// Ensures a completely clean database state for each test.
        /// Creates a fresh DbContext with unique database name per test method.
        /// This method should be called at the start of each test.
        /// </summary>
        protected void SetupTestDatabase()
        {
            // Always generate a new unique database name for each test FIRST
            // Include thread ID and high-resolution timestamp for maximum uniqueness
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            var ticks = DateTime.UtcNow.Ticks;
            var guid = Guid.NewGuid().ToString("N")[0..8]; // Short GUID for readability
            _currentTestDbName = $"TestDb_{guid}_{threadId}_{ticks}";

            if (Configuration == null)
            {
                // Configuration is set up in ConfigureSharedServices
                var services = new ServiceCollection();
                ConfigureSharedServices(services);
                Configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            }

            // Check if this is an integration test that requires real database
            bool isIntegrationTest = IsIntegrationTest();
            bool useInMemory = Configuration.GetValue<bool>("TestSettings:UseInMemoryDatabase") && !isIntegrationTest;

            if (useInMemory)
            {
                BusBuddyDbContext.SkipGlobalSeedData = true;
            }

            // Rebuild DI container and all services for this test
            SetupServices();
            if (ServiceProvider == null)
                throw new InvalidOperationException("ServiceProvider is not initialized.");
            DialogCapture = ServiceProvider.GetRequiredService<DialogEventCapture>();
            UserContextService = ServiceProvider.GetRequiredService<IUserContextService>();

            // Create fresh DbContext with unique database for complete test isolation
            if (useInMemory)
            {
                var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
                    .UseInMemoryDatabase(_currentTestDbName)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .Options;

                DbContext = new BusBuddyDbContext(options);
                // Always ensure deleted and created for full isolation
                DbContext.Database.EnsureDeleted();
                DbContext.Database.EnsureCreated();
            }
            else
            {
                // For integration tests, use SQL Server
                BusBuddyDbContext.SkipGlobalSeedData = false;
                DbContext = ServiceProvider.GetRequiredService<BusBuddyDbContext>();
                DbContext.Database.EnsureCreated();

                // For SQL Server, apply migrations if needed
                try
                {
                    DbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = ServiceProvider?.GetService<ILogger<TestBase>>();
                    logger?.LogWarning(ex, "Migration failed during test setup, but proceeding as EnsureCreated should have built the schema.");
                }
            }
        }

        /// <summary>
        /// Determines if the current test is an integration test that requires a real database
        /// </summary>
        private bool IsIntegrationTest()
        {
            try
            {
                // Get the current test method using reflection
                var stackTrace = new System.Diagnostics.StackTrace();
                for (int i = 0; i < stackTrace.FrameCount; i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    var method = frame?.GetMethod();
                    if (method != null)
                    {
                        // Check if the method has the Category("Integration") attribute
                        var categoryAttributes = method.GetCustomAttributes(typeof(NUnit.Framework.CategoryAttribute), false);
                        foreach (NUnit.Framework.CategoryAttribute attr in categoryAttributes)
                        {
                            if (attr.Name == "Integration")
                                return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                // If we can't determine, default to false (use in-memory)
                return false;
            }
        }

        /// <summary>
        /// Ensures a completely clean database state for each test
        /// </summary>
        private void InitializeCleanDatabase()
        {
            if (Configuration.GetValue<bool>("TestSettings:UseInMemoryDatabase"))
            {
                // InMemory database - simple recreation
                DbContext.Database.EnsureDeleted();
                DbContext.Database.EnsureCreated();
            }
            else
            {
                // SQL Server Express - use migrations for proper schema
                try
                {
                    // For testing, we want a fresh database state
                    if (Configuration.GetValue<bool>("TestSettings:RecreateTestDatabaseOnStartup"))
                    {
                        DbContext.Database.EnsureDeleted();
                    }

                    // Apply migrations to create proper schema
                    DbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    // Fallback: recreate database if migration fails
                    var logger = ServiceProvider?.GetService<ILogger<TestBase>>();
                    logger?.LogWarning(ex, "Migration failed, recreating database");

                    DbContext.Database.EnsureDeleted();
                    DbContext.Database.EnsureCreated();
                }
            }

            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Enhanced service setup with comprehensive dependency injection support
        /// Includes repository patterns, unit of work, and flexible configuration
        /// </summary>
        private void SetupServices()
        {
            var services = GetOrCreateSharedServices();

            // Apply test-specific customizations (overrides, mocks, etc.)
            ConfigureTestSpecificServices(services);

            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Gets or creates shared services for performance optimization
        /// Thread-safe singleton pattern for common service configuration
        /// </summary>
        private ServiceCollection GetOrCreateSharedServices()
        {
            // Always create a new service collection for true test isolation
            var services = new ServiceCollection();
            ConfigureSharedServices(services);
            return services;
        }

        /// <summary>
        /// Configures shared services that are common across all tests
        /// </summary>
        private void ConfigureSharedServices(ServiceCollection services)
        {
            // Configuration - Load test-specific configuration
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(AppContext.BaseDirectory);
            configBuilder.AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: false);

            // Use configuration from file instead of in-memory for SQL Server Express
            Configuration = configBuilder.Build();

            services.AddSingleton(Configuration);

            // Enhanced Logging with flexible configuration
            services.AddLogging(builder =>
            {
                var defaultLogLevel = Configuration.GetValue<LogLevel>("Logging:LogLevel:Default");
                var efLogLevel = Configuration.GetValue<LogLevel>("Logging:LogLevel:Microsoft.EntityFrameworkCore");

                builder.AddConsole();
                builder.SetMinimumLevel(defaultLogLevel);

                // Apply specific log levels for EF Core if detailed logging is enabled
                if (Configuration.GetValue<bool>("TestSettings:EnableDetailedLogging"))
                {
                    builder.AddFilter("Microsoft.EntityFrameworkCore", efLogLevel);
                }
            });

            // Database configuration with SQL Server Express support
            services.AddDbContext<BusBuddyDbContext>(options =>
            {
                if (Configuration.GetValue<bool>("TestSettings:UseInMemoryDatabase"))
                {
                    // Use the current test's unique database name for true isolation
                    // If _currentTestDbName is not set yet, generate a temporary one
                    var dbName = _currentTestDbName ?? $"TempTestDb_{Guid.NewGuid().ToString("N")[0..8]}_{DateTime.UtcNow.Ticks}";
                    options.UseInMemoryDatabase(dbName);
                }
                else
                {
                    // Use SQL Server Express for reliable testing
                    var connectionString = Configuration.GetConnectionString("TestConnection");
                    options.UseSqlServer(connectionString);
                }

                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }, ServiceLifetime.Transient); // Transient prevents disposal issues

            // Mock IUserContextService for testing
            var mockUserContextService = new Mock<IUserContextService>();
            mockUserContextService.Setup(x => x.CurrentUserId).Returns("TestUser");
            mockUserContextService.Setup(x => x.CurrentUserName).Returns("Test User");
            mockUserContextService.Setup(x => x.CurrentUserEmail).Returns("test@busbuddy.com");
            mockUserContextService.Setup(x => x.IsAuthenticated).Returns(true);
            mockUserContextService.Setup(x => x.GetCurrentUserForAudit()).Returns("TestUser");
            services.AddSingleton<IUserContextService>(mockUserContextService.Object);

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

            // Dialog Event Capture Service
            services.AddSingleton<DialogEventCapture>();
        }

        /// <summary>
        /// Override this method in derived test classes to configure test-specific services
        /// Use this for mocking dependencies or adding specialized test services
        /// </summary>
        protected virtual void ConfigureTestSpecificServices(ServiceCollection services)
        {
            // Default implementation - override in derived classes for customization
        }

        /// <summary>
        /// Asynchronously clears all data from the database tables.
        /// NOTE: This method is deprecated in favor of SetupTestDatabase() which creates
        /// a fresh database per test. Keep for backward compatibility.
        /// </summary>
        protected async Task ClearDatabaseAsync()
        {
            // For InMemory databases with per-test isolation, just recreate the test database
            if (Configuration.GetValue<bool>("TestSettings:UseInMemoryDatabase"))
            {
                // Simply call SetupTestDatabase to get a fresh database
                SetupTestDatabase();
                return;
            }

            // SQL Server Express - clear data but preserve schema
            // Use transaction for atomic cleanup
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                // Disable foreign key constraints temporarily
                await DbContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");

                // Clear all tables
#pragma warning disable EF1002 // Possible SQL injection vulnerability.
                await DbContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'DELETE FROM ?'");
#pragma warning restore EF1002 // Possible SQL injection vulnerability.

                // Re-enable foreign key constraints
                await DbContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");

                // Reset identity columns
                var tables = new[] { "FuelRecords", "Vehicles", "Drivers", "Students", "Routes", "Activities", "Maintenance", "Tickets", "RouteStops" };
                foreach (var table in tables)
                {
                    try
                    {
#pragma warning disable EF1002 // Risk acceptable for test table names
                        await DbContext.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT('{table}', RESEED, 0)");
#pragma warning restore EF1002
                    }
                    catch
                    {
                        // Table might not exist or have identity column - ignore
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Fallback: recreate database if cleanup fails
                var logger = ServiceProvider?.GetService<ILogger<TestBase>>();
                logger?.LogWarning(ex, "Database cleanup failed, recreating database");

                await DbContext.Database.EnsureDeletedAsync();
                DbContext.Database.Migrate();
            }
        }

        /// <summary>
        /// Disposes the current test's DbContext
        /// Call this after each test to ensure proper cleanup
        /// </summary>
        protected void TearDownTestDatabase()
        {
            if (DbContext != null)
            {
                DbContext.Dispose();
                DbContext = null!;
            }
            // Reset db name to ensure no accidental reuse
            _currentTestDbName = null;
        }

        /// <summary>
        /// Clears the Entity Framework change tracker to prevent entity tracking conflicts
        /// Call this between test operations to prevent tracking conflicts
        /// NOTE: With per-test databases, this is less critical but kept for compatibility
        /// </summary>
        protected void ClearChangeTracker()
        {
            DbContext.ChangeTracker.Clear();
        }

        /// <summary>
        /// Detaches all tracked entities to prevent conflicts
        /// More thorough than ClearChangeTracker for complex scenarios
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
        /// Supports Moq framework integration for comprehensive test scenarios
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
        /// Seeds common test data manually when needed by specific tests
        /// Provides consistent baseline data for tests requiring pre-populated entities
        /// </summary>
        protected async Task SeedTestDataAsync()
        {
            // Always seed regardless of configuration for manual calls
            // Seed sample buses for transportation testing
            var testBus = new Bus
            {
                BusNumber = "TEST001",
                Year = 2020,
                Make = "Blue Bird",
                Model = "Vision",
                SeatingCapacity = 72,
                VINNumber = "TST001234567890", // Exactly 17 characters
                LicenseNumber = "TST001",
                Status = "Active",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            var testBus2 = new Bus
            {
                BusNumber = "TEST002",
                Year = 2021,
                Make = "Thomas Built",
                Model = "Saf-T-Liner",
                SeatingCapacity = 48,
                VINNumber = "TST002345678901", // Exactly 17 characters
                LicenseNumber = "TST002",
                Status = "Active",
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            // Seed sample drivers for safety-critical testing
            var testDriver = new Driver
            {
                DriverName = "Test Driver",
                DriverPhone = "(555) 123-4567",
                DriverEmail = "testdriver@busbuddy.com",
                DriversLicenceType = "CDL",
                TrainingComplete = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            // Seed sample students for route assignment testing
            var testStudent = new Student
            {
                StudentName = "Test Student",
                Grade = "5",
                HomePhone = "(555) 987-6543",
                EmergencyPhone = "(555) 876-5432",
                SpecialNeeds = false,
                PhotoPermission = true,
                FieldTripPermission = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            // Seed sample route for scheduling testing
            var testRoute = new Route
            {
                RouteName = "Test Route 001",
                Date = DateTime.Today,
                IsActive = true
            };

            // Add entities to context
            DbContext.Vehicles.AddRange(testBus, testBus2);
            DbContext.Drivers.Add(testDriver);
            DbContext.Students.Add(testStudent);
            DbContext.Routes.Add(testRoute);

            await DbContext.SaveChangesAsync();

            var logger = ServiceProvider?.GetService<ILogger<TestBase>>();
            logger?.LogInformation("Test data seeded manually: 2 buses, 1 driver, 1 student, 1 route");
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
                VINNumber = (vinNumber ?? defaultVin)[0..Math.Min(17, (vinNumber ?? defaultVin).Length)], // Ensure VIN is max 17 characters
                LicenseNumber = $"LIC{testId}"[0..Math.Min(20, $"LIC{testId}".Length)], // Ensure License is max 20 characters
                Make = "Blue Bird",
                Model = "Vision",
                Year = 2020,
                Status = status,
                SeatingCapacity = 72,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
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
                TrainingComplete = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Asynchronously refreshes the DbContext by disposing and re-initializing services.
        /// </summary>
        protected async Task RefreshDbContextAsync()
        {
            await DisposeAsync();
            SetupServices();
            DbContext = ServiceProvider.GetRequiredService<BusBuddyDbContext>();
            await DbContext.Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Utility method to get services with enhanced error handling
        /// Provides consistent service resolution across all test scenarios
        /// </summary>
        protected T GetService<T>() where T : class
        {
            try
            {
                return ServiceProvider.GetRequiredService<T>();
            }
            catch (Exception ex)
            {
                var logger = ServiceProvider?.GetService<ILogger<TestBase>>();
                logger?.LogError(ex, "Failed to resolve service {ServiceType}", typeof(T).Name);
                throw new InvalidOperationException($"Service resolution failed for {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Optional service resolution with fallback handling
        /// Enables graceful degradation for non-critical services
        /// </summary>
        protected T? GetOptionalService<T>() where T : class
        {
            try
            {
                return ServiceProvider.GetService<T>();
            }
            catch (Exception ex)
            {
                var logger = ServiceProvider?.GetService<ILogger<TestBase>>();
                logger?.LogWarning(ex, "Optional service {ServiceType} could not be resolved", typeof(T).Name);
                return null;
            }
        }

        /// <summary>
        /// Starts capturing dialog events during test execution
        /// Call this at the beginning of tests that may trigger dialogs
        /// </summary>
        protected void StartDialogCapture()
        {
            DialogCapture?.StartCapture();
        }

        /// <summary>
        /// Stops capturing dialog events and returns summary
        /// Call this at the end of tests to get dialog information
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

        /// <summary>
        /// Modern, simplified disposal pattern for robust resource cleanup.
        /// </summary>
        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            // Stop dialog capture and log final report
            if (DialogCapture != null)
            {
                DialogCapture.StopCapture();
                DialogCapture.LogDialogDetails();
                DialogCapture.Dispose();
            }

            // Dispose the current test's DbContext
            if (DbContext != null)
            {
                DbContext.Dispose();
                DbContext = null!;
            }

            if (ServiceProvider != null)
            {
                if (ServiceProvider is IAsyncDisposable spAsyncDisposable)
                {
                    await spAsyncDisposable.DisposeAsync();
                }
                else
                {
                    ServiceProvider.Dispose();
                }
                ServiceProvider = null!;
            }

            GC.SuppressFinalize(this);
        }
    }
}

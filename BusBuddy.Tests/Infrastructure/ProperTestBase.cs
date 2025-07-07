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

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// Proper test base that RESPECTS the application's intended design
    /// Works WITH the seeded data and business logic, not against it
    /// </summary>
    public abstract class ProperTestBase : IDisposable, IAsyncDisposable
    {
        protected ServiceProvider ServiceProvider { get; private set; } = null!;
        protected BusBuddyDbContext DbContext { get; private set; } = null!;
        protected IConfiguration Configuration { get; private set; } = null!;

        // Known seeded data from the application design
        protected const int EXPECTED_SEEDED_BUSES = 2;
        protected const int EXPECTED_SEEDED_DRIVERS = 2;

        protected ProperTestBase()
        {
            SetupServices();
            DbContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<BusBuddyDbContext>(ServiceProvider);

            // Work with the application's intended design
            InitializeWithApplicationDefaults();
        }

        /// <summary>
        /// Initialize using the application's natural behavior
        /// This respects the seeded data and audit fields
        /// </summary>
        private void InitializeWithApplicationDefaults()
        {
            // Ensure database is created with its intended seeded data
            DbContext.Database.EnsureCreated();

            // Set a test audit user for proper audit trail testing
            DbContext.SetAuditUser("TestUser");
        }

        private void SetupServices()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Use the application's natural configuration approach
            var configBuilder = new ConfigurationBuilder();
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["TestSettings:UseInMemoryDatabase"] = "true",
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning"
            };
            configBuilder.AddInMemoryCollection(inMemorySettings);
            Configuration = configBuilder.Build();

            services.AddSingleton(Configuration);

            // Use transient to get fresh instances but allow natural seeding
            services.AddDbContext<BusBuddyDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                options.EnableSensitiveDataLogging();
            }, ServiceLifetime.Transient);

            // Register the full application service stack
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IBusRepository, BusRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IBusService, BusService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IFuelService, FuelService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<ITicketService, TicketService>();

            services.AddLogging(builder => builder.AddConsole());
        }

        /// <summary>
        /// Get the expected initial count for an entity type
        /// This respects the application's seeded data
        /// </summary>
        protected int GetExpectedInitialCount<T>() where T : class
        {
            return typeof(T).Name switch
            {
                nameof(Bus) => EXPECTED_SEEDED_BUSES,
                nameof(Driver) => EXPECTED_SEEDED_DRIVERS,
                _ => 0 // Other entities start empty
            };
        }

        /// <summary>
        /// Reset to clean state while preserving intended seeded data
        /// </summary>
        protected async Task ResetToCleanStateAsync()
        {
            // Clear change tracker but keep seeded data
            DbContext.ChangeTracker.Clear();

            // Recreate with fresh seeded data
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }

        public void Dispose()
        {
            ServiceProvider?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (ServiceProvider is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                ServiceProvider?.Dispose();
        }
    }
}

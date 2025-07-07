using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Bus_Buddy.Data;
using Bus_Buddy.Data.Interfaces;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Data.UnitOfWork;
using Bus_Buddy.Services;
using BusBuddy.Services;

namespace Bus_Buddy.Extensions;

/// <summary>
/// Extension methods for registering data services with dependency injection
/// Configures database context, repositories, and unit of work pattern
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all data services including DbContext, repositories, and Unit of Work
    /// </summary>
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext with SQL Server
        services.AddDbContext<BusBuddyDbContext>(options =>
        {
            options.UseInMemoryDatabase("BusBuddyDb");
            //var connectionString = configuration.GetConnectionString("DefaultConnection")
            //    ?? throw new InvalidOperationException("DefaultConnection string is not configured.");

            //options.UseSqlServer(connectionString, sqlOptions =>
            //{
            //    sqlOptions.EnableRetryOnFailure(
            //        maxRetryCount: 3,
            //        maxRetryDelay: TimeSpan.FromSeconds(30),
            //        errorNumbersToAdd: null);
            //    sqlOptions.CommandTimeout(30);
            //});

            // Enable sensitive data logging in development
#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // Register repositories - use fully qualified names to avoid ambiguity
        services.AddScoped<IActivityRepository, Bus_Buddy.Data.Repositories.ActivityRepository>();
        services.AddScoped<IBusRepository, Bus_Buddy.Data.Repositories.BusRepository>();
        services.AddScoped<IDriverRepository, Bus_Buddy.Data.Repositories.DriverRepository>();
        services.AddScoped<IRouteRepository, Bus_Buddy.Data.Repositories.RouteRepository>();
        services.AddScoped<IStudentRepository, Bus_Buddy.Data.Repositories.StudentRepository>();
        services.AddScoped<IFuelRepository, Bus_Buddy.Data.Repositories.FuelRepository>();
        services.AddScoped<IMaintenanceRepository, Bus_Buddy.Data.Repositories.MaintenanceRepository>();
        services.AddScoped<IScheduleRepository, Bus_Buddy.Data.Repositories.ScheduleRepository>();
        services.AddScoped<ISchoolCalendarRepository, Bus_Buddy.Data.Repositories.SchoolCalendarRepository>();
        services.AddScoped<IActivityScheduleRepository, Bus_Buddy.Data.Repositories.ActivityScheduleRepository>();

        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Unit of Work - NOTE: UnitOfWork constructor now requires IUserContextService
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register User Context Service (must be registered before UnitOfWork since it depends on it)
        services.AddScoped<IUserContextService, UserContextService>();

        // Register Business Services
        services.AddScoped<IBusService, BusService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IFuelService, FuelService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<ITicketService, TicketService>();

        return services;
    }

    /// <summary>
    /// Register additional data services like caching, data validation, etc.
    /// </summary>
    public static IServiceCollection AddDataExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        // Add memory cache for performance optimization
        services.AddMemoryCache();

        // Add data protection if needed (commented out - requires additional setup)
        // services.AddDataProtection();

        // Register data validation services
        services.AddScoped<IDataValidationService, DataValidationService>();

        // Register audit services
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }

    /// <summary>
    /// Register AI and advanced services for Bus Buddy
    /// </summary>
    public static IServiceCollection AddAIServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register HttpClient for AI services
        services.AddHttpClient<BusBuddyAIReportingService>();

        // Register AI services
        services.AddScoped<TransportationContext>();
        services.AddScoped<ContextAwarePromptBuilder>();
        services.AddScoped<BusBuddyAIReportingService>();
        services.AddScoped<SmartRouteOptimizationService>();

        // Register existing AI services if they exist
        services.AddScoped<XAIService>();
        services.AddScoped<GoogleEarthEngineService>();

        // Register background monitoring service
        services.AddHostedService<FleetMonitoringService>();

        return services;
    }

    /// <summary>
    /// Configure database migration and seeding
    /// </summary>
    public static async Task<IServiceProvider> InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<BusBuddyDbContext>(scope.ServiceProvider);

        try
        {
            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            // Seed initial data if needed
            await SeedDatabaseAsync(context);
        }
        catch (Exception ex)
        {
            // Log the error - in a real application, use proper logging
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }

        return serviceProvider;
    }

    private static async Task SeedDatabaseAsync(BusBuddyDbContext context)
    {
        // Check if data already exists
        if (await context.Vehicles.AnyAsync())
        {
            return; // Database already seeded
        }

        // Add your seeding logic here
        // This is where you would add default data, lookup values, etc.

        await context.SaveChangesAsync();
    }
}

/// <summary>
/// Interface for data validation services
/// </summary>
public interface IDataValidationService
{
    Task<bool> ValidateEntityAsync<T>(T entity) where T : class;
    Task<List<string>> GetValidationErrorsAsync<T>(T entity) where T : class;
}

/// <summary>
/// Implementation of data validation services
/// </summary>
public class DataValidationService : IDataValidationService
{
    public async Task<bool> ValidateEntityAsync<T>(T entity) where T : class
    {
        // Implement custom validation logic
        await Task.CompletedTask;
        return true;
    }

    public async Task<List<string>> GetValidationErrorsAsync<T>(T entity) where T : class
    {
        // Implement validation error collection
        await Task.CompletedTask;
        return new List<string>();
    }
}

/// <summary>
/// Interface for audit services
/// </summary>
public interface IAuditService
{
    Task LogChangeAsync<T>(T entity, string operation, string userId) where T : class;
    Task<List<AuditEntry>> GetAuditTrailAsync<T>(int entityId) where T : class;
}

/// <summary>
/// Implementation of audit services
/// </summary>
public class AuditService : IAuditService
{
    public async Task LogChangeAsync<T>(T entity, string operation, string userId) where T : class
    {
        // Implement audit logging
        await Task.CompletedTask;
    }

    public async Task<List<AuditEntry>> GetAuditTrailAsync<T>(int entityId) where T : class
    {
        // Implement audit trail retrieval
        await Task.CompletedTask;
        return new List<AuditEntry>();
    }
}

/// <summary>
/// Audit entry model
/// </summary>
public class AuditEntry
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string Changes { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.Core.Data;

/// <summary>
/// Factory for creating DbContext instances, useful for async and multi-threaded scenarios
/// where a context might be needed outside the standard DI lifecycle
/// </summary>
public class BusBuddyDbContextFactory : IBusBuddyDbContextFactory, IDesignTimeDbContextFactory<BusBuddyDbContext>
{
    private readonly IServiceProvider? _serviceProvider;

    // Parameterless constructor for design-time services
    public BusBuddyDbContextFactory()
    {
        _serviceProvider = null;
    }

    public BusBuddyDbContextFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a new instance of BusBuddyDbContext with proper configuration
    /// Use this method when you need a fresh context outside the DI lifecycle
    /// </summary>
    /// <returns>A new instance of BusBuddyDbContext</returns>
    public BusBuddyDbContext CreateDbContext()
    {
        if (_serviceProvider == null)
        {
            // Fallback for design-time scenarios
            return CreateDbContext(Array.Empty<string>());
        }

        // Create a scope to ensure proper dependency resolution
        using var scope = _serviceProvider.CreateScope();

        // Get a fresh DbContext instance from the DI container
        var context = scope.ServiceProvider.GetRequiredService<BusBuddyDbContext>();

        // Configure query tracking to improve performance for read-only operations
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return context;
    }

    /// <summary>
    /// Creates a new instance of BusBuddyDbContext for write operations
    /// </summary>
    /// <returns>A new instance of BusBuddyDbContext configured for tracking changes</returns>
    public BusBuddyDbContext CreateWriteDbContext()
    {
        if (_serviceProvider == null)
        {
            // Fallback for design-time scenarios
            var context = CreateDbContext(Array.Empty<string>());
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            return context;
        }

        // Create a scope to ensure proper dependency resolution
        using var scope = _serviceProvider.CreateScope();

        // Get a fresh DbContext instance from the DI container
        var dbContext = scope.ServiceProvider.GetRequiredService<BusBuddyDbContext>();

        // Configure for tracking entities when we need to make changes
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        return dbContext;
    }

    /// <summary>
    /// Creates a new instance of BusBuddyDbContext for design-time use
    /// This is typically used by EF Core tools for migrations and scaffolding
    /// </summary>
    /// <param name="args">Command-line arguments (not used)</param>
    /// <returns>A new instance of BusBuddyDbContext configured for design-time services</returns>
    public BusBuddyDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BusBuddyDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        return new BusBuddyDbContext(optionsBuilder.Options);
    }
}

/// <summary>
/// Interface for the DbContext factory to enable proper dependency injection
/// </summary>
public interface IBusBuddyDbContextFactory
{
    BusBuddyDbContext CreateDbContext();
    BusBuddyDbContext CreateWriteDbContext();
}

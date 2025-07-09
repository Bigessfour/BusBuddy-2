using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.Core.Data;

/// <summary>
/// Factory for creating DbContext instances, useful for async and multi-threaded scenarios
/// where a context might be needed outside the standard DI lifecycle
/// </summary>
public class BusBuddyDbContextFactory : IBusBuddyDbContextFactory
{
    private readonly IServiceProvider _serviceProvider;

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
        // Create a new scope to ensure proper dependency resolution
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
        // Create a new scope to ensure proper dependency resolution
        using var scope = _serviceProvider.CreateScope();

        // Get a fresh DbContext instance from the DI container
        var context = scope.ServiceProvider.GetRequiredService<BusBuddyDbContext>();

        // Configure for tracking entities when we need to make changes
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        return context;
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

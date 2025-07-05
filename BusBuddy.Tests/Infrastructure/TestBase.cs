using BusBuddy.Tests.Infrastructure;
using Bus_Buddy.Data;
using Bus_Buddy.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// Compatibility alias for existing tests that reference TestBase
    /// This allows gradual migration to LocalDbTestBase without breaking existing tests
    /// 
    /// MIGRATION NOTE: 
    /// - New tests should inherit from LocalDbTestBase directly
    /// - Existing tests can continue using TestBase (which now uses LocalDB)
    /// - This provides backward compatibility during the transition period
    /// </summary>
    public abstract class TestBase : LocalDbTestBase
    {
        private readonly IUserContextService _userContextService;

        /// <summary>
        /// TestBase now uses SQL Server LocalDB instead of in-memory testing
        /// This provides better compatibility with production SQL Server environment
        /// </summary>
        protected TestBase() : base()
        {
            // All functionality is inherited from LocalDbTestBase
            // This is just a compatibility wrapper for existing tests
            _userContextService = GetService<IUserContextService>();
        }

        #region Backward Compatibility Methods

        /// <summary>
        /// Compatibility method for existing tests
        /// Maps to the new LocalDB initialization
        /// </summary>
        protected virtual void SetupTestDatabase()
        {
            // The database is already set up in the constructor
            // This method is kept for backward compatibility
            try
            {
                // Only clear change tracker if DbContext is not disposed
                if (DbContext != null && !IsDbContextDisposed())
                {
                    ClearChangeTracker();
                }
            }
            catch (ObjectDisposedException)
            {
                // Context is disposed, this is expected during teardown
                // Do nothing as the database is already set up in constructor
            }
        }

        /// <summary>
        /// Compatibility method for existing tests
        /// Maps to the new fast database cleanup
        /// </summary>
        protected virtual void TearDownTestDatabase()
        {
            try
            {
                // Only perform cleanup if DbContext is not disposed
                if (DbContext != null && !IsDbContextDisposed())
                {
                    // Use fast database cleanup
                    ClearDatabaseAsync().GetAwaiter().GetResult();
                }
            }
            catch (ObjectDisposedException)
            {
                // Context is already disposed, cleanup not needed
            }
        }

        /// <summary>
        /// Check if DbContext is disposed without triggering exceptions
        /// </summary>
        private bool IsDbContextDisposed()
        {
            try
            {
                _ = DbContext.ChangeTracker;
                return false;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }

        /// <summary>
        /// Compatibility method for existing tests that used in-memory DbContext
        /// Now returns the LocalDB context
        /// </summary>
        protected virtual BusBuddyDbContext CreateInMemoryDbContext()
        {
            // Return the existing LocalDB context instead of creating in-memory
            // This maintains compatibility while providing better testing
            return DbContext;
        }

        /// <summary>
        /// Compatibility property for tests that reference UserContextService
        /// Returns the configured UserContextService from DI container
        /// </summary>
        protected virtual IUserContextService UserContextService => _userContextService;

        #endregion
    }
}

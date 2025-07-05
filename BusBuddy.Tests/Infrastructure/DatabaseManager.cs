using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Bus_Buddy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// Database manager for handling LocalDB test database operations
    /// Separates database naming and management logic from test base classes
    /// Optimized for performance and reliability
    /// </summary>
    public class DatabaseManager
    {
        private const string LocalDbInstance = "(localdb)\\mssqllocaldb";
        private const string DatabasePrefix = "BusBuddyTest_";
        private static readonly Dictionary<string, string> _databaseNameCache = new();
        private static readonly object _cachelock = new object();

        /// <summary>
        /// Generate unique database name for test class
        /// Uses caching to avoid recreating names for the same test class
        /// </summary>
        public string GenerateTestDatabaseName(string testClassName)
        {
            lock (_cachelock)
            {
                if (_databaseNameCache.TryGetValue(testClassName, out var cachedName))
                {
                    return cachedName;
                }

                // Create unique database name with timestamp and guid for isolation
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var uniqueId = Guid.NewGuid().ToString("N")[..8]; // First 8 chars
                var cleanClassName = SanitizeClassName(testClassName);

                var databaseName = $"{DatabasePrefix}{cleanClassName}_{timestamp}_{uniqueId}";

                // Ensure database name doesn't exceed SQL Server limits
                if (databaseName.Length > 128)
                {
                    databaseName = $"{DatabasePrefix}{uniqueId}_{timestamp}";
                }

                _databaseNameCache[testClassName] = databaseName;
                return databaseName;
            }
        }

        /// <summary>
        /// Build LocalDB connection string for test database
        /// </summary>
        public string BuildLocalDbConnectionString(string databaseName)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = LocalDbInstance,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,
                TrustServerCertificate = true,
                Encrypt = false,
                ConnectTimeout = 30,
                CommandTimeout = 30,
                Pooling = true,
                MinPoolSize = 1,
                MaxPoolSize = 5
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// Fast table truncation for database cleanup
        /// Much faster than dropping and recreating database
        /// </summary>
        public async Task TruncateAllTablesAsync(BusBuddyDbContext context)
        {
            const string truncateScript = @"
                -- Disable all foreign key constraints
                EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
                
                -- Truncate all tables except system tables
                EXEC sp_MSforeachtable '
                    IF ''?'' NOT LIKE ''%sysdiagram%'' 
                    BEGIN
                        DELETE FROM ?
                    END'
                
                -- Re-enable all foreign key constraints  
                EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
                
                -- Reset identity columns
                EXEC sp_MSforeachtable '
                    IF ''?'' NOT LIKE ''%sysdiagram%''
                    BEGIN
                        DBCC CHECKIDENT(''?'', RESEED, 0)
                    END'";

            try
            {
                await context.Database.ExecuteSqlRawAsync(truncateScript);
            }
            catch (Exception)
            {
                // Fallback to individual table truncation if batch fails
                await TruncateTablesIndividuallyAsync(context);
            }
        }

        /// <summary>
        /// Fallback method to truncate tables individually
        /// </summary>
        private async Task TruncateTablesIndividuallyAsync(BusBuddyDbContext context)
        {
            var tableNames = new[]
            {
                "Activities", "Tickets", "MaintenanceRecords", "FuelRecords",
                "ScheduleAssignments", "Schedules", "Routes", "StudentBusAssignments",
                "Students", "DriverBusAssignments", "Drivers", "Vehicles"
            };

            // Disable foreign key constraints
            await context.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");

            // Truncate tables in dependency order (table names are safe as they're predefined)
            foreach (var tableName in tableNames)
            {
                try
                {
                    // Using predefined table names, so SQL injection is not a concern
#pragma warning disable EF1002 // Risk of SQL injection
                    await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{tableName}]");
                    await context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT('[{tableName}]', RESEED, 0)");
#pragma warning restore EF1002 // Risk of SQL injection
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to truncate table {tableName}: {ex.Message}");
                }
            }

            // Re-enable foreign key constraints
            await context.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");
        }

        /// <summary>
        /// Get database statistics for monitoring performance
        /// </summary>
        public async Task<DatabaseStats> GetDatabaseStatsAsync(BusBuddyDbContext context)
        {
            const string statsQuery = @"
                SELECT 
                    DB_NAME() as DatabaseName,
                    (SELECT COUNT(*) FROM sys.tables WHERE type = 'U') as TableCount,
                    (SELECT SUM(rows) FROM sys.partitions WHERE index_id IN (0,1)) as TotalRows,
                    (SELECT SUM(total_pages) * 8 / 1024 FROM sys.allocation_units) as SizeMB";

            try
            {
                using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = statsQuery;

                if (command.Connection?.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection!.OpenAsync();
                }

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new DatabaseStats
                    {
                        DatabaseName = reader["DatabaseName"]?.ToString() ?? "Unknown",
                        TableCount = Convert.ToInt32(reader["TableCount"] ?? 0),
                        TotalRows = Convert.ToInt64(reader["TotalRows"] ?? 0),
                        SizeMB = Convert.ToDouble(reader["SizeMB"] ?? 0),
                        LastUpdated = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get database stats: {ex.Message}");
            }

            return new DatabaseStats
            {
                DatabaseName = "Unknown",
                TableCount = 0,
                TotalRows = 0,
                SizeMB = 0,
                LastUpdated = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Check if LocalDB instance is available
        /// </summary>
        public async Task<bool> IsLocalDbAvailableAsync()
        {
            try
            {
                var connectionString = BuildLocalDbConnectionString("master");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                await connection.CloseAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Cleanup orphaned test databases
        /// Removes test databases older than specified hours
        /// </summary>
        public async Task CleanupOrphanedTestDatabasesAsync(int olderThanHours = 24)
        {
            try
            {
                var connectionString = BuildLocalDbConnectionString("master");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var cleanupQuery = $@"
                    SELECT name 
                    FROM sys.databases 
                    WHERE name LIKE '{DatabasePrefix}%' 
                    AND create_date < DATEADD(hour, -{olderThanHours}, GETDATE())";

                using var command = new SqlCommand(cleanupQuery, connection);
                using var reader = await command.ExecuteReaderAsync();

                var databasesToDelete = new List<string>();
                while (await reader.ReadAsync())
                {
                    databasesToDelete.Add(reader["name"].ToString()!);
                }

                await reader.CloseAsync();

                foreach (var dbName in databasesToDelete)
                {
                    try
                    {
                        var dropQuery = $@"
                            ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE [{dbName}];";

                        using var dropCommand = new SqlCommand(dropQuery, connection);
                        await dropCommand.ExecuteNonQueryAsync();

                        Debug.WriteLine($"Cleaned up orphaned test database: {dbName}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to cleanup database {dbName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to cleanup orphaned databases: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear database name cache
        /// </summary>
        public void ClearCache()
        {
            lock (_cachelock)
            {
                _databaseNameCache.Clear();
            }
        }

        /// <summary>
        /// Sanitize class name for use in database name
        /// </summary>
        private static string SanitizeClassName(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return "Unknown";

            // Remove generic type markers and special characters
            var sanitized = className
                .Replace("`", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace(".", "_")
                .Replace(" ", "_")
                .Replace("-", "_");

            // Take only the last part if it's a fully qualified name
            var parts = sanitized.Split('_');
            var cleanName = parts.Length > 0 ? parts[^1] : sanitized;

            // Limit length to avoid database name issues
            return cleanName.Length > 50 ? cleanName[..50] : cleanName;
        }
    }

    /// <summary>
    /// Database statistics for performance monitoring
    /// </summary>
    public class DatabaseStats
    {
        public string DatabaseName { get; set; } = string.Empty;
        public int TableCount { get; set; }
        public long TotalRows { get; set; }
        public double SizeMB { get; set; }
        public DateTime LastUpdated { get; set; }

        public override string ToString()
        {
            return $"Database: {DatabaseName}, Tables: {TableCount}, Rows: {TotalRows:N0}, Size: {SizeMB:F2} MB";
        }
    }
}

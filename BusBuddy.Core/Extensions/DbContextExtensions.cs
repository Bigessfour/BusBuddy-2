using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Diagnostics;

namespace BusBuddy.Core.Extensions
{
    /// <summary>
    /// Extensions for DbContext to improve logging and performance tracking
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Executes a query with detailed performance logging
        /// </summary>
        public static async Task<T> ExecuteWithLoggingAsync<T>(
            this DbContext dbContext,
            ILogger logger,
            string queryName,
            Func<Task<T>> queryFunc,
            string? additionalContextName = null,
            object? additionalContextValue = null)
        {
            var stopwatch = Stopwatch.StartNew();

            using (LogContext.PushProperty("QueryName", queryName))
            using (LogContext.PushProperty("DatabaseOperation", true))
            {
                if (additionalContextName != null && additionalContextValue != null)
                {
                    using (LogContext.PushProperty(additionalContextName, additionalContextValue))
                    {
                        logger.LogDebug("Starting database query: {QueryName}", queryName);
                        try
                        {
                            var result = await queryFunc();
                            stopwatch.Stop();

                            using (LogContext.PushProperty("Duration", stopwatch.ElapsedMilliseconds))
                            {
                                logger.LogInformation("Database query {QueryName} completed in {Duration}ms",
                                    queryName, stopwatch.ElapsedMilliseconds);
                            }

                            return result;
                        }
                        catch (Exception ex)
                        {
                            stopwatch.Stop();

                            using (LogContext.PushProperty("Duration", stopwatch.ElapsedMilliseconds))
                            {
                                logger.LogError(ex, "Database query {QueryName} failed after {Duration}ms",
                                    queryName, stopwatch.ElapsedMilliseconds);
                            }

                            throw;
                        }
                    }
                }
                else
                {
                    logger.LogDebug("Starting database query: {QueryName}", queryName);
                    try
                    {
                        var result = await queryFunc();
                        stopwatch.Stop();

                        using (LogContext.PushProperty("Duration", stopwatch.ElapsedMilliseconds))
                        {
                            logger.LogInformation("Database query {QueryName} completed in {Duration}ms",
                                queryName, stopwatch.ElapsedMilliseconds);
                        }

                        return result;
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();

                        using (LogContext.PushProperty("Duration", stopwatch.ElapsedMilliseconds))
                        {
                            logger.LogError(ex, "Database query {QueryName} failed after {Duration}ms",
                                queryName, stopwatch.ElapsedMilliseconds);
                        }

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Executes an update/insert/delete operation with detailed performance logging
        /// </summary>
        public static async Task<int> SaveChangesWithLoggingAsync(
            this DbContext dbContext,
            ILogger logger,
            string operationName,
            string? additionalContextName = null,
            object? additionalContextValue = null)
        {
            var stopwatch = Stopwatch.StartNew();

            using (LogContext.PushProperty("OperationName", operationName))
            using (LogContext.PushProperty("DatabaseOperation", true))
            {
                if (additionalContextName != null && additionalContextValue != null)
                {
                    using (LogContext.PushProperty(additionalContextName, additionalContextValue))
                    {
                        logger.LogDebug("Starting database operation: {OperationName}", operationName);
                        try
                        {
                            var entriesBeforeSave = dbContext.ChangeTracker.Entries().Count();
                            var result = await dbContext.SaveChangesAsync();
                            stopwatch.Stop();

                            using (LogContext.PushProperty("Duration", stopwatch.ElapsedMilliseconds))
                            using (LogContext.PushProperty("ChangedEntities", result))
                            {
                                logger.LogInformation("Database operation {OperationName} completed in {Duration}ms. Changed {ChangedEntities} entities.",
                                    operationName, stopwatch.ElapsedMilliseconds, result);
                            }

                            return result;
                        }
                        catch (Exception ex)
                        {
                            stopwatch.Stop();

                            using (LogContext.PushProperty("Duration", stopwatch.ElapsedMilliseconds))
                            {
                                logger.LogError(ex, "Database operation {OperationName} failed after {Duration}ms",
                                    operationName, stopwatch.ElapsedMilliseconds);
                            }

                            throw;
                        }
                    }
                }
                else
                {
                    logger.LogDebug("Starting database operation: {OperationName}", operationName);
                    try
                    {
                        var entriesBeforeSave = dbContext.ChangeTracker.Entries().Count();
                        var result = await dbContext.SaveChangesAsync();
                        stopwatch.Stop();

                        using (LogContext.PushProperty("Duration", stopwatch.ElapsedMilliseconds))
                        using (LogContext.PushProperty("ChangedEntities", result))
                        {
                            logger.LogInformation("Database operation {OperationName} completed in {Duration}ms. Changed {ChangedEntities} entities.",
                                operationName, stopwatch.ElapsedMilliseconds, result);
                        }

                        return result;
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();

                        using (LogContext.PushProperty("Duration", stopwatch.ElapsedMilliseconds))
                        {
                            logger.LogError(ex, "Database operation {OperationName} failed after {Duration}ms",
                                operationName, stopwatch.ElapsedMilliseconds);
                        }

                        throw;
                    }
                }
            }
        }
    }
}

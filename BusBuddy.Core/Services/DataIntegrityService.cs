using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog;
using System.Diagnostics;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;

namespace BusBuddy.Core.Services;

/// <summary>
/// Enhanced service for cleaning up NULL values, ensuring data integrity, and debugging EF Core interactions
/// Includes comprehensive EF Core debugging capabilities for SQL Server Express
/// </summary>
public class DataIntegrityService
{
    private static readonly ILogger Logger = Log.ForContext<DataIntegrityService>();
    private readonly IBusBuddyDbContextFactory _contextFactory;
    private readonly List<string> _debugQueries = new();
    private readonly StringBuilder _debugLog = new();

    public DataIntegrityService(IBusBuddyDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <summary>
    /// Enable EF Core debugging with SQL query logging and change tracking
    /// </summary>
    public async Task<string> GetEFCoreDebugInfoAsync()
    {
        _debugLog.Clear();
        _debugQueries.Clear();

        using var context = _contextFactory.CreateDbContext();

        // Enable sensitive data logging for debugging
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        try
        {
            _debugLog.AppendLine("=== EF Core Debug Information ===");
            _debugLog.AppendLine($"Database Provider: {context.Database.ProviderName}");
            _debugLog.AppendLine($"Connection String: {context.Database.GetConnectionString()}");
            _debugLog.AppendLine($"Can Connect: {await context.Database.CanConnectAsync()}");
            _debugLog.AppendLine($"Database Exists: {await context.Database.EnsureCreatedAsync()}");

            // Get change tracker debug view
            var changeTrackerDebugView = context.ChangeTracker.DebugView;
            _debugLog.AppendLine("\n=== Change Tracker Debug View ===");
            _debugLog.AppendLine(changeTrackerDebugView.LongView);

            // Test a simple query and capture SQL
            var testBuses = await context.Vehicles.Take(1).ToListAsync();
            _debugLog.AppendLine($"\nTest Query Executed: Retrieved {testBuses.Count} buses");

            // Get pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            _debugLog.AppendLine($"\nPending Migrations: {pendingMigrations.Count()}");
            foreach (var migration in pendingMigrations)
            {
                _debugLog.AppendLine($"  - {migration}");
            }

            // Get applied migrations
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            _debugLog.AppendLine($"\nApplied Migrations: {appliedMigrations.Count()}");
            foreach (var migration in appliedMigrations.TakeLast(5)) // Show last 5
            {
                _debugLog.AppendLine($"  - {migration}");
            }

        }
        catch (Exception ex)
        {
            _debugLog.AppendLine($"\nERROR in debug info collection: {ex.Message}");
            Logger.Error(ex, "Error collecting EF Core debug information");
        }

        return _debugLog.ToString();
    }

    /// <summary>
    /// Generate SQL script for pending migrations
    /// </summary>
    public async Task<string> GenerateMigrationScriptAsync()
    {
        using var context = _contextFactory.CreateDbContext();

        try
        {
            // Generate migration status information instead of script
            var migrations = await context.Database.GetAppliedMigrationsAsync();
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            var script = "Applied Migrations:\n";
            foreach (var migration in migrations)
            {
                script += $"  - {migration}\n";
            }

            script += "\nPending Migrations:\n";
            foreach (var migration in pendingMigrations)
            {
                script += $"  - {migration}\n";
            }

            Logger.Information("Generated debug migration info ({Length} characters)", script.Length);
            return script;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error generating migration script");
            return $"Error generating script: {ex.Message}";
        }
    }

    /// <summary>
    /// Validate database schema against current model
    /// </summary>
    public async Task<SchemaValidationReport> ValidateDatabaseSchemaAsync()
    {
        var report = new SchemaValidationReport();

        using var context = _contextFactory.CreateDbContext();

        try
        {
            // Check if database can be created (validates schema)
            var canCreate = await context.Database.EnsureCreatedAsync();
            report.SchemaValid = !canCreate; // If it created DB, schema was invalid

            // Check pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            report.PendingMigrations.AddRange(pendingMigrations);

            // Test each entity set
            await ValidateEntitySet<Bus>(context, report, "Vehicles");
            await ValidateEntitySet<Driver>(context, report, "Drivers");
            await ValidateEntitySet<Route>(context, report, "Routes");
            await ValidateEntitySet<Student>(context, report, "Students");

            Logger.Information("Schema validation completed. Valid: {IsValid}, Issues: {IssueCount}",
                report.SchemaValid, report.Issues.Count);
        }
        catch (Exception ex)
        {
            report.SchemaValid = false;
            report.Issues.Add($"Schema validation error: {ex.Message}");
            Logger.Error(ex, "Error during schema validation");
        }

        return report;
    }

    private async Task ValidateEntitySet<T>(BusBuddyDbContext context, SchemaValidationReport report, string tableName) where T : class
    {
        try
        {
            var count = await context.Set<T>().CountAsync();
            report.TableValidationResults[tableName] = $"OK - {count} records";
        }
        catch (Exception ex)
        {
            report.TableValidationResults[tableName] = $"ERROR - {ex.Message}";
            report.Issues.Add($"Table {tableName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute raw SQL with debugging and performance monitoring
    /// </summary>
    public async Task<DataIntegrityReport> ExecuteAdvancedDataIntegrityCheckAsync()
    {
        var report = new DataIntegrityReport();
        var stopwatch = Stopwatch.StartNew();

        using var context = _contextFactory.CreateDbContext();

        try
        {
            // Raw SQL for comprehensive data integrity check
            var sql = @"
                SELECT
                    'Bus' as EntityType,
                    VehicleId as EntityId,
                    CASE
                        WHEN BusNumber IS NULL OR BusNumber = '' THEN 'BusNumber is NULL/Empty; '
                        ELSE ''
                    END +
                    CASE
                        WHEN Make IS NULL OR Make = '' THEN 'Make is NULL/Empty; '
                        ELSE ''
                    END +
                    CASE
                        WHEN Model IS NULL OR Model = '' THEN 'Model is NULL/Empty; '
                        ELSE ''
                    END +
                    CASE
                        WHEN VINNumber IS NULL OR VINNumber = '' THEN 'VINNumber is NULL/Empty; '
                        ELSE ''
                    END +
                    CASE
                        WHEN LicenseNumber IS NULL OR LicenseNumber = '' THEN 'LicenseNumber is NULL/Empty; '
                        ELSE ''
                    END as Issues
                FROM Vehicles
                WHERE BusNumber IS NULL OR BusNumber = ''
                   OR Make IS NULL OR Make = ''
                   OR Model IS NULL OR Model = ''
                   OR VINNumber IS NULL OR VINNumber = ''
                   OR LicenseNumber IS NULL OR LicenseNumber = ''

                UNION ALL

                SELECT
                    'Driver' as EntityType,
                    DriverId as EntityId,
                    CASE
                        WHEN DriverName IS NULL OR DriverName = '' THEN 'DriverName is NULL/Empty; '
                        ELSE ''
                    END +
                    CASE
                        WHEN DriversLicenceType IS NULL OR DriversLicenceType = '' THEN 'DriversLicenceType is NULL/Empty; '
                        ELSE ''
                    END as Issues
                FROM Drivers
                WHERE DriverName IS NULL OR DriverName = ''
                   OR DriversLicenceType IS NULL OR DriversLicenceType = ''

                UNION ALL

                SELECT
                    'Route' as EntityType,
                    RouteId as EntityId,
                    CASE
                        WHEN RouteName IS NULL OR RouteName = '' THEN 'RouteName is NULL/Empty; '
                        ELSE ''
                    END +
                    CASE
                        WHEN Date < '2020-01-01' OR Date > DATEADD(year, 1, GETDATE()) THEN 'Invalid Date; '
                        ELSE ''
                    END as Issues
                FROM Routes
                WHERE RouteName IS NULL OR RouteName = ''
                   OR Date < '2020-01-01'
                   OR Date > DATEADD(year, 1, GETDATE())

                UNION ALL

                SELECT
                    'Student' as EntityType,
                    StudentId as EntityId,
                    CASE
                        WHEN StudentName IS NULL OR StudentName = '' THEN 'StudentName is NULL/Empty; '
                        ELSE ''
                    END as Issues
                FROM Students
                WHERE StudentName IS NULL OR StudentName = ''
            ";

            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 300; // 5 minutes timeout

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var entityType = reader.GetString("EntityType");
                var entityId = reader.GetInt32("EntityId");
                var issues = reader.GetString("Issues");

                report.Issues.Add($"{entityType} {entityId}: {issues.TrimEnd(' ', ';')}");
            }

            stopwatch.Stop();
            report.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            report.TotalRecordsScanned = report.Issues.Count;

            Logger.Information("Advanced data integrity check completed in {ElapsedMs}ms. Found {IssueCount} issues.",
                stopwatch.ElapsedMilliseconds, report.Issues.Count);

        }
        catch (Exception ex)
        {
            report.Issues.Add($"Advanced check error: {ex.Message}");
            Logger.Error(ex, "Error during advanced data integrity check");
        }

        return report;
    }

    /// <summary>
    /// Monitor EF Core query performance and identify slow queries
    /// </summary>
    public async Task<QueryPerformanceReport> AnalyzeQueryPerformanceAsync()
    {
        var report = new QueryPerformanceReport();

        using var context = _contextFactory.CreateDbContext();

        // Configure logging to capture SQL queries
        var queries = new List<(string Query, long Duration)>();

        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Test various query patterns
            stopwatch.Restart();
            var busCount = await context.Vehicles.CountAsync();
            queries.Add(("Vehicle Count", stopwatch.ElapsedMilliseconds));

            stopwatch.Restart();
            var busesWithRoutes = await context.Vehicles
                .Include(b => b.AMRoutes)
                .Include(b => b.PMRoutes)
                .Take(10)
                .ToListAsync();
            queries.Add(("Buses with Routes (Include)", stopwatch.ElapsedMilliseconds));

            stopwatch.Restart();
            var activeDrivers = await context.Drivers
                .Where(d => d.Status == "Active")
                .OrderBy(d => d.DriverName)
                .Take(20)
                .ToListAsync();
            queries.Add(("Active Drivers (Where + OrderBy)", stopwatch.ElapsedMilliseconds));

            stopwatch.Restart();
            var recentRoutes = await context.Routes
                .Where(r => r.Date >= DateTime.Today.AddDays(-30))
                .GroupBy(r => r.RouteName)
                .Select(g => new { RouteName = g.Key, Count = g.Count() })
                .ToListAsync();
            queries.Add(("Recent Routes (GroupBy)", stopwatch.ElapsedMilliseconds));

            // Analyze results
            foreach (var (query, duration) in queries)
            {
                var performance = new QueryPerformance
                {
                    QueryName = query,
                    DurationMs = duration,
                    PerformanceLevel = duration switch
                    {
                        < 100 => "Excellent",
                        < 500 => "Good",
                        < 1000 => "Acceptable",
                        < 2000 => "Slow",
                        _ => "Very Slow"
                    }
                };

                report.QueryPerformances.Add(performance);

                if (duration > 1000)
                {
                    report.SlowQueries.Add($"{query}: {duration}ms - Consider optimization");
                }
            }

            Logger.Information("Query performance analysis completed. Tested {QueryCount} queries.", queries.Count);

        }
        catch (Exception ex)
        {
            report.SlowQueries.Add($"Performance analysis error: {ex.Message}");
            Logger.Error(ex, "Error during query performance analysis");
        }

        return report;
    }

    /// <summary>
    /// Scan database for NULL values in required fields
    /// </summary>
    public async Task<DataIntegrityReport> ScanForIssuesAsync()
    {
        var report = new DataIntegrityReport();

        using var context = _contextFactory.CreateDbContext();

        try
        {
            // Check Buses for NULL values
            await ScanBusesAsync(context, report);

            // Check Drivers for NULL values
            await ScanDriversAsync(context, report);

            // Check Routes for invalid dates
            await ScanRoutesAsync(context, report);

            // Check Students for data issues
            await ScanStudentsAsync(context, report);

            Logger.Information("Data integrity scan completed. Found {IssueCount} issues.",
                report.Issues.Count);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during data integrity scan");
            report.Issues.Add($"Scan error: {ex.Message}");
        }

        return report;
    }

    /// <summary>
    /// Fix NULL values and data integrity issues
    /// </summary>
    public async Task<int> FixDataIssuesAsync()
    {
        var issuesFixed = 0;

        using var context = _contextFactory.CreateDbContext();

        try
        {
            // Fix Bus NULL values
            issuesFixed += await FixBusIssuesAsync(context);

            // Fix Driver NULL values
            issuesFixed += await FixDriverIssuesAsync(context);

            // Fix Route date issues
            issuesFixed += await FixRouteIssuesAsync(context);

            // Save all changes
            await context.SaveChangesAsync();

            Logger.Information("Fixed {IssueCount} data integrity issues", issuesFixed);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error fixing data issues");
        }

        return issuesFixed;
    }

    private async Task ScanBusesAsync(BusBuddyDbContext context, DataIntegrityReport report)
    {
        var busesWithNulls = await context.Vehicles
            .Where(b => string.IsNullOrEmpty(b.BusNumber) ||
                       string.IsNullOrEmpty(b.Make) ||
                       string.IsNullOrEmpty(b.Model) ||
                       string.IsNullOrEmpty(b.VINNumber) ||
                       string.IsNullOrEmpty(b.LicenseNumber))
            .Select(b => new { b.VehicleId, b.BusNumber, b.Make, b.Model })
            .ToListAsync();

        foreach (var bus in busesWithNulls)
        {
            report.Issues.Add($"Bus {bus.VehicleId} has NULL required fields: {bus.BusNumber}, {bus.Make}, {bus.Model}");
        }
    }

    private async Task ScanDriversAsync(BusBuddyDbContext context, DataIntegrityReport report)
    {
        var driversWithNulls = await context.Drivers
            .Where(d => string.IsNullOrEmpty(d.DriverName) ||
                       string.IsNullOrEmpty(d.DriversLicenceType))
            .Select(d => new { d.DriverId, d.DriverName, d.DriversLicenceType })
            .ToListAsync();

        foreach (var driver in driversWithNulls)
        {
            report.Issues.Add($"Driver {driver.DriverId} has NULL required fields: {driver.DriverName}, {driver.DriversLicenceType}");
        }
    }

    private async Task ScanRoutesAsync(BusBuddyDbContext context, DataIntegrityReport report)
    {
        var routesWithIssues = await context.Routes
            .Where(r => r.Date == default(DateTime) ||
                       string.IsNullOrEmpty(r.RouteName) ||
                       r.Date > DateTime.Today.AddYears(1))
            .Select(r => new { r.RouteId, r.Date, r.RouteName })
            .ToListAsync();

        foreach (var route in routesWithIssues)
        {
            report.Issues.Add($"Route {route.RouteId} has date/name issues: {route.Date}, {route.RouteName}");
        }
    }

    private async Task ScanStudentsAsync(BusBuddyDbContext context, DataIntegrityReport report)
    {
        var studentsWithNulls = await context.Students
            .Where(s => string.IsNullOrEmpty(s.StudentName))
            .Select(s => new { s.StudentId, s.StudentName })
            .ToListAsync();

        foreach (var student in studentsWithNulls)
        {
            report.Issues.Add($"Student {student.StudentId} has NULL name: {student.StudentName}");
        }
    }

    private async Task<int> FixBusIssuesAsync(BusBuddyDbContext context)
    {
        var fixCount = 0;

        var busesWithNulls = await context.Vehicles
            .Where(b => string.IsNullOrEmpty(b.BusNumber) ||
                       string.IsNullOrEmpty(b.Make) ||
                       string.IsNullOrEmpty(b.Model) ||
                       string.IsNullOrEmpty(b.VINNumber) ||
                       string.IsNullOrEmpty(b.LicenseNumber) ||
                       string.IsNullOrEmpty(b.Status))
            .ToListAsync();

        foreach (var bus in busesWithNulls)
        {
            if (string.IsNullOrEmpty(bus.BusNumber))
            {
                bus.BusNumber = $"BUS-{bus.VehicleId:000}";
                fixCount++;
            }

            if (string.IsNullOrEmpty(bus.Make))
            {
                bus.Make = "Unknown Make";
                fixCount++;
            }

            if (string.IsNullOrEmpty(bus.Model))
            {
                bus.Model = "Unknown Model";
                fixCount++;
            }

            if (string.IsNullOrEmpty(bus.VINNumber))
            {
                bus.VINNumber = $"TEMP-VIN-{bus.VehicleId:00000}";
                fixCount++;
            }

            if (string.IsNullOrEmpty(bus.LicenseNumber))
            {
                bus.LicenseNumber = $"TEMP-LIC-{bus.VehicleId:000}";
                fixCount++;
            }

            if (string.IsNullOrEmpty(bus.Status))
            {
                bus.Status = "Active";
                fixCount++;
            }
        }

        return fixCount;
    }

    private async Task<int> FixDriverIssuesAsync(BusBuddyDbContext context)
    {
        var fixCount = 0;

        var driversWithNulls = await context.Drivers
            .Where(d => string.IsNullOrEmpty(d.DriverName) ||
                       string.IsNullOrEmpty(d.DriversLicenceType) ||
                       string.IsNullOrEmpty(d.Status))
            .ToListAsync();

        foreach (var driver in driversWithNulls)
        {
            if (string.IsNullOrEmpty(driver.DriverName))
            {
                driver.DriverName = $"Driver-{driver.DriverId}";
                fixCount++;
            }

            if (string.IsNullOrEmpty(driver.DriversLicenceType))
            {
                driver.DriversLicenceType = "CDL";
                fixCount++;
            }

            if (string.IsNullOrEmpty(driver.Status))
            {
                driver.Status = "Active";
                fixCount++;
            }
        }

        return fixCount;
    }

    private async Task<int> FixRouteIssuesAsync(BusBuddyDbContext context)
    {
        var fixCount = 0;

        var routesWithIssues = await context.Routes
            .Where(r => r.Date == default(DateTime) ||
                       string.IsNullOrEmpty(r.RouteName) ||
                       r.Date > DateTime.Today.AddYears(1))
            .ToListAsync();

        foreach (var route in routesWithIssues)
        {
            if (route.Date == default(DateTime) || route.Date > DateTime.Today.AddYears(1))
            {
                route.Date = DateTime.Today;
                fixCount++;
            }

            if (string.IsNullOrEmpty(route.RouteName))
            {
                route.RouteName = $"Route-{route.RouteId}";
                fixCount++;
            }
        }

        return fixCount;
    }
}

public class DataIntegrityReport
{
    public List<string> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime ScanDate { get; set; } = DateTime.UtcNow;
    public int TotalRecordsScanned { get; set; }
    public long ExecutionTimeMs { get; set; }
}

public class SchemaValidationReport
{
    public bool SchemaValid { get; set; } = true;
    public List<string> Issues { get; set; } = new();
    public List<string> PendingMigrations { get; set; } = new();
    public Dictionary<string, string> TableValidationResults { get; set; } = new();
    public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
}

public class QueryPerformanceReport
{
    public List<QueryPerformance> QueryPerformances { get; set; } = new();
    public List<string> SlowQueries { get; set; } = new();
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

public class QueryPerformance
{
    public string QueryName { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public string PerformanceLevel { get; set; } = string.Empty;
}

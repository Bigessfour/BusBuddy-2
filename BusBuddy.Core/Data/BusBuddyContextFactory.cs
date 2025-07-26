using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using System;
using System.IO;

namespace BusBuddy.Core.Data
{
    /// <summary>
    /// Production-ready Design-Time Factory for BusBuddyContext
    /// Enables Entity Framework migrations and design-time operations
    /// Enhanced with comprehensive Serilog logging and environment detection
    /// </summary>
    public class BusBuddyContextFactory : IDesignTimeDbContextFactory<BusBuddyContext>
    {
        private static readonly Serilog.ILogger Logger = Log.ForContext<BusBuddyContextFactory>();

        public BusBuddyContext CreateDbContext(string[] args)
        {
            using (LogContext.PushProperty("Operation", "DesignTimeContextCreation"))
            using (LogContext.PushProperty("FactoryType", "BusBuddyContextFactory"))
            {
                Logger.Information("Creating design-time BusBuddyContext for migrations and tooling");

                try
                {
                    // Build configuration from multiple sources with priority order
                    var configuration = BuildConfiguration(args);

                    // Create DbContextOptions with production-ready settings
                    var optionsBuilder = new DbContextOptionsBuilder<BusBuddyContext>();
                    ConfigureDbContext(optionsBuilder, configuration);

                    Logger.Information("Design-time context created successfully");
                    return new BusBuddyContext(optionsBuilder.Options, configuration);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to create design-time BusBuddyContext");
                    throw;
                }
            }
        }

        private IConfiguration BuildConfiguration(string[] args)
        {
            Logger.Debug("Building configuration for design-time context");

            var basePath = GetBasePath();
            Logger.Debug("Using base path: {BasePath}", basePath);

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("BUSBUDDY_")
                .AddCommandLine(args ?? Array.Empty<string>());

            var configuration = configurationBuilder.Build();

            var environment = configuration["Environment"]
                           ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                           ?? "Development";

            Logger.Information("Configuration built for environment: {Environment}", environment);
            return configuration;
        }

        private string GetBasePath()
        {
            // Try multiple locations to find the configuration files
            var currentDirectory = Directory.GetCurrentDirectory();
            var locations = new[]
            {
                currentDirectory,
                Path.Combine(currentDirectory, "BusBuddy.Core"),
                Path.Combine(currentDirectory, "BusBuddy.WPF"),
                Path.GetDirectoryName(typeof(BusBuddyContextFactory).Assembly.Location) ?? currentDirectory
            };

            foreach (var location in locations)
            {
                var appsettingsPath = Path.Combine(location, "appsettings.json");
                if (File.Exists(appsettingsPath))
                {
                    Logger.Debug("Found appsettings.json at: {Location}", location);
                    return location;
                }
            }

            Logger.Warning("appsettings.json not found in any expected location, using current directory: {CurrentDirectory}", currentDirectory);
            return currentDirectory;
        }

        private void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
        {
            var connectionString = GetConnectionString(configuration);

            Logger.Information("Configuring DbContext with connection type: {ConnectionType}",
                GetConnectionType(connectionString));

            if (IsAzureSqlConnection(connectionString))
            {
                ConfigureAzureSqlServer(optionsBuilder, connectionString);
            }
            else if (IsSqlServerConnection(connectionString))
            {
                ConfigureSqlServer(optionsBuilder, connectionString);
            }
            else
            {
                ConfigureSqlite(optionsBuilder, connectionString);
            }

            // Design-time specific configurations
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging(IsDevelopment(configuration));

            // Add Serilog integration for EF Core logging
            optionsBuilder.LogTo(message => Logger.Debug("EF Core Design-Time: {Message}", message));
        }

        private string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Data Source=BusBuddy.db";
                Logger.Warning("No connection string found, using default SQLite: {ConnectionString}", connectionString);
            }
            else
            {
                Logger.Debug("Using configured connection string");
            }

            return connectionString;
        }

        private void ConfigureAzureSqlServer(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            Logger.Information("Configuring for Azure SQL Server with enhanced retry policies");

            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(120); // Extended timeout for Azure
                sqlOptions.MigrationsAssembly("BusBuddy.Core");
            });
        }

        private void ConfigureSqlServer(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            Logger.Information("Configuring for SQL Server with retry policies");

            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(60);
                sqlOptions.MigrationsAssembly("BusBuddy.Core");
            });
        }

        private void ConfigureSqlite(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            Logger.Information("Configuring for SQLite development database");

            optionsBuilder.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(30);
                sqliteOptions.MigrationsAssembly("BusBuddy.Core");
            });
        }

        private static string GetConnectionType(string connectionString)
        {
            if (IsAzureSqlConnection(connectionString)) return "Azure SQL";
            if (IsSqlServerConnection(connectionString)) return "SQL Server";
            return "SQLite";
        }

        private static bool IsAzureSqlConnection(string connectionString)
        {
            return connectionString.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSqlServerConnection(string connectionString)
        {
            return connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase)
                   && !IsAzureSqlConnection(connectionString);
        }

        private static bool IsDevelopment(IConfiguration configuration)
        {
            var environment = configuration["Environment"]
                           ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                           ?? "Development";
            return environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}

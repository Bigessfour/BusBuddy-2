using BusBuddy.Core.Data;
using BusBuddy.Core.Extensions;
using BusBuddy.Core.Utilities;
using BusBuddy.DeploymentValidator.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusBuddy.DeploymentValidator
{
    /// <summary>
    /// Console application for validating deployment readiness
    /// Can be run independently of the main WPF application
    /// Version: 2.0
    /// </summary>
    class Program
    {
        private static readonly string Version = "2.0.0";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        static async Task<int> Main(string[] args)
        {
            // Parse command line arguments (simple parsing)
            var environment = GetArgValue(args, "--environment", "Production");
            var verbose = HasArg(args, "--verbose");
            var outputFormat = GetArgValue(args, "--output", "console");
            var logsDir = GetArgValue(args, "--logs-dir", "logs");

            // Display help if requested
            if (HasArg(args, "--help") || HasArg(args, "-h"))
            {
                DisplayHelp();
                return 0;
            }

            return await RunValidation(environment, verbose, outputFormat, logsDir);
        }

        private static void DisplayHelp()
        {
            Console.WriteLine($"BusBuddy Deployment Validation Tool v{Version}");
            Console.WriteLine("Usage: BusBuddy.DeploymentValidator [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --environment <env>    Environment to validate (Development, Production) [default: Production]");
            Console.WriteLine("  --verbose              Enable verbose logging");
            Console.WriteLine("  --output <format>      Output format: console, json, or both [default: console]");
            Console.WriteLine("  --logs-dir <dir>       Directory for log files [default: logs]");
            Console.WriteLine("  --help, -h             Show this help message");
        }

        private static string GetArgValue(string[] args, string argName, string defaultValue)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == argName)
                    return args[i + 1];
            }
            return defaultValue;
        }

        private static bool HasArg(string[] args, string argName)
        {
            return args.Contains(argName);
        }

        private static async Task<int> RunValidation(string environment, bool verbose, string outputFormat, string logsDir)
        {
            var startTime = DateTime.Now;

            if (outputFormat is "console" or "both")
            {
                Console.WriteLine("🚌 BusBuddy Deployment Validation Tool v" + Version);
                Console.WriteLine("======================================");
                Console.WriteLine($"Environment: {environment}");
                Console.WriteLine($"Started at: {startTime}");
                Console.WriteLine();
            }

            try
            {
                // Ensure logs directory exists
                Directory.CreateDirectory(logsDir);

                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                // Build service collection
                var services = new ServiceCollection();
                ConfigureServices(services, configuration, verbose);

                // Build service provider
                using var serviceProvider = services.BuildServiceProvider();

                // Get validation service
                var validationService = serviceProvider.GetRequiredService<ConsoleStartupValidationService>();

                if (outputFormat is "console" or "both")
                {
                    Console.WriteLine("🔍 Running comprehensive deployment validation...");
                    Console.WriteLine();
                }

                // Run validation
                var validationResult = await validationService.ValidateStartupAsync();

                // Prepare results for JSON output
                var results = new
                {
                    Version,
                    Environment = environment,
                    Timestamp = startTime,
                    ValidationResults = validationResult.ValidationResults.Select(r => new
                    {
                        r.ValidationName,
                        r.IsValid,
                        r.Message,
                        r.ErrorMessage,
                        r.ValidationTimeMs
                    }),
                    Summary = new
                    {
                        validationResult.IsValid,
                        TotalChecks = validationResult.ValidationResults.Count,
                        PassedChecks = validationResult.ValidationResults.Count(r => r.IsValid),
                        FailedChecks = validationResult.ValidationResults.Count(r => !r.IsValid),
                        validationResult.TotalValidationTimeMs
                    }
                };

                // Output to console
                if (outputFormat is "console" or "both")
                {
                    DisplayConsoleResults(validationResult);
                }

                // Output to JSON
                if (outputFormat is "json" or "both")
                {
                    var jsonPath = Path.Combine(logsDir, $"validation_results_{startTime:yyyyMMdd_HHmmss}.json");
                    await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(results, JsonOptions));

                    if (outputFormat is "both")
                        Console.WriteLine($"📄 JSON results saved to: {jsonPath}");
                    else
                        Console.WriteLine(JsonSerializer.Serialize(results, JsonOptions));
                }

                // Generate detailed report
                try
                {
                    var checklist = await validationService.GenerateDeploymentChecklistAsync();
                    var reportPath = Path.Combine(logsDir, $"deployment_validation_{startTime:yyyyMMdd_HHmmss}.log");
                    await File.WriteAllTextAsync(reportPath, checklist);

                    if (outputFormat is "console" or "both")
                        Console.WriteLine($"📄 Detailed report saved to: {reportPath}");
                }
                catch (Exception ex)
                {
                    if (outputFormat is "console" or "both")
                        Console.WriteLine($"⚠️  Could not save detailed report: {ex.Message}");
                }

                // Return appropriate exit code
                return validationResult.IsValid ? 0 : 1;
            }
            catch (Exception ex)
            {
                var error = new { Error = ex.Message, StackTrace = ex.StackTrace, Timestamp = DateTime.Now };

                if (outputFormat is "json" or "both")
                {
                    var errorPath = Path.Combine(logsDir, $"validation_error_{startTime:yyyyMMdd_HHmmss}.json");
                    await File.WriteAllTextAsync(errorPath, JsonSerializer.Serialize(error, JsonOptions));
                }

                if (outputFormat is "console" or "both")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ VALIDATION FAILED WITH ERROR: {ex.Message}");
                    Console.WriteLine();
                    Console.WriteLine("Stack trace:");
                    Console.WriteLine(ex.StackTrace);
                    Console.ResetColor();
                }

                return 2; // Error exit code
            }
        }

        private static void DisplayConsoleResults(StartupValidationResult validationResult)
        {
            Console.WriteLine("📊 VALIDATION RESULTS");
            Console.WriteLine("====================");
            Console.WriteLine();

            foreach (var result in validationResult.ValidationResults)
            {
                var status = result.IsValid ? "✅ PASS" : "❌ FAIL";
                var message = result.IsValid ? result.Message : result.ErrorMessage;

                Console.WriteLine($"{status} {result.ValidationName}");
                Console.WriteLine($"     {message}");
                Console.WriteLine($"     Time: {result.ValidationTimeMs}ms");
                Console.WriteLine();
            }

            // Overall result
            Console.WriteLine("📋 DEPLOYMENT STATUS");
            Console.WriteLine("===================");

            if (validationResult.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ READY FOR DEPLOYMENT");
                Console.WriteLine($"All {validationResult.ValidationResults.Count} validation checks passed.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ NOT READY FOR DEPLOYMENT");

                var failedCount = validationResult.ValidationResults.Count(r => !r.IsValid);
                Console.WriteLine($"{failedCount} validation checks failed.");
                Console.WriteLine();
                Console.WriteLine("Failed checks:");

                foreach (var failure in validationResult.ValidationResults.Where(r => !r.IsValid))
                {
                    Console.WriteLine($"  • {failure.ValidationName}: {failure.ErrorMessage}");
                }
            }

            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"Total validation time: {validationResult.TotalValidationTimeMs}ms");
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, bool verbose)
        {
            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
            });

            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Add DbContext using proper configuration
            services.AddDbContext<BusBuddyDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                if (verbose)
                {
                    options.EnableSensitiveDataLogging();
                    options.LogTo(Console.WriteLine);
                }
            });

            // Use the BusBuddy.Core service extensions to register all services properly
            services.AddDataServices(configuration);

            // Add the console startup validation service
            services.AddScoped<ConsoleStartupValidationService>();
        }
    }
}

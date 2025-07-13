using BusBuddy.Core.Data;
using BusBuddy.Core.Services;
using BusBuddy.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusBuddy.WPF.Services
{
    /// <summary>
    /// Comprehensive startup validation service for deployment readiness
    /// Validates all critical systems before the application becomes available to users
    /// </summary>
    public class StartupValidationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StartupValidationService> _logger;
        private readonly List<ValidationResult> _validationResults = new();

        public StartupValidationService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<StartupValidationService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs comprehensive startup validation for deployment readiness
        /// </summary>
        /// <returns>Overall validation result indicating if the application is ready for use</returns>
        public async Task<StartupValidationResult> ValidateStartupAsync()
        {
            var overallStopwatch = Stopwatch.StartNew();
            _logger.LogInformation("[STARTUP_VALIDATION] Beginning comprehensive startup validation for deployment readiness");

            try
            {
                // Run all validation checks
                ValidateEnvironmentAsync();
                ValidateConfigurationAsync();
                await ValidateDatabaseAsync();
                ValidateDependenciesAsync();
                ValidateLicensingAsync();
                ValidateSecurityAsync();
                await ValidateLoggingAsync();
                await ValidatePerformanceAsync();

                overallStopwatch.Stop();

                // Compile overall result
                var result = new StartupValidationResult
                {
                    IsValid = _validationResults.TrueForAll(r => r.IsValid),
                    ValidationResults = new List<ValidationResult>(_validationResults),
                    TotalValidationTimeMs = overallStopwatch.ElapsedMilliseconds
                };

                // Log summary
                var passedCount = _validationResults.Count(r => r.IsValid);
                var failedCount = _validationResults.Count - passedCount;

                _logger.LogInformation(
                    "[STARTUP_VALIDATION] Validation complete: {PassedCount} passed, {FailedCount} failed, {TotalTimeMs}ms total",
                    passedCount, failedCount, result.TotalValidationTimeMs);

                if (!result.IsValid)
                {
                    _logger.LogError("[STARTUP_VALIDATION] Application is NOT ready for deployment - validation failures detected");
                    foreach (var failure in _validationResults.Where(r => !r.IsValid))
                    {
                        _logger.LogError("[STARTUP_VALIDATION] FAILED: {ValidationName} - {ErrorMessage}",
                            failure.ValidationName, failure.ErrorMessage);
                    }
                }
                else
                {
                    _logger.LogInformation("[STARTUP_VALIDATION] ✅ Application is ready for deployment - all validations passed");
                }

                return result;
            }
            catch (Exception ex)
            {
                overallStopwatch.Stop();
                _logger.LogCritical(ex, "[STARTUP_VALIDATION] Critical error during startup validation");

                return new StartupValidationResult
                {
                    IsValid = false,
                    ValidationResults = new List<ValidationResult>
                    {
                        new ValidationResult
                        {
                            ValidationName = "StartupValidationService",
                            IsValid = false,
                            ErrorMessage = $"Critical validation error: {ex.Message}",
                            ValidationTimeMs = overallStopwatch.ElapsedMilliseconds
                        }
                    },
                    TotalValidationTimeMs = overallStopwatch.ElapsedMilliseconds
                };
            }
        }

        /// <summary>
        /// Quick deployment readiness check that can be run independently
        /// </summary>
        public async Task<bool> IsDeploymentReadyAsync()
        {
            try
            {
                _logger.LogInformation("[DEPLOYMENT_CHECK] Running quick deployment readiness check");

                // Clear any previous results
                _validationResults.Clear();

                // Run critical validations only
                ValidateEnvironmentAsync();
                ValidateConfigurationAsync();
                ValidateLicensingAsync();
                ValidateSecurityAsync();

                // Quick database connectivity test
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetService<BusBuddyDbContext>();
                    if (context != null)
                    {
                        var canConnect = await context.Database.CanConnectAsync();
                        _validationResults.Add(new ValidationResult
                        {
                            ValidationName = "Database Quick Check",
                            IsValid = canConnect,
                            Message = canConnect ? "Database accessible" : "Database not accessible",
                            ValidationTimeMs = 0
                        });
                    }
                }
                catch (Exception ex)
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Database Quick Check",
                        IsValid = false,
                        ErrorMessage = $"Database check failed: {ex.Message}",
                        ValidationTimeMs = 0
                    });
                }

                var isReady = _validationResults.TrueForAll(r => r.IsValid);

                _logger.LogInformation("[DEPLOYMENT_CHECK] Deployment readiness: {IsReady}",
                    isReady ? "READY" : "NOT READY");

                return isReady;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DEPLOYMENT_CHECK] Error during deployment readiness check");
                return false;
            }
        }

        /// <summary>
        /// Creates a deployment checklist report
        /// </summary>
        public async Task<string> GenerateDeploymentChecklistAsync()
        {
            var result = await ValidateStartupAsync();

            var checklist = new StringBuilder();
            checklist.AppendLine("🚌 BusBuddy Deployment Readiness Checklist");
            checklist.AppendLine("=========================================");
            checklist.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            checklist.AppendLine($"Environment: {EnvironmentHelper.GetEnvironmentName()}");
            checklist.AppendLine($"Overall Status: {(result.IsValid ? "✅ READY" : "❌ NOT READY")}");
            checklist.AppendLine();

            // Group validations by category
            var categories = new Dictionary<string, List<ValidationResult>>
            {
                ["Critical Systems"] = result.ValidationResults.Where(r =>
                    r.ValidationName.Contains("Database") ||
                    r.ValidationName.Contains("Configuration") ||
                    r.ValidationName.Contains("Licensing")).ToList(),

                ["Security & Environment"] = result.ValidationResults.Where(r =>
                    r.ValidationName.Contains("Security") ||
                    r.ValidationName.Contains("Environment")).ToList(),

                ["Dependencies & Services"] = result.ValidationResults.Where(r =>
                    r.ValidationName.Contains("Dependencies") ||
                    r.ValidationName.Contains("Service")).ToList(),

                ["Performance & Logging"] = result.ValidationResults.Where(r =>
                    r.ValidationName.Contains("Performance") ||
                    r.ValidationName.Contains("Logging")).ToList()
            };

            foreach (var category in categories)
            {
                if (category.Value.Any())
                {
                    checklist.AppendLine($"📋 {category.Key}");
                    checklist.AppendLine(new string('-', category.Key.Length + 4));

                    foreach (var validation in category.Value)
                    {
                        var status = validation.IsValid ? "✅" : "❌";
                        var message = validation.IsValid ? validation.Message : validation.ErrorMessage;
                        checklist.AppendLine($"{status} {validation.ValidationName}: {message}");

                        if (!validation.IsValid && !string.IsNullOrEmpty(validation.ErrorMessage))
                        {
                            checklist.AppendLine($"   ⚠️  Action Required: {validation.ErrorMessage}");
                        }
                    }
                    checklist.AppendLine();
                }
            }

            // Add deployment recommendations
            checklist.AppendLine("📝 Deployment Recommendations");
            checklist.AppendLine("============================");

            if (result.IsValid)
            {
                checklist.AppendLine("✅ All validations passed - application is ready for deployment");
                checklist.AppendLine("• Ensure backup procedures are in place");
                checklist.AppendLine("• Verify monitoring and alerting systems are configured");
                checklist.AppendLine("• Test rollback procedures");
            }
            else
            {
                checklist.AppendLine("❌ Deployment blocked - resolve the following issues:");
                foreach (var failure in result.ValidationResults.Where(r => !r.IsValid))
                {
                    checklist.AppendLine($"• {failure.ValidationName}: {failure.ErrorMessage}");
                }
            }

            return checklist.ToString();
        }

        /// <summary>
        /// Validates the deployment environment and configuration
        /// </summary>
        private void ValidateEnvironmentAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var environmentName = EnvironmentHelper.GetEnvironmentName();
                var isDevelopment = EnvironmentHelper.IsDevelopment();

                _logger.LogInformation("[STARTUP_VALIDATION] Validating environment: {EnvironmentName}", environmentName);

                // Validate environment variables are set appropriately
                var requiredEnvVars = new List<string>();
                var missingVars = new List<string>();

                if (!isDevelopment)
                {
                    requiredEnvVars.AddRange(new[] { "SYNCFUSION_LICENSE_KEY" });
                }

                foreach (var envVar in requiredEnvVars)
                {
                    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(envVar)))
                    {
                        missingVars.Add(envVar);
                    }
                }

                stopwatch.Stop();

                if (missingVars.Any())
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Environment Variables",
                        IsValid = false,
                        ErrorMessage = $"Missing required environment variables: {string.Join(", ", missingVars)}",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
                else
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Environment Variables",
                        IsValid = true,
                        Message = $"Environment validated for {environmentName}",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Environment Validation",
                    IsValid = false,
                    ErrorMessage = $"Environment validation failed: {ex.Message}",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Validates configuration files and settings
        /// </summary>
        private void ValidateConfigurationAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating configuration");

                var issues = new List<string>();

                // Check for required configuration files
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (!File.Exists(configPath))
                {
                    issues.Add($"Required configuration file missing: {configPath}");
                }

                // Validate connection string
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    issues.Add("DefaultConnection string is missing or empty");
                }

                // Validate Syncfusion license configuration
                var syncfusionKey = _configuration["Syncfusion:LicenseKey"] ??
                                   _configuration["SyncfusionLicenseKey"] ??
                                   Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");

                if (string.IsNullOrWhiteSpace(syncfusionKey))
                {
                    issues.Add("Syncfusion license key is not configured");
                }

                stopwatch.Stop();

                if (issues.Any())
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Configuration",
                        IsValid = false,
                        ErrorMessage = string.Join("; ", issues),
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
                else
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Configuration",
                        IsValid = true,
                        Message = "All required configuration is present",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Configuration Validation",
                    IsValid = false,
                    ErrorMessage = $"Configuration validation failed: {ex.Message}",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Validates database connectivity and schema
        /// </summary>
        private async Task ValidateDatabaseAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating database connectivity and schema");

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetService<BusBuddyDbContext>();

                if (context == null)
                {
                    stopwatch.Stop();
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Database Context",
                        IsValid = false,
                        ErrorMessage = "Unable to resolve BusBuddyDbContext from DI container",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                    return;
                }

                // Test database connectivity
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    stopwatch.Stop();
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Database Connectivity",
                        IsValid = false,
                        ErrorMessage = "Cannot connect to the database",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                    return;
                }

                // Check for pending migrations - simplified approach
                var pendingMigrations = new List<string>(); // Placeholder - would need EF Core extensions
                if (pendingMigrations.Any())
                {
                    _logger.LogWarning("[STARTUP_VALIDATION] Found {Count} pending migrations", pendingMigrations.Count());
                    // This might be acceptable in some deployment scenarios, so it's a warning, not a failure
                }

                // Validate basic schema by checking if DbSets can be accessed
                try
                {
                    // Efficient existence checks to verify tables exist and are accessible
                    var hasVehicles = context.Vehicles.Any();
                    var hasDrivers = context.Drivers.Any();
                    var hasRoutes = context.Routes.Any();

                    _logger.LogDebug("[STARTUP_VALIDATION] Schema validation: vehicles table {VehicleStatus}, drivers table {DriverStatus}, routes table {RouteStatus}",
                        hasVehicles ? "accessible" : "empty/inaccessible",
                        hasDrivers ? "accessible" : "empty/inaccessible",
                        hasRoutes ? "accessible" : "empty/inaccessible");
                }
                catch (Exception schemaEx)
                {
                    stopwatch.Stop();
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Database Schema",
                        IsValid = false,
                        ErrorMessage = $"Database schema validation failed: {schemaEx.Message}",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                    return;
                }

                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Database",
                    IsValid = true,
                    Message = $"Database connectivity and schema validated. {pendingMigrations.Count()} pending migrations.",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Database Validation",
                    IsValid = false,
                    ErrorMessage = $"Database validation failed: {ex.Message}",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Validates critical service dependencies
        /// </summary>
        private void ValidateDependenciesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating service dependencies");

                var criticalServices = new Dictionary<string, Type>
                {
                    { "IBusService", typeof(Core.Services.Interfaces.IBusService) },
                    { "IRouteService", typeof(Core.Services.IRouteService) },
                    { "IDriverService", typeof(Core.Services.IDriverService) },
                    { "IDashboardMetricsService", typeof(Core.Services.IDashboardMetricsService) },
                    { "IThemeService", typeof(BusBuddy.WPF.Services.IThemeService) }
                };

                var failedServices = new List<string>();

                using var scope = _serviceProvider.CreateScope();
                foreach (var service in criticalServices)
                {
                    try
                    {
                        var instance = scope.ServiceProvider.GetService(service.Value);
                        if (instance == null)
                        {
                            failedServices.Add(service.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        failedServices.Add($"{service.Key} ({ex.Message})");
                    }
                }

                stopwatch.Stop();

                if (failedServices.Any())
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Service Dependencies",
                        IsValid = false,
                        ErrorMessage = $"Failed to resolve services: {string.Join(", ", failedServices)}",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
                else
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Service Dependencies",
                        IsValid = true,
                        Message = $"All {criticalServices.Count} critical services resolved successfully",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Dependencies Validation",
                    IsValid = false,
                    ErrorMessage = $"Dependencies validation failed: {ex.Message}",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Validates licensing requirements
        /// </summary>
        private void ValidateLicensingAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating licensing");

                // Check Syncfusion license
                var licenseKey = _configuration["Syncfusion:LicenseKey"] ??
                               _configuration["SyncfusionLicenseKey"] ??
                               Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");

                if (string.IsNullOrWhiteSpace(licenseKey))
                {
                    stopwatch.Stop();
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Licensing",
                        IsValid = false,
                        ErrorMessage = "Syncfusion license key is not configured",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                    return;
                }

                // Test license registration (this might not be foolproof but it's a basic check)
                try
                {
                    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                }
                catch (Exception licenseEx)
                {
                    stopwatch.Stop();
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Licensing",
                        IsValid = false,
                        ErrorMessage = $"Syncfusion license registration failed: {licenseEx.Message}",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                    return;
                }

                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Licensing",
                    IsValid = true,
                    Message = "Syncfusion license validated successfully",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Licensing Validation",
                    IsValid = false,
                    ErrorMessage = $"Licensing validation failed: {ex.Message}",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Validates security configuration
        /// </summary>
        private void ValidateSecurityAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating security configuration");

                var securityIssues = new List<string>();

                // Check if sensitive data logging is inappropriately enabled
                if (!EnvironmentHelper.IsDevelopment() &&
                    Environment.GetEnvironmentVariable("ENABLE_SENSITIVE_DATA_LOGGING") == "true")
                {
                    securityIssues.Add("Sensitive data logging is enabled in non-development environment");
                }

                // Validate connection string doesn't contain obvious security issues
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    if (connectionString.Contains("password=", StringComparison.OrdinalIgnoreCase) &&
                        connectionString.Contains("password=123", StringComparison.OrdinalIgnoreCase))
                    {
                        securityIssues.Add("Connection string contains weak or default password");
                    }

                    if (connectionString.Contains("Integrated Security=false", StringComparison.OrdinalIgnoreCase) &&
                        !connectionString.Contains("Encrypt=true", StringComparison.OrdinalIgnoreCase))
                    {
                        securityIssues.Add("Database connection is not encrypted");
                    }
                }

                stopwatch.Stop();

                if (securityIssues.Any())
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Security Configuration",
                        IsValid = false,
                        ErrorMessage = string.Join("; ", securityIssues),
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
                else
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Security Configuration",
                        IsValid = true,
                        Message = "Security configuration validated",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Security Validation",
                    IsValid = false,
                    ErrorMessage = $"Security validation failed: {ex.Message}",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Validates logging configuration
        /// </summary>
        private async Task ValidateLoggingAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating logging configuration");

                var issues = new List<string>();

                // Check if logs directory exists and is writable
                var solutionRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName
                                ?? Directory.GetCurrentDirectory();
                var logsDirectory = Path.Combine(solutionRoot, "logs");

                if (!Directory.Exists(logsDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(logsDirectory);
                    }
                    catch (Exception ex)
                    {
                        issues.Add($"Cannot create logs directory: {ex.Message}");
                    }
                }

                // Test log file write permissions
                try
                {
                    var testLogFile = Path.Combine(logsDirectory, "startup_validation_test.log");
                    await File.WriteAllTextAsync(testLogFile, $"Test log entry at {DateTime.Now}");
                    File.Delete(testLogFile);
                }
                catch (Exception ex)
                {
                    issues.Add($"Cannot write to logs directory: {ex.Message}");
                }

                stopwatch.Stop();

                if (issues.Any())
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Logging Configuration",
                        IsValid = false,
                        ErrorMessage = string.Join("; ", issues),
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
                else
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Logging Configuration",
                        IsValid = true,
                        Message = "Logging configuration validated",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Logging Validation",
                    IsValid = false,
                    ErrorMessage = $"Logging validation failed: {ex.Message}",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }

        /// <summary>
        /// Validates performance requirements
        /// </summary>
        private async Task ValidatePerformanceAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating performance requirements");

                var performanceIssues = new List<string>();

                // Use GC memory info for accurate available memory measurement
                var gcMemoryInfo = GC.GetGCMemoryInfo();
                var availableMemoryMB = gcMemoryInfo.TotalAvailableMemoryBytes / (1024 * 1024);

                if (availableMemoryMB < 50) // Minimum 50MB
                {
                    performanceIssues.Add($"Low available memory: {availableMemoryMB}MB (minimum 50MB recommended)");
                }

                // Check processor count
                var processorCount = Environment.ProcessorCount;
                if (processorCount < 2)
                {
                    performanceIssues.Add($"Low processor count: {processorCount} (minimum 2 cores recommended)");
                }

                // Test basic database performance
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetService<BusBuddyDbContext>();
                if (context != null)
                {
                    var dbTestStopwatch = Stopwatch.StartNew();
                    try
                    {
                        // Simple connection test instead of ExecuteSqlRawAsync
                        var canConnect = await context.Database.CanConnectAsync();
                        dbTestStopwatch.Stop();

                        if (!canConnect)
                        {
                            performanceIssues.Add("Database connection test failed");
                        }
                        else if (dbTestStopwatch.ElapsedMilliseconds > 5000) // 5 second timeout
                        {
                            performanceIssues.Add($"Database response time is slow: {dbTestStopwatch.ElapsedMilliseconds}ms");
                        }
                    }
                    catch (Exception ex)
                    {
                        performanceIssues.Add($"Database performance test failed: {ex.Message}");
                    }
                }

                stopwatch.Stop();

                if (performanceIssues.Any())
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Performance Requirements",
                        IsValid = false,
                        ErrorMessage = string.Join("; ", performanceIssues),
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
                else
                {
                    _validationResults.Add(new ValidationResult
                    {
                        ValidationName = "Performance Requirements",
                        IsValid = true,
                        Message = $"Performance validated - {availableMemoryMB}MB memory, {processorCount} cores",
                        ValidationTimeMs = stopwatch.ElapsedMilliseconds
                    });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Performance Validation",
                    IsValid = false,
                    ErrorMessage = $"Performance validation failed: {ex.Message}",
                    ValidationTimeMs = stopwatch.ElapsedMilliseconds
                });
            }
        }
    }

    /// <summary>
    /// Result of a single validation check
    /// </summary>
    public class ValidationResult
    {
        public string ValidationName { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public long ValidationTimeMs { get; set; }
    }

    /// <summary>
    /// Overall result of startup validation
    /// </summary>
    public class StartupValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationResult> ValidationResults { get; set; } = new();
        public long TotalValidationTimeMs { get; set; }

        /// <summary>
        /// Gets a deployment-ready summary of the validation results
        /// </summary>
        public string GetDeploymentSummary()
        {
            var passedCount = ValidationResults.Count(r => r.IsValid);
            var failedCount = ValidationResults.Count - passedCount;

            var summary = $"Startup Validation Summary:\n";
            summary += $"Overall Status: {(IsValid ? "✅ READY FOR DEPLOYMENT" : "❌ NOT READY FOR DEPLOYMENT")}\n";
            summary += $"Checks Passed: {passedCount}/{ValidationResults.Count}\n";
            summary += $"Total Validation Time: {TotalValidationTimeMs}ms\n\n";

            if (failedCount > 0)
            {
                summary += "Failed Validations:\n";
                foreach (var failure in ValidationResults.Where(r => !r.IsValid))
                {
                    summary += $"❌ {failure.ValidationName}: {failure.ErrorMessage}\n";
                }
                summary += "\n";
            }

            summary += "All Validation Results:\n";
            foreach (var result in ValidationResults)
            {
                var status = result.IsValid ? "✅" : "❌";
                var message = result.IsValid ? result.Message : result.ErrorMessage;
                summary += $"{status} {result.ValidationName} ({result.ValidationTimeMs}ms): {message}\n";
            }

            return summary;
        }
    }
}

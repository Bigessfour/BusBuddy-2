using BusBuddy.Core.Data;
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

namespace BusBuddy.DeploymentValidator.Services
{
    /// <summary>
    /// Console-only startup validation service for deployment readiness
    /// Validates all critical systems without WPF dependencies
    /// </summary>
    public class ConsoleStartupValidationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConsoleStartupValidationService> _logger;
        private readonly List<ValidationResult> _validationResults = new();

        public ConsoleStartupValidationService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<ConsoleStartupValidationService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs comprehensive startup validation for deployment readiness
        /// </summary>
        public async Task<StartupValidationResult> ValidateStartupAsync()
        {
            var overallStopwatch = Stopwatch.StartNew();
            _logger.LogInformation("[STARTUP_VALIDATION] Beginning comprehensive startup validation for deployment readiness");

            try
            {
                // Run all validation checks
                ValidateEnvironment();
                ValidateConfiguration();
                await ValidateDatabaseAsync();
                ValidateDependencies();
                ValidateLicensing();
                ValidateSecurity();
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

        private void ValidateEnvironment()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var environmentName = EnvironmentHelper.GetEnvironmentName();
                var isDevelopment = EnvironmentHelper.IsDevelopment();

                _logger.LogInformation("[STARTUP_VALIDATION] Validating environment: {EnvironmentName}", environmentName);

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

        private void ValidateConfiguration()
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

                // Basic schema validation
                try
                {
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
                    Message = "Database connectivity and schema validated successfully",
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

        private void ValidateDependencies()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating service dependencies");

                var criticalServices = new Dictionary<string, Type>
                {
                    { "IBusService", typeof(BusBuddy.Core.Services.Interfaces.IBusService) },
                    { "IRouteService", typeof(BusBuddy.Core.Services.IRouteService) },
                    { "IDriverService", typeof(BusBuddy.Core.Services.IDriverService) },
                    { "IDashboardMetricsService", typeof(BusBuddy.Core.Services.IDashboardMetricsService) }
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

        private void ValidateLicensing()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating licensing");

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

                stopwatch.Stop();
                _validationResults.Add(new ValidationResult
                {
                    ValidationName = "Licensing",
                    IsValid = true,
                    Message = "Syncfusion license key is configured",
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

        private void ValidateSecurity()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating security configuration");

                var securityIssues = new List<string>();

                if (!EnvironmentHelper.IsDevelopment() &&
                    Environment.GetEnvironmentVariable("ENABLE_SENSITIVE_DATA_LOGGING") == "true")
                {
                    securityIssues.Add("Sensitive data logging is enabled in non-development environment");
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    if (connectionString.Contains("password=123", StringComparison.OrdinalIgnoreCase))
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

        private async Task ValidateLoggingAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("[STARTUP_VALIDATION] Validating logging configuration");

                var issues = new List<string>();

                var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");

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

                if (availableMemoryMB < 50)
                {
                    performanceIssues.Add($"Low available memory: {availableMemoryMB}MB (minimum 50MB recommended)");
                }

                var processorCount = Environment.ProcessorCount;
                if (processorCount < 2)
                {
                    performanceIssues.Add($"Low processor count: {processorCount} (minimum 2 cores recommended)");
                }

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetService<BusBuddyDbContext>();
                if (context != null)
                {
                    var dbTestStopwatch = Stopwatch.StartNew();
                    try
                    {
                        var canConnect = await context.Database.CanConnectAsync();
                        dbTestStopwatch.Stop();

                        if (!canConnect)
                        {
                            performanceIssues.Add("Database connection test failed");
                        }
                        else if (dbTestStopwatch.ElapsedMilliseconds > 5000)
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

            foreach (var validation in result.ValidationResults)
            {
                var status = validation.IsValid ? "✅" : "❌";
                var message = validation.IsValid ? validation.Message : validation.ErrorMessage;
                checklist.AppendLine($"{status} {validation.ValidationName}: {message}");
            }

            return checklist.ToString();
        }
    }

    public class ValidationResult
    {
        public string ValidationName { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public long ValidationTimeMs { get; set; }
    }

    public class StartupValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationResult> ValidationResults { get; set; } = new();
        public long TotalValidationTimeMs { get; set; }
    }
}

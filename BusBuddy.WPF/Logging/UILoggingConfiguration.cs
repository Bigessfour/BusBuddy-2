using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;

namespace BusBuddy.WPF.Logging
{
    /// <summary>
    /// Consolidated logging configuration for UI and Syncfusion controls
    /// </summary>
    public static class UILoggingConfiguration
    {
        /// <summary>
        /// Configure consolidated logging with minimal file count while preserving all information
        /// </summary>
        public static LoggerConfiguration ConfigureUILogging(this LoggerConfiguration loggerConfig, string logsDirectory)
        {
            // Ensure logs directory exists
            Directory.CreateDirectory(logsDirectory);

            return loggerConfig
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", "BusBuddy")
                .Enrich.WithProperty("ApplicationVersion", typeof(UILoggingConfiguration).Assembly.GetName().Version?.ToString() ?? "Unknown")
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.With(new PerformanceEnricher())
                .Enrich.With(new BusBuddy.Core.Logging.QueryTrackingEnricher())

                // CONSOLIDATED: Main application log with all events (replaces multiple specialized logs)
                .WriteTo.File(
                    path: Path.Combine(logsDirectory, "busbuddy-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    shared: true,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{ThreadId}] [{MachineName}] [{UIOperation}{SyncfusionOperation}{DatabaseOperation}] {Message:lj}{NewLine}{Exception}")

                // CONSOLIDATED: Errors and warnings only (includes all error types)
                .WriteTo.Logger(errorLogger => errorLogger
                    .Filter.ByIncludingOnly(evt => evt.Level >= LogEventLevel.Warning)
                    .WriteTo.File(
                        path: Path.Combine(logsDirectory, "errors-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30, // Keep errors longer
                        shared: true,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{ThreadId}] [{MachineName}] [{UIOperation}{SyncfusionOperation}{DatabaseOperation}] {Message:lj}{NewLine}{Exception}"))

                // CONSOLIDATED: Performance issues only (slow operations across all components)
                .WriteTo.Logger(performanceLogger => performanceLogger
                    .Filter.ByIncludingOnly(evt =>
                        evt.Level >= LogEventLevel.Warning &&
                        (evt.MessageTemplate.Text.Contains("SLOW") ||
                         evt.MessageTemplate.Text.Contains("[PERF") ||
                         evt.MessageTemplate.Text.Contains("Duration") ||
                         (evt.Properties.ContainsKey("DurationMs") &&
                          evt.Properties["DurationMs"] is ScalarValue scalar &&
                          scalar.Value is double duration && duration > 100)))
                    .WriteTo.File(
                        path: Path.Combine(logsDirectory, "performance-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        shared: true,
                        outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] [PERF] [{DurationMs}ms] [{Operation}{UIOperation}{SyncfusionOperation}] {Message:lj}{NewLine}{Exception}"))

                // Console output for development (reduced verbosity)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Information);
        }

        /// <summary>
        /// Register UI logging services with dependency injection
        /// </summary>
        public static IServiceCollection AddUILogging(this IServiceCollection services)
        {
            // Configure Serilog as the primary logger
            services.AddSingleton<ILogger>(Log.Logger);

            // Register UI-specific logging services
            services.AddSingleton<UIPerformanceLogger>();
            services.AddSingleton<ILogEventEnricher, PerformanceEnricher>();

            return services;
        }

        /// <summary>
        /// Create and configure Serilog logger specifically for UI components
        /// </summary>
        public static ILogger CreateUILogger(string logsDirectory)
        {
            var logger = new LoggerConfiguration()
                .ConfigureUILogging(logsDirectory)
                .CreateLogger();

            Log.Logger = logger;
            return logger;
        }

        /// <summary>
        /// Log event levels for different UI operations (using Serilog conventions)
        /// </summary>
        public static class UILogLevels
        {
            public const LogEventLevel ButtonClick = LogEventLevel.Information;
            public const LogEventLevel Navigation = LogEventLevel.Information;
            public const LogEventLevel WindowOperation = LogEventLevel.Information;
            public const LogEventLevel SyncfusionEvent = LogEventLevel.Debug;
            public const LogEventLevel SyncfusionLifecycle = LogEventLevel.Information;
            public const LogEventLevel SyncfusionTheme = LogEventLevel.Information;
            public const LogEventLevel PerformanceWarning = LogEventLevel.Warning;
            public const LogEventLevel UIError = LogEventLevel.Error;
        }

        /// <summary>
        /// Standard message templates for consistent logging
        /// </summary>
        public static class UIMessageTemplates
        {
            public const string ButtonClick = "[UI_CLICK] Button '{ButtonName}' clicked in {CallerMethod}";
            public const string Navigation = "[UI_NAV] Navigating from '{FromView}' to '{ToView}' in {CallerMethod}";
            public const string WindowOperation = "[UI_WINDOW] Window '{WindowName}' {Operation} in {CallerMethod}";
            public const string SyncfusionInit = "[SF_INIT] Syncfusion {ControlType} '{ControlName}' initializing in {CallerMethod}";
            public const string SyncfusionEvent = "[SF_EVENT] Syncfusion {ControlType} '{ControlName}' fired '{EventName}' in {CallerMethod}";
            public const string PerformanceWarning = "[UI_PERF_SLOW] Operation '{OperationName}' took {DurationMs}ms (SLOW) in {CallerMethod}";
            public const string UIError = "[UI_ERROR] UI error in '{Context}' in {CallerMethod}: {ErrorMessage}";
        }
    }
}

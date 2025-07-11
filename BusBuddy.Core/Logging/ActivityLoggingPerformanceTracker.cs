using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BusBuddy.Core.Logging
{
    /// <summary>
    /// Provides DEBUG instrumentation for activity logging services
    /// </summary>
    public class ActivityLoggingPerformanceTracker : IDisposable
    {
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;
        private readonly string _operationName;
        private readonly string _methodName;
        private readonly string? _additionalContext;
        private bool _disposed;

        /// <summary>
        /// Creates a new instance of the ActivityLoggingPerformanceTracker for DEBUG instrumentation
        /// </summary>
        /// <param name="logger">The logger to write to</param>
        /// <param name="operationName">Name of the logging operation</param>
        /// <param name="additionalContext">Optional additional context information</param>
        /// <param name="callerMemberName">Automatically populated with the calling method name</param>
        public ActivityLoggingPerformanceTracker(
            ILogger logger,
            string operationName,
            string? additionalContext = null,
            [CallerMemberName] string callerMemberName = "")
        {
            _logger = logger;
            _operationName = operationName;
            _methodName = callerMemberName;
            _additionalContext = additionalContext;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

#if DEBUG
            // Only log entry in DEBUG builds
            if (string.IsNullOrEmpty(additionalContext))
            {
                _logger.LogDebug("[ACTIVITY_ENTRY] Starting {Operation} in {Method}",
                    _operationName, _methodName);
            }
            else
            {
                _logger.LogDebug("[ACTIVITY_ENTRY] Starting {Operation} in {Method} - Context: {Context}",
                    _operationName, _methodName, _additionalContext);
            }
#endif
        }

        /// <summary>
        /// Logs completion of the operation with timing information
        /// </summary>
        /// <param name="result">Optional result information</param>
        public void Complete(string? result = null)
        {
            if (_disposed) return;

            _stopwatch.Stop();
            var elapsed = _stopwatch.ElapsedMilliseconds;

#if DEBUG
            // Only log completion in DEBUG builds
            if (string.IsNullOrEmpty(result))
            {
                _logger.LogDebug("[ACTIVITY_EXIT] Completed {Operation} in {Method} - Duration: {Duration}ms",
                    _operationName, _methodName, elapsed);
            }
            else
            {
                _logger.LogDebug("[ACTIVITY_EXIT] Completed {Operation} in {Method} - Duration: {Duration}ms - Result: {Result}",
                    _operationName, _methodName, elapsed, result);
            }

            // Log performance warning for slow operations
            if (elapsed > 50)
            {
                _logger.LogWarning("[ACTIVITY_PERF] Slow {Operation} in {Method} - Duration: {Duration}ms",
                    _operationName, _methodName, elapsed);
            }
#endif
        }

        /// <summary>
        /// Logs an error that occurred during the operation
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        public void Error(Exception exception)
        {
            if (_disposed) return;

            _stopwatch.Stop();
            var elapsed = _stopwatch.ElapsedMilliseconds;

#if DEBUG
            // In DEBUG mode, log detailed error information
            _logger.LogError(exception,
                "[ACTIVITY_ERROR] Error in {Operation} in {Method} - Duration: {Duration}ms - Error: {ErrorMessage}",
                _operationName, _methodName, elapsed, exception.Message);

            // Add inner exception details if available
            if (exception.InnerException != null)
            {
                _logger.LogDebug("[ACTIVITY_ERROR_DETAIL] Inner exception: {InnerError}",
                    exception.InnerException.Message);
            }
#else
            // In production, log minimal error information
            _logger.LogError(exception, 
                "Error in {Operation}: {ErrorMessage}", 
                _operationName, exception.Message);
#endif
        }

        /// <summary>
        /// Disposes the tracker and logs completion if not already logged
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Complete the operation if not already completed
            if (_stopwatch.IsRunning)
            {
                Complete("Disposed without explicit completion");
            }

            _disposed = true;
        }
    }
}

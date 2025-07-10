using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Utility class for tracking and reporting application performance metrics
    /// </summary>
    public class PerformanceMonitor
    {
        private readonly ILogger? _logger;
        private readonly ConcurrentDictionary<string, OperationMetric> _metrics = new();

        public PerformanceMonitor(ILogger? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Tracks the execution time of an operation and logs the results
        /// </summary>
        /// <param name="operationName">The name of the operation being tracked</param>
        /// <param name="action">The action to execute and measure</param>
        public void TrackOperation(string operationName, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Tracks the execution time of an async operation and logs the results
        /// </summary>
        /// <param name="operationName">The name of the operation being tracked</param>
        /// <param name="asyncAction">The async action to execute and measure</param>
        public async Task TrackOperationAsync(string operationName, Func<Task> asyncAction)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await asyncAction();
            }
            finally
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Tracks the execution time of an async operation with a return value and logs the results
        /// </summary>
        /// <typeparam name="T">The return type of the operation</typeparam>
        /// <param name="operationName">The name of the operation being tracked</param>
        /// <param name="asyncFunc">The async function to execute and measure</param>
        /// <returns>The result of the operation</returns>
        public async Task<T> TrackOperationAsync<T>(string operationName, Func<Task<T>> asyncFunc)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await asyncFunc();
            }
            finally
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Records a metric for a completed operation
        /// </summary>
        /// <param name="operationName">The operation name</param>
        /// <param name="elapsed">The elapsed time</param>
        public void RecordMetric(string operationName, TimeSpan elapsed)
        {
            _metrics.AddOrUpdate(
                operationName,
                // Add new metric if key doesn't exist
                _ => new OperationMetric
                {
                    OperationName = operationName,
                    ExecutionCount = 1,
                    TotalTime = elapsed,
                    MinTime = elapsed,
                    MaxTime = elapsed,
                    LastExecutionTime = DateTime.Now
                },
                // Update existing metric
                (_, existingMetric) =>
                {
                    existingMetric.ExecutionCount++;
                    existingMetric.TotalTime += elapsed;
                    existingMetric.MinTime = elapsed < existingMetric.MinTime ? elapsed : existingMetric.MinTime;
                    existingMetric.MaxTime = elapsed > existingMetric.MaxTime ? elapsed : existingMetric.MaxTime;
                    existingMetric.LastExecutionTime = DateTime.Now;
                    return existingMetric;
                });

            // Log the operation
            _logger?.LogInformation(
                "Performance: {0} completed in {1}ms",
                operationName,
                elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Gets a copy of all collected metrics
        /// </summary>
        public IReadOnlyList<OperationMetric> GetAllMetrics()
        {
            return new List<OperationMetric>(_metrics.Values);
        }

        /// <summary>
        /// Clears all collected metrics
        /// </summary>
        public void ClearMetrics()
        {
            _metrics.Clear();
        }

        /// <summary>
        /// Creates a performance report for logging or display
        /// </summary>
        public string GenerateReport()
        {
            var metrics = GetAllMetrics();
            var report = new System.Text.StringBuilder();
            report.AppendLine("Performance Metrics Report");
            report.AppendLine("=========================");
            report.AppendLine();

            foreach (var metric in metrics)
            {
                report.AppendLine($"Operation: {metric.OperationName}");
                report.AppendLine($"  Executions: {metric.ExecutionCount}");
                report.AppendLine($"  Average Time: {metric.AverageTime.TotalMilliseconds:F2}ms");
                report.AppendLine($"  Min Time: {metric.MinTime.TotalMilliseconds:F2}ms");
                report.AppendLine($"  Max Time: {metric.MaxTime.TotalMilliseconds:F2}ms");
                report.AppendLine($"  Last Execution: {metric.LastExecutionTime}");
                report.AppendLine();
            }

            return report.ToString();
        }
    }

    /// <summary>
    /// Represents performance metrics for a specific operation
    /// </summary>
    public class OperationMetric
    {
        /// <summary>
        /// The name of the operation
        /// </summary>
        public string OperationName { get; set; } = string.Empty;

        /// <summary>
        /// The number of times the operation has been executed
        /// </summary>
        public int ExecutionCount { get; set; }

        /// <summary>
        /// The total time spent executing the operation
        /// </summary>
        public TimeSpan TotalTime { get; set; }

        /// <summary>
        /// The minimum execution time recorded
        /// </summary>
        public TimeSpan MinTime { get; set; } = TimeSpan.MaxValue;

        /// <summary>
        /// The maximum execution time recorded
        /// </summary>
        public TimeSpan MaxTime { get; set; } = TimeSpan.MinValue;

        /// <summary>
        /// When the operation was last executed
        /// </summary>
        public DateTime LastExecutionTime { get; set; }

        /// <summary>
        /// The average execution time
        /// </summary>
        public TimeSpan AverageTime => ExecutionCount > 0
            ? TimeSpan.FromTicks(TotalTime.Ticks / ExecutionCount)
            : TimeSpan.Zero;
    }
}

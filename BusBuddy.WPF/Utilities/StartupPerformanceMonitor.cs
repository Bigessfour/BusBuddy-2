using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Specialized performance monitor for tracking application startup sequence
    /// </summary>
    public class StartupPerformanceMonitor
    {
        private readonly ILogger<StartupPerformanceMonitor> _logger;
        private readonly Stopwatch _totalStopwatch = new();
        private readonly Dictionary<string, TimeSpan> _stepDurations = new();
        private readonly Dictionary<string, DateTime> _stepStartTimes = new();
        private readonly List<string> _startupSequence = new();

        private DateTime _startTime;
        private string _currentStep = string.Empty;

        public StartupPerformanceMonitor(ILogger<StartupPerformanceMonitor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts measuring the total startup time
        /// </summary>
        public void Start()
        {
            _startTime = DateTime.Now;
            _totalStopwatch.Start();
            _logger.LogInformation("[STARTUP_PERF] StartupPerformanceMonitor initialized at {StartTime}", _startTime);
        }

        /// <summary>
        /// Starts timing a specific startup step
        /// </summary>
        /// <param name="stepName">The name of the startup step</param>
        public void BeginStep(string stepName)
        {
            // Complete previous step if exists
            if (!string.IsNullOrEmpty(_currentStep))
            {
                EndStep();
            }

            _currentStep = stepName;
            _startupSequence.Add(stepName);
            _stepStartTimes[stepName] = DateTime.Now;

            _logger.LogInformation(
                "[STARTUP_PERF] Step started: {StepName}, Time since app start: {ElapsedMs}ms",
                stepName,
                _totalStopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Ends timing the current startup step
        /// </summary>
        public void EndStep()
        {
            if (string.IsNullOrEmpty(_currentStep))
                return;

            if (!_stepStartTimes.TryGetValue(_currentStep, out var startTime))
                return;

            var duration = DateTime.Now - startTime;
            _stepDurations[_currentStep] = duration;

            _logger.LogInformation(
                "[STARTUP_PERF] Step completed: {StepName}, Duration: {DurationMs}ms, Time since app start: {ElapsedMs}ms",
                _currentStep,
                duration.TotalMilliseconds,
                _totalStopwatch.ElapsedMilliseconds);

            // Add a structured log entry with specific duration field
            _logger.LogInformation(
                "[STARTUP_PERF] {StepName} completed. Duration:{DurationMs}ms ElapsedSinceStart:{ElapsedMs}ms",
                _currentStep,
                duration.TotalMilliseconds,
                _totalStopwatch.ElapsedMilliseconds);

            _currentStep = string.Empty;
        }

        /// <summary>
        /// Ends a specific named step (can be used from async context where current step might be lost)
        /// </summary>
        /// <param name="stepName">The name of the step to end</param>
        public void EndNamedStep(string stepName)
        {
            if (!_stepStartTimes.TryGetValue(stepName, out var startTime))
                return;

            var duration = DateTime.Now - startTime;
            _stepDurations[stepName] = duration;

            _logger.LogInformation(
                "[STARTUP_PERF] Named step completed: {StepName}, Duration: {DurationMs}ms, Time since app start: {ElapsedMs}ms",
                stepName,
                duration.TotalMilliseconds,
                _totalStopwatch.ElapsedMilliseconds);

            // Add a structured log entry with specific duration field
            _logger.LogInformation(
                "[STARTUP_PERF] {StepName} completed. Duration:{DurationMs}ms ElapsedSinceStart:{ElapsedMs}ms",
                stepName,
                duration.TotalMilliseconds,
                _totalStopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Tracks an asynchronous step by timing its execution
        /// </summary>
        /// <param name="stepName">The name of the step</param>
        /// <param name="action">The async action to perform</param>
        public async Task TrackStepAsync(string stepName, Func<Task> action)
        {
            BeginStep(stepName);
            try
            {
                await action();
            }
            finally
            {
                EndStep();
            }
        }

        /// <summary>
        /// Completes startup timing and logs the summary
        /// </summary>
        public void Complete()
        {
            // End any current step
            if (!string.IsNullOrEmpty(_currentStep))
            {
                EndStep();
            }

            _totalStopwatch.Stop();
            var totalDuration = _totalStopwatch.Elapsed;

            _logger.LogInformation(
                "[STARTUP_PERF] Application startup completed in {TotalDurationMs}ms",
                totalDuration.TotalMilliseconds);

            // Generate detailed summary
            _logger.LogInformation("[STARTUP_PERF] ===== Startup Performance Summary =====");

            foreach (var step in _startupSequence)
            {
                if (_stepDurations.TryGetValue(step, out var duration))
                {
                    var percentage = (duration.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100;
                    _logger.LogInformation(
                        "[STARTUP_PERF] Step: {StepName}, Duration: {DurationMs}ms ({Percentage:F1}% of total)",
                        step,
                        duration.TotalMilliseconds,
                        percentage);
                }
            }

            _logger.LogInformation("[STARTUP_PERF] Total startup time: {TotalDurationMs}ms", totalDuration.TotalMilliseconds);
            _logger.LogInformation("[STARTUP_PERF] ===== End of Startup Performance Summary =====");
        }

        /// <summary>
        /// Gets all recorded step durations
        /// </summary>
        public IReadOnlyDictionary<string, TimeSpan> GetStepDurations() => _stepDurations;

        /// <summary>
        /// Gets the total startup duration
        /// </summary>
        public TimeSpan GetTotalDuration() => _totalStopwatch.Elapsed;
    }
}

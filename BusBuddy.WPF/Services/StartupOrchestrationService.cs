using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.WPF.Services
{
    /// <summary>
    /// Orchestrates the application startup sequence with progress updates
    /// </summary>
    public class StartupOrchestrationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StartupOrchestrationService> _logger;

        public StartupOrchestrationService(
            IServiceProvider serviceProvider,
            ILogger<StartupOrchestrationService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the complete startup sequence with progress updates
        /// </summary>
        /// <param name="loadingViewModel">The loading view model to update with progress</param>
        /// <returns>Startup result with execution details</returns>
        public async Task<StartupResult> ExecuteStartupSequenceAsync(LoadingViewModel loadingViewModel)
        {
            var overallStopwatch = Stopwatch.StartNew();
            var result = new StartupResult();

            _logger.LogInformation("[STARTUP_ORCHESTRATION] Beginning coordinated startup sequence");

            try
            {
                // Reset the loading view model
                loadingViewModel.Reset();
                loadingViewModel.Status = "Initializing application...";
                loadingViewModel.ProgressPercentage = 0;
                loadingViewModel.IsIndeterminate = false;

                // Phase 1: Environment Validation (0-25%)
                await ExecutePhaseAsync(
                    "Environment Validation",
                    async () =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var validationService = scope.ServiceProvider.GetRequiredService<StartupValidationService>();
                        var validationResult = await validationService.ValidateStartupAsync();
                        result.ValidationResult = validationResult;
                        return validationResult.IsValid;
                    },
                    loadingViewModel,
                    0, 25,
                    result
                );

                // Phase 2: Service Optimization (25-50%)
                await ExecutePhaseAsync(
                    "Optimizing Services",
                    async () =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var optimizationService = scope.ServiceProvider.GetRequiredService<StartupOptimizationService>();
                        await optimizationService.PreloadCriticalServicesAsync();
                        return true;
                    },
                    loadingViewModel,
                    25, 50,
                    result
                );

                // Phase 3: Database Preparation (50-75%)
                await ExecutePhaseAsync(
                    "Preparing Database",
                    async () =>
                    {
                        // Simulate database preparation — replace with actual database initialization if needed
                        await Task.Delay(500);
                        return true;
                    },
                    loadingViewModel,
                    50, 75,
                    result
                );

                // Phase 4: Final Initialization (75-100%)
                await ExecutePhaseAsync(
                    "Finalizing Initialization",
                    async () =>
                    {
                        // Final preparation steps
                        await Task.Delay(300);
                        return true;
                    },
                    loadingViewModel,
                    75, 100,
                    result
                );

                // Mark as complete
                loadingViewModel.CompleteInitialization();
                overallStopwatch.Stop();

                result.TotalExecutionTimeMs = overallStopwatch.ElapsedMilliseconds;
                result.IsSuccessful = true;

                _logger.LogInformation("[STARTUP_ORCHESTRATION] ✅ Startup sequence completed successfully in {DurationMs}ms",
                    result.TotalExecutionTimeMs);

                return result;
            }
            catch (Exception ex)
            {
                overallStopwatch.Stop();
                result.TotalExecutionTimeMs = overallStopwatch.ElapsedMilliseconds;
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;

                loadingViewModel.Status = "Startup failed - see logs for details";
                _logger.LogError(ex, "[STARTUP_ORCHESTRATION] ❌ Startup sequence failed after {DurationMs}ms",
                    result.TotalExecutionTimeMs);

                return result;
            }
        }

        /// <summary>
        /// Executes a single startup phase with progress updates
        /// </summary>
        private async Task ExecutePhaseAsync(
            string phaseName,
            Func<Task<bool>> phaseAction,
            LoadingViewModel loadingViewModel,
            int startProgress,
            int endProgress,
            StartupResult overallResult)
        {
            var phaseStopwatch = Stopwatch.StartNew();
            var phaseResult = new PhaseResult { PhaseName = phaseName };
            
            _logger.LogInformation("[STARTUP_ORCHESTRATION] Starting phase: {PhaseName}", phaseName);

            loadingViewModel.Status = phaseName;
            loadingViewModel.ProgressPercentage = startProgress;

            try
            {
                var success = await phaseAction();
                phaseResult.IsSuccessful = success;

                // Animate progress to end value
                for (int i = startProgress; i <= endProgress; i += 2)
                {
                    loadingViewModel.ProgressPercentage = Math.Min(i, endProgress);
                    await Task.Delay(20); // Small delay for smooth animation
                }

                phaseStopwatch.Stop();
                phaseResult.ExecutionTimeMs = phaseStopwatch.ElapsedMilliseconds;

                if (success)
                {
                    _logger.LogInformation("[STARTUP_ORCHESTRATION] ✅ Phase '{PhaseName}' completed in {DurationMs}ms",
                        phaseName, phaseStopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning("[STARTUP_ORCHESTRATION] ⚠️ Phase '{PhaseName}' completed with warnings in {DurationMs}ms",
                        phaseName, phaseStopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                phaseStopwatch.Stop();
                phaseResult.IsSuccessful = false;
                phaseResult.ExecutionTimeMs = phaseStopwatch.ElapsedMilliseconds;
                phaseResult.ErrorMessage = ex.Message;
                
                _logger.LogError(ex, "[STARTUP_ORCHESTRATION] ❌ Phase '{PhaseName}' failed after {DurationMs}ms",
                    phaseName, phaseStopwatch.ElapsedMilliseconds);
                throw;
            }
            finally
            {
                overallResult.PhaseResults.Add(phaseResult);
            }
        }
    }

    /// <summary>
    /// Result of the startup orchestration process
    /// </summary>
    public class StartupResult
    {
        public bool IsSuccessful { get; set; }
        public long TotalExecutionTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
        public object? ValidationResult { get; set; }
        public List<PhaseResult> PhaseResults { get; set; } = new List<PhaseResult>();

        public string GetExecutionSummary()
        {
            var summary = $"Startup Duration: {TotalExecutionTimeMs}ms\n";
            summary += $"Status: {(IsSuccessful ? "✅ Success" : "❌ Failed")}\n";

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                summary += $"Error: {ErrorMessage}\n";
            }

            return summary;
        }
    }

    /// <summary>
    /// Result of an individual startup phase
    /// </summary>
    public class PhaseResult
    {
        public string PhaseName { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
        public long ExecutionTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

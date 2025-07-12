using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Helper class for managing background tasks with proper thread priority
    /// </summary>
    public class BackgroundTaskManager
    {
        private readonly ILogger<BackgroundTaskManager> _logger;

        public BackgroundTaskManager(ILogger<BackgroundTaskManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Runs a task in the background with lower thread priority to avoid impacting UI responsiveness
        /// </summary>
        /// <param name="action">The action to run</param>
        /// <param name="taskName">Name of the task for logging</param>
        /// <param name="delayMs">Optional delay before starting the task (in milliseconds)</param>
        public void RunLowPriorityTask(Action action, string taskName, int delayMs = 0)
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                try
                {
                    if (delayMs > 0)
                    {
                        await Task.Delay(delayMs);
                    }

                    // Set lower thread priority for background operations
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                    _logger.LogInformation("[BACKGROUND] Starting background task: {TaskName}", taskName);
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    // Execute the action
                    action();

                    stopwatch.Stop();
                    _logger.LogInformation("[BACKGROUND] Completed background task: {TaskName} in {ElapsedMs}ms",
                        taskName, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[BACKGROUND] Error in background task {TaskName}: {ErrorMessage}",
                        taskName, ex.Message);
                }
            });
        }

        /// <summary>
        /// Runs an async task in the background with lower thread priority to avoid impacting UI responsiveness
        /// </summary>
        /// <param name="asyncAction">The async action to run</param>
        /// <param name="taskName">Name of the task for logging</param>
        /// <param name="delayMs">Optional delay before starting the task (in milliseconds)</param>
        public void RunLowPriorityTaskAsync(Func<Task> asyncAction, string taskName, int delayMs = 0)
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                try
                {
                    if (delayMs > 0)
                    {
                        await Task.Delay(delayMs);
                    }

                    // Set lower thread priority for background operations
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                    _logger.LogInformation("[BACKGROUND] Starting async background task: {TaskName}", taskName);
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    // Execute the async action
                    await asyncAction();

                    stopwatch.Stop();
                    _logger.LogInformation("[BACKGROUND] Completed async background task: {TaskName} in {ElapsedMs}ms",
                        taskName, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[BACKGROUND] Error in async background task {TaskName}: {ErrorMessage}",
                        taskName, ex.Message);
                }
            });
        }
    }
}

using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace BusBuddy.WPF.Logging
{
    /// <summary>
    /// Enhanced logging service focused on UI performance and Syncfusion controls
    /// </summary>
    public class UIPerformanceLogger : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, Stopwatch> _activeOperations;
        private readonly ConcurrentDictionary<string, int> _operationCounters;
        private bool _disposed;

        public UIPerformanceLogger(ILogger<UIPerformanceLogger> logger)
        {
            _logger = logger;
            _activeOperations = new ConcurrentDictionary<string, Stopwatch>();
            _operationCounters = new ConcurrentDictionary<string, int>();
        }

        #region UI Control Interaction Logging

        /// <summary>
        /// Logs button click events with performance tracking
        /// </summary>
        public void LogButtonClick(string buttonName, string? context = null, [CallerMemberName] string callerMethod = "")
        {
            var operationId = $"ButtonClick_{buttonName}_{DateTime.Now.Ticks}";
            var count = _operationCounters.AddOrUpdate($"ButtonClick_{buttonName}", 1, (k, v) => v + 1);

            using (LogContext.PushProperty("UIOperation", "ButtonClick"))
            using (LogContext.PushProperty("ButtonName", buttonName))
            using (LogContext.PushProperty("ClickCount", count))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                if (!string.IsNullOrEmpty(context))
                {
                    using (LogContext.PushProperty("Context", context))
                    {
                        _logger.LogInformation("[UI_CLICK] Button '{ButtonName}' clicked (#{ClickCount}) in {CallerMethod} - Context: {Context}",
                            buttonName, count, callerMethod, context);
                    }
                }
                else
                {
                    _logger.LogInformation("[UI_CLICK] Button '{ButtonName}' clicked (#{ClickCount}) in {CallerMethod}",
                        buttonName, count, callerMethod);
                }
            }

            StartUIOperation(operationId, $"Button click: {buttonName}");
        }

        /// <summary>
        /// Logs navigation events in the application
        /// </summary>
        public void LogNavigation(string fromView, string toView, string? navigationContext = null, [CallerMemberName] string callerMethod = "")
        {
            var operationId = $"Navigation_{fromView}_to_{toView}_{DateTime.Now.Ticks}";

            using (LogContext.PushProperty("UIOperation", "Navigation"))
            using (LogContext.PushProperty("FromView", fromView))
            using (LogContext.PushProperty("ToView", toView))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                if (!string.IsNullOrEmpty(navigationContext))
                {
                    using (LogContext.PushProperty("NavigationContext", navigationContext))
                    {
                        _logger.LogInformation("[UI_NAV] Navigating from '{FromView}' to '{ToView}' in {CallerMethod} - Context: {NavigationContext}",
                            fromView, toView, callerMethod, navigationContext);
                    }
                }
                else
                {
                    _logger.LogInformation("[UI_NAV] Navigating from '{FromView}' to '{ToView}' in {CallerMethod}",
                        fromView, toView, callerMethod);
                }
            }

            StartUIOperation(operationId, $"Navigation: {fromView} → {toView}");
        }

        /// <summary>
        /// Logs window operations (open, close, resize, etc.)
        /// </summary>
        public void LogWindowOperation(string windowName, string operation, object? details = null, [CallerMemberName] string callerMethod = "")
        {
            var operationId = $"Window_{operation}_{windowName}_{DateTime.Now.Ticks}";

            using (LogContext.PushProperty("UIOperation", "WindowOperation"))
            using (LogContext.PushProperty("WindowName", windowName))
            using (LogContext.PushProperty("Operation", operation))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                if (details != null)
                {
                    using (LogContext.PushProperty("Details", details))
                    {
                        _logger.LogInformation("[UI_WINDOW] Window '{WindowName}' {Operation} in {CallerMethod} - Details: {Details}",
                            windowName, operation, callerMethod, details);
                    }
                }
                else
                {
                    _logger.LogInformation("[UI_WINDOW] Window '{WindowName}' {Operation} in {CallerMethod}",
                        windowName, operation, callerMethod);
                }
            }

            StartUIOperation(operationId, $"Window operation: {windowName} {operation}");
        }

        #endregion

        #region Syncfusion Control Logging

        /// <summary>
        /// Logs Syncfusion control initialization and theme application
        /// </summary>
        public void LogSyncfusionControlInit(string controlType, string controlName, string? theme = null, [CallerMemberName] string callerMethod = "")
        {
            var operationId = $"SfControl_{controlType}_{controlName}_{DateTime.Now.Ticks}";

            using (LogContext.PushProperty("UIOperation", "SyncfusionControlInit"))
            using (LogContext.PushProperty("ControlType", controlType))
            using (LogContext.PushProperty("ControlName", controlName))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                if (!string.IsNullOrEmpty(theme))
                {
                    using (LogContext.PushProperty("Theme", theme))
                    {
                        _logger.LogInformation("[SF_INIT] Syncfusion {ControlType} '{ControlName}' initializing with theme '{Theme}' in {CallerMethod}",
                            controlType, controlName, theme, callerMethod);
                    }
                }
                else
                {
                    _logger.LogInformation("[SF_INIT] Syncfusion {ControlType} '{ControlName}' initializing in {CallerMethod}",
                        controlType, controlName, callerMethod);
                }
            }

            StartUIOperation(operationId, $"Syncfusion control init: {controlType} {controlName}");
        }

        /// <summary>
        /// Logs Syncfusion control events (selection, data binding, etc.)
        /// </summary>
        public void LogSyncfusionEvent(string controlType, string controlName, string eventName, object? eventData = null, [CallerMemberName] string callerMethod = "")
        {
            using (LogContext.PushProperty("UIOperation", "SyncfusionEvent"))
            using (LogContext.PushProperty("ControlType", controlType))
            using (LogContext.PushProperty("ControlName", controlName))
            using (LogContext.PushProperty("EventName", eventName))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                if (eventData != null)
                {
                    using (LogContext.PushProperty("EventData", eventData))
                    {
                        _logger.LogDebug("[SF_EVENT] Syncfusion {ControlType} '{ControlName}' fired '{EventName}' in {CallerMethod} - Data: {EventData}",
                            controlType, controlName, eventName, callerMethod, eventData);
                    }
                }
                else
                {
                    _logger.LogDebug("[SF_EVENT] Syncfusion {ControlType} '{ControlName}' fired '{EventName}' in {CallerMethod}",
                        controlType, controlName, eventName, callerMethod);
                }
            }
        }

        /// <summary>
        /// Logs Syncfusion theme application events
        /// </summary>
        public void LogSyncfusionThemeChange(string fromTheme, string toTheme, string? scope = null, [CallerMemberName] string callerMethod = "")
        {
            var operationId = $"SfTheme_{fromTheme}_to_{toTheme}_{DateTime.Now.Ticks}";

            using (LogContext.PushProperty("UIOperation", "SyncfusionThemeChange"))
            using (LogContext.PushProperty("FromTheme", fromTheme))
            using (LogContext.PushProperty("ToTheme", toTheme))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                if (!string.IsNullOrEmpty(scope))
                {
                    using (LogContext.PushProperty("Scope", scope))
                    {
                        _logger.LogInformation("[SF_THEME] Syncfusion theme changing from '{FromTheme}' to '{ToTheme}' for scope '{Scope}' in {CallerMethod}",
                            fromTheme, toTheme, scope, callerMethod);
                    }
                }
                else
                {
                    _logger.LogInformation("[SF_THEME] Syncfusion theme changing from '{FromTheme}' to '{ToTheme}' globally in {CallerMethod}",
                        fromTheme, toTheme, callerMethod);
                }
            }

            StartUIOperation(operationId, $"Syncfusion theme change: {fromTheme} → {toTheme}");
        }

        /// <summary>
        /// Logs Syncfusion data binding operations
        /// </summary>
        public void LogSyncfusionDataBinding(string controlType, string controlName, string dataSourceType, int itemCount, [CallerMemberName] string callerMethod = "")
        {
            var operationId = $"SfDataBind_{controlType}_{controlName}_{DateTime.Now.Ticks}";

            using (LogContext.PushProperty("UIOperation", "SyncfusionDataBinding"))
            using (LogContext.PushProperty("ControlType", controlType))
            using (LogContext.PushProperty("ControlName", controlName))
            using (LogContext.PushProperty("DataSourceType", dataSourceType))
            using (LogContext.PushProperty("ItemCount", itemCount))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                _logger.LogInformation("[SF_DATABIND] Syncfusion {ControlType} '{ControlName}' binding to {DataSourceType} with {ItemCount} items in {CallerMethod}",
                    controlType, controlName, dataSourceType, itemCount, callerMethod);
            }

            StartUIOperation(operationId, $"Syncfusion data binding: {controlType} {controlName} ({itemCount} items)");
        }

        #endregion

        #region Performance Tracking

        /// <summary>
        /// Starts tracking a UI operation
        /// </summary>
        public void StartUIOperation(string operationId, string operationDescription, [CallerMemberName] string callerMethod = "")
        {
            var stopwatch = Stopwatch.StartNew();
            _activeOperations.TryAdd(operationId, stopwatch);

            using (LogContext.PushProperty("OperationId", operationId))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                _logger.LogDebug("[UI_PERF_START] Starting UI operation '{OperationDescription}' (ID: {OperationId}) in {CallerMethod}",
                    operationDescription, operationId, callerMethod);
            }
        }

        /// <summary>
        /// Ends tracking a UI operation and logs performance metrics
        /// </summary>
        public void EndUIOperation(string operationId, string? result = null, [CallerMemberName] string callerMethod = "")
        {
            if (_activeOperations.TryRemove(operationId, out var stopwatch))
            {
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;

                using (LogContext.PushProperty("OperationId", operationId))
                using (LogContext.PushProperty("Duration", duration))
                using (LogContext.PushProperty("CallerMethod", callerMethod))
                {
                    if (duration > 100) // Log warning for slow UI operations
                    {
                        _logger.LogWarning("[UI_PERF_SLOW] UI operation '{OperationId}' completed in {Duration}ms (SLOW) in {CallerMethod} - Result: {Result}",
                            operationId, duration, callerMethod, result ?? "None");
                    }
                    else if (!string.IsNullOrEmpty(result))
                    {
                        _logger.LogInformation("[UI_PERF_END] UI operation '{OperationId}' completed in {Duration}ms in {CallerMethod} - Result: {Result}",
                            operationId, duration, callerMethod, result);
                    }
                    else
                    {
                        _logger.LogDebug("[UI_PERF_END] UI operation '{OperationId}' completed in {Duration}ms in {CallerMethod}",
                            operationId, duration, callerMethod);
                    }
                }
            }
            else
            {
                _logger.LogWarning("[UI_PERF_WARNING] Attempted to end unknown UI operation '{OperationId}' in {CallerMethod}",
                    operationId, callerMethod);
            }
        }

        /// <summary>
        /// Logs UI responsiveness metrics
        /// </summary>
        public void LogUIResponsiveness(string controlName, string action, TimeSpan responseTime, [CallerMemberName] string callerMethod = "")
        {
            var responsiveMs = responseTime.TotalMilliseconds;

            using (LogContext.PushProperty("UIOperation", "Responsiveness"))
            using (LogContext.PushProperty("ControlName", controlName))
            using (LogContext.PushProperty("Action", action))
            using (LogContext.PushProperty("ResponseTimeMs", responsiveMs))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                if (responsiveMs > 200)
                {
                    _logger.LogWarning("[UI_RESPONSE_SLOW] Control '{ControlName}' {Action} took {ResponseTimeMs}ms (SLOW) in {CallerMethod}",
                        controlName, action, responsiveMs, callerMethod);
                }
                else if (responsiveMs > 50)
                {
                    _logger.LogInformation("[UI_RESPONSE] Control '{ControlName}' {Action} took {ResponseTimeMs}ms in {CallerMethod}",
                        controlName, action, responsiveMs, callerMethod);
                }
                else
                {
                    _logger.LogDebug("[UI_RESPONSE_FAST] Control '{ControlName}' {Action} took {ResponseTimeMs}ms (FAST) in {CallerMethod}",
                        controlName, action, responsiveMs, callerMethod);
                }
            }
        }

        #endregion

        #region Error and Exception Logging

        /// <summary>
        /// Logs UI-related exceptions with enhanced context
        /// </summary>
        public void LogUIException(Exception exception, string context, string? controlName = null, [CallerMemberName] string callerMethod = "")
        {
            using (LogContext.PushProperty("UIOperation", "Exception"))
            using (LogContext.PushProperty("Context", context))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                if (!string.IsNullOrEmpty(controlName))
                {
                    using (LogContext.PushProperty("ControlName", controlName))
                    {
                        _logger.LogError(exception, "[UI_ERROR] UI exception in '{Context}' for control '{ControlName}' in {CallerMethod}: {ErrorMessage}",
                            context, controlName, callerMethod, exception.Message);
                    }
                }
                else
                {
                    _logger.LogError(exception, "[UI_ERROR] UI exception in '{Context}' in {CallerMethod}: {ErrorMessage}",
                        context, callerMethod, exception.Message);
                }
            }
        }

        /// <summary>
        /// Logs Syncfusion-specific exceptions
        /// </summary>
        public void LogSyncfusionException(Exception exception, string controlType, string? controlName = null, [CallerMemberName] string callerMethod = "")
        {
            using (LogContext.PushProperty("UIOperation", "SyncfusionException"))
            using (LogContext.PushProperty("ControlType", controlType))
            using (LogContext.PushProperty("CallerMethod", callerMethod))
            {
                if (!string.IsNullOrEmpty(controlName))
                {
                    using (LogContext.PushProperty("ControlName", controlName))
                    {
                        _logger.LogError(exception, "[SF_ERROR] Syncfusion {ControlType} '{ControlName}' exception in {CallerMethod}: {ErrorMessage}",
                            controlType, controlName, callerMethod, exception.Message);
                    }
                }
                else
                {
                    _logger.LogError(exception, "[SF_ERROR] Syncfusion {ControlType} exception in {CallerMethod}: {ErrorMessage}",
                        controlType, callerMethod, exception.Message);
                }
            }
        }

        #endregion

        #region Resource Management

        public void Dispose()
        {
            if (_disposed) return;

            // Log any remaining active operations
            foreach (var kvp in _activeOperations)
            {
                kvp.Value.Stop();
                _logger.LogWarning("[UI_PERF_ORPHAN] UI operation '{OperationId}' was not properly ended - Duration: {Duration}ms",
                    kvp.Key, kvp.Value.ElapsedMilliseconds);
            }

            _activeOperations.Clear();
            _operationCounters.Clear();
            _disposed = true;
        }

        #endregion
    }
}

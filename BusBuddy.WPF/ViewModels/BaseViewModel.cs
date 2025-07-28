using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using Serilog.Context;

namespace BusBuddy.WPF.ViewModels
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Gets a contextualized Serilog logger for this ViewModel.
        /// Automatically includes the ViewModel type and calling method context.
        /// </summary>
        protected ILogger Logger => Log.ForContext(GetType());

        /// <summary>
        /// Gets a contextualized logger with additional method context.
        /// </summary>
        protected ILogger GetLoggerWithContext([CallerMemberName] string? methodName = null)
        {
            return Logger.ForContext("MethodName", methodName ?? "Unknown");
        }

        /// <summary>
        /// Enhanced data loading with structured logging and enrichment.
        /// </summary>
        protected async Task LoadDataAsync(Func<Task> loadAction, [CallerMemberName] string? methodName = null)
        {
            var logger = GetLoggerWithContext(methodName);
            var correlationId = Guid.NewGuid().ToString("N")[..8];

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("ViewModelType", GetType().Name))
            using (LogContext.PushProperty("OperationType", "DataLoad"))
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    logger.Information("UI Data loading started for {MethodName} in {ViewModelType}", methodName, GetType().Name);
                    IsLoading = true;
                    ErrorMessage = null;

                    await loadAction();

                    stopwatch.Stop();
                    logger.Information("UI Data loading completed successfully for {MethodName} in {ElapsedMs}ms",
                        methodName, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                    using (LogContext.PushProperty("HasInnerException", ex.InnerException != null))
                    {
                        logger.Error(ex, "UI Data loading failed for {MethodName} after {ElapsedMs}ms: {ErrorMessage}",
                            methodName, stopwatch.ElapsedMilliseconds, ex.Message);

                        ErrorMessage = $"Failed to load data: {ex.Message}";

                        // Enhanced inner exception logging
                        if (ex.InnerException != null)
                        {
                            logger.Error(ex.InnerException, "UI Inner exception details: {InnerExceptionType} - {InnerMessage}",
                                ex.InnerException.GetType().Name, ex.InnerException.Message);
                            ErrorMessage += $" (Inner: {ex.InnerException.Message})";
                        }
                    }
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        /// <summary>
        /// Enhanced command execution with structured logging.
        /// </summary>
        protected async Task ExecuteCommandAsync(Func<Task> commandAction, [CallerMemberName] string? commandName = null)
        {
            var logger = GetLoggerWithContext(commandName);
            var correlationId = Guid.NewGuid().ToString("N")[..8];

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("ViewModelType", GetType().Name))
            using (LogContext.PushProperty("OperationType", "Command"))
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    logger.Information("UI Command execution started: {CommandName} in {ViewModelType}", commandName, GetType().Name);

                    await commandAction();

                    stopwatch.Stop();
                    logger.Information("UI Command execution completed: {CommandName} in {ElapsedMs}ms",
                        commandName, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                    {
                        logger.Error(ex, "UI Command execution failed: {CommandName} after {ElapsedMs}ms - {ErrorMessage}",
                            commandName, stopwatch.ElapsedMilliseconds, ex.Message);
                    }

                    throw; // Re-throw to allow proper error handling upstream
                }
            }
        }

        /// <summary>
        /// Logs property changes with structured context.
        /// </summary>
        protected void LogPropertyChange<T>(T oldValue, T newValue, [CallerMemberName] string? propertyName = null)
        {
            using (LogContext.PushProperty("ViewModelType", GetType().Name))
            using (LogContext.PushProperty("OperationType", "PropertyChange"))
            {
                Logger.Debug("UI Property changed: {PropertyName} from {OldValue} to {NewValue} in {ViewModelType}",
                    propertyName, oldValue, newValue, GetType().Name);
            }
        }

        /// <summary>
        /// Logs validation errors with structured context.
        /// </summary>
        protected void LogValidationError(string validationMessage, [CallerMemberName] string? methodName = null)
        {
            using (LogContext.PushProperty("ViewModelType", GetType().Name))
            using (LogContext.PushProperty("OperationType", "Validation"))
            {
                Logger.Warning("UI Validation error in {MethodName}: {ValidationMessage}", methodName, validationMessage);
            }
        }

        /// <summary>
        /// Logs user interactions with structured context.
        /// </summary>
        protected void LogUserInteraction(string interaction, object? context = null, [CallerMemberName] string? methodName = null)
        {
            using (LogContext.PushProperty("ViewModelType", GetType().Name))
            using (LogContext.PushProperty("OperationType", "UserInteraction"))
            {
                if (context != null)
                {
                    Logger.Information("UI User interaction: {Interaction} in {MethodName} with context {@Context}",
                        interaction, methodName, context);
                }
                else
                {
                    Logger.Information("UI User interaction: {Interaction} in {MethodName}", interaction, methodName);
                }
            }
        }
    }
}

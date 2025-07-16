using BusBuddy.WPF.ViewModels;
using Serilog;
using Serilog.Context;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace BusBuddy.WPF.Views
{
    /// <summary>
    /// Interaction logic for StudentListView.xaml
    /// Enhanced with structured Serilog logging for comprehensive observability
    /// </summary>
    public partial class StudentListView : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<StudentListView>();
        private readonly Stopwatch _initializationStopwatch;
        private readonly string _correlationId;

        public StudentListView()
        {
            _correlationId = Guid.NewGuid().ToString("N")[..8];
            _initializationStopwatch = Stopwatch.StartNew();

            using (LogContext.PushProperty("CorrelationId", _correlationId))
            using (LogContext.PushProperty("ViewType", nameof(StudentListView)))
            using (LogContext.PushProperty("OperationType", "ViewInitialization"))
            {
                Logger.Information("StudentListView initialization starting");

                try
                {
                    InitializeComponent();

                    // Wire up event handlers for enhanced logging
                    this.Loaded += OnViewLoaded;
                    this.Unloaded += OnViewUnloaded;

                    _initializationStopwatch.Stop();
                    Logger.Information("StudentListView initialization completed successfully in {ElapsedMs}ms",
                        _initializationStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _initializationStopwatch.Stop();

                    using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                    using (LogContext.PushProperty("HasInnerException", ex.InnerException != null))
                    {
                        Logger.Error(ex, "StudentListView initialization failed after {ElapsedMs}ms: {ErrorMessage}",
                            _initializationStopwatch.ElapsedMilliseconds, ex.Message);

                        if (ex.InnerException != null)
                        {
                            Logger.Error(ex.InnerException, "Inner exception during view initialization: {InnerExceptionType} - {InnerMessage}",
                                ex.InnerException.GetType().Name, ex.InnerException.Message);
                        }
                    }

                    throw; // Re-throw to ensure proper error handling upstream
                }
            }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var interactionStopwatch = Stopwatch.StartNew();

            using (LogContext.PushProperty("CorrelationId", _correlationId))
            using (LogContext.PushProperty("ViewType", nameof(StudentListView)))
            using (LogContext.PushProperty("OperationType", "UserInteraction"))
            using (LogContext.PushProperty("InteractionType", "ListViewItemDoubleClick"))
            {
                try
                {
                    Logger.Information("User double-clicked on ListView item");

                    if (sender is ListViewItem item && DataContext is StudentListViewModel viewModel)
                    {
                        var studentData = item.DataContext as BusBuddy.Core.Models.Student;

                        using (LogContext.PushProperty("StudentId", studentData?.StudentId))
                        using (LogContext.PushProperty("StudentName", studentData?.StudentName))
                        {
                            Logger.Information("Executing OpenStudentDetailCommand for student {StudentName} (ID: {StudentId})",
                                studentData?.StudentName ?? "Unknown", studentData?.StudentId);

                            viewModel.OpenStudentDetailCommand.Execute(item.DataContext);

                            interactionStopwatch.Stop();
                            Logger.Information("ListView double-click interaction completed in {ElapsedMs}ms",
                                interactionStopwatch.ElapsedMilliseconds);
                        }
                    }
                    else
                    {
                        Logger.Warning("ListView double-click interaction failed: Invalid sender type or missing DataContext");
                        Logger.Debug("Sender type: {SenderType}, DataContext type: {DataContextType}",
                            sender?.GetType().Name ?? "null", DataContext?.GetType().Name ?? "null");
                    }
                }
                catch (Exception ex)
                {
                    interactionStopwatch.Stop();

                    using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                    {
                        Logger.Error(ex, "Error during ListView item double-click interaction after {ElapsedMs}ms: {ErrorMessage}",
                            interactionStopwatch.ElapsedMilliseconds, ex.Message);
                    }

                    // Don't re-throw UI interaction exceptions to avoid crashing the UI
                }
            }
        }

        private void SfDataGrid_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            var interactionStopwatch = Stopwatch.StartNew();

            using (LogContext.PushProperty("CorrelationId", _correlationId))
            using (LogContext.PushProperty("ViewType", nameof(StudentListView)))
            using (LogContext.PushProperty("OperationType", "UserInteraction"))
            using (LogContext.PushProperty("InteractionType", "SfDataGridCellDoubleClick"))
            {
                try
                {
                    Logger.Information("User double-clicked on SfDataGrid cell");

                    if (DataContext is StudentListViewModel viewModel && e.Record != null)
                    {
                        var studentData = e.Record as BusBuddy.Core.Models.Student;

                        using (LogContext.PushProperty("StudentId", studentData?.StudentId))
                        using (LogContext.PushProperty("StudentName", studentData?.StudentName))
                        using (LogContext.PushProperty("ColumnName", e.Column?.MappingName))
                        {
                            Logger.Information("Executing ViewDetailsCommand for student {StudentName} (ID: {StudentId}) from column {ColumnName}",
                                studentData?.StudentName ?? "Unknown",
                                studentData?.StudentId,
                                e.Column?.MappingName ?? "Unknown");

                            // Set the selected student in the ViewModel before executing command
                            viewModel.SelectedStudent = studentData;

                            viewModel.ViewDetailsCommand.Execute(null);

                            interactionStopwatch.Stop();
                            Logger.Information("SfDataGrid cell double-click interaction completed in {ElapsedMs}ms",
                                interactionStopwatch.ElapsedMilliseconds);
                        }
                    }
                    else
                    {
                        Logger.Warning("SfDataGrid cell double-click interaction failed: Invalid DataContext or null record");
                        Logger.Debug("DataContext type: {DataContextType}, Record is null: {RecordIsNull}",
                            DataContext?.GetType().Name ?? "null", e.Record == null);
                    }
                }
                catch (Exception ex)
                {
                    interactionStopwatch.Stop();

                    using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                    using (LogContext.PushProperty("ColumnName", e.Column?.MappingName))
                    {
                        Logger.Error(ex, "Error during SfDataGrid cell double-click interaction after {ElapsedMs}ms: {ErrorMessage}",
                            interactionStopwatch.ElapsedMilliseconds, ex.Message);
                    }

                    // Don't re-throw UI interaction exceptions to avoid crashing the UI
                }
            }
        }

        #region Enhanced Logging Support Methods

        /// <summary>
        /// Logs view lifecycle events with structured context
        /// </summary>
        private void LogViewLifecycleEvent(string eventName, object? context = null)
        {
            using (LogContext.PushProperty("CorrelationId", _correlationId))
            using (LogContext.PushProperty("ViewType", nameof(StudentListView)))
            using (LogContext.PushProperty("OperationType", "ViewLifecycle"))
            using (LogContext.PushProperty("LifecycleEvent", eventName))
            {
                if (context != null)
                {
                    Logger.Information("View lifecycle event: {LifecycleEvent} with context {@Context}", eventName, context);
                }
                else
                {
                    Logger.Information("View lifecycle event: {LifecycleEvent}", eventName);
                }
            }
        }

        /// <summary>
        /// Logs user interactions with detailed context
        /// </summary>
        private void LogUserInteraction(string interaction, object? additionalContext = null)
        {
            using (LogContext.PushProperty("CorrelationId", _correlationId))
            using (LogContext.PushProperty("ViewType", nameof(StudentListView)))
            using (LogContext.PushProperty("OperationType", "UserInteraction"))
            using (LogContext.PushProperty("InteractionType", interaction))
            {
                if (additionalContext != null)
                {
                    Logger.Information("User interaction: {InteractionType} with context {@AdditionalContext}",
                        interaction, additionalContext);
                }
                else
                {
                    Logger.Information("User interaction: {InteractionType}", interaction);
                }
            }
        }

        /// <summary>
        /// Logs view state changes with performance metrics
        /// </summary>
        private void LogViewStateChange(string stateChange, TimeSpan? duration = null, object? stateContext = null)
        {
            using (LogContext.PushProperty("CorrelationId", _correlationId))
            using (LogContext.PushProperty("ViewType", nameof(StudentListView)))
            using (LogContext.PushProperty("OperationType", "ViewStateChange"))
            using (LogContext.PushProperty("StateChange", stateChange))
            {
                if (duration.HasValue && stateContext != null)
                {
                    Logger.Information("View state change: {StateChange} completed in {ElapsedMs}ms with context {@StateContext}",
                        stateChange, duration.Value.TotalMilliseconds, stateContext);
                }
                else if (duration.HasValue)
                {
                    Logger.Information("View state change: {StateChange} completed in {ElapsedMs}ms",
                        stateChange, duration.Value.TotalMilliseconds);
                }
                else if (stateContext != null)
                {
                    Logger.Information("View state change: {StateChange} with context {@StateContext}",
                        stateChange, stateContext);
                }
                else
                {
                    Logger.Information("View state change: {StateChange}", stateChange);
                }
            }
        }

        #endregion

        #region View Event Handlers with Enhanced Logging

        protected override void OnInitialized(EventArgs e)
        {
            var eventStopwatch = Stopwatch.StartNew();

            try
            {
                base.OnInitialized(e);

                eventStopwatch.Stop();
                LogViewLifecycleEvent("OnInitialized", new { Duration = eventStopwatch.ElapsedMilliseconds });
            }
            catch (Exception ex)
            {
                eventStopwatch.Stop();

                using (LogContext.PushProperty("CorrelationId", _correlationId))
                using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                {
                    Logger.Error(ex, "Error during OnInitialized after {ElapsedMs}ms: {ErrorMessage}",
                        eventStopwatch.ElapsedMilliseconds, ex.Message);
                }

                throw;
            }
        }

        private void OnViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var eventStopwatch = Stopwatch.StartNew();

            try
            {
                eventStopwatch.Stop();
                LogViewLifecycleEvent("OnLoaded", new
                {
                    Duration = eventStopwatch.ElapsedMilliseconds,
                    TotalInitializationTime = _initializationStopwatch.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                eventStopwatch.Stop();

                using (LogContext.PushProperty("CorrelationId", _correlationId))
                using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                {
                    Logger.Error(ex, "Error during OnLoaded after {ElapsedMs}ms: {ErrorMessage}",
                        eventStopwatch.ElapsedMilliseconds, ex.Message);
                }
            }
        }

        private void OnViewUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var eventStopwatch = Stopwatch.StartNew();

            try
            {
                LogViewLifecycleEvent("OnUnloaded", new { ViewLifetime = _initializationStopwatch.Elapsed });

                eventStopwatch.Stop();
                Logger.Information("View unloaded successfully in {ElapsedMs}ms, total lifetime: {TotalLifetimeMs}ms",
                    eventStopwatch.ElapsedMilliseconds, _initializationStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                eventStopwatch.Stop();

                using (LogContext.PushProperty("CorrelationId", _correlationId))
                using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                {
                    Logger.Error(ex, "Error during OnUnloaded after {ElapsedMs}ms: {ErrorMessage}",
                        eventStopwatch.ElapsedMilliseconds, ex.Message);
                }
            }
        }

        #endregion
    }
}

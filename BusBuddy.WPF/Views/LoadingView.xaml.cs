using System.Windows.Controls;
using System.Windows;
using Serilog;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BusBuddy.WPF.Views
{
    /// <summary>
    /// Optimized Loading View with centralized theme management and enhanced progress feedback
    /// Implements standardized sizing, real-time progress updates, and preload triggering
    /// </summary>
    public partial class LoadingView : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<LoadingView>();
        private LoadingViewModel? _loadingViewModel;

        public LoadingView()
        {
            try
            {
                Logger.Debug("LoadingView initializing with enhanced progress feedback");

                // Theme is managed centrally - no per-view theme operations needed
                InitializeComponent();

                // Set standardized minimum dimensions for consistent sizing
                this.MinWidth = 600;
                this.MinHeight = 400;

                Logger.Debug("LoadingView initialized successfully with standardized sizing");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "LoadingView initialization failed: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Handle the Loaded event to trigger preload operations
        /// </summary>
        private void LoadingView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Debug("LoadingView Loaded event - triggering enhanced startup sequence");

                // Get the LoadingViewModel from DataContext or DI container
                _loadingViewModel = this.DataContext as LoadingViewModel;

                if (_loadingViewModel == null && Application.Current is App app)
                {
                    _loadingViewModel = app.Services?.GetService<LoadingViewModel>();
                    if (_loadingViewModel != null)
                    {
                        this.DataContext = _loadingViewModel;
                        Logger.Debug("LoadingViewModel resolved from DI container");
                    }
                }

                if (_loadingViewModel != null)
                {
                    // Subscribe to completion event for navigation triggers
                    _loadingViewModel.InitializationCompleted += OnInitializationCompleted;

                    // Reset the loading state to ensure fresh start
                    _loadingViewModel.Reset();

                    // Start with a welcoming status message
                    _loadingViewModel.Status = "Welcome to Bus Buddy";
                    _loadingViewModel.IsIndeterminate = true;

                    Logger.Information("✅ LoadingView preload sequence initiated");
                }
                else
                {
                    Logger.Warning("⚠️ LoadingViewModel not available - using fallback loading display");
                }

                // Trigger any additional preload operations
                TriggerPreloadOperations();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "LoadingView Loaded event failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle the Unloaded event for cleanup
        /// </summary>
        private void LoadingView_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_loadingViewModel != null)
                {
                    _loadingViewModel.InitializationCompleted -= OnInitializationCompleted;
                    Logger.Debug("LoadingView cleanup completed");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "LoadingView Unloaded event failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle initialization completion for potential navigation triggers
        /// </summary>
        private void OnInitializationCompleted(object? sender, EventArgs e)
        {
            try
            {
                Logger.Information("🎉 LoadingView received initialization completion signal");

                // Could trigger navigation here if needed, but App.xaml.cs orchestration handles this
                // This event can be used for view-specific completion actions
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "InitializationCompleted handler failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Trigger preload operations that can run while the loading screen is visible
        /// </summary>
        private void TriggerPreloadOperations()
        {
            try
            {
                Logger.Debug("Triggering LoadingView preload operations");

                // These operations can run in background while loading screen is showing
                // Startup orchestration from App.xaml.cs will handle the main initialization

                if (Application.Current is App app && app.Services != null)
                {
                    // Validate theme health during loading using direct resource checks
                    try
                    {
                        var themeResource = Application.Current.TryFindResource("ContentForeground");
                        bool themeHealthy = themeResource != null;
                        Logger.Debug("Theme health validation during preload: {IsHealthy}", themeHealthy);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex, "Theme validation during preload failed");
                    }

                    // Could add other preload operations here like:
                    // - Cache validation
                    // - Resource pre-warming
                    // - Database connection checks
                }

                Logger.Debug("✅ LoadingView preload operations completed");
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "LoadingView preload operations had issues: {ErrorMessage}", ex.Message);
                // Don't throw - preload operations are optional optimizations
            }
        }

        /// <summary>
        /// Update progress with a specific message and percentage
        /// Can be called from external services to update loading progress
        /// </summary>
        public void UpdateProgress(string message, int percentage)
        {
            try
            {
                if (_loadingViewModel != null)
                {
                    _loadingViewModel.Status = message;
                    _loadingViewModel.ProgressPercentage = percentage;
                    _loadingViewModel.IsIndeterminate = false;

                    Logger.Debug("LoadingView progress updated: {Message} ({Percentage}%)", message, percentage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update LoadingView progress: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Set indeterminate progress with message
        /// </summary>
        public void SetIndeterminateProgress(string message)
        {
            try
            {
                if (_loadingViewModel != null)
                {
                    _loadingViewModel.Status = message;
                    _loadingViewModel.IsIndeterminate = true;

                    Logger.Debug("LoadingView set to indeterminate: {Message}", message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to set LoadingView indeterminate progress: {ErrorMessage}", ex.Message);
            }
        }
    }
}

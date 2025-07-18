using System;
using System.Windows;
using System.Windows.Controls;
using Syncfusion.UI.Xaml.Chat;
using Serilog;
using Serilog.Context;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Views.XAI
{
    /// <summary>
    /// 🚀 OPTIMIZED XAI CHAT VIEW - Performance-tuned AI assistant interface
    /// - Uses Syncfusion SfAIAssistView for native performance and theme integration
    /// - Implements lazy loading for chat history with background processing
    /// - Standardized 400px sidebar width with responsive height management
    /// - Debounced API calls using reactive patterns for optimization
    /// - Minimal loading states with efficient message rendering
    /// </summary>
    public partial class OptimizedXAIChatView : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<OptimizedXAIChatView>();
        private bool _isInitialized = false;

        public OptimizedXAIChatView()
        {
            try
            {
                using (LogContext.PushProperty("View", "OptimizedXAIChat"))
                using (LogContext.PushProperty("Operation", "Initialize"))
                {
                    Logger.Debug("Starting Optimized XAI Chat View initialization");

                    InitializeComponent();

                    // Set DataContext to optimized ViewModel
                    this.DataContext = App.ServiceProvider.GetRequiredService<BusBuddy.WPF.ViewModels.XAI.OptimizedXAIChatViewModel>();

                    // Subscribe to events for performance monitoring
                    this.Loaded += OptimizedXAIChatView_Loaded;
                    this.Unloaded += OptimizedXAIChatView_Unloaded;

                    Logger.Information("🚀 Optimized XAI Chat View initialized with performance optimizations");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize Optimized XAI Chat View");
            }
        }

        /// <summary>
        /// Handle view loaded event - Initialize lazy loading
        /// </summary>
        private async void OptimizedXAIChatView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized) return;

            try
            {
                Logger.Debug("Optimized XAI Chat View loaded - starting lazy initialization");

                var viewModel = this.DataContext as BusBuddy.WPF.ViewModels.XAI.OptimizedXAIChatViewModel;
                if (viewModel != null)
                {
                    // Initialize chat with lazy loading
                    await viewModel.InitializeLazyAsync();
                }

                _isInitialized = true;
                Logger.Information("✅ Optimized XAI Chat View lazy initialization completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed during XAI Chat View lazy initialization");
            }
        }

        /// <summary>
        /// Handle view unloaded event - Cleanup resources
        /// </summary>
        private void OptimizedXAIChatView_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Debug("Optimized XAI Chat View unloaded - cleaning up resources");

                var viewModel = this.DataContext as BusBuddy.WPF.ViewModels.XAI.OptimizedXAIChatViewModel;
                viewModel?.Cleanup();
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error during XAI Chat View cleanup");
            }
        }

    }
}

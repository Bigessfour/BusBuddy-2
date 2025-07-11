using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.Views.Activity
{
    public partial class ActivityLoggingView : UserControl
    {
        private readonly ILogger<ActivityLoggingView>? _logger;

        public ActivityLoggingView()
        {
            InitializeComponent();

            // Use DI to resolve the viewmodel if available
            if (Application.Current is App app && app.Services != null)
            {
                // Try to get the logger
                _logger = app.Services.GetService<ILogger<ActivityLoggingView>>();
                _logger?.LogInformation("ActivityLoggingView loaded");

                var vm = app.Services.GetService<ViewModels.ActivityLoggingViewModel>();
                if (vm != null)
                {
                    DataContext = vm;
                    _logger?.LogInformation("ActivityLoggingViewModel successfully set as DataContext");
                }
                else
                {
                    _logger?.LogWarning("ActivityLoggingViewModel could not be resolved from DI container");
                }
            }
        }
    }
}

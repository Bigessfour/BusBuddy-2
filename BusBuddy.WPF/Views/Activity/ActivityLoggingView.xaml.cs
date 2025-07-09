using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace BusBuddy.WPF.Views.Activity
{
    public partial class ActivityLoggingView : UserControl
    {
        public ActivityLoggingView()
        {
            InitializeComponent();

            // Use DI to resolve the viewmodel if available
            if (Application.Current is App app && app.Services != null)
            {
                var vm = app.Services.GetService<ViewModels.ActivityLogViewModel>();
                if (vm != null)
                {
                    DataContext = vm;
                }
            }
        }
    }
}

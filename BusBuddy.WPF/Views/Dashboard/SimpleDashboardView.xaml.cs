using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Views.Dashboard
{
    /// <summary>
    /// Simple Dashboard View with TabControl fallback
    /// Implements Development Plan Phase 6A fallback requirement
    /// </summary>
    public partial class SimpleDashboardView : UserControl
    {
        public SimpleDashboardView()
        {
            InitializeComponent();
            InitializeDataContext();
        }

        /// <summary>
        /// Initialize data context with DashboardViewModel from DI
        /// </summary>
        private void InitializeDataContext()
        {
            try
            {
                if (Application.Current is App app && app.Services != null)
                {
                    this.DataContext = app.Services.GetService<DashboardViewModel>();
                    System.Diagnostics.Debug.WriteLine("SimpleDashboardView: Successfully set DashboardViewModel from DI");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SimpleDashboardView: Error setting DataContext - {ex.Message}");
            }
        }
    }
}

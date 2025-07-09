using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Views.Maintenance
{
    public partial class MaintenanceTrackingView : UserControl
    {
        public MaintenanceTrackingView()
        {
            InitializeComponent();
            if (System.Windows.Application.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetService<BusBuddy.WPF.ViewModels.MaintenanceTrackingViewModel>();
            }
        }
    }
}

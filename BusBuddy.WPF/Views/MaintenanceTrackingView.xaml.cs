using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class MaintenanceTrackingView : UserControl
    {
        public MaintenanceTrackingView()
        {
            InitializeComponent();
            if (App.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetService(typeof(BusBuddy.WPF.ViewModels.MaintenanceTrackingViewModel));
            }
        }
    }
}

using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class MaintenanceTrackingView : UserControl
    {
        public MaintenanceTrackingView()
        {
            InitializeComponent();
            // For demo/testing: instantiate service and viewmodel directly. Replace with DI as needed.
            DataContext = new BusBuddy.WPF.ViewModels.MaintenanceTrackingViewModel(new BusBuddy.WPF.ViewModels.MaintenanceService());
        }
    }
}

using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class BusManagementView : UserControl
    {
        public BusManagementView()
        {
            InitializeComponent();
            // For demo/testing: instantiate service and viewmodel directly. Replace with DI as needed.
            DataContext = new BusBuddy.WPF.ViewModels.BusManagementViewModel(new BusBuddy.WPF.ViewModels.BusService());
        }
    }
}

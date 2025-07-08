using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class FuelManagementView : UserControl
    {
        public FuelManagementView()
        {
            InitializeComponent();
            // Use DI to resolve the real IFuelService and viewmodel
            var app = System.Windows.Application.Current as App;
            if (app?.Services != null)
            {
                var vm = app.Services.GetService(typeof(BusBuddy.WPF.ViewModels.FuelManagementViewModel)) as BusBuddy.WPF.ViewModels.FuelManagementViewModel;
                DataContext = vm;
            }
        }
    }
}

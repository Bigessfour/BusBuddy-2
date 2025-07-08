using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class BusManagementView : UserControl
    {
        public BusManagementView()
        {
            InitializeComponent();
            // Use DI to resolve the real IBusService and viewmodel
            var app = System.Windows.Application.Current as App;
            if (app?.Services != null)
            {
                var vm = app.Services.GetService(typeof(BusBuddy.WPF.ViewModels.BusManagementViewModel)) as BusBuddy.WPF.ViewModels.BusManagementViewModel;
                DataContext = vm;
            }
        }
    }
}

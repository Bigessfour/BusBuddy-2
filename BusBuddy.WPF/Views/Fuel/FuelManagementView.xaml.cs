using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace BusBuddy.WPF.Views.Fuel
{
    public partial class FuelManagementView : UserControl
    {
        public FuelManagementView()
        {
            InitializeComponent();

            // Use DI to resolve the real IFuelService and viewmodel
            if (Application.Current is App app && app.Services != null)
            {
                var vm = app.Services.GetRequiredService<BusBuddy.WPF.ViewModels.FuelManagementViewModel>();
                DataContext = vm;
            }
        }
    }
}

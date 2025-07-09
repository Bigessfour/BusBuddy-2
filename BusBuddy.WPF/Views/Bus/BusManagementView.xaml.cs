using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace BusBuddy.WPF.Views.Bus
{
    public partial class BusManagementView : UserControl
    {
        public BusManagementView()
        {
            InitializeComponent();
            // Use DI to resolve the real IBusService and viewmodel
            if (System.Windows.Application.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetService<BusManagementViewModel>();
            }
        }
    }
}

using BusBuddy.WPF;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace BusBuddy.WPF.Views.Driver
{
    public partial class DriverManagementView : UserControl
    {
        public DriverManagementView()
        {
            InitializeComponent();
            if (System.Windows.Application.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetRequiredService<DriverManagementViewModel>();
            }
        }
    }
}

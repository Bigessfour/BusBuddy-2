using BusBuddy.WPF;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace BusBuddy.WPF.Views.Route
{
    public partial class RouteManagementView : UserControl
    {
        public RouteManagementView()
        {
            InitializeComponent();
            if (System.Windows.Application.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetRequiredService<RouteManagementViewModel>();
            }
        }
    }
}

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
            var app = (App)System.Windows.Application.Current;
            if (app.Services != null)
            {
                DataContext = app.Services.GetRequiredService<RouteManagementViewModel>();
            }
        }
    }
}

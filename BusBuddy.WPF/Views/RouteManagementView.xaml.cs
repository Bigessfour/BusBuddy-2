using System.Windows;
using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class RouteManagementView : UserControl
    {
        public RouteManagementView()
        {
            InitializeComponent();
            var app = (App)System.Windows.Application.Current;
            if (app.Services != null)
            {
                DataContext = app.Services.GetService(typeof(ViewModels.RouteManagementViewModel));
            }
        }
    }
}

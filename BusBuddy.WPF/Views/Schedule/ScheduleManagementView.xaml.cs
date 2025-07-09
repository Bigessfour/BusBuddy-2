using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Views.Schedule
{
    public partial class ScheduleManagementView : UserControl
    {
        public ScheduleManagementView()
        {
            InitializeComponent();
            var app = (App)System.Windows.Application.Current;
            if (app.Services != null)
            {
                DataContext = app.Services.GetService<ViewModels.ScheduleManagementViewModel>();
            }
        }
    }
}

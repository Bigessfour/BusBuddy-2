using BusBuddy.WPF;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using BusBuddy.WPF.ViewModels.Schedule;

namespace BusBuddy.WPF.Views.Schedule
{
    public partial class ScheduleManagementView : UserControl
    {
        public ScheduleManagementView()
        {
            InitializeComponent();
            if (System.Windows.Application.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetRequiredService<ScheduleManagementViewModel>();
            }
        }
    }
}

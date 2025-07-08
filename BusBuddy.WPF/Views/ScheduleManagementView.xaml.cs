using BusBuddy.Core.Services;

using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class ScheduleManagementView : UserControl
    {
        public ScheduleManagementView()
        {
            InitializeComponent();
            var app = (App)System.Windows.Application.Current;
            if (app.Services != null)
            {
                DataContext = app.Services.GetService(typeof(ViewModels.ScheduleManagementViewModel));
            }
        }
    }
}

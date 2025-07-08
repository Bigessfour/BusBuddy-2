
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views
{
    public partial class ScheduleManagementView : UserControl
    {
        public ScheduleManagementView()
        {
            InitializeComponent();
            // Use DI to resolve the service and viewmodel
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var service = ((App)System.Windows.Application.Current).Services;
            var scheduleService = service.GetService(typeof(IScheduleService)) as IScheduleService;
            DataContext = new ScheduleManagementViewModel(scheduleService!);
#pragma warning restore CS8602
        }
    }
}

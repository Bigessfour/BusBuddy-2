using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.Services;

namespace BusBuddy.WPF.Views.Activity
{
    public partial class ActivityScheduleView : UserControl
    {
        private readonly DialogService _dialogService = new DialogService();

        public ActivityScheduleView()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.Activity.ActivityScheduleViewModel;
            if (vm != null)
            {
                vm.OpenScheduleDialog(false);
                var result = _dialogService.ShowActivityScheduleDialog(vm);
                if (result == true)
                {
                    _ = vm.LoadSchedulesAsync();
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.Activity.ActivityScheduleViewModel;
            if (vm != null)
            {
                vm.OpenScheduleDialog(true);
                var result = _dialogService.ShowActivityScheduleDialog(vm);
                if (result == true)
                {
                    _ = vm.LoadSchedulesAsync();
                }
            }
        }
    }
}

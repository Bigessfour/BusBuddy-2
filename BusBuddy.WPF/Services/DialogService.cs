using System.Windows;

namespace BusBuddy.WPF.Services
{
    public class DialogService
    {
        public bool? ShowActivityScheduleDialog(object dataContext)
        {
            var dialog = new Views.Activity.ActivityScheduleDialog
            {
                DataContext = dataContext,
                Owner = Application.Current?.MainWindow
            };
            return dialog.ShowDialog();
        }
    }
}

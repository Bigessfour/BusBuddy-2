using System.Windows;
using BusBuddy.WPF.ViewModels.Schedule;

namespace BusBuddy.WPF.Views.Schedule
{
    public partial class AddEditScheduleDialog : Window
    {
        public AddEditScheduleDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

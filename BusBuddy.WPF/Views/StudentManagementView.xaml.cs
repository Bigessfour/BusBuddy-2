using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class StudentManagementView : UserControl
    {
        public StudentManagementView()
        {
            InitializeComponent();
            var app = System.Windows.Application.Current as App;
            if (app?.Services != null)
            {
                DataContext = app.Services.GetService(typeof(BusBuddy.WPF.ViewModels.StudentManagementViewModel));
            }
        }
    }
}

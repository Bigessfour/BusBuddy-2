using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class StudentManagementView : UserControl
    {
        public StudentManagementView()
        {
            InitializeComponent();
            // Use DI to resolve the real IStudentService and viewmodel
            var app = System.Windows.Application.Current as App;
            if (app?.Services != null)
            {
                var studentService = app.Services.GetService(typeof(BusBuddy.Core.Services.IStudentService)) as BusBuddy.Core.Services.IStudentService;
                DataContext = new BusBuddy.WPF.ViewModels.StudentManagementViewModel(studentService!);
            }
        }
    }
}

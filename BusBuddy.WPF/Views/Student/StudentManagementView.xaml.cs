using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Views.Student
{
    public partial class StudentManagementView : UserControl
    {
        public StudentManagementView()
        {
            InitializeComponent();
            var app = System.Windows.Application.Current as App;
            if (app?.Services != null)
            {
                DataContext = app.Services.GetService<BusBuddy.WPF.ViewModels.StudentManagementViewModel>();
            }
        }
    }
}

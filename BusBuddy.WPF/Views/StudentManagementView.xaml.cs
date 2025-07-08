using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class StudentManagementView : UserControl
    {
        public StudentManagementView()
        {
            InitializeComponent();
            // For demo/testing: instantiate service and viewmodel directly. Replace with DI as needed.
            DataContext = new BusBuddy.WPF.ViewModels.StudentManagementViewModel(new BusBuddy.WPF.ViewModels.StudentService());
        }
    }
}

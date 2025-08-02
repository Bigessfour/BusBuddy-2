using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels.Student;

namespace BusBuddy.WPF.Views.Student
{
    /// <summary>
    /// Interaction logic for StudentForm.xaml
    /// </summary>
    public partial class StudentForm : Window
    {
        public StudentForm()
        {
            InitializeComponent();
            DataContext = new StudentFormViewModel();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is StudentFormViewModel viewModel)
            {
                // Handle save logic
                DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
        }
    }
}

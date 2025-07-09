using BusBuddy.WPF.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace BusBuddy.WPF.Views
{
    /// <summary>
    /// Interaction logic for StudentListView.xaml
    /// </summary>
    public partial class StudentListView : UserControl
    {
        public StudentListView()
        {
            InitializeComponent();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && DataContext is StudentListViewModel viewModel)
            {
                viewModel.OpenStudentDetailCommand.Execute(item.DataContext);
            }
        }
    }
}

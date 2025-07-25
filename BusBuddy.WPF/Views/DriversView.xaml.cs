using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views
{
    /// <summary>
    /// Phase 1 Drivers View - Simple and functional
    /// </summary>
    public partial class DriversView : UserControl
    {
        public DriversView()
        {
            InitializeComponent();
            var viewModel = new DriversViewModel();
            DataContext = viewModel;

            // Load data when view is created
            _ = viewModel.LoadDriversAsync();
        }
    }
}

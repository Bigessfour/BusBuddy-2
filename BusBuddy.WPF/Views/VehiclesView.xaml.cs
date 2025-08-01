using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views
{
    /// <summary>
    /// Phase 1 Vehicles View - Simple and functional
    /// </summary>
    public partial class VehiclesView : UserControl
    {
        public VehiclesView()
        {
            InitializeComponent();
            var viewModel = new VehiclesViewModel();
            DataContext = viewModel;

            // Load data when view is created
            _ = viewModel.LoadVehiclesAsync();
        }
    }
}

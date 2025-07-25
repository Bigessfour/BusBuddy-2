using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views
{
    /// <summary>
    /// Phase 1 Activity Schedule View - Simple and functional
    /// </summary>
    public partial class ActivityScheduleView : UserControl
    {
        public ActivityScheduleView()
        {
            InitializeComponent();
            var viewModel = new ActivityScheduleViewModel();
            DataContext = viewModel;

            // Load data when view is created
            _ = viewModel.LoadActivitiesAsync();
        }
    }
}

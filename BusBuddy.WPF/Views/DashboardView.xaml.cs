using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views
{
    /// <summary>
    /// Phase 1 Dashboard View - Simple and functional
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            var vm = new DashboardViewModel();
            DataContext = vm;
            Loaded += async (s, e) => await vm.LoadDashboardAsync();
        }
    }
}

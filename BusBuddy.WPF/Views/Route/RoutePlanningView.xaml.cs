using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views.Route
{
    /// <summary>
    /// Interaction logic for RoutePlanningView.xaml
    /// </summary>
    public partial class RoutePlanningView : UserControl
    {
        public RoutePlanningView()
        {
            InitializeComponent();
            // Note: DataContext should be set by the parent container or dependency injection
            // DataContext = new RoutePlanningViewModel();
        }
    }
}

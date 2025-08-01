using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace BusBuddy.WPF.Controls
{
    /// <summary>
    /// Interaction logic for QuickActionsPanel.xaml
    /// </summary>
    public partial class QuickActionsPanel : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<QuickActionsPanel>();

        public QuickActionsPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent() => throw new NotImplementedException();

        private void AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("Add vehicle action requested");
            MessageBox.Show("Add Vehicle functionality will be implemented in next phase.",
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddDriver_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("Add driver action requested");
            MessageBox.Show("Add Driver functionality will be implemented in next phase.",
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ScheduleRoute_Click(object sender, RoutedEventArgs e)
        {
            Logger.Information("Schedule route action requested");
            MessageBox.Show("Schedule Route functionality will be implemented in next phase.",
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

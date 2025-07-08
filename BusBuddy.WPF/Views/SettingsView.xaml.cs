using System.Windows.Controls;

namespace BusBuddy.WPF.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            if (System.Windows.Application.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetService(typeof(BusBuddy.WPF.ViewModels.SettingsViewModel));
            }
        }
    }
}

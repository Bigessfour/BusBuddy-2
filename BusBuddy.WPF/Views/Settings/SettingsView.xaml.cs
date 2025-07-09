using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace BusBuddy.WPF.Views.Settings
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            if (System.Windows.Application.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetRequiredService<SettingsViewModel>();
            }
        }
    }
}

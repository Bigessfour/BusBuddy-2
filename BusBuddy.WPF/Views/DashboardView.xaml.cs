using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly string _layoutFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BusBuddy", "docking-layout.xml");

        // dockingManager is defined in XAML and generated as a field by InitializeComponent

        public DashboardView()
        {
            InitializeComponent();
            // Set DataContext using DI with null checks to avoid CS8602
            var app = Application.Current as App;
            if (app?.Services != null)
            {
                DataContext = app.Services.GetService(typeof(BusBuddy.WPF.ViewModels.DashboardViewModel));
            }
            else
            {
                // Fallback: DataContext remains null or set to a default instance if desired
            }
            if (DataContext is DashboardViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DashboardViewModel.SelectedModule) && DataContext is DashboardViewModel viewModel)
            {
                // Activate the document by name (string) as required by Syncfusion DockingManager
                DockingManager.ActivateWindow(viewModel.SelectedModule);
            }
        }
    }
}

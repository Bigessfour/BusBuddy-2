using System;
using System.Windows.Controls;
using Serilog;
using BusBuddy.WPF.ViewModels;
using Serilog.Context;

namespace BusBuddy.WPF.Views.GoogleEarth
{
    /// <summary>
    /// Interaction logic for GoogleEarthView.xaml
    /// Google Earth integration for advanced geospatial mapping and route visualization
    /// </summary>
    public partial class GoogleEarthView : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<GoogleEarthView>();

        public GoogleEarthView()
        {
            using (LogContext.PushProperty("ViewInitialization", "GoogleEarthView"))
            {
                InitializeComponent();
                Logger.Information("Google Earth view initialized successfully");
            }
        }

        /// <summary>
        /// Handles map layer selection changes
        /// </summary>
        private void MapLayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is GoogleEarthViewModel viewModel && MapLayerComboBox.SelectedItem is Syncfusion.Windows.Tools.Controls.ComboBoxItemAdv selectedItem)
            {
                var layerType = selectedItem.Content?.ToString();
                if (!string.IsNullOrEmpty(layerType))
                {
                    using (LogContext.PushProperty("UserInteraction", "MapLayerChange"))
                    using (LogContext.PushProperty("LayerType", layerType))
                    {
                        viewModel.ChangeMapLayer(layerType);
                        Logger.Information("Map layer changed to: {LayerType}", layerType);
                    }
                }
            }
        }

        /// <summary>
        /// Handles data context changes to wire up view model events
        /// </summary>
        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is GoogleEarthViewModel viewModel)
            {
                using (LogContext.PushProperty("ViewModelBinding", "GoogleEarthViewModel"))
                {
                    // Wire up any view model events if needed
                    Logger.Debug("Google Earth view model connected successfully");
                }
            }
        }

        /// <summary>
        /// Override to handle data context changes
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            DataContextChanged += OnDataContextChanged;
        }
    }
}

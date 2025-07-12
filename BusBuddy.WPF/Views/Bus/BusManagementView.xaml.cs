using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using BusBuddy.Core.Models;

namespace BusBuddy.WPF.Views.Bus
{
    public partial class BusManagementView : UserControl
    {
        private readonly ILogger<BusManagementView>? _logger;
        private bool _isInitialized = false;

        // Define a static method to check if InitializeComponent exists
        private static readonly bool _initializeComponentExists =
            typeof(BusManagementView).GetMethod("InitializeComponent",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic) != null;

        public BusManagementView()
        {
            // First attempt to create a minimal UI directly in code
            // This ensures we have a functional UI even if XAML loading fails
            CreateMinimalUI();

            try
            {
                // Only try InitializeComponent if it exists
                if (_initializeComponentExists)
                {
                    // Using dynamic invoke for resilience
                    typeof(BusManagementView).GetMethod("InitializeComponent",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic)?.Invoke(this, null);

                    // If we got here, XAML loaded successfully, so remove the minimal UI
                    this.Content = null; // Clear the minimal UI
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("InitializeComponent method not found, using minimal UI");
                }
            }
            catch (Exception ex)
            {
                // Log the exception but continue with the minimal UI
                System.Diagnostics.Debug.WriteLine($"InitializeComponent failed: {ex.Message}");
            }

            // Use DI to resolve the real IBusService and viewmodel
            if (System.Windows.Application.Current is App app && app.Services != null)
            {
                var viewModel = app.Services.GetService<BusManagementViewModel>();
                DataContext = viewModel;
                _logger = app.Services.GetService<ILogger<BusManagementView>>();

                // If we're using the minimal UI, connect the DataGrid to the ViewModel
                if (!_initializeComponentExists || this.Content is Grid grid)
                {
                    ConnectMinimalUIToViewModel(viewModel);
                }
            }

            // Add handlers for the UserControl lifecycle
            Loaded += UserControl_Loaded;
            IsVisibleChanged += UserControl_IsVisibleChanged;
        }

        /// <summary>
        /// Handle the control being loaded
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _logger?.LogDebug("BusManagementView loaded");
            _isInitialized = true;
            RefreshDataIfVisible();
        }

        /// <summary>
        /// Handle visibility changes to refresh data when the panel becomes visible
        /// </summary>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                _logger?.LogDebug("BusManagementView became visible");
                RefreshDataIfVisible();
            }
        }

        /// <summary>
        /// Refresh data if the view is visible and initialized
        /// </summary>
        private void RefreshDataIfVisible()
        {
            if (_isInitialized && IsVisible && DataContext is BusManagementViewModel viewModel)
            {
                _logger?.LogInformation("Refreshing bus data because view became active");
                viewModel.LoadBusesCommand.Execute(null);
            }
        }

        /// <summary>
        /// Handles the header click event for column sorting
        /// </summary>
        private void OnHeaderClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is BusManagementViewModel viewModel && sender is GridHeaderCellControl headerCell)
            {
                var column = headerCell.Column;
                if (column != null)
                {
                    viewModel.SortCommand.Execute(column.MappingName);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles the page size selection change
        /// </summary>
        private void PageSize_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is BusManagementViewModel viewModel && sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is ComboBoxItem selectedItem &&
                    int.TryParse(selectedItem.Content.ToString(), out int pageSize))
                {
                    viewModel.PageSize = pageSize;
                    viewModel.CurrentPage = 1; // Reset to first page when changing page size
                    viewModel.LoadBusesCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Handles the selection changed event for the DataGrid
        /// </summary>
        private void DataGrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if (DataContext is BusManagementViewModel viewModel && sender is SfDataGrid dataGrid)
            {
                // The SelectedItem binding already updates the ViewModel's SelectedBus property
                // This method can be used for additional actions when selection changes

                // Enable/disable buttons based on selection
                if (viewModel.UpdateBusCommand is CommunityToolkit.Mvvm.Input.IRelayCommand updateCommand)
                    updateCommand.NotifyCanExecuteChanged();

                if (viewModel.DeleteBusCommand is CommunityToolkit.Mvvm.Input.IRelayCommand deleteCommand)
                    deleteCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Creates a minimal UI when XAML loading fails
        /// </summary>
        private void CreateMinimalUI()
        {
            // Create a minimal UI with a grid and message
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Add a header
            TextBlock header = new TextBlock
            {
                Text = "Bus Management",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(20, 20, 20, 15)
            };
            Grid.SetRow(header, 0);
            grid.Children.Add(header);

            // Add a data grid (basic, not Syncfusion)
            DataGrid dataGrid = new DataGrid
            {
                Margin = new Thickness(20),
                AutoGenerateColumns = true,
                IsReadOnly = true
            };
            Grid.SetRow(dataGrid, 1);
            grid.Children.Add(dataGrid);

            // Set the content of this UserControl
            this.Content = grid;
        }

        /// <summary>
        /// Connects the minimal UI to the ViewModel when XAML loading fails
        /// </summary>
        private void ConnectMinimalUIToViewModel(BusManagementViewModel? viewModel)
        {
            if (viewModel == null) return;

            if (this.Content is Grid grid)
            {
                // Find the DataGrid in our minimal UI
                DataGrid? dataGrid = grid.Children.OfType<DataGrid>().FirstOrDefault();
                if (dataGrid != null)
                {
                    // Create a binding for the DataGrid
                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding("Buses")
                    {
                        Mode = System.Windows.Data.BindingMode.OneWay
                    };
                    dataGrid.SetBinding(DataGrid.ItemsSourceProperty, binding);

                    // Set up selection changed event
                    dataGrid.SelectionChanged += (s, e) =>
                    {
                        if (dataGrid.SelectedItem != null)
                        {
                            viewModel.SelectedBus = (BusBuddy.Core.Models.Bus)dataGrid.SelectedItem;

                            // Refresh command states
                            if (viewModel.UpdateBusCommand is CommunityToolkit.Mvvm.Input.IRelayCommand updateCommand)
                                updateCommand.NotifyCanExecuteChanged();

                            if (viewModel.DeleteBusCommand is CommunityToolkit.Mvvm.Input.IRelayCommand deleteCommand)
                                deleteCommand.NotifyCanExecuteChanged();
                        }
                    };

                    // Add some basic columns that match our data
                    dataGrid.Columns.Clear();
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Bus Number", Binding = new System.Windows.Data.Binding("BusNumber") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Year", Binding = new System.Windows.Data.Binding("Year") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Make", Binding = new System.Windows.Data.Binding("Make") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Model", Binding = new System.Windows.Data.Binding("Model") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Capacity", Binding = new System.Windows.Data.Binding("SeatingCapacity") });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new System.Windows.Data.Binding("Status") });

                    // Add refresh and action buttons
                    StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(20, 10, 20, 20) };

                    Button refreshButton = new Button { Content = "Refresh", Margin = new Thickness(5, 5, 5, 5), Padding = new Thickness(15, 5, 15, 5) };
                    refreshButton.Click += (s, e) => viewModel.LoadBusesCommand.Execute(null);

                    Button addButton = new Button { Content = "Add New Bus", Margin = new Thickness(5, 5, 5, 5), Padding = new Thickness(15, 5, 15, 5) };
                    addButton.Click += (s, e) => viewModel.AddBusCommand.Execute(null);

                    Button editButton = new Button { Content = "Edit Selected", Margin = new Thickness(5, 5, 5, 5), Padding = new Thickness(15, 5, 15, 5) };
                    editButton.Click += (s, e) => viewModel.UpdateBusCommand.Execute(null);
                    System.Windows.Data.Binding canEditBinding = new System.Windows.Data.Binding("SelectedBus") { Converter = new BooleanConverter() };
                    editButton.SetBinding(Button.IsEnabledProperty, canEditBinding);

                    Button deleteButton = new Button { Content = "Delete", Margin = new Thickness(5, 5, 5, 5), Padding = new Thickness(15, 5, 15, 5), Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FFDDDD")) };
                    deleteButton.Click += (s, e) => viewModel.DeleteBusCommand.Execute(null);
                    System.Windows.Data.Binding canDeleteBinding = new System.Windows.Data.Binding("SelectedBus") { Converter = new BooleanConverter() };
                    deleteButton.SetBinding(Button.IsEnabledProperty, canDeleteBinding);

                    buttonPanel.Children.Add(refreshButton);
                    buttonPanel.Children.Add(addButton);
                    buttonPanel.Children.Add(editButton);
                    buttonPanel.Children.Add(deleteButton);

                    // Add a new row for buttons
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    Grid.SetRow(buttonPanel, 2);
                    grid.Children.Add(buttonPanel);

                    // Initial data load
                    viewModel.LoadBusesCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Simple converter to enable/disable buttons based on selection
        /// </summary>
        private class BooleanConverter : System.Windows.Data.IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value != null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}

using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using System.Windows.Controls;

namespace BusBuddy.WPF.Views.Bus
{
    public partial class BusManagementView : UserControl
    {
        public BusManagementView()
        {
            InitializeComponent();
            // Use DI to resolve the real IBusService and viewmodel
            if (System.Windows.Application.Current is App app && app.Services != null)
            {
                DataContext = app.Services.GetService<BusManagementViewModel>();
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
    }
}

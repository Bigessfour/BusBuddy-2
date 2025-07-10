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
    }
}

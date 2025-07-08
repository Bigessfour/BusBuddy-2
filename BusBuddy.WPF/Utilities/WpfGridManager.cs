using System.Windows;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.SfSkinManager;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// WPF Grid Manager - 100% Syncfusion SfDataGrid Compliant
    /// Reference: https://help.syncfusion.com/wpf/datagrid/overview
    /// </summary>
    public static class WpfGridManager
    {
        /// <summary>
        /// Configure SfDataGrid with official Syncfusion settings
        /// Per Syncfusion documentation: https://help.syncfusion.com/wpf/datagrid/getting-started
        /// </summary>
        public static void ConfigureDataGrid(SfDataGrid dataGrid)
        {
            // Official Syncfusion SfDataGrid configuration
            dataGrid.AutoGenerateColumns = false;
            dataGrid.AllowEditing = true;
            dataGrid.AllowSorting = true;
            dataGrid.AllowFiltering = true;
            dataGrid.ShowGroupDropArea = true;
            dataGrid.AllowGrouping = true;
            dataGrid.GridValidationMode = GridValidationMode.InEdit;
            dataGrid.SelectionMode = GridSelectionMode.Extended;
            dataGrid.NavigationMode = NavigationMode.Row;
            dataGrid.ColumnSizer = GridLengthUnitType.Auto;

            // Apply theme using SfSkinManager
            ApplyOffice2019Styling(dataGrid);
        }

        /// <summary>
        /// Apply Office2019Colorful theme to SfDataGrid
        /// Per Syncfusion documentation: https://help.syncfusion.com/wpf/themes/skin-manager#set-theme
        /// </summary>
        public static void ApplyOffice2019Styling(SfDataGrid dataGrid)
        {
            // CRITICAL: Set ApplyThemeAsDefaultStyle before theme application
            SfSkinManager.ApplyThemeAsDefaultStyle = true;

            // Apply theme using official Syncfusion method
            SfSkinManager.SetTheme(dataGrid, new Theme("Office2019Colorful"));
        }

        /// <summary>
        /// Configure SfDataGrid columns using official Syncfusion column types
        /// Per Syncfusion documentation: https://help.syncfusion.com/wpf/datagrid/column-types
        /// </summary>
        public static void ConfigureStandardColumns(SfDataGrid dataGrid)
        {
            // Clear existing columns
            dataGrid.Columns.Clear();

            // Add columns using official Syncfusion column types
            dataGrid.Columns.Add(new GridTextColumn
            {
                MappingName = "Id",
                HeaderText = "ID",
                Width = 80
            });

            dataGrid.Columns.Add(new GridTextColumn
            {
                MappingName = "Name",
                HeaderText = "Name",
                Width = 200
            });

            dataGrid.Columns.Add(new GridDateTimeColumn
            {
                MappingName = "CreatedDate",
                HeaderText = "Created Date",
                Width = 150
            });

            dataGrid.Columns.Add(new GridComboBoxColumn
            {
                MappingName = "Status",
                HeaderText = "Status",
                Width = 120
            });
        }
    }
}

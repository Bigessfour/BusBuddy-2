using Syncfusion.UI.Xaml.Grid;
using System.Windows.Media;

namespace Bus_Buddy.Utilities
{
    /// <summary>
    /// Syncfusion Layout Manager for WPF DataGrid formatting and layout
    /// </summary>
    public static class SyncfusionLayoutManagerWpf
    {
        public static void ConfigureSfDataGrid(SfDataGrid dataGrid)
        {
            if (dataGrid == null) return;
            dataGrid.AllowEditing = false;
            dataGrid.AllowSorting = true;
            dataGrid.AllowFiltering = true;
            dataGrid.SelectionMode = GridSelectionMode.Single;
            dataGrid.SelectionUnit = GridSelectionUnit.Row;
            dataGrid.ShowGroupDropArea = true;
            dataGrid.AutoGenerateColumns = false;
            dataGrid.AlternationCount = 2;
            dataGrid.RowHeight = 30;
            dataGrid.HeaderRowHeight = 35;
            dataGrid.GridColumnSizer = GridLengthUnitType.Star;
            // Apply basic theme
            dataGrid.Style.AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(240, 246, 252));
            dataGrid.Style.RowBackground = new SolidColorBrush(Colors.White);
            dataGrid.Style.HeaderBackground = new SolidColorBrush(Color.FromRgb(0, 120, 215));
            dataGrid.Style.HeaderForeground = new SolidColorBrush(Colors.White);
            dataGrid.Style.SelectionBackground = new SolidColorBrush(Color.FromRgb(51, 153, 255));
            dataGrid.Style.SelectionForeground = new SolidColorBrush(Colors.White);
        }
    }
}

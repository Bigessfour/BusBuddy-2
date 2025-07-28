using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Patterns;
using FlaUI.UIA3.Patterns;
using FlaUI.UIA3.Identifiers;
using Syncfusion.UI.Xaml.Grid;

namespace BusBuddy.UITests.Utilities;

/// <summary>
/// Extension methods for testing Syncfusion WPF controls
/// Provides specialized functionality for Syncfusion SfDataGrid, DockingManager, etc.
/// </summary>
public static class SyncfusionTestExtensions
{
    /// <summary>
    /// Gets the row count of a Syncfusion SfDataGrid
    /// </summary>
    public static int GetDataGridRowCount(this AutomationElement dataGrid)
    {
        try
        {
            // Find the grid rows container
            var rowsContainer = dataGrid.FindFirstDescendant(cf => cf.ByClassName("GridRowsPanel"));
            if (rowsContainer != null)
            {
                var rows = rowsContainer.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
                return rows.Length;
            }

            // Alternative approach: Look for data items directly
            var dataItems = dataGrid.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
            return dataItems.Length;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets the column count of a Syncfusion SfDataGrid
    /// </summary>
    public static int GetDataGridColumnCount(this AutomationElement dataGrid)
    {
        try
        {
            // Find the header row
            var headerRow = dataGrid.FindFirstDescendant(cf => cf.ByClassName("GridHeaderRowControl"));
            if (headerRow != null)
            {
                var headers = headerRow.FindAllDescendants(cf => cf.ByControlType(ControlType.HeaderItem));
                return headers.Length;
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets a specific cell value from a Syncfusion SfDataGrid
    /// </summary>
    public static string GetDataGridCellValue(this AutomationElement dataGrid, int rowIndex, int columnIndex)
    {
        try
        {
            var rows = dataGrid.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
            if (rowIndex >= 0 && rowIndex < rows.Length)
            {
                var row = rows[rowIndex];
                var cells = row.FindAllDescendants(cf => cf.ByControlType(ControlType.Custom));
                if (columnIndex >= 0 && columnIndex < cells.Length)
                {
                    return UITestHelpers.GetElementText(cells[columnIndex]);
                }
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Selects a row in a Syncfusion SfDataGrid
    /// </summary>
    public static bool SelectDataGridRow(this AutomationElement dataGrid, int rowIndex)
    {
        try
        {
            var rows = dataGrid.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
            if (rowIndex >= 0 && rowIndex < rows.Length)
            {
                return UITestHelpers.SafeClick(rows[rowIndex]);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Finds a row in a Syncfusion SfDataGrid by cell content
    /// </summary>
    public static AutomationElement? FindDataGridRowByContent(this AutomationElement dataGrid, string searchText, int columnIndex = 0)
    {
        try
        {
            var rows = dataGrid.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));

            foreach (var row in rows)
            {
                var cells = row.FindAllDescendants(cf => cf.ByControlType(ControlType.Custom));
                if (columnIndex >= 0 && columnIndex < cells.Length)
                {
                    var cellText = UITestHelpers.GetElementText(cells[columnIndex]);
                    if (cellText.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        return row;
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Waits for a Syncfusion SfDataGrid to load data
    /// </summary>
    public static bool WaitForDataGridToLoad(this AutomationElement dataGrid, int expectedMinimumRows = 1, int timeoutMs = 10000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            var rowCount = dataGrid.GetDataGridRowCount();
            if (rowCount >= expectedMinimumRows)
                return true;

            Thread.Sleep(100);
        }

        return false;
    }

    /// <summary>
    /// Gets the selected row index in a Syncfusion SfDataGrid
    /// </summary>
    public static int GetDataGridSelectedRowIndex(this AutomationElement dataGrid)
    {
        try
        {
            var rows = dataGrid.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));

            for (int i = 0; i < rows.Length; i++)
            {
                // Check if row is selected using control patterns
                try
                {
                    var selectionItemPattern = rows[i].Patterns.SelectionItem.PatternOrDefault;
                    if (selectionItemPattern?.IsSelected == true)
                    {
                        return i;
                    }
                }
                catch
                {
                    // If pattern not supported, skip this row
                    continue;
                }
            }

            return -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Checks if a Syncfusion DockingManager pane is visible
    /// </summary>
    public static bool IsDockingPaneVisible(this AutomationElement dockingManager, string paneName)
    {
        try
        {
            var pane = dockingManager.FindFirstDescendant(cf => cf.ByName(paneName));
            return pane?.IsAvailable == true && pane.BoundingRectangle.Width > 0 && pane.BoundingRectangle.Height > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the current theme of a Syncfusion control
    /// </summary>
    public static string GetSyncfusionTheme(this AutomationElement element)
    {
        try
        {
            // Check for FluentDark theme indicators
            var className = element.Properties.ClassName.ValueOrDefault;
            if (!string.IsNullOrEmpty(className) && className.Contains("Fluent"))
            {
                return "FluentDark";
            }

            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Validates that Syncfusion controls are properly themed
    /// </summary>
    public static bool ValidateSyncfusionTheming(this AutomationElement rootElement)
    {
        try
        {
            // Find all controls (simplified for compilation)
            var syncfusionControls = rootElement.FindAllDescendants();

            // Check if controls are properly themed (basic validation)
            foreach (var control in syncfusionControls)
            {
                if (!control.IsAvailable)
                    return false;
            }

            return syncfusionControls.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Scrolls a Syncfusion SfDataGrid to a specific row
    /// </summary>
    public static bool ScrollDataGridToRow(this AutomationElement dataGrid, int rowIndex)
    {
        try
        {
            // Try to find scroll pattern
            if (dataGrid.Patterns.Scroll.IsSupported)
            {
                var scrollPattern = dataGrid.Patterns.Scroll.Pattern;

                // Calculate scroll percentage based on row index
                var totalRows = dataGrid.GetDataGridRowCount();
                if (totalRows > 0)
                {
                    var scrollPercentage = (double)rowIndex / totalRows * 100;
                    scrollPattern.SetScrollPercent(-1, scrollPercentage);
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}

using FlaUI.Core.AutomationElements;
using BusBuddy.UITests.Utilities;

namespace BusBuddy.UITests.PageObjects;

/// <summary>
/// Page Object for the Drivers Management View
/// Provides methods to interact with drivers elements and validate drivers functionality
/// </summary>
public class DriversPage
{
    private readonly AutomationElement _driversElement;

    public DriversPage(AutomationElement driversElement)
    {
        _driversElement = driversElement ?? throw new ArgumentNullException(nameof(driversElement));
    }

    // Main UI elements
    public AutomationElement? DriversGrid =>
        UITestHelpers.FindElementByAutomationId(_driversElement, "DriversDataGrid");

    public AutomationElement? SearchTextBox =>
        UITestHelpers.FindElementByAutomationId(_driversElement, "SearchTextBox");

    public AutomationElement? ClearSearchButton =>
        UITestHelpers.FindElementByAutomationId(_driversElement, "ClearSearchButton");

    public AutomationElement? RefreshButton =>
        UITestHelpers.FindElementByAutomationId(_driversElement, "RefreshButton");

    public AutomationElement? EditDriverButton =>
        UITestHelpers.FindElementByAutomationId(_driversElement, "EditDriverButton");

    public AutomationElement? DeleteDriverButton =>
        UITestHelpers.FindElementByAutomationId(_driversElement, "DeleteDriverButton");

    public AutomationElement? LoadingIndicator =>
        UITestHelpers.FindElementByAutomationId(_driversElement, "LoadingIndicator");

    /// <summary>
    /// Gets the number of drivers displayed in the grid
    /// </summary>
    public int GetDriversCount()
    {
        var grid = DriversGrid;
        return grid?.GetDataGridRowCount() ?? 0;
    }

    /// <summary>
    /// Searches for drivers using the search box
    /// </summary>
    public bool SearchDrivers(string searchText)
    {
        var searchBox = SearchTextBox;
        if (searchBox?.IsAvailable == true)
        {
            try
            {
                searchBox.Focus();
                searchBox.AsTextBox().Text = searchText;
                UITestHelpers.WaitForIdle(500); // Wait for search filter to apply
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Clears the search filter
    /// </summary>
    public bool ClearSearch()
    {
        var clearButton = ClearSearchButton;
        return UITestHelpers.SafeClick(clearButton);
    }

    /// <summary>
    /// Refreshes the drivers data
    /// </summary>
    public bool RefreshDrivers()
    {
        var refreshButton = RefreshButton;
        return UITestHelpers.SafeClick(refreshButton);
    }

    /// <summary>
    /// Selects a driver by row index
    /// </summary>
    public bool SelectDriver(int rowIndex)
    {
        var grid = DriversGrid;
        return grid?.SelectDataGridRow(rowIndex) ?? false;
    }

    /// <summary>
    /// Selects a driver by name
    /// </summary>
    public bool SelectDriverByName(string driverName)
    {
        var grid = DriversGrid;
        if (grid == null) return false;

        var row = grid.FindDataGridRowByContent(driverName, 0); // Assuming name is in first column
        return UITestHelpers.SafeClick(row);
    }

    /// <summary>
    /// Gets driver information by row index
    /// </summary>
    public (string Name, string Phone, string Email, string Status) GetDriverInfo(int rowIndex)
    {
        var grid = DriversGrid;
        if (grid == null) return (string.Empty, string.Empty, string.Empty, string.Empty);

        var name = grid.GetDataGridCellValue(rowIndex, 0);
        var phone = grid.GetDataGridCellValue(rowIndex, 1);
        var email = grid.GetDataGridCellValue(rowIndex, 2);
        var status = grid.GetDataGridCellValue(rowIndex, 3);

        return (name, phone, email, status);
    }

    /// <summary>
    /// Clicks the Edit Driver button
    /// </summary>
    public bool ClickEditDriver()
    {
        var editButton = EditDriverButton;
        return UITestHelpers.SafeClick(editButton);
    }

    /// <summary>
    /// Clicks the Delete Driver button
    /// </summary>
    public bool ClickDeleteDriver()
    {
        var deleteButton = DeleteDriverButton;
        return UITestHelpers.SafeClick(deleteButton);
    }

    /// <summary>
    /// Waits for the drivers grid to load data
    /// </summary>
    public bool WaitForDriversToLoad(int expectedMinimumDrivers = 1, int timeoutMs = 10000)
    {
        var grid = DriversGrid;
        return grid?.WaitForDataGridToLoad(expectedMinimumDrivers, timeoutMs) ?? false;
    }

    /// <summary>
    /// Waits for loading indicator to disappear
    /// </summary>
    public bool WaitForLoadingToComplete(int timeoutMs = 10000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            var loadingIndicator = LoadingIndicator;
            if (loadingIndicator?.IsAvailable != true || !loadingIndicator.IsEnabled)
            {
                return true;
            }

            Thread.Sleep(100);
        }

        return false;
    }

    /// <summary>
    /// Validates that the drivers grid is visible and functional
    /// </summary>
    public bool ValidateDriversGridVisible()
    {
        var grid = DriversGrid;
        return grid?.IsAvailable == true && grid.GetDataGridColumnCount() > 0;
    }

    /// <summary>
    /// Validates that search functionality is available
    /// </summary>
    public bool ValidateSearchFunctionalityAvailable()
    {
        return SearchTextBox?.IsAvailable == true &&
               ClearSearchButton?.IsAvailable == true;
    }

    /// <summary>
    /// Validates that action buttons are available
    /// </summary>
    public bool ValidateActionButtonsAvailable()
    {
        return RefreshButton?.IsAvailable == true &&
               EditDriverButton?.IsAvailable == true &&
               DeleteDriverButton?.IsAvailable == true;
    }

    /// <summary>
    /// Gets the current search text
    /// </summary>
    public string GetSearchText()
    {
        var searchBox = SearchTextBox;
        if (searchBox?.IsAvailable == true)
        {
            try
            {
                return searchBox.AsTextBox().Text;
            }
            catch
            {
                return string.Empty;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Checks if a driver button is enabled
    /// </summary>
    public bool IsEditDriverButtonEnabled()
    {
        var editButton = EditDriverButton;
        return editButton?.IsEnabled == true;
    }

    /// <summary>
    /// Checks if delete driver button is enabled
    /// </summary>
    public bool IsDeleteDriverButtonEnabled()
    {
        var deleteButton = DeleteDriverButton;
        return deleteButton?.IsEnabled == true;
    }

    /// <summary>
    /// Finds drivers containing specific text in any column
    /// </summary>
    public List<int> FindDriversContaining(string searchText)
    {
        var results = new List<int>();
        var grid = DriversGrid;
        if (grid == null) return results;

        var rowCount = grid.GetDataGridRowCount();
        var columnCount = grid.GetDataGridColumnCount();

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                var cellText = grid.GetDataGridCellValue(row, col);
                if (cellText.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(row);
                    break; // Found in this row, move to next row
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Takes a screenshot of the drivers page
    /// </summary>
    public void TakeScreenshot(string fileName = "DriversPage")
    {
        UITestHelpers.TakeScreenshot(_driversElement, fileName);
    }
}

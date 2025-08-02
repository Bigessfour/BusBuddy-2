using FlaUI.Core.AutomationElements;
using BusBuddy.UITests.Utilities;

namespace BusBuddy.UITests.PageObjects;

/// <summary>
/// Page Object for the Dashboard View
/// Provides methods to interact with dashboard elements and validate dashboard state
/// </summary>
public class DashboardPage
{
    private readonly AutomationElement _dashboardElement;

    public DashboardPage(AutomationElement dashboardElement)
    {
        _dashboardElement = dashboardElement ?? throw new ArgumentNullException(nameof(dashboardElement));
    }

    // Dashboard metric elements
    public AutomationElement? TotalDriversElement =>
        UITestHelpers.FindElementByAutomationId(_dashboardElement, "TotalDriversText");

    public AutomationElement? TotalVehiclesElement =>
        UITestHelpers.FindElementByAutomationId(_dashboardElement, "TotalVehiclesText");

    public AutomationElement? TotalActivitiesElement =>
        UITestHelpers.FindElementByAutomationId(_dashboardElement, "TotalActivitiesText");

    public AutomationElement? ActiveDriversElement =>
        UITestHelpers.FindElementByAutomationId(_dashboardElement, "ActiveDriversText");

    // Recent activities grid
    public AutomationElement? RecentActivitiesGrid =>
        UITestHelpers.FindElementByAutomationId(_dashboardElement, "RecentActivitiesGrid");

    /// <summary>
    /// Gets the total drivers count displayed on dashboard
    /// </summary>
    public int GetTotalDriversCount()
    {
        var element = TotalDriversElement;
        var text = UITestHelpers.GetElementText(element);
        return int.TryParse(text, out var count) ? count : 0;
    }

    /// <summary>
    /// Gets the total vehicles count displayed on dashboard
    /// </summary>
    public int GetTotalVehiclesCount()
    {
        var element = TotalVehiclesElement;
        var text = UITestHelpers.GetElementText(element);
        return int.TryParse(text, out var count) ? count : 0;
    }

    /// <summary>
    /// Gets the total activities count displayed on dashboard
    /// </summary>
    public int GetTotalActivitiesCount()
    {
        var element = TotalActivitiesElement;
        var text = UITestHelpers.GetElementText(element);
        return int.TryParse(text, out var count) ? count : 0;
    }

    /// <summary>
    /// Gets the active drivers count displayed on dashboard
    /// </summary>
    public int GetActiveDriversCount()
    {
        var element = ActiveDriversElement;
        var text = UITestHelpers.GetElementText(element);
        return int.TryParse(text, out var count) ? count : 0;
    }

    /// <summary>
    /// Gets the number of recent activities displayed
    /// </summary>
    public int GetRecentActivitiesCount()
    {
        var grid = RecentActivitiesGrid;
        return grid?.GetDataGridRowCount() ?? 0;
    }

    /// <summary>
    /// Validates that all dashboard metrics are displayed
    /// </summary>
    public bool ValidateDashboardMetricsVisible()
    {
        return TotalDriversElement?.IsAvailable == true &&
               TotalVehiclesElement?.IsAvailable == true &&
               TotalActivitiesElement?.IsAvailable == true &&
               ActiveDriversElement?.IsAvailable == true;
    }

    /// <summary>
    /// Validates that dashboard metrics have reasonable values
    /// </summary>
    public bool ValidateDashboardMetricsValues()
    {
        var totalDrivers = GetTotalDriversCount();
        var totalVehicles = GetTotalVehiclesCount();
        var totalActivities = GetTotalActivitiesCount();
        var activeDrivers = GetActiveDriversCount();

        return totalDrivers >= 0 &&
               totalVehicles >= 0 &&
               totalActivities >= 0 &&
               activeDrivers >= 0 &&
               activeDrivers <= totalDrivers; // Active drivers should not exceed total
    }

    /// <summary>
    /// Waits for dashboard data to load
    /// </summary>
    public bool WaitForDashboardDataToLoad(int timeoutMs = 10000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            if (ValidateDashboardMetricsVisible() && ValidateDashboardMetricsValues())
            {
                return true;
            }

            Thread.Sleep(100);
        }

        return false;
    }

    /// <summary>
    /// Gets recent activity information by index
    /// </summary>
    public (string Date, string Activity, string Destination) GetRecentActivityInfo(int index)
    {
        var grid = RecentActivitiesGrid;
        if (grid == null)
        {
            return (string.Empty, string.Empty, string.Empty);
        }


        var date = grid.GetDataGridCellValue(index, 0);
        var activity = grid.GetDataGridCellValue(index, 1);
        var destination = grid.GetDataGridCellValue(index, 2);

        return (date, activity, destination);
    }

    /// <summary>
    /// Validates that recent activities grid is populated
    /// </summary>
    public bool ValidateRecentActivitiesVisible()
    {
        var grid = RecentActivitiesGrid;
        return grid?.IsAvailable == true && GetRecentActivitiesCount() > 0;
    }

    /// <summary>
    /// Takes a screenshot of the dashboard
    /// </summary>
    public void TakeScreenshot(string fileName = "Dashboard")
    {
        UITestHelpers.TakeScreenshot(_dashboardElement, fileName);
    }
}

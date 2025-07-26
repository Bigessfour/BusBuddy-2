using System.Windows;
using System.Windows.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Microsoft.EntityFrameworkCore;
using BusBuddy.Core.Data;
using BusBuddy.UITests.Builders;

namespace BusBuddy.UITests.Utilities;

/// <summary>
/// General UI testing utilities for BusBuddy application
/// Provides common functionality for UI automation tests
/// </summary>
public static class UITestHelpers
{
    private static UIA3Automation? _automation;

    /// <summary>
    /// Gets the UIA3 automation instance
    /// </summary>
    public static UIA3Automation Automation
    {
        get
        {
            _automation ??= new UIA3Automation();
            return _automation;
        }
    }

    /// <summary>
    /// Waits for the UI to become idle
    /// </summary>
    public static void WaitForIdle(int timeoutMs = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            Application.Current?.Dispatcher.Invoke(() => { }, DispatcherPriority.Background);
            Thread.Sleep(50);
        }
    }

    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    public static BusBuddyDbContext CreateInMemoryContext(string databaseName = "BusBuddyTestDb")
    {
        var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new BusBuddyDbContext(options);
    }

    /// <summary>
    /// Seeds test data into the in-memory database
    /// </summary>
    public static void SeedTestData(BusBuddyDbContext context)
    {
        // Clear existing data
        context.Drivers.RemoveRange(context.Drivers);
        context.Vehicles.RemoveRange(context.Vehicles);
        context.ActivitySchedules.RemoveRange(context.ActivitySchedules);

        // Add test drivers
        var drivers = DriverTestDataBuilder.CreateDriversList(5);
        context.Drivers.AddRange(drivers);

        // Add test vehicles
        var vehicles = VehicleTestDataBuilder.CreateVehicleFleet(3);
        context.Vehicles.AddRange(vehicles);

        // Add test activity schedules
        var activities = ActivityScheduleTestDataBuilder.CreateWeeklySchedule(7);
        context.ActivitySchedules.AddRange(activities);

        context.SaveChanges();
    }

    /// <summary>
    /// Finds an element by automation ID with retry logic
    /// </summary>
    public static AutomationElement? FindElementByAutomationId(AutomationElement parent, string automationId, int timeoutMs = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                var element = parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
                if (element != null)
                    return element;
            }
            catch
            {
                // Element not found, continue waiting
            }

            Thread.Sleep(100);
        }

        return null;
    }

    /// <summary>
    /// Finds an element by name with retry logic
    /// </summary>
    public static AutomationElement? FindElementByName(AutomationElement parent, string name, int timeoutMs = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                var element = parent.FindFirstDescendant(cf => cf.ByName(name));
                if (element != null)
                    return element;
            }
            catch
            {
                // Element not found, continue waiting
            }

            Thread.Sleep(100);
        }

        return null;
    }

    /// <summary>
    /// Takes a screenshot of the current window
    /// </summary>
    public static void TakeScreenshot(AutomationElement window, string fileName)
    {
        try
        {
            var screenshot = window.Capture();
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
            Directory.CreateDirectory(directory);

            var fullPath = Path.Combine(directory, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            screenshot.ToFile(fullPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to take screenshot: {ex.Message}");
        }
    }

    /// <summary>
    /// Simulates user typing with realistic delays
    /// </summary>
    public static void TypeText(string text, int delayBetweenCharsMs = 50)
    {
        foreach (char c in text)
        {
            SendKeys.SendWait(c.ToString());
            Thread.Sleep(delayBetweenCharsMs);
        }
    }

    /// <summary>
    /// Waits for an element to become available
    /// </summary>
    public static bool WaitForElement(Func<AutomationElement?> elementGetter, int timeoutMs = 10000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            var element = elementGetter();
            if (element != null && element.IsAvailable)
                return true;

            Thread.Sleep(100);
        }

        return false;
    }

    /// <summary>
    /// Safely clicks an element if it's available and enabled
    /// </summary>
    public static bool SafeClick(AutomationElement? element)
    {
        if (element?.IsAvailable == true && element.IsEnabled)
        {
            try
            {
                element.Click();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to click element: {ex.Message}");
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the text content of an element safely
    /// </summary>
    public static string GetElementText(AutomationElement? element)
    {
        if (element?.IsAvailable == true)
        {
            try
            {
                // Try different ways to get text
                if (!string.IsNullOrEmpty(element.Name))
                    return element.Name;

                if (element.Patterns.Text.IsSupported)
                    return element.Patterns.Text.Pattern.DocumentRange.GetText(-1);

                if (element.Patterns.Value.IsSupported)
                    return element.Patterns.Value.Pattern.Value;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get element text: {ex.Message}");
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Disposes automation resources
    /// </summary>
    public static void CleanupAutomation()
    {
        _automation?.Dispose();
        _automation = null;
    }
}

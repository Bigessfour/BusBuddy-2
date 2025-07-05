using NUnit.Framework;
using FluentAssertions;
using FluentAssertions.Execution;
using Bus_Buddy.Utilities;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms;
using Syncfusion.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BusBuddy.Tests.Infrastructure;

namespace BusBuddy.Tests.UnitTests.UI
{
    /// <summary>
    /// Comprehensive Syncfusion testing based on v30.1.37 documentation and best practices
    /// 
    /// TESTING APPROACH BASED ON RESEARCH:
    /// - Configuration matrix testing for all parameter combinations
    /// - Performance and memory leak testing
    /// - Visual consistency and theme validation
    /// - Error handling and edge cases
    /// - Accessibility and high-DPI support
    /// - Integration testing with real data scenarios
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)] // Required for Syncfusion Windows Forms controls
    [Category("SyncfusionComprehensive")]
    public class ComprehensiveSyncfusionTests : TestBase
    {
        private SfDataGrid _testDataGrid = null!;
        private MetroForm _testForm = null!;
        private List<IDisposable> _disposables = null!;

        [SetUp]
        public void Setup()
        {
            _disposables = new List<IDisposable>();
            _testDataGrid = new SfDataGrid();
            _testForm = new MetroForm
            {
                Size = new Size(1200, 800),
                Text = "Syncfusion Test Form"
            };

            _disposables.Add(_testDataGrid);
            _disposables.Add(_testForm);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var disposable in _disposables.Where(d => d != null))
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disposing test resource: {ex.Message}");
                }
            }
            _disposables.Clear();
        }

        #region Configuration Matrix Testing

        [Test]
        [TestCase(true, true, true, Description = "Full config: FullScreen + Enhancements + Grouping")]
        [TestCase(true, true, false, Description = "FullScreen + Enhancements")]
        [TestCase(true, false, true, Description = "FullScreen + Grouping")]
        [TestCase(true, false, false, Description = "FullScreen only")]
        [TestCase(false, true, true, Description = "Enhancements + Grouping")]
        [TestCase(false, true, false, Description = "Enhancements only")]
        [TestCase(false, false, true, Description = "Grouping only")]
        [TestCase(false, false, false, Description = "Minimal configuration")]
        public void SfDataGrid_ConfigurationMatrix_AllCombinationsSucceed(bool enableFullScreen, bool enableVisualEnhancements, bool enableGrouping)
        {
            // Act
            using (new AssertionScope())
            {
                System.Action configureAction = () =>
                {
                    SyncfusionLayoutManager.ConfigureSfDataGrid(_testDataGrid, enableFullScreen, enableVisualEnhancements);
                    if (!enableGrouping)
                    {
                        _testDataGrid.AllowGrouping = false;
                    }
                };

                // Assert - Configuration should always succeed
                configureAction.Should().NotThrow("all valid configuration combinations should work");

                // Verify core properties are always set correctly
                _testDataGrid.AllowEditing.Should().BeFalse();
                _testDataGrid.AllowSorting.Should().BeTrue();
                _testDataGrid.AllowFiltering.Should().BeTrue();
                _testDataGrid.AutoSizeColumnsMode.Should().Be(AutoSizeColumnsMode.Fill);

                // Verify conditional settings
                if (enableFullScreen)
                {
                    _testDataGrid.EnableDataVirtualization.Should().BeTrue();
                    _testDataGrid.Dock.Should().Be(DockStyle.Fill);
                }

                _testDataGrid.AllowGrouping.Should().Be(enableGrouping);
            }
        }

        #endregion

        #region Performance Testing

        [Test]
        [Timeout(5000)] // 5 second timeout for performance
        public void SfDataGrid_LargeDataConfiguration_CompletesInReasonableTime()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();
            var largeTestData = GenerateLargeTestDataSet(10000);

            // Act
            SyncfusionLayoutManager.ConfigureSfDataGrid(_testDataGrid, enableFullScreen: true, enableVisualEnhancements: true);
            _testDataGrid.DataSource = largeTestData;

            stopwatch.Stop();

            // Assert - Performance benchmarks
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000,
                "configuring grid with 10K records should complete within 2 seconds");

            _testDataGrid.EnableDataVirtualization.Should().BeTrue(
                "data virtualization should be enabled for large datasets");
        }

        [Test]
        public void SfDataGrid_MemoryUsage_DoesNotLeak()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            var gridsToTest = 50;

            // Act - Create and dispose multiple grids
            for (int i = 0; i < gridsToTest; i++)
            {
                using var grid = new SfDataGrid();
                SyncfusionLayoutManager.ConfigureSfDataGrid(grid, false, false);
                grid.DataSource = GenerateSmallTestDataSet();
            }

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = finalMemory - initialMemory;

            // Assert - Memory should not increase significantly
            memoryIncrease.Should().BeLessThan(10_000_000, // 10MB threshold
                "memory usage should not increase significantly after disposing grids");
        }

        #endregion

        #region Visual and Theme Testing

        [Test]
        public void MetroForm_ThemeApplication_ConfiguresCorrectly()
        {
            // Act
            SyncfusionUIEnhancer.ApplyOffice2016Theme(_testForm);

            // Assert - Theme properties should be set
            _testForm.MetroColor.Should().NotBe(Color.Empty);
            _testForm.CaptionBarColor.Should().NotBe(Color.Empty);
            _testForm.CaptionForeColor.Should().Be(Color.White);
            _testForm.Font.FontFamily.Name.Should().Be("Segoe UI");
        }

        [Test]
        public void SfDataGrid_StyleConsistency_MaintainsStandardAppearance()
        {
            // Act
            SyncfusionLayoutManager.ApplyGridStyling(_testDataGrid);

            // Assert - All style properties should follow Bus Buddy standards
            using (new AssertionScope())
            {
                _testDataGrid.Style.BorderColor.Should().Be(SyncfusionLayoutManager.GRID_BORDER_COLOR);
                _testDataGrid.Style.BorderStyle.Should().Be(BorderStyle.FixedSingle);

                // Header consistency
                _testDataGrid.Style.HeaderStyle.BackColor.Should().Be(SyncfusionLayoutManager.HEADER_BACKGROUND);
                _testDataGrid.Style.HeaderStyle.TextColor.Should().Be(SyncfusionLayoutManager.HEADER_TEXT_COLOR);
                _testDataGrid.Style.HeaderStyle.Font.Facename.Should().Be("Segoe UI");

                // Cell consistency
                _testDataGrid.Style.CellStyle.TextColor.Should().Be(SyncfusionLayoutManager.CELL_TEXT_COLOR);
                _testDataGrid.Style.CellStyle.Font.Facename.Should().Be("Segoe UI");
            }
        }

        [Test]
        public void VisualEnhancementManager_DiagnosticsReport_ProvidesValidInformation()
        {
            // Act
            var diagnostics = VisualEnhancementManager.GetVisualEnhancementDiagnostics(_testForm);

            // Assert
            diagnostics.Should().NotBeNullOrEmpty();
            diagnostics.Should().Contain("Form:");
            diagnostics.Should().Contain("Font:");
            diagnostics.Should().Contain("DPI:");
            diagnostics.Should().Contain("Syncfusion Controls:");
        }

        #endregion

        #region Error Handling and Edge Cases

        [Test]
        public void SyncfusionLayoutManager_NullInputHandling_ThrowsAppropriateExceptions()
        {
            // Test null grid parameter
            System.Action nullGridAction = () => SyncfusionLayoutManager.ConfigureSfDataGrid(null!, false, false);
            nullGridAction.Should().Throw<ArgumentNullException>();

            // Test null form parameter
            System.Action nullFormAction = () => SyncfusionLayoutManager.ConfigureFormForFullScreen(null!);
            nullFormAction.Should().Throw<ArgumentNullException>();

            // Test null panel parameter
            System.Action nullPanelAction = () => SyncfusionLayoutManager.ConfigureGradientPanel(null!);
            nullPanelAction.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void SfDataGrid_WithDisposedControl_HandlesGracefully()
        {
            // Arrange
            var disposableGrid = new SfDataGrid();
            SyncfusionLayoutManager.ConfigureSfDataGrid(disposableGrid, false, false);
            disposableGrid.Dispose();

            // Act & Assert - Accessing disposed control should not crash test infrastructure
            System.Action accessAction = () =>
            {
                var _ = disposableGrid.IsDisposed; // This should not throw
            };

            accessAction.Should().NotThrow("accessing IsDisposed property should be safe");
        }

        [Test]
        public void ColumnAlignment_EdgeCases_HandleGracefully()
        {
            // Test with empty string column name
            System.Action emptyNameAction = () => SyncfusionLayoutManager.ConfigureColumnAlignment(_testDataGrid, "", HorizontalAlignment.Left);
            emptyNameAction.Should().NotThrow("empty column name should be handled gracefully");

            // Test with null format
            System.Action nullFormatAction = () => SyncfusionLayoutManager.ConfigureColumnAlignment(_testDataGrid, "TestColumn", HorizontalAlignment.Left, null);
            nullFormatAction.Should().NotThrow("null format should be handled gracefully");

            // Test with extreme width values
            System.Action extremeWidthAction = () => SyncfusionLayoutManager.ConfigureColumnAlignment(_testDataGrid, "TestColumn", HorizontalAlignment.Left, null, 0);
            extremeWidthAction.Should().NotThrow("zero width should be handled gracefully");
        }

        #endregion

        #region Accessibility and High-DPI Testing

        [Test]
        public void SfDataGrid_AccessibilityProperties_AreConfiguredCorrectly()
        {
            // Act
            SyncfusionLayoutManager.ConfigureSfDataGrid(_testDataGrid, false, false);

            // Assert - Accessibility features
            _testDataGrid.NavigationMode.Should().Be(Syncfusion.WinForms.DataGrid.Enums.NavigationMode.Cell,
                "keyboard navigation should be enabled");
            _testDataGrid.ShowRowHeader.Should().BeFalse("row headers should be disabled for cleaner UI");
            _testDataGrid.ShowToolTip.Should().BeTrue("tooltips should be enabled for accessibility");
            _testDataGrid.ShowHeaderToolTip.Should().BeTrue("header tooltips should be enabled for accessibility");
        }

        [Test]
        public void FormConfiguration_HighDPISupport_IsEnabledCorrectly()
        {
            // Act
            SyncfusionLayoutManager.ConfigureFormForFullScreen(_testForm);
            SyncfusionUIEnhancer.ConfigureDPIAwareness(_testForm);

            // Assert - DPI awareness
            _testForm.AutoScaleMode.Should().Be(AutoScaleMode.Dpi, "DPI scaling should be enabled");
            _testForm.AutoScaleDimensions.Should().Be(new SizeF(96F, 96F), "standard DPI dimensions should be set");
            _testForm.Font.Name.Should().Be("Segoe UI", "should use DPI-friendly font");
        }

        #endregion

        #region Integration Testing

        [Test]
        public void BusManagementGrid_SpecificConfiguration_SetsCorrectProperties()
        {
            // Arrange
            var busData = GenerateBusTestData();

            // Act
            SyncfusionLayoutManager.ConfigureBusManagementGrid(_testDataGrid);
            _testDataGrid.DataSource = busData;

            // Assert - Bus-specific configuration
            _testDataGrid.AllowEditing.Should().BeFalse("bus data should be read-only");
            _testDataGrid.AllowSorting.Should().BeTrue("bus data should be sortable");
            _testDataGrid.AutoSizeColumnsMode.Should().Be(AutoSizeColumnsMode.Fill);
        }

        [Test]
        public void TicketManagementGrid_SpecificConfiguration_SetsCorrectProperties()
        {
            // Arrange
            var ticketData = GenerateTicketTestData();

            // Act
            SyncfusionLayoutManager.ConfigureTicketManagementGrid(_testDataGrid);
            _testDataGrid.DataSource = ticketData;

            // Assert - Ticket-specific configuration
            _testDataGrid.AllowEditing.Should().BeFalse("ticket data should be read-only");
            _testDataGrid.AllowSorting.Should().BeTrue("ticket data should be sortable");
            _testDataGrid.AutoSizeColumnsMode.Should().Be(AutoSizeColumnsMode.Fill);
        }

        #endregion

        #region Helper Methods

        private List<object> GenerateLargeTestDataSet(int count)
        {
            var data = new List<object>();
            for (int i = 0; i < count; i++)
            {
                data.Add(new
                {
                    Id = i,
                    Name = $"Test Item {i}",
                    Value = i * 10,
                    Date = DateTime.Now.AddDays(-i),
                    IsActive = i % 2 == 0
                });
            }
            return data;
        }

        private List<object> GenerateSmallTestDataSet()
        {
            return new List<object>
            {
                new { Id = 1, Name = "Test 1", Value = 100 },
                new { Id = 2, Name = "Test 2", Value = 200 },
                new { Id = 3, Name = "Test 3", Value = 300 }
            };
        }

        private List<object> GenerateBusTestData()
        {
            return new List<object>
            {
                new { VehicleId = 1, VehicleNumber = "BUS001", Capacity = 50, IsActive = true, LastMaintenance = DateTime.Now.AddDays(-30), Mileage = 15000 },
                new { VehicleId = 2, VehicleNumber = "BUS002", Capacity = 45, IsActive = true, LastMaintenance = DateTime.Now.AddDays(-15), Mileage = 22000 },
                new { VehicleId = 3, VehicleNumber = "BUS003", Capacity = 55, IsActive = false, LastMaintenance = DateTime.Now.AddDays(-60), Mileage = 8000 }
            };
        }

        private List<object> GenerateTicketTestData()
        {
            return new List<object>
            {
                new { Id = 1, TravelDate = DateTime.Now.AddDays(1), IssuedDate = DateTime.Now, Price = 25.50m, Status = "Active", PaymentMethod = "Credit Card" },
                new { Id = 2, TravelDate = DateTime.Now.AddDays(2), IssuedDate = DateTime.Now, Price = 30.00m, Status = "Active", PaymentMethod = "Cash" },
                new { Id = 3, TravelDate = DateTime.Now.AddDays(-1), IssuedDate = DateTime.Now.AddDays(-2), Price = 20.00m, Status = "Used", PaymentMethod = "Debit Card" }
            };
        }

        #endregion
    }
}

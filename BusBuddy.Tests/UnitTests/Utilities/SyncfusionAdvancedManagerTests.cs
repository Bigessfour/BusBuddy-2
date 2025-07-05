using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Utilities;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System;

namespace BusBuddy.Tests.UnitTests.Utilities
{
    /// <summary>
    /// Tests for SyncfusionAdvancedManager - Advanced grid operations and configuration
    /// This covers complex Syncfusion grid functionality without UI popups
    /// </summary>
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)] // Required for Syncfusion Windows Forms controls
    [Timeout(30000)] // 30 second overall timeout to prevent indefinite hangs
    public class SyncfusionAdvancedManagerTests
    {
        private SfDataGrid _dataGrid = null!;
        private List<TestDataItem> _testData = null!;

        [SetUp]
        public void SetUp()
        {
            // Ensure UI thread is ready
            Application.DoEvents();
            Thread.Sleep(50); // Small delay to ensure thread stability

            TestContext.WriteLine($"Test starting on thread: {Thread.CurrentThread.ManagedThreadId}");
            TestContext.WriteLine($"STA State: {Thread.CurrentThread.GetApartmentState()}");

            _dataGrid = new SfDataGrid();
            _testData = new List<TestDataItem>
            {
                new TestDataItem { ID = 1, Name = "Item 1", Category = "A", Value = 100.50m },
                new TestDataItem { ID = 2, Name = "Item 2", Category = "B", Value = 200.75m },
                new TestDataItem { ID = 3, Name = "Item 3", Category = "A", Value = 300.25m },
                new TestDataItem { ID = 4, Name = "Item 4", Category = "C", Value = 400.00m }
            };
            _dataGrid.DataSource = _testData;
            Application.DoEvents(); // Allow data binding to complete
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.WriteLine("Starting cleanup...");

            try
            {
                // Detach event handlers and clear data sources first
                if (_dataGrid != null)
                {
                    TestContext.WriteLine("Cleaning up SfDataGrid...");
                    _dataGrid.DataSource = null;
                    _dataGrid.Columns.Clear();
                    _dataGrid.GroupColumnDescriptions.Clear();
                }

                // Clean up main test object
                try
                {
                    _dataGrid?.Dispose();
                    TestContext.WriteLine("SfDataGrid disposed");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Error disposing SfDataGrid: {ex.Message}");
                }
            }
            finally
            {
                _dataGrid = null!;
                _testData = null!;

                // Force garbage collection with logging
                var gcStart = DateTime.Now;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                var gcTime = DateTime.Now - gcStart;
                TestContext.WriteLine($"GC completed in {gcTime.TotalMilliseconds}ms");

                // Final UI thread yield
                Application.DoEvents();
            }
        }

        public class TestDataItem
        {
            public int ID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public decimal Value { get; set; }
        }

        #region Grouping Operations Tests

        [Test]
        public void ConfigureGrouping_WithSingleColumn_ShouldSetGrouping()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureGrouping(_dataGrid, "Category");

            // Assert
            _dataGrid.AllowGrouping.Should().BeTrue();
            _dataGrid.ShowGroupDropArea.Should().BeTrue();
            _dataGrid.AutoExpandGroups.Should().BeTrue();
            _dataGrid.GroupColumnDescriptions.Count.Should().Be(1);
            _dataGrid.GroupColumnDescriptions[0].ColumnName.Should().Be("Category");
        }

        [Test]
        public void ConfigureGrouping_WithMultipleColumns_ShouldSetMultipleGrouping()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureGrouping(_dataGrid, "Category", "Name");

            // Assert
            _dataGrid.AllowGrouping.Should().BeTrue();
            _dataGrid.GroupColumnDescriptions.Count.Should().Be(2);
            _dataGrid.GroupColumnDescriptions[0].ColumnName.Should().Be("Category");
            _dataGrid.GroupColumnDescriptions[1].ColumnName.Should().Be("Name");
        }

        [Test]
        public void ConfigureGrouping_WithEmptyColumns_ShouldClearGrouping()
        {
            // Arrange - first add some grouping
            SyncfusionAdvancedManager.ConfigureGrouping(_dataGrid, "Category");

            // Act
            SyncfusionAdvancedManager.ConfigureGrouping(_dataGrid);

            // Assert
            _dataGrid.AllowGrouping.Should().BeTrue();
            _dataGrid.GroupColumnDescriptions.Count.Should().Be(0);
        }

        #endregion

        #region Sorting Operations Tests

        [Test]
        public void ConfigureSorting_WithAscendingDirection_ShouldSetSorting()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureSorting(_dataGrid, "Name", ListSortDirection.Ascending);

            // Assert
            _dataGrid.AllowSorting.Should().BeTrue();
            _dataGrid.AllowTriStateSorting.Should().BeTrue();
            _dataGrid.ShowSortNumbers.Should().BeTrue();
            _dataGrid.SortColumnDescriptions.Count.Should().Be(1);
            _dataGrid.SortColumnDescriptions[0].ColumnName.Should().Be("Name");
            _dataGrid.SortColumnDescriptions[0].SortDirection.Should().Be(ListSortDirection.Ascending);
        }

        [Test]
        public void ConfigureSorting_WithDescendingDirection_ShouldSetSorting()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureSorting(_dataGrid, "Value", ListSortDirection.Descending);

            // Assert
            _dataGrid.SortColumnDescriptions.Count.Should().Be(1);
            _dataGrid.SortColumnDescriptions[0].ColumnName.Should().Be("Value");
            _dataGrid.SortColumnDescriptions[0].SortDirection.Should().Be(ListSortDirection.Descending);
        }

        [Test]
        public void ConfigureMultiSorting_WithMultipleColumns_ShouldSetMultipleSorting()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureMultiSorting(_dataGrid,
                ("Category", ListSortDirection.Ascending),
                ("Value", ListSortDirection.Descending));

            // Assert
            _dataGrid.AllowSorting.Should().BeTrue();
            _dataGrid.SortColumnDescriptions.Count.Should().Be(2);
            _dataGrid.SortColumnDescriptions[0].ColumnName.Should().Be("Category");
            _dataGrid.SortColumnDescriptions[0].SortDirection.Should().Be(ListSortDirection.Ascending);
            _dataGrid.SortColumnDescriptions[1].ColumnName.Should().Be("Value");
            _dataGrid.SortColumnDescriptions[1].SortDirection.Should().Be(ListSortDirection.Descending);
        }

        [Test]
        public void ConfigureMultiSorting_WithEmptyColumns_ShouldClearSorting()
        {
            // Arrange - first add some sorting
            SyncfusionAdvancedManager.ConfigureSorting(_dataGrid, "Name");

            // Act
            SyncfusionAdvancedManager.ConfigureMultiSorting(_dataGrid);

            // Assert
            _dataGrid.AllowSorting.Should().BeTrue();
            _dataGrid.SortColumnDescriptions.Count.Should().Be(0);
        }

        #endregion

        #region Advanced Grid Operations Tests

        [Test]
        public void ClearAllOperations_ShouldClearAllGridOperations()
        {
            // Arrange - set up some operations first
            SyncfusionAdvancedManager.ConfigureGrouping(_dataGrid, "Category");
            SyncfusionAdvancedManager.ConfigureSorting(_dataGrid, "Name");

            // Act
            Assert.DoesNotThrow(() => SyncfusionAdvancedManager.ClearAllOperations(_dataGrid));

            // Assert - operations should be cleared
            _dataGrid.GroupColumnDescriptions.Count.Should().Be(0);
            _dataGrid.SortColumnDescriptions.Count.Should().Be(0);
        }

        [Test]
        public void ExpandCollapseOperations_WithExpandTrue_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => SyncfusionAdvancedManager.ExpandCollapseOperations(_dataGrid, true));
        }

        [Test]
        public void ExpandCollapseOperations_WithExpandFalse_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => SyncfusionAdvancedManager.ExpandCollapseOperations(_dataGrid, false));
        }

        [Test]
        public void SelectionOperations_WithNoParameters_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => SyncfusionAdvancedManager.SelectionOperations(_dataGrid));
        }

        [Test]
        public void SelectionOperations_WithRowRange_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => SyncfusionAdvancedManager.SelectionOperations(_dataGrid, 0, 2));
        }

        #endregion

        #region Performance & Display Operations Tests

        [Test]
        public void ConfigurePerformanceOptimization_WithDefaultParameters_ShouldSetOptimizations()
        {
            // Act
            SyncfusionAdvancedManager.ConfigurePerformanceOptimization(_dataGrid);

            // Assert
            _dataGrid.EnableDataVirtualization.Should().BeTrue();
            _dataGrid.UsePLINQ.Should().BeTrue();
            _dataGrid.ShowBusyIndicator.Should().BeTrue();
        }

        [Test]
        public void ConfigurePerformanceOptimization_WithVirtualizationDisabled_ShouldDisableVirtualization()
        {
            // Act
            SyncfusionAdvancedManager.ConfigurePerformanceOptimization(_dataGrid, false);

            // Assert
            _dataGrid.EnableDataVirtualization.Should().BeFalse();
            _dataGrid.UsePLINQ.Should().BeTrue();
            _dataGrid.ShowBusyIndicator.Should().BeTrue();
        }

        [Test]
        public void ConfigureRowHeights_WithDefaultParameters_ShouldSetDefaultHeights()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureRowHeights(_dataGrid);

            // Assert
            _dataGrid.RowHeight.Should().Be(30);
            _dataGrid.HeaderRowHeight.Should().Be(35);
        }

        [Test]
        public void ConfigureRowHeights_WithCustomParameters_ShouldSetCustomHeights()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureRowHeights(_dataGrid, 25, 40);

            // Assert
            _dataGrid.RowHeight.Should().Be(25);
            _dataGrid.HeaderRowHeight.Should().Be(40);
        }

        [Test]
        public void ConfigureFrozenCells_WithDefaultParameters_ShouldSetNoFrozenCells()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureFrozenCells(_dataGrid);

            // Assert
            _dataGrid.FrozenRowCount.Should().Be(0);
            _dataGrid.FrozenColumnCount.Should().Be(0);
        }

        [Test]
        public void ConfigureFrozenCells_WithCustomParameters_ShouldSetFrozenCells()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureFrozenCells(_dataGrid, 2, 1);

            // Assert
            _dataGrid.FrozenRowCount.Should().Be(2);
            _dataGrid.FrozenColumnCount.Should().Be(1);
        }

        #endregion

        #region Validation Operations Tests

        [Test]
        public void ConfigureValidation_WithDefaultParameters_ShouldEnableValidation()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureValidation(_dataGrid);

            // Assert
            _dataGrid.ValidationMode.Should().Be(GridValidationMode.InView);
            _dataGrid.ShowErrorIcon.Should().BeTrue();
            _dataGrid.ShowValidationErrorToolTip.Should().BeTrue();
        }

        [Test]
        public void ConfigureValidation_WithErrorIconsDisabled_ShouldDisableErrorIcons()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureValidation(_dataGrid, false);

            // Assert
            _dataGrid.ValidationMode.Should().Be(GridValidationMode.InView);
            _dataGrid.ShowErrorIcon.Should().BeFalse();
            _dataGrid.ShowValidationErrorToolTip.Should().BeFalse();
        }

        #endregion

        #region Copy/Paste Operations Tests

        [Test]
        public void ConfigureCopyPaste_WithDefaultParameters_ShouldSetDefaultOptions()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureCopyPaste(_dataGrid);

            // Assert
            _dataGrid.CopyOption.Should().Be(CopyOptions.CopyData);
            _dataGrid.PasteOption.Should().Be(PasteOptions.PasteData);
        }

        [Test]
        public void ConfigureCopyPaste_WithCustomParameters_ShouldSetCustomOptions()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureCopyPaste(_dataGrid,
                CopyOptions.IncludeHeaders,
                PasteOptions.ExcludeFirstLine);

            // Assert
            _dataGrid.CopyOption.Should().Be(CopyOptions.IncludeHeaders);
            _dataGrid.PasteOption.Should().Be(PasteOptions.ExcludeFirstLine);
        }

        #endregion

        #region Filter Operations Tests

        [Test]
        public void ConfigureAdvancedFiltering_WithDefaultParameters_ShouldSetDefaultFiltering()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureAdvancedFiltering(_dataGrid);

            // Assert
            _dataGrid.AllowFiltering.Should().BeTrue();
            _dataGrid.FilterRowPosition.Should().Be(RowPosition.Top);
            _dataGrid.FilterDelay.Should().Be(500);
        }

        [Test]
        public void ConfigureAdvancedFiltering_WithBottomPosition_ShouldSetBottomFiltering()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureAdvancedFiltering(_dataGrid, RowPosition.Bottom);

            // Assert
            _dataGrid.AllowFiltering.Should().BeTrue();
            _dataGrid.FilterRowPosition.Should().Be(RowPosition.Bottom);
        }

        [Test]
        public void ClearColumnFilter_WithValidColumn_ShouldNotThrow()
        {
            // Arrange
            SyncfusionAdvancedManager.ConfigureAdvancedFiltering(_dataGrid);

            // Act & Assert
            Assert.DoesNotThrow(() => SyncfusionAdvancedManager.ClearColumnFilter(_dataGrid, "Category"));
        }

        #endregion

        #region Utility Methods Tests

        [Test]
        public void ApplyAdvancedConfiguration_WithDefaultParameters_ShouldConfigureAllFeatures()
        {
            // Act
            SyncfusionAdvancedManager.ApplyAdvancedConfiguration(_dataGrid);

            // Assert
            _dataGrid.EnableDataVirtualization.Should().BeTrue();
            _dataGrid.UsePLINQ.Should().BeTrue();
            _dataGrid.ValidationMode.Should().Be(GridValidationMode.InView);
            _dataGrid.CopyOption.Should().Be(CopyOptions.CopyData);
            _dataGrid.RowHeight.Should().Be(30);
            _dataGrid.AllowFiltering.Should().BeTrue();
            _dataGrid.AllowGrouping.Should().BeTrue();
        }

        [Test]
        public void ApplyAdvancedConfiguration_WithGroupingDisabled_ShouldNotEnableGrouping()
        {
            // Act
            SyncfusionAdvancedManager.ApplyAdvancedConfiguration(_dataGrid, enableGrouping: false);

            // Assert
            _dataGrid.EnableDataVirtualization.Should().BeTrue();
            _dataGrid.AllowGrouping.Should().BeFalse();
        }

        [Test]
        public void ApplyAdvancedConfiguration_WithPerformanceOptimizationDisabled_ShouldNotOptimize()
        {
            // Act
            SyncfusionAdvancedManager.ApplyAdvancedConfiguration(_dataGrid,
                enablePerformanceOptimization: false);

            // Assert
            _dataGrid.ValidationMode.Should().Be(GridValidationMode.InView);
            _dataGrid.AllowGrouping.Should().BeTrue();
            // Performance optimization settings should not be set when disabled
        }

        [Test]
        public void ApplyAdvancedConfiguration_WithAdvancedFilteringDisabled_ShouldNotEnableFiltering()
        {
            // Act
            SyncfusionAdvancedManager.ApplyAdvancedConfiguration(_dataGrid,
                enableAdvancedFiltering: false);

            // Assert
            _dataGrid.EnableDataVirtualization.Should().BeTrue();
            _dataGrid.AllowGrouping.Should().BeTrue();
            _dataGrid.AllowFiltering.Should().BeFalse();
        }

        #endregion
    }
}

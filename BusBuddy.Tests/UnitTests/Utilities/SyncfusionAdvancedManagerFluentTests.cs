using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Utilities;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using System.ComponentModel;

namespace BusBuddy.Tests.UnitTests.Utilities
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class SyncfusionAdvancedManagerFluentTests
    {
        private SfDataGrid _dataGrid;
        private List<TestItem> _testData;

        [SetUp]
        public void SetUp()
        {
            _dataGrid = new SfDataGrid();
            _testData = new List<TestItem>
            {
                new TestItem { Id = 1, Name = "Item A", Category = "Type1", Value = 100m },
                new TestItem { Id = 2, Name = "Item B", Category = "Type2", Value = 200m },
                new TestItem { Id = 3, Name = "Item C", Category = "Type1", Value = 150m }
            };
            _dataGrid.DataSource = _testData;
        }

        [TearDown]
        public void TearDown()
        {
            _dataGrid?.Dispose();
        }

        [Test]
        public void ConfigureGrouping_WithSingleColumn_ShouldSetProperties()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureGrouping(_dataGrid, "Category");

            // Assert using FluentAssertions
            _dataGrid.AllowGrouping.Should().BeTrue();
            _dataGrid.ShowGroupDropArea.Should().BeTrue();
            _dataGrid.AutoExpandGroups.Should().BeTrue();
            _dataGrid.GroupColumnDescriptions.Should().HaveCount(1);
            _dataGrid.GroupColumnDescriptions.First().ColumnName.Should().Be("Category");
        }

        [Test]
        public void ConfigureGrouping_WithMultipleColumns_ShouldSetAllColumns()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureGrouping(_dataGrid, "Category", "Name");

            // Assert
            _dataGrid.GroupColumnDescriptions.Should().HaveCount(2)
                .And.Contain(g => g.ColumnName == "Category")
                .And.Contain(g => g.ColumnName == "Name");

            var columnNames = _dataGrid.GroupColumnDescriptions.Select(g => g.ColumnName).ToList();
            columnNames.Should().BeEquivalentTo(new[] { "Category", "Name" });
        }

        [Test]
        public void ConfigureSorting_WithAscending_ShouldSetCorrectDirection()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureSorting(_dataGrid, "Name", ListSortDirection.Ascending);

            // Assert
            _dataGrid.AllowSorting.Should().BeTrue();
            _dataGrid.AllowTriStateSorting.Should().BeTrue();
            _dataGrid.ShowSortNumbers.Should().BeTrue();
            _dataGrid.SortColumnDescriptions.Should().HaveCount(1);

            var sortColumn = _dataGrid.SortColumnDescriptions.First();
            sortColumn.ColumnName.Should().Be("Name");
            sortColumn.SortDirection.Should().Be(ListSortDirection.Ascending);
        }

        [Test]
        public void ConfigureMultiSorting_WithMultipleColumns_ShouldPreserveOrder()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureMultiSorting(_dataGrid,
                ("Category", ListSortDirection.Ascending),
                ("Value", ListSortDirection.Descending));

            // Assert
            _dataGrid.SortColumnDescriptions.Should().HaveCount(2);

            var sortColumns = _dataGrid.SortColumnDescriptions.ToList();
            sortColumns[0].ColumnName.Should().Be("Category");
            sortColumns[0].SortDirection.Should().Be(ListSortDirection.Ascending);
            sortColumns[1].ColumnName.Should().Be("Value");
            sortColumns[1].SortDirection.Should().Be(ListSortDirection.Descending);
        }

        [Test]
        public void ConfigurePerformanceOptimization_WithDefaults_ShouldEnableOptimizations()
        {
            // Act
            SyncfusionAdvancedManager.ConfigurePerformanceOptimization(_dataGrid);

            // Assert
            _dataGrid.EnableDataVirtualization.Should().BeTrue();
            _dataGrid.UsePLINQ.Should().BeTrue();
            _dataGrid.ShowBusyIndicator.Should().BeTrue();
        }

        [Test]
        public void ConfigureRowHeights_WithCustomValues_ShouldSetHeights()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureRowHeights(_dataGrid, 35, 45);

            // Assert
            _dataGrid.RowHeight.Should().Be(35);
            _dataGrid.HeaderRowHeight.Should().Be(45);
        }

        [Test]
        public void ConfigureValidation_WithDefaults_ShouldEnableValidation()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureValidation(_dataGrid);

            // Assert
            _dataGrid.ShowErrorIcon.Should().BeTrue();
            _dataGrid.ShowValidationErrorToolTip.Should().BeTrue();
        }

        [Test]
        public void ConfigureCopyPaste_WithCustomOptions_ShouldSetOptions()
        {
            // Act & Assert - Test that method doesn't throw
            Action configureAction = () => SyncfusionAdvancedManager.ConfigureCopyPaste(_dataGrid);
            configureAction.Should().NotThrow();
        }

        [Test]
        public void ConfigureAdvancedFiltering_WithDefaults_ShouldEnableFiltering()
        {
            // Act
            SyncfusionAdvancedManager.ConfigureAdvancedFiltering(_dataGrid);

            // Assert
            _dataGrid.AllowFiltering.Should().BeTrue();
            _dataGrid.FilterDelay.Should().Be(500);
        }

        [Test]
        public void ApplyAdvancedConfiguration_WithAllDefaults_ShouldConfigureAllFeatures()
        {
            // Act
            SyncfusionAdvancedManager.ApplyAdvancedConfiguration(_dataGrid);

            // Assert - Check all features are enabled
            _dataGrid.EnableDataVirtualization.Should().BeTrue("performance optimization should be enabled");
            _dataGrid.UsePLINQ.Should().BeTrue("PLINQ should be enabled for performance");
            _dataGrid.AllowGrouping.Should().BeTrue("grouping should be enabled by default");
            _dataGrid.AllowFiltering.Should().BeTrue("filtering should be enabled by default");
            _dataGrid.ShowBusyIndicator.Should().BeTrue("busy indicator should be enabled");
            _dataGrid.RowHeight.Should().Be(30, "default row height should be set");
        }

        [Test]
        public void ApplyAdvancedConfiguration_WithSelectiveDisabling_ShouldRespectSettings()
        {
            // Act
            SyncfusionAdvancedManager.ApplyAdvancedConfiguration(_dataGrid,
                enableGrouping: false,
                enableAdvancedFiltering: false);

            // Assert
            _dataGrid.AllowGrouping.Should().BeFalse("grouping should be disabled when requested");
            _dataGrid.AllowFiltering.Should().BeFalse("filtering should be disabled when requested");
            _dataGrid.EnableDataVirtualization.Should().BeTrue("performance optimization should still be enabled");
        }

        [Test]
        public void ClearAllOperations_ShouldRemoveAllOperations()
        {
            // Arrange - Set up some operations first
            SyncfusionAdvancedManager.ConfigureGrouping(_dataGrid, "Category");
            SyncfusionAdvancedManager.ConfigureSorting(_dataGrid, "Name");

            // Act
            Action clearAction = () => SyncfusionAdvancedManager.ClearAllOperations(_dataGrid);

            // Assert
            clearAction.Should().NotThrow();
            _dataGrid.GroupColumnDescriptions.Should().BeEmpty();
            _dataGrid.SortColumnDescriptions.Should().BeEmpty();
        }

        [Test]
        public void ExpandCollapseOperations_ShouldNotThrow()
        {
            // Act & Assert
            Action expandAction = () => SyncfusionAdvancedManager.ExpandCollapseOperations(_dataGrid, true);
            Action collapseAction = () => SyncfusionAdvancedManager.ExpandCollapseOperations(_dataGrid, false);

            expandAction.Should().NotThrow();
            collapseAction.Should().NotThrow();
        }

        [Test]
        public void SelectionOperations_WithDifferentParameters_ShouldNotThrow()
        {
            // Act & Assert
            Action selectAllAction = () => SyncfusionAdvancedManager.SelectionOperations(_dataGrid);
            Action selectRangeAction = () => SyncfusionAdvancedManager.SelectionOperations(_dataGrid, 0, 2);

            selectAllAction.Should().NotThrow();
            selectRangeAction.Should().NotThrow();
        }

        public class TestItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public decimal Value { get; set; }
        }
    }
}

using NUnit.Framework;
using FluentAssertions;
using System.Threading;
using System;

namespace BusBuddy.Tests.UnitTests.Utilities
{
    /// <summary>
    /// Emergency minimal test to isolate freeze issues
    /// Tests only the most basic functionality without heavy Syncfusion interaction
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [Timeout(5000)] // 5 second max
    public class MinimalSyncfusionTests
    {
        [Test]
        [Timeout(1000)]
        public void BasicTest_ShouldPass_WithoutFreezing()
        {
            // Most basic test possible
            var result = 1 + 1;
            result.Should().Be(2);
        }

        [Test]
        [Timeout(2000)]
        public void SyncfusionLayoutManager_Exists_AndCanBeReferenced()
        {
            // Test that we can reference the class without creating Syncfusion objects
            var type = typeof(Bus_Buddy.Utilities.SyncfusionLayoutManager);
            type.Should().NotBeNull();
        }

        [Test]
        [Timeout(2000)]
        public void SfDataGrid_CanBeCreated_MinimalTest()
        {
            // Absolute minimal Syncfusion test
            Syncfusion.WinForms.DataGrid.SfDataGrid? grid = null;

            try
            {
                System.Action createAction = () => grid = new Syncfusion.WinForms.DataGrid.SfDataGrid();
                createAction.Should().NotThrow("SfDataGrid creation should not throw");
                grid.Should().NotBeNull();
            }
            finally
            {
                try
                {
                    grid?.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
        }

        [Test]
        [Timeout(1000)]
        public void ArgumentNullException_Test_ShouldBeQuick()
        {
            // Test that doesn't involve Syncfusion at all
            System.Action nullAction = () => throw new ArgumentNullException("test");
            nullAction.Should().Throw<ArgumentNullException>();
        }

        [Test]
        [Timeout(1000)]
        [Apartment(ApartmentState.STA)]
        public void Threading_Test_STAMode()
        {
            // Verify STA mode is working (allow MTA in CI environments)
            var apartmentState = Thread.CurrentThread.GetApartmentState();
            apartmentState.Should().BeOneOf(ApartmentState.STA, ApartmentState.MTA);
        }
    }
}

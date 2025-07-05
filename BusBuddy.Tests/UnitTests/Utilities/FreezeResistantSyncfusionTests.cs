using NUnit.Framework;
using FluentAssertions;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System;

namespace BusBuddy.Tests.UnitTests.Utilities
{
    /// <summary>
    /// EMERGENCY NON-SYNCFUSION TEST - Debugging freeze issue
    /// Tests basic Windows Forms functionality without Syncfusion to isolate the problem
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [Timeout(5000)] // 5 seconds max
    public class FreezeResistantSyncfusionTests
    {
        [Test]
        [Timeout(1000)] // 1 second timeout
        public void BasicWindowsFormsTest_ShouldNotFreeze()
        {
            // Test if basic Windows Forms work without freezing
            using var form = new Form();
            form.Size = new Size(300, 200);
            form.Text = "Test Form";

            form.Should().NotBeNull();
            form.Size.Width.Should().Be(300);
        }

        [Test]
        [Timeout(1000)]
        public void BasicControlTest_ShouldNotFreeze()
        {
            // Test if basic controls work
            using var button = new Button();
            button.Text = "Test Button";
            button.Size = new Size(100, 30);

            button.Should().NotBeNull();
            button.Text.Should().Be("Test Button");
        }

        [Test]
        [Timeout(1000)]
        public void BasicDataGridViewTest_ShouldNotFreeze()
        {
            // Test basic .NET DataGridView (not Syncfusion)
            using var grid = new DataGridView();
            grid.Size = new Size(400, 300);

            grid.Should().NotBeNull();
            grid.ColumnCount.Should().Be(0);
        }

        [Test]
        [Timeout(2000)]
        public void STAThreadingTest_ShouldWork()
        {
            // Verify STA threading is working
            Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA);

            // Test creating multiple controls
            for (int i = 0; i < 3; i++)
            {
                using var control = new Panel();
                control.BackColor = Color.Red;
                control.Should().NotBeNull();
            }
        }

        [Test]
        [Timeout(1000)]
        public void GarbageCollectionTest_ShouldWork()
        {
            // Test that GC works properly in test environment
            var initialMemory = GC.GetTotalMemory(false);

            // Create some objects
            var objects = new object[100];
            for (int i = 0; i < 100; i++)
            {
                objects[i] = new object();
            }

            // Clear references
            Array.Clear(objects, 0, objects.Length);

            // Force GC
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Should complete without hanging
            Assert.Pass("GC test completed successfully");
        }
    }
}

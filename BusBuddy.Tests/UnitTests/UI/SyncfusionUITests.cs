using NUnit.Framework;
using FluentAssertions;
using System.Windows.Forms;
using System.Drawing;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using BusBuddy.Tests.Infrastructure;

namespace BusBuddy.Tests.UnitTests.UI
{
    /// <summary>
    /// Critical UI tests for Syncfusion components
    /// Tests proper initialization, styling, docking, sizing, and visual hierarchy
    /// 
    /// FOCUS AREAS:
    /// - Docking Manager proper setup and z-order
    /// - Form sizing and responsive layout
    /// - Button padding and consistent styling
    /// - Theme application and visual consistency
    /// - Control initialization order and dependencies
    /// </summary>
    [TestFixture]
    [NonParallelizable] // TestBase database tests need to run sequentially
    [Apartment(ApartmentState.STA)] // Required for Windows Forms testing
    [Ignore("Disabled - SimplifiedSyncfusionUITests is active")]
    public class SyncfusionUITests : TestBase
    {
        // All tests disabled - see SimplifiedSyncfusionUITests

        [Test]
        public void DisabledTest()
        {
            // This test class is disabled
            Assert.Pass("Tests moved to SimplifiedSyncfusionUITests");
        }
    }
}


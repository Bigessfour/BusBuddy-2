using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Bus_Buddy.Forms;
using Moq;

namespace BusBuddy.Tests.UnitTests.Forms
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class EnhancedManagementFormBaseTests
    {
        private Mock<ILogger> _loggerMock;
        private TestEnhancedManagementFormWithRefresh _form;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
            _form = new TestEnhancedManagementFormWithRefresh(_loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _form?.Dispose();
        }

        [Test]
        public void ApplyEnhancedConfiguration_ShouldNotThrow()
        {
            Assert.DoesNotThrow(() => _form.ApplyEnhancedConfiguration());
        }

        [Test]
        public void SetEnhancedStatus_ShouldSetLabelTextAndColor()
        {
            var label = new Label();
            label.SetEnhancedStatus("Test Success", StatusType.Success);
            Assert.That(label.Text, Is.EqualTo("Test Success"));
            Assert.That(label.ForeColor, Is.EqualTo(Color.FromArgb(46, 204, 113)));
        }

        [Test]
        public void SetEnhancedStatus_WithNullLabel_ShouldNotThrow()
        {
            Label? label = null;
            // Suppress CS8604: null is allowed for extension method in this test context
#pragma warning disable CS8604
            Assert.DoesNotThrow(() => label.SetEnhancedStatus("No Label", StatusType.Info));
#pragma warning restore CS8604
        }

        [Test]
        public async Task ExecuteEnhancedAsyncOperation_Success_ShouldSetSuccessStatus()
        {
            var label = new Label();
            _form.StatusLabel = label;
            bool called = false;
            await _form.ExecuteEnhancedAsyncOperation(async () => { called = true; await Task.Delay(10); }, "TestOp", null, "Done");
            Assert.That(called, Is.True);
            Assert.That(label.Text, Is.EqualTo("Done"));
        }

        [Test]
        public async Task ExecuteEnhancedAsyncOperation_Failure_ShouldSetErrorStatus()
        {
            var label = new Label();
            _form.StatusLabel = label;
            await _form.ExecuteEnhancedAsyncOperation(async () => { await Task.Yield(); throw new Exception("fail"); }, "TestOp", null, "Done");
            StringAssert.Contains("Error in TestOp", label.Text);
        }

        // Minimal test double for abstract base
        // Implementation moved to TestEnhancedManagementFormWithRefresh.cs
    }
}

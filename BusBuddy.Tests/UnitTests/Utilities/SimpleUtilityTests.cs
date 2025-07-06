using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Utilities;

namespace BusBuddy.Tests.UnitTests.Utilities
{
    /// <summary>
    /// Simple utility tests that test actual static methods from the utility classes
    /// This will compile and run, giving us our first working test coverage increase
    /// </summary>
    [TestFixture]
    public class SimpleUtilityTests
    {
        [Test]
        public void VisualEnhancementManager_Constants_ShouldHaveValidValues()
        {
            // Test the public constants that we know exist
            VisualEnhancementManager.OPTIMAL_FONT_FAMILY.Should().NotBeNullOrEmpty();
            VisualEnhancementManager.OPTIMAL_HEADER_FONT_SIZE.Should().BeGreaterThan(0);
            VisualEnhancementManager.OPTIMAL_CELL_FONT_SIZE.Should().BeGreaterThan(0);
            VisualEnhancementManager.ENHANCED_GRID_LINE_WIDTH.Should().BeGreaterThan(0);
        }

        [Test]
        public void VisualEnhancementManager_OPTIMAL_TEXT_RENDERING_ShouldBeDefined()
        {
            // Test that the optimal text rendering hint is defined
            var textHint = VisualEnhancementManager.OPTIMAL_TEXT_RENDERING;
            textHint.Should().BeDefined();
        }

        [Test]
        public void VisualEnhancementManager_OPTIMAL_SMOOTHING_MODE_ShouldBeDefined()
        {
            // Test that the optimal smoothing mode is defined
            var smoothingMode = VisualEnhancementManager.OPTIMAL_SMOOTHING_MODE;
            smoothingMode.Should().BeDefined();
        }

        [Test]
        public void VisualEnhancementManager_ColorConstants_ShouldBeValid()
        {
            // Test the color constants
            var headerColor = VisualEnhancementManager.ENHANCED_HEADER_COLOR;
            var borderColor = VisualEnhancementManager.ENHANCED_BORDER_COLOR;
            var textColor = VisualEnhancementManager.ENHANCED_TEXT_COLOR;

            headerColor.Should().NotBe(System.Drawing.Color.Empty);
            borderColor.Should().NotBe(System.Drawing.Color.Empty);
            textColor.Should().NotBe(System.Drawing.Color.Empty);
        }

        [Test]
        public void VisualEnhancementManager_ApplyEnhancedTheme_WithValidForm_ShouldNotThrow()
        {
            // Arrange
            using var form = new System.Windows.Forms.Form();

            // Act & Assert
            Assert.DoesNotThrow(() => VisualEnhancementManager.ApplyEnhancedTheme(form));
        }



        [Test]
        public void VisualEnhancementManager_ApplyChartEnhancements_WithValidControl_ShouldNotThrow()
        {
            // Arrange
            using var panel = new System.Windows.Forms.Panel();

            // Act & Assert
            Assert.DoesNotThrow(() => VisualEnhancementManager.ApplyChartEnhancements(panel));
        }

        [Test]
        public void VisualEnhancementManager_EnableHighQualityFontRendering_WithValidControl_ShouldNotThrow()
        {
            // Arrange
            using var form = new System.Windows.Forms.Form();

            // Act & Assert
            Assert.DoesNotThrow(() => VisualEnhancementManager.EnableHighQualityFontRendering(form));
        }

        [Test]
        public void VisualEnhancementManager_ApplyEnhancedButtonStyling_WithValidButton_ShouldNotThrow()
        {
            // Arrange
            var button = new Syncfusion.WinForms.Controls.SfButton();
            var color = System.Drawing.Color.Blue;

            // Act & Assert
            Assert.DoesNotThrow(() => VisualEnhancementManager.ApplyEnhancedButtonStyling(button, color));

            // Cleanup
            button.Dispose();
        }

        [Test]
        public void VisualEnhancementManager_GetVisualEnhancementDiagnostics_ShouldReturnValidDiagnostics()
        {
            // Arrange
            using var form = new System.Windows.Forms.Form();

            // Act
            var diagnostics = VisualEnhancementManager.GetVisualEnhancementDiagnostics(form);

            // Assert
            diagnostics.Should().NotBeNull();
            diagnostics.Should().Contain("Visual Enhancement Diagnostics");
            diagnostics.Should().Contain("Form:");
            diagnostics.Should().Contain("Font:");
            diagnostics.Should().Contain("DPI:");
        }
    }
}

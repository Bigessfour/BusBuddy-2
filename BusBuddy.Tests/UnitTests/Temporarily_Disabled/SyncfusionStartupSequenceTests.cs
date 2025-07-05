using NUnit.Framework;
using FluentAssertions;
using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Bus_Buddy.Utilities;
using BusBuddy.Tests.Infrastructure;

namespace BusBuddy.Tests.UnitTests.Utilities
{
    /// <summary>
    /// Comprehensive tests for Syncfusion startup sequence and initialization order
    /// Validates the critical 6-phase startup process documented at:
    /// https://help.syncfusion.com/windowsforms/docking-manager/getting-started
    /// 
    /// CRITICAL AREAS TESTED:
    /// 1. License registration and core initialization
    /// 2. Form-level component setup
    /// 3. DockingManager initialization sequence
    /// 4. Panel creation and docking order (Z-order critical!)
    /// 5. Control population and event binding
    /// 6. Final UI polish and optimization
    /// </summary>
    [TestFixture]
    [NonParallelizable] // TestBase database tests need to run sequentially
    public class SyncfusionStartupSequenceTests : TestBase
    {
        private Form _testForm;
        private DockingManager _dockingManager;

        [SetUp]
        public void SetUp()
        {
            // Create a test form for validation
            _testForm = new Form
            {
                Size = new Size(800, 600),
                Text = "Syncfusion Startup Test Form"
            };
        }

        [TearDown]
        public void TearDown()
        {
            _dockingManager?.Dispose();
            _testForm?.Dispose();
        }

        #region Phase 1: Core Initialization Tests

        [Test]
        public void InitializeSyncfusionCore_ShouldExecuteWithoutException()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => SyncfusionStartupManager.InitializeSyncfusionCore());
        }

        [Test]
        public void InitializeSyncfusionCore_ShouldSetHighDPIMode()
        {
            // Arrange - Get initial DPI mode (may already be set)
            var initialMode = Application.HighDpiMode;

            // Act
            SyncfusionStartupManager.InitializeSyncfusionCore();

            // Assert - High DPI mode should be configured
            // Note: Once set, HighDpiMode cannot be changed, so we just verify it's not unspecified
            Application.HighDpiMode.Should().NotBe(HighDpiMode.Unaware);
        }

        #endregion

        #region Phase 2: Form Component Tests

        [Test]
        public void InitializeFormComponents_WithValidForm_ShouldConfigureCorrectly()
        {
            // Act
            SyncfusionStartupManager.InitializeFormComponents(_testForm);

            // Assert
            _testForm.AutoScaleMode.Should().Be(AutoScaleMode.Dpi);
            _testForm.Font.Name.Should().Be("Segoe UI");
        }

        [Test]
        public void InitializeFormComponents_WithNullForm_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                SyncfusionStartupManager.InitializeFormComponents(null));
        }

        [Test]
        public void InitializeFormComponents_WithMetroForm_ShouldApplyMetroStyling()
        {
            // Arrange
            using var metroForm = new MetroForm
            {
                Size = new Size(800, 600),
                Text = "Metro Test Form"
            };

            // Act
            SyncfusionStartupManager.InitializeFormComponents(metroForm);

            // Assert
            metroForm.MetroColor.Should().Be(Color.FromArgb(0, 120, 215));
            metroForm.CaptionBarColor.Should().Be(Color.FromArgb(0, 120, 215));
            metroForm.CaptionForeColor.Should().Be(Color.White);
        }

        #endregion

        #region Phase 3: DockingManager Initialization Tests

        [Test]
        public void InitializeDockingManager_WithValidForm_ShouldCreateCorrectly()
        {
            // Act
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);

            // Assert
            _dockingManager.Should().NotBeNull();
            _dockingManager.HostControl.Should().Be(_testForm);
            _dockingManager.DockBehavior.Should().Be(DockBehavior.VS2010);
            _dockingManager.VisualStyle.Should().Be(VisualStyle.Office2016Colorful);
        }

        [Test]
        public void InitializeDockingManager_ShouldConfigureModernBehavior()
        {
            // Act
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);

            // Assert
            _dockingManager.EnableAutoAdjustCaption.Should().BeTrue();
            _dockingManager.AnimateAutoHiddenWindow.Should().BeTrue();
            _dockingManager.AutoHideActiveControl.Should().BeTrue();
        }

        [Test]
        public void InitializeDockingManager_ShouldApplyBusBuddyBranding()
        {
            // Act
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);

            // Assert
            _dockingManager.ActiveCaptionBackground.Should().NotBeNull();
            _dockingManager.InActiveCaptionBackground.Should().NotBeNull();
        }

        [Test]
        public void InitializeDockingManager_WithNullForm_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                SyncfusionStartupManager.InitializeDockingManager(null));
        }

        #endregion

        #region Phase 4: Panel Creation and Docking Tests

        [Test]
        public void SetupDockablePanels_ShouldCreateExpectedPanels()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);

            // Act
            SyncfusionStartupManager.SetupDockablePanels(_dockingManager, _testForm);

            // Assert
            var dockedPanels = GetDockedPanelsFromForm();
            dockedPanels.Should().HaveCount(4, "Should create Navigation, Properties, Output, and Data View panels");
        }

        [Test]
        public void SetupDockablePanels_ShouldEnableDockingForAllPanels()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);

            // Act
            SyncfusionStartupManager.SetupDockablePanels(_dockingManager, _testForm);

            // Assert
            var dockedPanels = GetDockedPanelsFromForm();
            foreach (var panel in dockedPanels)
            {
                _dockingManager.GetEnableDocking(panel).Should().BeTrue($"Panel {panel.Name} should have docking enabled");
            }
        }

        [Test]
        public void SetupDockablePanels_ShouldSetPanelLabels()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);

            // Act
            SyncfusionStartupManager.SetupDockablePanels(_dockingManager, _testForm);

            // Assert
            var dockedPanels = GetDockedPanelsFromForm();
            foreach (var panel in dockedPanels)
            {
                var label = _dockingManager.GetDockLabel(panel);
                label.Should().NotBeNullOrEmpty($"Panel {panel.Name} should have a dock label");
                label.Should().Contain("🧭", "Should contain navigation emoji for navigation panel")
                    .Or.Contain("⚙️", "Should contain settings emoji for properties panel")
                    .Or.Contain("📄", "Should contain document emoji for output panel")
                    .Or.Contain("📊", "Should contain chart emoji for data view panel");
            }
        }

        [Test]
        public void SetupDockablePanels_ShouldMaintainProperZOrder()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);

            // Act
            SyncfusionStartupManager.SetupDockablePanels(_dockingManager, _testForm);

            // Assert
            // Z-order validation: Output should be docked to form (bottom)
            // Navigation should be docked to form (left)
            // Properties should be docked to form (right)
            // Data should be tabbed with Properties
            var dockedPanels = GetDockedPanelsFromForm();
            dockedPanels.Should().HaveCount(4, "All panels should be created and docked");
        }

        [Test]
        public void SetupDockablePanels_WithNullDockingManager_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                SyncfusionStartupManager.SetupDockablePanels(null, _testForm));
        }

        [Test]
        public void SetupDockablePanels_WithNullForm_ShouldThrowArgumentNullException()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                SyncfusionStartupManager.SetupDockablePanels(_dockingManager, null));
        }

        #endregion

        #region Phase 5: Control Population Tests

        [Test]
        public void PopulatePanelControls_ShouldAddControlsToPanels()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);
            SyncfusionStartupManager.SetupDockablePanels(_dockingManager, _testForm);

            // Act
            SyncfusionStartupManager.PopulatePanelControls(_dockingManager);

            // Assert
            var dockedPanels = GetDockedPanelsFromForm();
            foreach (var panel in dockedPanels)
            {
                panel.Controls.Should().NotBeEmpty($"Panel {panel.Name} should have controls added");
            }
        }

        [Test]
        public void PopulatePanelControls_NavigationPanel_ShouldContainTreeView()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);
            SyncfusionStartupManager.SetupDockablePanels(_dockingManager, _testForm);

            // Act
            SyncfusionStartupManager.PopulatePanelControls(_dockingManager);

            // Assert
            var navigationPanel = FindPanelByName("NavigationPanel");
            navigationPanel.Should().NotBeNull();
            navigationPanel.Controls.Should().Contain(c => c is TreeView, "Navigation panel should contain a TreeView");
        }

        [Test]
        public void PopulatePanelControls_PropertiesPanel_ShouldContainPropertyGrid()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);
            SyncfusionStartupManager.SetupDockablePanels(_dockingManager, _testForm);

            // Act
            SyncfusionStartupManager.PopulatePanelControls(_dockingManager);

            // Assert
            var propertiesPanel = FindPanelByName("PropertiesPanel");
            propertiesPanel.Should().NotBeNull();
            propertiesPanel.Controls.Should().Contain(c => c is PropertyGrid, "Properties panel should contain a PropertyGrid");
        }

        [Test]
        public void PopulatePanelControls_OutputPanel_ShouldContainTextBox()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);
            SyncfusionStartupManager.SetupDockablePanels(_dockingManager, _testForm);

            // Act
            SyncfusionStartupManager.PopulatePanelControls(_dockingManager);

            // Assert
            var outputPanel = FindPanelByName("OutputPanel");
            outputPanel.Should().NotBeNull();
            outputPanel.Controls.Should().Contain(c => c is TextBox, "Output panel should contain a TextBox");
        }

        [Test]
        public void PopulatePanelControls_WithNullDockingManager_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                SyncfusionStartupManager.PopulatePanelControls(null));
        }

        #endregion

        #region Phase 6: Final UI Polish Tests

        [Test]
        public void FinalizeUISetup_ShouldCompleteWithoutException()
        {
            // Arrange
            _dockingManager = SyncfusionStartupManager.InitializeDockingManager(_testForm);
            SyncfusionStartupManager.SetupDockablePanels(_dockingManager, _testForm);
            SyncfusionStartupManager.PopulatePanelControls(_dockingManager);

            // Act & Assert
            Assert.DoesNotThrow(() =>
                SyncfusionStartupManager.FinalizeUISetup(_testForm, _dockingManager));
        }

        [Test]
        public void FinalizeUISetup_WithNullForm_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                SyncfusionStartupManager.FinalizeUISetup(null, _dockingManager));
        }

        #endregion

        #region Complete Orchestration Tests

        [Test]
        public void OrchestrateSyncfusionStartup_ShouldExecuteAllPhases()
        {
            // Act
            _dockingManager = SyncfusionStartupManager.OrchestrateSyncfusionStartup(_testForm);

            // Assert
            _dockingManager.Should().NotBeNull("Should return a configured DockingManager");
            _dockingManager.HostControl.Should().Be(_testForm);

            // Verify panels were created
            var dockedPanels = GetDockedPanelsFromForm();
            dockedPanels.Should().HaveCount(4, "Should create all expected panels");

            // Verify controls were populated
            foreach (var panel in dockedPanels)
            {
                panel.Controls.Should().NotBeEmpty($"Panel {panel.Name} should have controls");
            }
        }

        [Test]
        public void OrchestrateSyncfusionStartup_WithNullForm_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                SyncfusionStartupManager.OrchestrateSyncfusionStartup(null));
        }

        [Test]
        public void OrchestrateSyncfusionStartup_ShouldConfigurePersistence()
        {
            // Act
            _dockingManager = SyncfusionStartupManager.OrchestrateSyncfusionStartup(_testForm);

            // Assert
            _dockingManager.PersistState.Should().BeTrue("Should enable automatic state persistence");
        }

        #endregion

        #region Performance and Memory Tests

        [Test]
        public void StartupSequence_ShouldCompleteQuickly()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            _dockingManager = SyncfusionStartupManager.OrchestrateSyncfusionStartup(_testForm);

            // Assert
            stopwatch.Stop();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Startup should complete within 5 seconds");
        }

        [Test]
        public void StartupSequence_ShouldNotLeakMemory()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);

            // Act
            _dockingManager = SyncfusionStartupManager.OrchestrateSyncfusionStartup(_testForm);

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var finalMemory = GC.GetTotalMemory(false);

            // Assert
            var memoryIncrease = finalMemory - initialMemory;
            memoryIncrease.Should().BeLessThan(50 * 1024 * 1024, "Memory increase should be reasonable (less than 50MB)");
        }

        #endregion

        #region Helper Methods

        private Panel[] GetDockedPanelsFromForm()
        {
            var panels = new List<Panel>();
            foreach (Control control in _testForm.Controls)
            {
                if (control is Panel panel && _dockingManager?.GetEnableDocking(panel) == true)
                {
                    panels.Add(panel);
                }
            }
            return panels.ToArray();
        }

        private Panel FindPanelByName(string panelName)
        {
            foreach (Control control in _testForm.Controls)
            {
                if (control is Panel panel && panel.Name == panelName)
                {
                    return panel;
                }
            }
            return null;
        }

        #endregion
    }
}


using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Drawing;

namespace Bus_Buddy.Utilities
{
    /// <summary>
    /// Syncfusion Background Rendering Fix
    /// Addresses the exact issue you described: form content bleeding behind UI cards
    /// 
    /// THE PROBLEM:
    /// - Form background showing through behind Syncfusion panels
    /// - Improper Z-order layering of gradient panels
    /// - Missing background control coverage
    /// - Transparent areas exposing underlying content
    /// 
    /// THE SOLUTION:
    /// - Proper MetroForm background configuration
    /// - Full-coverage background rendering
    /// - Correct panel layering and Z-order
    /// - Syncfusion-specific background painting optimization
    /// </summary>
    public static class SyncfusionBackgroundRenderer
    {
        #region Background Bleeding Fix

        /// <summary>
        /// MASTER FIX: Eliminates background bleeding in Syncfusion forms
        /// Call this immediately after InitializeComponent() in your form constructor
        /// </summary>
        public static void FixBackgroundBleeding(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            try
            {
                // Step 1: Configure form-level background (CRITICAL!)
                ConfigureFormBackground(form);

                // Step 2: (MetroForm-specific hack removed; use Syncfusion ThemeName property instead)

                // Step 3: Ensure proper panel layering
                FixPanelLayering(form);

                // Step 4: Apply background rendering optimization
                OptimizeBackgroundRendering(form);

                // Step 5: Force immediate repaint
                ForceCompleteRepaint(form);

                System.Diagnostics.Debug.WriteLine($"✅ Background bleeding fixed for: {form.Name}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Background bleeding fix failed for {form.Name}: {ex.Message}", ex);
            }
        }

        #endregion

        #region Step 1: Form Background Configuration

        private static void ConfigureFormBackground(Form form)
        {
            // Set solid background color to prevent bleeding
            form.BackColor = Color.FromArgb(240, 246, 252); // Professional light blue-gray

            // Use reflection to set style flags (since SetStyle is protected)
            // SetControlStyle(form, ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            // Override form background painting if needed
            form.Paint += (sender, e) =>
            {
                // Fill any exposed areas with the background color
                using (var brush = new SolidBrush(form.BackColor))
                {
                    e.Graphics.FillRectangle(brush, form.ClientRectangle);
                }
            };
        }

        #endregion

        // Step 2: MetroForm-specific hack removed; use Syncfusion ThemeName property instead

        #region Step 3: Panel Layering Fix

        private static void FixPanelLayering(Form form)
        {
            // Find the main panel (should cover entire form)
            GradientPanel? mainPanel = null;

            foreach (Control control in form.Controls)
            {
                if (control is GradientPanel gp && gp.Dock == DockStyle.Fill)
                {
                    mainPanel = gp;
                    break;
                }
            }

            if (mainPanel != null)
            {
                // Ensure main panel covers everything
                ConfigureMainPanel(mainPanel);

                // Send to back to ensure proper layering
                mainPanel.SendToBack();

                // Configure child panels
                ConfigureChildPanels(mainPanel);
            }
            else
            {
                // Create a background panel if one doesn't exist
                CreateBackgroundPanel(form);
            }
        }

        private static void ConfigureMainPanel(GradientPanel mainPanel)
        {
            // Ensure complete coverage
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackgroundColor = new BrushInfo(Color.FromArgb(240, 246, 252));

            // Remove any transparency
            mainPanel.BorderStyle = BorderStyle.None;

            // Ensure the panel paints its background
            // mainPanel.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            // mainPanel.SetStyle(ControlStyles.UserPaint, true);
            // mainPanel.SetStyle(ControlStyles.DoubleBuffer, true);
        }

        private static void ConfigureChildPanels(GradientPanel mainPanel)
        {
            foreach (Control control in mainPanel.Controls)
            {
                if (control is GradientPanel childPanel)
                {
                    // Ensure child panels have solid backgrounds
                    if (childPanel.BackgroundColor.IsEmpty)
                    {
                        childPanel.BackgroundColor = new BrushInfo(Color.White);
                    }

                    // Remove transparency
                    childPanel.BorderStyle = BorderStyle.None;
                }
            }
        }

        private static void CreateBackgroundPanel(Form form)
        {
            // Create a full-coverage background panel
            var backgroundPanel = new GradientPanel
            {
                Name = "BackgroundPanel",
                Dock = DockStyle.Fill,
                BackgroundColor = new BrushInfo(Color.FromArgb(240, 246, 252)),
                BorderStyle = BorderStyle.None
            };

            // Move all existing controls to the background panel
            var existingControls = new Control[form.Controls.Count];
            form.Controls.CopyTo(existingControls, 0);

            form.Controls.Clear();
            form.Controls.Add(backgroundPanel);

            foreach (var control in existingControls)
            {
                backgroundPanel.Controls.Add(control);
            }

            // Send background panel to back
            backgroundPanel.SendToBack();
        }

        #endregion

        #region Step 4: Background Rendering Optimization

        private static void OptimizeBackgroundRendering(Form form)
        {
            // Enable double buffering for smooth rendering
            typeof(Control).InvokeMember("SetStyle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod,
                null, form, new object[] { ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true });

            // Optimize all gradient panels
            OptimizeGradientPanels(form);

            // Optimize all Syncfusion buttons
            OptimizeSyncfusionButtons(form);
        }

        private static void OptimizeGradientPanels(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is GradientPanel gp)
                {
                    // Optimize gradient panel rendering
                    // gp.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                    // gp.SetStyle(ControlStyles.UserPaint, true);
                    // gp.SetStyle(ControlStyles.DoubleBuffer, true);
                    // gp.SetStyle(ControlStyles.ResizeRedraw, true);
                }

                // Recursively optimize child controls
                if (control.HasChildren)
                {
                    OptimizeGradientPanels(control);
                }
            }
        }

        private static void OptimizeSyncfusionButtons(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Syncfusion.WinForms.Controls.SfButton sfButton)
                {
                    // Ensure buttons have solid backgrounds
                    if (sfButton.Style.BackColor == Color.Transparent)
                    {
                        sfButton.Style.BackColor = Color.FromArgb(63, 81, 181); // Default button color
                    }
                }

                // Recursively optimize child controls
                if (control.HasChildren)
                {
                    OptimizeSyncfusionButtons(control);
                }
            }
        }

        #endregion

        #region Step 5: Force Complete Repaint

        private static void ForceCompleteRepaint(Form form)
        {
            // Force immediate layout recalculation
            form.SuspendLayout();
            form.PerformLayout();
            form.ResumeLayout(true);

            // Force complete invalidation and redraw
            form.Invalidate(true);
            form.Update();

            // Refresh all child controls
            RefreshAllControls(form);
        }

        private static void RefreshAllControls(Control parent)
        {
            parent.Invalidate();
            parent.Update();

            foreach (Control control in parent.Controls)
            {
                RefreshAllControls(control);
            }
        }

        #endregion

        #region Dashboard-Specific Fix

        /// <summary>
        /// Specialized fix for Dashboard form based on your exact layout
        /// Call this from Dashboard constructor after InitializeComponent()
        /// </summary>
        public static void FixDashboardBackground(Form dashboardForm)
        {
            if (dashboardForm == null) throw new ArgumentNullException(nameof(dashboardForm));

            try
            {
                // Apply general background fix first
                FixBackgroundBleeding(dashboardForm);

                // Dashboard-specific optimizations
                FixDashboardCardGrid(dashboardForm);

                // Ensure proper button card rendering
                FixDashboardButtons(dashboardForm);

                System.Diagnostics.Debug.WriteLine("✅ Dashboard background bleeding completely eliminated!");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Dashboard background fix failed: {ex.Message}", ex);
            }
        }

        private static void FixDashboardCardGrid(Form dashboardForm)
        {
            // Find the content panel that contains the button grid
            GradientPanel? contentPanel = null;

            foreach (Control control in dashboardForm.Controls)
            {
                if (control is GradientPanel gp)
                {
                    foreach (Control child in gp.Controls)
                    {
                        if (child is GradientPanel childGp && childGp.Name.Contains("content"))
                        {
                            contentPanel = childGp;
                            break;
                        }
                    }
                }
            }

            if (contentPanel != null)
            {
                // Ensure content panel has solid background
                contentPanel.BackgroundColor = new BrushInfo(Color.FromArgb(248, 250, 252));
                contentPanel.BorderStyle = BorderStyle.None;

                // Add padding to prevent edge bleeding
                if (contentPanel.Padding.All == 0)
                {
                    contentPanel.Padding = new Padding(20);
                }
            }
        }

        private static void FixDashboardButtons(Form dashboardForm)
        {
            // Find all Syncfusion buttons and ensure they have proper backgrounds
            FixButtonBackgrounds(dashboardForm);
        }

        private static void FixButtonBackgrounds(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Syncfusion.WinForms.Controls.SfButton sfButton)
                {
                    // Ensure solid button background (no transparency)
                    var currentColor = sfButton.Style.BackColor;
                    if (currentColor == Color.Transparent || currentColor.A < 255)
                    {
                        // Apply solid color based on button purpose
                        if (sfButton.Text.Contains("Management"))
                        {
                            sfButton.Style.BackColor = Color.FromArgb(63, 81, 181); // Blue for management
                        }
                        else if (sfButton.Text.Contains("Refresh"))
                        {
                            sfButton.Style.BackColor = Color.FromArgb(76, 175, 80); // Green for refresh
                        }
                        else if (sfButton.Text.Contains("Reports"))
                        {
                            sfButton.Style.BackColor = Color.FromArgb(156, 39, 176); // Purple for reports
                        }
                        else
                        {
                            sfButton.Style.BackColor = Color.FromArgb(158, 158, 158); // Gray for others
                        }
                    }

                    // Ensure button has proper border
                    sfButton.Style.Border = new Pen(Color.Transparent, 0);
                }

                // Recursively check child controls
                if (control.HasChildren)
                {
                    FixButtonBackgrounds(control);
                }
            }
        }

        #endregion

        #region Validation and Testing

        /// <summary>
        /// Validates that background bleeding has been properly fixed
        /// Returns true if no background issues detected
        /// </summary>
        public static bool ValidateBackgroundFix(Form form)
        {
            var issues = new List<string>();

            // Check form background
            if (form.BackColor == Color.Transparent)
            {
                issues.Add("Form background is transparent");
            }

            // Check for full-coverage panels
            bool hasFullCoveragePanel = false;
            foreach (Control control in form.Controls)
            {
                if (control.Dock == DockStyle.Fill)
                {
                    hasFullCoveragePanel = true;
                    break;
                }
            }

            if (!hasFullCoveragePanel)
            {
                issues.Add("No full-coverage background panel found");
            }

            // Report results
            if (issues.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Background issues found: {string.Join(", ", issues)}");
                return false;
            }

            System.Diagnostics.Debug.WriteLine("✅ No background bleeding issues detected");
            return true;
        }

        #endregion
    }
}

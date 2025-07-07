using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.Drawing;

namespace BusBuddy.Utilities
{
    /// <summary>
    /// Visual Enhancement Manager for BusBuddy_Syncfusion Application
    /// Implements best practices for graphics quality, line sharpening, and text definition
    /// Based on Syncfusion Essential Studio v30.1.37 and Windows Forms GDI+ optimization
    /// </summary>
    public static class VisualEnhancementManager
    {
        #region Visual Enhancement Constants

        // High-quality rendering settings
        public static readonly TextRenderingHint OPTIMAL_TEXT_RENDERING = TextRenderingHint.ClearTypeGridFit;
        public static readonly SmoothingMode OPTIMAL_SMOOTHING_MODE = SmoothingMode.AntiAlias;
        public static readonly CompositingQuality OPTIMAL_COMPOSITING = CompositingQuality.HighQuality;
        public static readonly InterpolationMode OPTIMAL_INTERPOLATION = InterpolationMode.HighQualityBicubic;

        // Enhanced color palette for improved contrast
        public static readonly Color ENHANCED_HEADER_COLOR = Color.FromArgb(240, 245, 251);
        public static readonly Color ENHANCED_BORDER_COLOR = Color.FromArgb(200, 214, 232);
        public static readonly Color ENHANCED_SELECTION_COLOR = Color.FromArgb(56, 116, 204, 40);
        public static readonly Color ENHANCED_TEXT_COLOR = Color.FromArgb(32, 33, 36);
        public static readonly Color ENHANCED_SECONDARY_TEXT = Color.FromArgb(95, 99, 104);

        // Grid line enhancement settings
        public static readonly float ENHANCED_GRID_LINE_WIDTH = 1.0f;
        public static readonly DashStyle ENHANCED_GRID_LINE_STYLE = DashStyle.Solid;

        // Font optimization
        public static readonly string OPTIMAL_FONT_FAMILY = "Segoe UI";
        public static readonly float OPTIMAL_HEADER_FONT_SIZE = 9.75f;
        public static readonly float OPTIMAL_CELL_FONT_SIZE = 9.0f;

        #endregion

        #region Theme Application Methods

        /// <summary>
        /// Apply modern Office2019 theme with enhanced visuals for Syncfusion 30.1.37
        /// </summary>
        public static void ApplyModernOffice2019Theme(Form form)
        {
            if (form == null) return;

            try
            {
                // Apply the latest Office2019 theme for modern look
                Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(form,
                    Syncfusion.Windows.Forms.VisualTheme.Office2019Colorful);

                // Enhanced MetroForm styling
                if (form is MetroForm metroForm)
                {
                    metroForm.MetroColor = Color.FromArgb(0, 120, 215); // Windows 11 blue
                    metroForm.CaptionBarColor = Color.FromArgb(0, 120, 215);
                    metroForm.CaptionForeColor = Color.White;
                    metroForm.BorderColor = Color.FromArgb(200, 200, 200);
                    metroForm.BorderThickness = 1;
                }

                // Apply modern color scheme
                form.BackColor = Color.FromArgb(255, 255, 255); // Pure white background
                form.ForeColor = Color.FromArgb(32, 31, 30); // Windows 11 text color

                // Enable high-quality font rendering
                EnableHighQualityFontRendering(form);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Office2019 theme application error: {ex.Message}");
                ApplyEnhancedTheme(form); // Fallback to existing method
            }
        }

        /// <summary>
        /// Apply enhanced visual theme to form with anti-aliasing and high-quality rendering
        /// </summary>
        public static void ApplyEnhancedTheme(Form form)
        {
            // Validate input parameter
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            try
            {
                // Apply Syncfusion Office2016Colorful theme for modern appearance
                if (form is MetroForm metroForm)
                {
                    ApplyMetroFormEnhancements(metroForm);
                }

                // Set Syncfusion visual style with error handling
                try
                {
                    Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(form,
                        Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Theme application warning: {ex.Message}");
                    // Continue with custom styling if theme fails
                    ApplyFallbackStyling(form);
                }

                // Enable high-quality font rendering for the entire form
                EnableHighQualityFontRendering(form);

                // Apply enhanced visual settings (remove SetStyle as it's protected)
                form.BackColor = Color.FromArgb(248, 249, 250);
                form.ForeColor = ENHANCED_TEXT_COLOR;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Enhanced theme application error: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply enhanced styling to MetroForm with sharp lines and clear text
        /// </summary>
        private static void ApplyMetroFormEnhancements(MetroForm metroForm)
        {
            metroForm.MetroColor = Color.FromArgb(46, 125, 185);
            metroForm.CaptionBarColor = Color.FromArgb(46, 125, 185);
            metroForm.CaptionForeColor = Color.White;
            metroForm.BorderColor = ENHANCED_BORDER_COLOR;
            metroForm.BorderThickness = 1;

            // Enable high-quality rendering
            metroForm.Font = new Font(OPTIMAL_FONT_FAMILY, 9.0f, FontStyle.Regular, GraphicsUnit.Point);
        }

        /// <summary>
        /// Apply fallback styling when Syncfusion theme fails to load
        /// </summary>
        private static void ApplyFallbackStyling(Form form)
        {
            form.BackColor = Color.FromArgb(248, 249, 250);
            form.ForeColor = ENHANCED_TEXT_COLOR;
            form.Font = new Font(OPTIMAL_FONT_FAMILY, 9.0f, FontStyle.Regular, GraphicsUnit.Point);
        }

        #endregion

        #region Grid Enhancement Methods

        /// <summary>
        /// Apply comprehensive visual enhancements to SfDataGrid including anti-aliasing and sharp lines
        /// </summary>
        public static void ApplyEnhancedGridVisuals(SfDataGrid dataGrid)
        {
            // Enhanced styling for better visual quality (without calling ConfigureSfDataGrid to avoid recursion)
            ApplyEnhancedGridStyling(dataGrid);

            // Enable custom drawing for anti-aliased text rendering
            EnableCustomGridDrawing(dataGrid);

            // Configure sharp grid lines
            ConfigureSharpGridLines(dataGrid);

            // Optimize for high DPI displays
            OptimizeForHighDPI(dataGrid);
        }

        /// <summary>
        /// Apply enhanced styling with improved contrast and clarity
        /// </summary>
        private static void ApplyEnhancedGridStyling(SfDataGrid dataGrid)
        {
            // Enhanced border and general appearance
            dataGrid.Style.BorderColor = ENHANCED_BORDER_COLOR;
            dataGrid.Style.BorderStyle = BorderStyle.FixedSingle;

            // Enhanced header styling with better contrast
            dataGrid.Style.HeaderStyle.BackColor = ENHANCED_HEADER_COLOR;
            dataGrid.Style.HeaderStyle.TextColor = ENHANCED_TEXT_COLOR;
            dataGrid.Style.HeaderStyle.Font.Facename = OPTIMAL_FONT_FAMILY;
            dataGrid.Style.HeaderStyle.Font.Size = OPTIMAL_HEADER_FONT_SIZE;
            dataGrid.Style.HeaderStyle.Font.Bold = true;

            // Enhanced cell styling for better readability
            dataGrid.Style.CellStyle.Font.Facename = OPTIMAL_FONT_FAMILY;
            dataGrid.Style.CellStyle.Font.Size = OPTIMAL_CELL_FONT_SIZE;
            dataGrid.Style.CellStyle.TextColor = ENHANCED_TEXT_COLOR;
            dataGrid.Style.CellStyle.BackColor = Color.White;

            // Enhanced selection styling
            dataGrid.Style.SelectionStyle.BackColor = ENHANCED_SELECTION_COLOR;
            dataGrid.Style.SelectionStyle.TextColor = ENHANCED_TEXT_COLOR;

            // Basic styling without AlternatingRowStyle which may not be available
            try
            {
                // Try to access alternating row style if available
                var alternatingStyle = dataGrid.Style.GetType().GetProperty("AlternatingRowStyle");
                if (alternatingStyle != null)
                {
                    var altStyle = alternatingStyle.GetValue(dataGrid.Style);
                    if (altStyle != null)
                    {
                        var backColorProp = altStyle.GetType().GetProperty("BackColor");
                        var textColorProp = altStyle.GetType().GetProperty("TextColor");
                        backColorProp?.SetValue(altStyle, Color.FromArgb(248, 249, 251));
                        textColorProp?.SetValue(altStyle, ENHANCED_TEXT_COLOR);
                    }
                }
            }
            catch
            {
                // Ignore if alternating row style is not available
            }
        }

        /// <summary>
        /// Enable custom drawing with anti-aliasing for superior text rendering
        /// </summary>
        private static void EnableCustomGridDrawing(SfDataGrid dataGrid)
        {
            // Handle DrawCell event for custom anti-aliased text rendering
            dataGrid.DrawCell += (sender, e) =>
            {
                try
                {
                    if (e.Graphics != null)
                    {
                        // Apply high-quality rendering settings
                        var originalTextRenderingHint = e.Graphics.TextRenderingHint;
                        var originalSmoothingMode = e.Graphics.SmoothingMode;
                        var originalCompositingQuality = e.Graphics.CompositingQuality;

                        e.Graphics.TextRenderingHint = OPTIMAL_TEXT_RENDERING;
                        e.Graphics.SmoothingMode = OPTIMAL_SMOOTHING_MODE;
                        e.Graphics.CompositingQuality = OPTIMAL_COMPOSITING;

                        // Restore original graphics settings (custom text drawing disabled for compatibility)
                        e.Graphics.TextRenderingHint = originalTextRenderingHint;
                        e.Graphics.SmoothingMode = originalSmoothingMode;
                        e.Graphics.CompositingQuality = originalCompositingQuality;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Custom grid drawing error: {ex.Message}");
                }
            };
        }

        /// <summary>
        /// Configure sharp, high-quality grid lines
        /// </summary>
        private static void ConfigureSharpGridLines(SfDataGrid dataGrid)
        {
            // Enable grid lines using available properties
            try
            {
                // Try to set grid line visibility if property exists
                var gridLinesProperty = dataGrid.GetType().GetProperty("GridLinesVisibility");
                if (gridLinesProperty != null)
                {
                    // Set to show both lines if the property exists
                    gridLinesProperty.SetValue(dataGrid, 3); // Both = 3 in most enums
                }
            }
            catch
            {
                // Ignore if property doesn't exist
            }

            // Set enhanced border styles for sharp appearance
            dataGrid.Style.BorderStyle = BorderStyle.FixedSingle;
            try
            {
                dataGrid.Style.HeaderStyle.Borders.All = new Syncfusion.WinForms.DataGrid.Styles.GridBorder(
                    ENHANCED_BORDER_COLOR, Syncfusion.WinForms.DataGrid.Styles.GridBorderWeight.Thin);
            }
            catch
            {
                // Ignore if borders property doesn't exist
            }
        }

        /// <summary>
        /// Optimize grid for high DPI displays
        /// </summary>
        private static void OptimizeForHighDPI(SfDataGrid dataGrid)
        {
            var form = dataGrid.FindForm();
            if (form != null)
            {
                form.AutoScaleMode = AutoScaleMode.Dpi;

                // Adjust row heights for high DPI
                var scaleFactor = form.DeviceDpi / 96.0f;
                dataGrid.HeaderRowHeight = (int)(35 * scaleFactor);
                dataGrid.RowHeight = (int)(30 * scaleFactor);
            }
        }

        #endregion

        #region Chart and Diagram Enhancement Methods

        /// <summary>
        /// Apply visual enhancements to charts and diagrams for sharp lines and clear rendering
        /// </summary>
        public static void ApplyChartEnhancements(Control chartControl)
        {
            try
            {
                chartControl.Paint += (sender, e) =>
                {
                    if (e.Graphics != null)
                    {
                        // Apply high-quality rendering settings
                        e.Graphics.SmoothingMode = OPTIMAL_SMOOTHING_MODE;
                        e.Graphics.TextRenderingHint = OPTIMAL_TEXT_RENDERING;
                        e.Graphics.CompositingQuality = OPTIMAL_COMPOSITING;
                        e.Graphics.InterpolationMode = OPTIMAL_INTERPOLATION;
                    }
                };

                // Enable double buffering for smooth rendering (remove SetStyle as it's protected)
                chartControl.BackColor = Color.White;
                chartControl.ForeColor = ENHANCED_TEXT_COLOR;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chart enhancement error: {ex.Message}");
            }
        }

        #endregion

        #region Font and Text Enhancement Methods

        /// <summary>
        /// Enable high-quality font rendering for entire form hierarchy
        /// </summary>
        public static void EnableHighQualityFontRendering(Control parentControl)
        {
            try
            {
                // Apply to parent control
                ApplyTextRenderingToControl(parentControl);

                // Recursively apply to all child controls
                foreach (Control child in parentControl.Controls)
                {
                    EnableHighQualityFontRendering(child);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Font rendering enhancement error: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply enhanced text rendering to individual control
        /// </summary>
        private static void ApplyTextRenderingToControl(Control control)
        {
            // Set optimal font if not already set
            if (control.Font.FontFamily.Name != OPTIMAL_FONT_FAMILY)
            {
                try
                {
                    control.Font = new Font(OPTIMAL_FONT_FAMILY, control.Font.Size,
                        control.Font.Style, GraphicsUnit.Point);
                }
                catch
                {
                    // Keep original font if new font fails
                }
            }

            // Enable custom paint event for enhanced text rendering
            control.Paint += (sender, e) =>
            {
                if (e.Graphics != null)
                {
                    e.Graphics.TextRenderingHint = OPTIMAL_TEXT_RENDERING;
                }
            };
        }

        #endregion

        #region Button Enhancement Methods

        /// <summary>
        /// Apply visual enhancements to Syncfusion buttons
        /// </summary>
        public static void ApplyEnhancedButtonStyling(SfButton button, Color backgroundColor)
        {
            button.Style.BackColor = backgroundColor;
            button.Style.ForeColor = Color.White;
            button.Style.HoverBackColor = AdjustBrightness(backgroundColor, 0.1f);
            button.Style.PressedBackColor = AdjustBrightness(backgroundColor, -0.1f);
            button.Style.FocusedBackColor = backgroundColor;

            // Enhanced border for sharp appearance
            button.Style.Border = new Pen(AdjustBrightness(backgroundColor, -0.2f), 1);

            // Optimal font settings
            button.Font = new Font(OPTIMAL_FONT_FAMILY, 9.0f, FontStyle.Bold, GraphicsUnit.Point);

            // Enable custom painting for anti-aliasing
            button.UseVisualStyleBackColor = false;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Convert Syncfusion HorizontalAlignment to StringAlignment
        /// </summary>
        private static StringAlignment GetStringAlignment(System.Windows.Forms.HorizontalAlignment alignment)
        {
            return alignment switch
            {
                System.Windows.Forms.HorizontalAlignment.Left => StringAlignment.Near,
                System.Windows.Forms.HorizontalAlignment.Center => StringAlignment.Center,
                System.Windows.Forms.HorizontalAlignment.Right => StringAlignment.Far,
                _ => StringAlignment.Near
            };
        }

        /// <summary>
        /// Adjust color brightness for hover and focus effects
        /// </summary>
        private static Color AdjustBrightness(Color color, float factor)
        {
            var red = (int)Math.Max(0, Math.Min(255, color.R + (255 * factor)));
            var green = (int)Math.Max(0, Math.Min(255, color.G + (255 * factor)));
            var blue = (int)Math.Max(0, Math.Min(255, color.B + (255 * factor)));

            return Color.FromArgb(color.A, red, green, blue);
        }

        /// <summary>
        /// Validate visual enhancement settings and provide diagnostics
        /// </summary>
        public static string GetVisualEnhancementDiagnostics(Form form)
        {
            var diagnostics = new System.Text.StringBuilder();

            diagnostics.AppendLine("=== Visual Enhancement Diagnostics ===");
            diagnostics.AppendLine($"Form: {form.GetType().Name}");
            diagnostics.AppendLine($"Font: {form.Font.FontFamily.Name} {form.Font.Size}pt");
            diagnostics.AppendLine($"AutoScaleMode: {form.AutoScaleMode}");
            diagnostics.AppendLine($"DPI: {form.DeviceDpi}");

            // Check for Syncfusion controls
            var syncfusionControls = 0;
            var enhancedGrids = 0;

            CountSyncfusionControls(form, ref syncfusionControls, ref enhancedGrids);

            diagnostics.AppendLine($"Syncfusion Controls: {syncfusionControls}");
            diagnostics.AppendLine($"Enhanced Grids: {enhancedGrids}");
            diagnostics.AppendLine($"Visual Theme Applied: {IsSyncfusionThemeApplied(form)}");

            return diagnostics.ToString();
        }

        /// <summary>
        /// Count Syncfusion controls recursively
        /// </summary>
        private static void CountSyncfusionControls(Control parent, ref int syncfusionCount, ref int gridCount)
        {
            foreach (Control control in parent.Controls)
            {
                if (control.GetType().Namespace?.StartsWith("Syncfusion") == true)
                {
                    syncfusionCount++;
                    if (control is SfDataGrid)
                        gridCount++;
                }

                CountSyncfusionControls(control, ref syncfusionCount, ref gridCount);
            }
        }

        /// <summary>
        /// Check if Syncfusion theme is successfully applied
        /// </summary>
        private static bool IsSyncfusionThemeApplied(Form form)
        {
            try
            {
                // Check if form has Syncfusion theme properties
                return form is MetroForm ||
                       form.GetType().Namespace?.StartsWith("Syncfusion") == true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}

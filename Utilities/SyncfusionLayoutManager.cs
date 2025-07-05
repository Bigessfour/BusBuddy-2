using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using Syncfusion.WinForms.DataGrid.Styles;
using Syncfusion.Windows.Forms.Tools;
using System.Drawing;
using System.Windows.Forms;

namespace Bus_Buddy.Utilities
{
    /// <summary>
    /// Syncfusion Layout Manager for consistent grid formatting and form layout management
    /// Based on Syncfusion Essential Studio Version 30.1.37 documentation
    /// </summary>
    public static class SyncfusionLayoutManager
    {
        #region Grid Configuration Constants

        // Standard spacing and sizing
        public const int STANDARD_PADDING = 20;
        public const int CONTROL_SPACING = 10;
        public const int BUTTON_HEIGHT = 40;
        public const int BUTTON_WIDTH = 120;
        public const int HEADER_HEIGHT = 70;
        public const int CONTROL_PANEL_HEIGHT = 120;
        public const int STATUS_BAR_HEIGHT = 30;

        // Grid styling colors
        public static readonly Color GRID_BORDER_COLOR = Color.FromArgb(227, 227, 227);
        public static readonly Color HEADER_BACKGROUND = Color.FromArgb(248, 249, 250);
        public static readonly Color HEADER_TEXT_COLOR = Color.FromArgb(68, 68, 68);
        public static readonly Color SELECTION_COLOR = Color.FromArgb(142, 68, 173, 30);
        public static readonly Color CELL_TEXT_COLOR = Color.FromArgb(68, 68, 68);

        // Brand colors
        public static readonly Color PRIMARY_COLOR = Color.FromArgb(142, 68, 173);
        public static readonly Color SUCCESS_COLOR = Color.FromArgb(76, 175, 80);
        public static readonly Color WARNING_COLOR = Color.FromArgb(255, 152, 0);
        public static readonly Color DANGER_COLOR = Color.FromArgb(244, 67, 54);
        public static readonly Color INFO_COLOR = Color.FromArgb(33, 150, 243);

        #endregion

        #region Grid Configuration Methods

        /// <summary>
        /// Configure SfDataGrid with optimal settings for Bus Buddy application
        /// Based on Syncfusion documentation for column sizing and alignment
        /// Enhanced with visual optimization features
        /// IMPLEMENTS DOCUMENTED SYNCFUSION METHODS
        /// </summary>
        public static void ConfigureSfDataGrid(SfDataGrid dataGrid, bool enableFullScreen = true, bool enableVisualEnhancements = true)
        {
            // Validate parameters
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid), "DataGrid cannot be null");

            // Basic grid configuration - DOCUMENTED METHODS
            dataGrid.AllowEditing = false;
            dataGrid.AllowDeleting = false;
            dataGrid.AllowSorting = true;
            dataGrid.AllowFiltering = true;
            dataGrid.AllowResizingColumns = true;
            dataGrid.AllowDraggingColumns = true;
            dataGrid.AllowGrouping = true;

            // Enhanced grid features - DOCUMENTED METHODS
            dataGrid.EditMode = EditMode.SingleClick;
            dataGrid.ValidationMode = GridValidationMode.InView;
            dataGrid.NavigationMode = Syncfusion.WinForms.DataGrid.Enums.NavigationMode.Cell;
            dataGrid.ShowGroupDropArea = true;
            dataGrid.ShowToolTip = true;
            dataGrid.ShowHeaderToolTip = true;
            dataGrid.ShowSortNumbers = true;
            dataGrid.AllowTriStateSorting = true;

            // Auto-sizing configuration for optimal column layout
            dataGrid.AutoSizeColumnsMode = AutoSizeColumnsMode.Fill;
            dataGrid.AutoSizeController.AutoSizeRange = AutoSizeRange.VisibleRows;
            dataGrid.AutoSizeController.AutoSizeCalculationMode = AutoSizeCalculationMode.Default;

            // Selection and display settings
            dataGrid.SelectionMode = GridSelectionMode.Single;
            dataGrid.SelectionUnit = SelectionUnit.Row;
            dataGrid.ShowRowHeader = false;
            dataGrid.ShowBusyIndicator = true;

            // Header configuration
            dataGrid.HeaderRowHeight = 35;
            dataGrid.RowHeight = 30;

            // Apply styling (enhanced or standard)
            if (enableVisualEnhancements)
            {
                // Apply enhanced visual styling using the VisualEnhancementManager
                VisualEnhancementManager.ApplyEnhancedGridVisuals(dataGrid);
            }
            else
            {
                // Apply standard styling
                ApplyGridStyling(dataGrid);
            }

            // Enable full screen optimization if requested
            if (enableFullScreen)
            {
                System.Diagnostics.Debug.WriteLine("ConfigureSfDataGrid: Calling ConfigureForFullScreen");
                ConfigureForFullScreen(dataGrid);
                System.Diagnostics.Debug.WriteLine($"ConfigureSfDataGrid: After ConfigureForFullScreen, Dock = {dataGrid.Dock}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ConfigureSfDataGrid: Full screen not enabled, skipping ConfigureForFullScreen");
            }
        }

        /// <summary>
        /// Apply consistent styling to SfDataGrid based on Syncfusion documentation
        /// </summary>
        public static void ApplyGridStyling(SfDataGrid dataGrid)
        {
            // Validate parameters
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid), "DataGrid cannot be null");

            // Border and general appearance
            dataGrid.Style.BorderColor = GRID_BORDER_COLOR;
            dataGrid.Style.BorderStyle = BorderStyle.FixedSingle;

            // Header styling
            dataGrid.Style.HeaderStyle.BackColor = HEADER_BACKGROUND;
            dataGrid.Style.HeaderStyle.TextColor = HEADER_TEXT_COLOR;
            dataGrid.Style.HeaderStyle.Font.Facename = "Segoe UI";
            dataGrid.Style.HeaderStyle.Font.Size = 9F;
            dataGrid.Style.HeaderStyle.Font.Bold = true;
            dataGrid.Style.HeaderStyle.HorizontalAlignment = HorizontalAlignment.Center;
            dataGrid.Style.HeaderStyle.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;

            // Cell styling
            dataGrid.Style.CellStyle.Font.Facename = "Segoe UI";
            dataGrid.Style.CellStyle.Font.Size = 9F;
            dataGrid.Style.CellStyle.TextColor = CELL_TEXT_COLOR;
            dataGrid.Style.CellStyle.HorizontalAlignment = HorizontalAlignment.Left;
            dataGrid.Style.CellStyle.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;

            // Selection styling
            dataGrid.Style.SelectionStyle.BackColor = SELECTION_COLOR;
            dataGrid.Style.SelectionStyle.TextColor = CELL_TEXT_COLOR;

            // CheckBox styling for consistent appearance
            dataGrid.Style.CheckBoxStyle.CheckedBackColor = PRIMARY_COLOR;
            dataGrid.Style.CheckBoxStyle.UncheckedBackColor = Color.White;
            dataGrid.Style.CheckBoxStyle.CheckedBorderColor = PRIMARY_COLOR;
            dataGrid.Style.CheckBoxStyle.UncheckedBorderColor = GRID_BORDER_COLOR;
        }

        /// <summary>
        /// Configure grid for full screen display optimization
        /// </summary>
        public static void ConfigureForFullScreen(SfDataGrid dataGrid)
        {
            // Validate parameters
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid), "DataGrid cannot be null");

            System.Diagnostics.Debug.WriteLine($"ConfigureForFullScreen: Before setting Dock, current value = {dataGrid.Dock}");

            // Optimize for full screen viewing
            dataGrid.Dock = DockStyle.Fill;
            System.Diagnostics.Debug.WriteLine($"ConfigureForFullScreen: After setting Dock = Fill, current value = {dataGrid.Dock}");

            dataGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Enable data virtualization for performance with large datasets
            dataGrid.EnableDataVirtualization = true;

            // Optimize scrolling for full screen
            dataGrid.AutoSizeController.AutoSizeRange = AutoSizeRange.VisibleRows;
        }

        /// <summary>
        /// Configure specific column alignment and formatting
        /// </summary>
        public static void ConfigureColumnAlignment(SfDataGrid dataGrid, string columnName,
            HorizontalAlignment alignment, string? format = null, int? width = null)
        {
            if (dataGrid.Columns[columnName] != null)
            {
                var column = dataGrid.Columns[columnName];

                // Set alignment
                column.CellStyle.HorizontalAlignment = alignment;
                column.HeaderStyle.HorizontalAlignment = alignment;

                // Set format if provided
                if (!string.IsNullOrEmpty(format))
                {
                    column.Format = format;
                }

                // Set width if provided
                if (width.HasValue)
                {
                    column.Width = width.Value;
                    column.AutoSizeColumnsMode = AutoSizeColumnsMode.None;
                }
            }
        }

        #endregion

        #region Form Layout Management

        /// <summary>
        /// Configure form for optimal full screen display
        /// </summary>
        public static void ConfigureFormForFullScreen(Form form)
        {
            // Validate parameters
            if (form == null)
                throw new ArgumentNullException(nameof(form), "Form cannot be null");

            // Set form properties for optimal full screen experience
            form.WindowState = FormWindowState.Maximized;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimumSize = new Size(1200, 800);

            // Enable responsive scaling
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.AutoScaleDimensions = new SizeF(96F, 96F);

            // Set font for consistency
            form.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        }

        /// <summary>
        /// Configure panel layout using TableLayoutPanel for consistent alignment
        /// </summary>
        public static TableLayoutPanel CreateResponsiveTableLayout(int columns, int rows)
        {
            var tableLayout = new TableLayoutPanel
            {
                ColumnCount = columns,
                RowCount = rows,
                Dock = DockStyle.Fill,
                Padding = new Padding(STANDARD_PADDING),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // Set equal column widths
            for (int i = 0; i < columns; i++)
            {
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            }

            // Set row heights
            for (int i = 0; i < rows; i++)
            {
                tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            return tableLayout;
        }

        /// <summary>
        /// Configure GradientPanel with consistent styling
        /// </summary>
        public static void ConfigureGradientPanel(GradientPanel panel, Color? backgroundColor = null)
        {
            if (panel == null)
                throw new ArgumentNullException(nameof(panel), "GradientPanel cannot be null");

            panel.BorderStyle = BorderStyle.None;

            if (backgroundColor.HasValue)
            {
                panel.BackgroundColor = new Syncfusion.Drawing.BrushInfo(backgroundColor.Value);
            }
            else
            {
                panel.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.White);
            }
        }

        #endregion

        #region Button Configuration

        /// <summary>
        /// Configure Syncfusion SfButton with consistent styling
        /// </summary>
        public static void ConfigureSfButton(Syncfusion.WinForms.Controls.SfButton button,
            string text, Color backgroundColor, Point location, int tabIndex = 0)
        {
            button.Text = text;
            button.Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT);
            button.Location = location;
            button.TabIndex = tabIndex;

            // Style configuration
            button.Style.BackColor = backgroundColor;
            button.Style.ForeColor = Color.White;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.UseVisualStyleBackColor = false;

            // Set anchor for responsive design
            button.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        }

        #endregion

        #region Specific Grid Configurations

        /// <summary>
        /// Configure Bus Management grid with optimal column settings
        /// </summary>
        public static void ConfigureBusManagementGrid(SfDataGrid dataGrid)
        {
            ConfigureSfDataGrid(dataGrid, true);

            // Configure specific columns after data binding
            dataGrid.DataSourceChanged += (sender, e) =>
            {
                ConfigureColumnAlignment(dataGrid, "VehicleId", HorizontalAlignment.Center, null, 80);
                ConfigureColumnAlignment(dataGrid, "VehicleNumber", HorizontalAlignment.Center, null, 120);
                ConfigureColumnAlignment(dataGrid, "Capacity", HorizontalAlignment.Center, null, 80);
                ConfigureColumnAlignment(dataGrid, "IsActive", HorizontalAlignment.Center, null, 80);
                ConfigureColumnAlignment(dataGrid, "LastMaintenance", HorizontalAlignment.Center, "MM/dd/yyyy", 120);
                ConfigureColumnAlignment(dataGrid, "Mileage", HorizontalAlignment.Right, "N0", 100);
            };
        }

        /// <summary>
        /// Configure Ticket Management grid with optimal column settings
        /// </summary>
        public static void ConfigureTicketManagementGrid(SfDataGrid dataGrid)
        {
            ConfigureSfDataGrid(dataGrid, true);

            // Configure specific columns after data binding
            dataGrid.DataSourceChanged += (sender, e) =>
            {
                ConfigureColumnAlignment(dataGrid, "Id", HorizontalAlignment.Center, null, 80);
                ConfigureColumnAlignment(dataGrid, "TravelDate", HorizontalAlignment.Center, "MM/dd/yyyy", 100);
                ConfigureColumnAlignment(dataGrid, "IssuedDate", HorizontalAlignment.Center, "MM/dd/yyyy HH:mm", 130);
                ConfigureColumnAlignment(dataGrid, "Price", HorizontalAlignment.Right, "C2", 80);
                ConfigureColumnAlignment(dataGrid, "Status", HorizontalAlignment.Center, null, 80);
                ConfigureColumnAlignment(dataGrid, "PaymentMethod", HorizontalAlignment.Center, null, 100);
            };
        }

        #endregion
    }
}

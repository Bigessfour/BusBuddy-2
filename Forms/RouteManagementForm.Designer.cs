using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using System.Windows.Forms;

namespace Bus_Buddy.Forms;

partial class RouteManagementForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    // Declare Syncfusion controls
    private GradientPanel mainPanel;
    private GradientPanel headerPanel;
    private GradientPanel contentPanel;
    private GradientPanel buttonPanel;
    private AutoLabel titleLabel;
    private AutoLabel statusLabel;

    // Data grid for displaying routes - Using Syncfusion SfDataGrid
    private SfDataGrid routeDataGrid;

    // Action buttons
    private SfButton addRouteButton;
    private SfButton editRouteButton;
    private SfButton deleteRouteButton;
    private SfButton viewStopsButton;
    private SfButton refreshButton;
    private SfButton closeButton;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();

        // Suspend layout for better performance during initialization
        this.SuspendLayout();

        // Initialize Syncfusion components
        this.mainPanel = new Syncfusion.Windows.Forms.Tools.GradientPanel();
        this.headerPanel = new Syncfusion.Windows.Forms.Tools.GradientPanel();
        this.contentPanel = new Syncfusion.Windows.Forms.Tools.GradientPanel();
        this.buttonPanel = new Syncfusion.Windows.Forms.Tools.GradientPanel();
        this.titleLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();
        this.statusLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel();

        // Initialize data grid - Using Syncfusion SfDataGrid
        this.routeDataGrid = new Syncfusion.WinForms.DataGrid.SfDataGrid();
        this.routeDataGrid.BeginInit();

        // Initialize buttons
        this.addRouteButton = new Syncfusion.WinForms.Controls.SfButton();
        this.editRouteButton = new Syncfusion.WinForms.Controls.SfButton();
        this.deleteRouteButton = new Syncfusion.WinForms.Controls.SfButton();
        this.viewStopsButton = new Syncfusion.WinForms.Controls.SfButton();
        this.refreshButton = new Syncfusion.WinForms.Controls.SfButton();
        this.closeButton = new Syncfusion.WinForms.Controls.SfButton();

        // Main Panel
        this.mainPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.mainPanel.Location = new System.Drawing.Point(0, 0);
        this.mainPanel.Name = "mainPanel";
        this.mainPanel.Size = new System.Drawing.Size(1200, 700);
        this.mainPanel.TabIndex = 0;

        // Header Panel
        this.headerPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.headerPanel.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(255, 87, 34));
        this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.headerPanel.Location = new System.Drawing.Point(0, 0);
        this.headerPanel.Name = "headerPanel";
        this.headerPanel.Size = new System.Drawing.Size(1200, 60);
        this.headerPanel.TabIndex = 1;

        // Title Label
        this.titleLabel.BackColor = System.Drawing.Color.Transparent;
        this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
        this.titleLabel.ForeColor = System.Drawing.Color.White;
        this.titleLabel.Location = new System.Drawing.Point(20, 15);
        this.titleLabel.Name = "titleLabel";
        this.titleLabel.Size = new System.Drawing.Size(300, 30);
        this.titleLabel.TabIndex = 0;
        this.titleLabel.Text = "Route Management - Bus Routes";

        // Button Panel
        this.buttonPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.buttonPanel.Location = new System.Drawing.Point(0, 640);
        this.buttonPanel.Name = "buttonPanel";
        this.buttonPanel.Size = new System.Drawing.Size(1200, 60);
        this.buttonPanel.TabIndex = 2;
        this.buttonPanel.Padding = new System.Windows.Forms.Padding(10);

        // Content Panel
        this.contentPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.contentPanel.Location = new System.Drawing.Point(0, 60);
        this.contentPanel.Name = "contentPanel";
        this.contentPanel.Size = new System.Drawing.Size(1200, 580);
        this.contentPanel.TabIndex = 3;
        this.contentPanel.Padding = new System.Windows.Forms.Padding(20);

        // Status Label
        this.statusLabel.BackColor = System.Drawing.Color.Transparent;
        this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.statusLabel.ForeColor = System.Drawing.Color.Gray;
        this.statusLabel.Location = new System.Drawing.Point(30, 20);
        this.statusLabel.Name = "statusLabel";
        this.statusLabel.Size = new System.Drawing.Size(300, 20);
        this.statusLabel.TabIndex = 1;
        this.statusLabel.Text = "Loading...";

        // Route Data Grid - Syncfusion SfDataGrid Configuration
        this.routeDataGrid.AllowEditing = false;
        this.routeDataGrid.AllowDeleting = false;
        this.routeDataGrid.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.Fill;
        this.routeDataGrid.BackColor = System.Drawing.Color.White;
        this.routeDataGrid.HeaderRowHeight = 30;
        this.routeDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.routeDataGrid.Location = new System.Drawing.Point(20, 40);
        this.routeDataGrid.Name = "routeDataGrid";
        this.routeDataGrid.ShowRowHeader = false;
        this.routeDataGrid.SelectionMode = Syncfusion.WinForms.DataGrid.Enums.GridSelectionMode.Single;
        this.routeDataGrid.SelectionUnit = Syncfusion.WinForms.DataGrid.Enums.SelectionUnit.Row;
        this.routeDataGrid.Size = new System.Drawing.Size(1160, 520);
        this.routeDataGrid.TabIndex = 0;
        this.routeDataGrid.Style.BorderColor = System.Drawing.Color.FromArgb(227, 227, 227);
        this.routeDataGrid.Style.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

        // Configure action buttons
        ConfigureActionButton(this.addRouteButton, "Add Route", 20, 15, 0, System.Drawing.Color.FromArgb(76, 175, 80));
        ConfigureActionButton(this.editRouteButton, "Edit Route", 140, 15, 1, System.Drawing.Color.FromArgb(255, 152, 0));
        ConfigureActionButton(this.deleteRouteButton, "Delete Route", 260, 15, 2, System.Drawing.Color.FromArgb(244, 67, 54));
        ConfigureActionButton(this.viewStopsButton, "View Stops", 380, 15, 3, System.Drawing.Color.FromArgb(255, 87, 34));
        ConfigureActionButton(this.refreshButton, "Refresh", 500, 15, 4, System.Drawing.Color.FromArgb(33, 150, 243));
        ConfigureActionButton(this.closeButton, "Close", 1080, 15, 5, System.Drawing.Color.FromArgb(158, 158, 158));

        // Initially disable edit/delete/view buttons until selection is made
        this.editRouteButton.Enabled = false;
        this.deleteRouteButton.Enabled = false;
        this.viewStopsButton.Enabled = false;

        // Add event handlers
        this.addRouteButton.Click += new System.EventHandler(this.AddRouteButton_Click);
        this.editRouteButton.Click += new System.EventHandler(this.EditRouteButton_Click);
        this.deleteRouteButton.Click += new System.EventHandler(this.DeleteRouteButton_Click);
        this.viewStopsButton.Click += new System.EventHandler(this.ViewStopsButton_Click);
        this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
        this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);

        // Add controls to panels
        this.headerPanel.Controls.Add(this.titleLabel);

        this.contentPanel.Controls.Add(this.routeDataGrid);
        this.contentPanel.Controls.Add(this.statusLabel);

        this.buttonPanel.Controls.Add(this.addRouteButton);
        this.buttonPanel.Controls.Add(this.editRouteButton);
        this.buttonPanel.Controls.Add(this.deleteRouteButton);
        this.buttonPanel.Controls.Add(this.viewStopsButton);
        this.buttonPanel.Controls.Add(this.refreshButton);
        this.buttonPanel.Controls.Add(this.closeButton);

        this.mainPanel.Controls.Add(this.contentPanel);
        this.mainPanel.Controls.Add(this.buttonPanel);
        this.mainPanel.Controls.Add(this.headerPanel);

        // Form settings
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.ClientSize = new System.Drawing.Size(1200, 700);
        this.Controls.Add(this.mainPanel);
        this.Text = "Route Management";
        this.MinimumSize = new System.Drawing.Size(1000, 600);
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

        // Apply Syncfusion theme integration
        try
        {
            // Set Office2016 visual style using SkinManager
            Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(this, Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);
        }
        catch (System.Exception)
        {
            // Could not apply theme
        }

        // Set Syncfusion MetroForm styles
        this.MetroColor = System.Drawing.Color.FromArgb(255, 87, 34);
        this.CaptionBarColor = System.Drawing.Color.FromArgb(255, 87, 34);
        this.CaptionForeColor = System.Drawing.Color.White;
        this.Text = "Route Management - Bus Routes";

        // Configure data grid columns
        this.routeDataGrid.AutoGenerateColumns = false;
        this.routeDataGrid.Columns.Clear();

        // Apply Office2016 styling to the grid
        this.routeDataGrid.Style.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(255, 87, 34);
        this.routeDataGrid.Style.HeaderStyle.TextColor = System.Drawing.Color.White;
        this.routeDataGrid.Style.HeaderStyle.Font.Bold = true;
        this.routeDataGrid.Style.BorderColor = System.Drawing.Color.FromArgb(227, 227, 227);
        this.routeDataGrid.Style.SelectionStyle.BackColor = System.Drawing.Color.FromArgb(255, 87, 34, 50);
        this.routeDataGrid.Style.SelectionStyle.TextColor = System.Drawing.Color.Black;

        // Add custom columns using Syncfusion GridTextColumn
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "RouteId", HeaderText = "Route ID", Width = 80, AllowEditing = false });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "RouteName", HeaderText = "Route Name", Width = 200, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "Description", HeaderText = "Description", Width = 300, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "AMVehicleId", HeaderText = "AM Vehicle ID", Width = 100, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "PMVehicleId", HeaderText = "PM Vehicle ID", Width = 100, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "AMDriverId", HeaderText = "AM Driver ID", Width = 100, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "PMDriverId", HeaderText = "PM Driver ID", Width = 100, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "AMBeginMiles", HeaderText = "AM Begin Miles", Width = 100, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "AMEndMiles", HeaderText = "AM End Miles", Width = 100, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "AMRiders", HeaderText = "AM Riders", Width = 80, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "PMBeginMiles", HeaderText = "PM Begin Miles", Width = 100, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "PMEndMiles", HeaderText = "PM End Miles", Width = 100, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridTextColumn() { MappingName = "PMRiders", HeaderText = "PM Riders", Width = 80, AllowSorting = true });
        this.routeDataGrid.Columns.Add(new GridCheckBoxColumn() { MappingName = "IsActive", HeaderText = "Active", Width = 60, AllowSorting = true });

        // Enable advanced grid features
        this.routeDataGrid.AllowSorting = true;
        this.routeDataGrid.AllowFiltering = false; // Disable built-in filtering for custom implementation
        this.routeDataGrid.AllowResizingColumns = true;
        this.routeDataGrid.ShowRowHeader = false;
        this.routeDataGrid.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.None;

        // Setup SfDataGrid event handlers
        this.routeDataGrid.SelectionChanged += new Syncfusion.WinForms.DataGrid.Events.SelectionChangedEventHandler(this.RouteDataGrid_SelectionChanged);
        this.routeDataGrid.CellDoubleClick += new Syncfusion.WinForms.DataGrid.Events.CellClickEventHandler(this.RouteDataGrid_CellDoubleClick);
        this.routeDataGrid.QueryRowStyle += new Syncfusion.WinForms.DataGrid.Events.QueryRowStyleEventHandler(this.RouteDataGrid_QueryRowStyle);

        // Complete data grid initialization
        this.routeDataGrid.EndInit();

        // Resume layout
        this.ResumeLayout(false);
    }

    private void ConfigureActionButton(SfButton button, string title, int x, int y, int tabIndex, System.Drawing.Color color)
    {
        button.AccessibleName = title;
        button.BackColor = color;
        button.ForeColor = System.Drawing.Color.White;
        button.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        button.Location = new System.Drawing.Point(x, y);
        button.Name = title.Replace(" ", "").ToLower() + "Button";
        button.Size = new System.Drawing.Size(100, 30);
        button.TabIndex = tabIndex;
        button.Text = title;
        button.UseVisualStyleBackColor = false;
    }

    #endregion
}

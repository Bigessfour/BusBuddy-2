using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using System.Windows.Forms;

namespace Bus_Buddy.Forms;

partial class BusManagementForm
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

    // Data grid for displaying buses - Using Syncfusion SfDataGrid
    private SfDataGrid busDataGrid;

    // Action buttons
    private SfButton addBusButton;
    private SfButton editBusButton;
    private SfButton deleteBusButton;
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
        this.busDataGrid = new Syncfusion.WinForms.DataGrid.SfDataGrid();
        this.busDataGrid.BeginInit();

        // Initialize buttons
        this.addBusButton = new Syncfusion.WinForms.Controls.SfButton();
        this.editBusButton = new Syncfusion.WinForms.Controls.SfButton();
        this.deleteBusButton = new Syncfusion.WinForms.Controls.SfButton();
        this.refreshButton = new Syncfusion.WinForms.Controls.SfButton();
        this.closeButton = new Syncfusion.WinForms.Controls.SfButton();

        // Main Panel
        this.mainPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.mainPanel.Location = new System.Drawing.Point(0, 0);
        this.mainPanel.Name = "mainPanel";
        this.mainPanel.Size = new System.Drawing.Size(1000, 700);
        this.mainPanel.TabIndex = 0;

        // Header Panel
        this.headerPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.headerPanel.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(63, 81, 181));
        this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.headerPanel.Location = new System.Drawing.Point(0, 0);
        this.headerPanel.Name = "headerPanel";
        this.headerPanel.Size = new System.Drawing.Size(1000, 60);
        this.headerPanel.TabIndex = 1;

        // Title Label
        this.titleLabel.BackColor = System.Drawing.Color.Transparent;
        this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
        this.titleLabel.ForeColor = System.Drawing.Color.White;
        this.titleLabel.Location = new System.Drawing.Point(20, 15);
        this.titleLabel.Name = "titleLabel";
        this.titleLabel.Size = new System.Drawing.Size(300, 30);
        this.titleLabel.TabIndex = 0;
        this.titleLabel.Text = "Bus Management - Fleet Vehicles";

        // Button Panel
        this.buttonPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.buttonPanel.Location = new System.Drawing.Point(0, 640);
        this.buttonPanel.Name = "buttonPanel";
        this.buttonPanel.Size = new System.Drawing.Size(1000, 60);
        this.buttonPanel.TabIndex = 2;
        this.buttonPanel.Padding = new System.Windows.Forms.Padding(10);

        // Content Panel
        this.contentPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.contentPanel.Location = new System.Drawing.Point(0, 60);
        this.contentPanel.Name = "contentPanel";
        this.contentPanel.Size = new System.Drawing.Size(1000, 580);
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

        // Bus Data Grid - Syncfusion SfDataGrid Configuration
        this.busDataGrid.AllowEditing = false;
        this.busDataGrid.AllowDeleting = false;
        this.busDataGrid.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.Fill;
        this.busDataGrid.BackColor = System.Drawing.Color.White;
        this.busDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.busDataGrid.Location = new System.Drawing.Point(20, 40);
        this.busDataGrid.Name = "busDataGrid";
        this.busDataGrid.ShowRowHeader = false;
        this.busDataGrid.SelectionMode = Syncfusion.WinForms.DataGrid.Enums.GridSelectionMode.Single;
        this.busDataGrid.SelectionUnit = Syncfusion.WinForms.DataGrid.Enums.SelectionUnit.Row;
        this.busDataGrid.Size = new System.Drawing.Size(960, 520);
        this.busDataGrid.TabIndex = 0;
        this.busDataGrid.Style.BorderColor = System.Drawing.Color.FromArgb(227, 227, 227);
        this.busDataGrid.Style.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

        // Configure action buttons
        ConfigureActionButton(this.addBusButton, "Add Bus", 20, 15, 0, System.Drawing.Color.FromArgb(76, 175, 80));
        ConfigureActionButton(this.editBusButton, "Edit Bus", 140, 15, 1, System.Drawing.Color.FromArgb(255, 152, 0));
        ConfigureActionButton(this.deleteBusButton, "Delete Bus", 260, 15, 2, System.Drawing.Color.FromArgb(244, 67, 54));
        ConfigureActionButton(this.refreshButton, "Refresh", 380, 15, 3, System.Drawing.Color.FromArgb(33, 150, 243));
        ConfigureActionButton(this.closeButton, "Close", 860, 15, 4, System.Drawing.Color.FromArgb(158, 158, 158));

        // Add event handlers
        this.addBusButton.Click += new System.EventHandler(this.AddBusButton_Click);
        this.editBusButton.Click += new System.EventHandler(this.EditBusButton_Click);
        this.deleteBusButton.Click += new System.EventHandler(this.DeleteBusButton_Click);
        this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
        this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);

        // Add controls to panels
        this.headerPanel.Controls.Add(this.titleLabel);

        this.contentPanel.Controls.Add(this.busDataGrid);
        this.contentPanel.Controls.Add(this.statusLabel);

        this.buttonPanel.Controls.Add(this.addBusButton);
        this.buttonPanel.Controls.Add(this.editBusButton);
        this.buttonPanel.Controls.Add(this.deleteBusButton);
        this.buttonPanel.Controls.Add(this.refreshButton);
        this.buttonPanel.Controls.Add(this.closeButton);

        this.mainPanel.Controls.Add(this.contentPanel);
        this.mainPanel.Controls.Add(this.buttonPanel);
        this.mainPanel.Controls.Add(this.headerPanel);

        // Form settings
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.ClientSize = new System.Drawing.Size(1000, 700);
        this.Controls.Add(this.mainPanel);
        this.Text = "Bus Management";
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

        // Complete data grid initialization
        this.busDataGrid.EndInit();

        // Resume layout
        this.ResumeLayout(false);

        // Apply enhanced visual theme system
        Utilities.VisualEnhancementManager.ApplyEnhancedTheme(this);

        // Configure form for full screen using layout manager
        Utilities.SyncfusionLayoutManager.ConfigureFormForFullScreen(this);

        // Apply Syncfusion theme integration with enhanced visuals
        try
        {
            // Set Office2016 visual style using SkinManager
            Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(this, Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);
        }
        catch
        {
            // Fallback styling is handled by other managers
        }

        // Set enhanced MetroForm styles
        this.MetroColor = System.Drawing.Color.FromArgb(46, 125, 185);
        this.CaptionBarColor = System.Drawing.Color.FromArgb(46, 125, 185);
        this.CaptionForeColor = System.Drawing.Color.White;
        this.Text = "Bus Management - Fleet Vehicles";

        // Configure the data grid with standardized configuration
        Utilities.SyncfusionLayoutManager.ConfigureSfDataGrid(busDataGrid, true, true);
        Utilities.SyncfusionAdvancedManager.ApplyAdvancedConfiguration(busDataGrid);
        Utilities.SyncfusionLayoutManager.ConfigureBusManagementGrid(busDataGrid);
        Utilities.VisualEnhancementManager.ApplyEnhancedGridVisuals(busDataGrid);
        Utilities.SyncfusionLayoutManager.ApplyGridStyling(busDataGrid);

        // Enable high-quality font rendering
        Utilities.VisualEnhancementManager.EnableHighQualityFontRendering(this);
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

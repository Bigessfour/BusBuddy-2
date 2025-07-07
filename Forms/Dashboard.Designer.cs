using Syncfusion.Windows.Forms.Tools;
using System.Windows.Forms;
using Bus_Buddy.Utilities;

namespace Bus_Buddy;

partial class Dashboard
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Designer Controls
    // Note: MainTabControl and primary tabs are defined in Dashboard.cs
    // This designer file contains only the layout and initialization code
    private TabControlAdv mainTabControl;
    private TabPageAdv fleetTab;
    private TabPageAdv routesTab;
    private TabPageAdv maintenanceTab;
    private TabPageAdv studentsTab;
    private TabPageAdv reportsTab;
    private TabPageAdv aiAssistantTab;
    private TabPageAdv analyticsTab;
    private TabPageAdv settingsTab;
    private TabPageAdv geeMapsTab;

    // Dashboard controls
    private Label subtitleLabel;
    #endregion

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();

        // Form settings
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.ClientSize = new System.Drawing.Size(1200, 800);
        this.Text = "Bus Buddy - Dashboard";
        this.MinimumSize = new System.Drawing.Size(1000, 600);
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

        // Apply enhanced visual theme system
        VisualEnhancementManager.ApplyEnhancedTheme(this);

        // Set Syncfusion MetroForm styles with enhanced colors
        this.MetroColor = System.Drawing.Color.FromArgb(46, 125, 185);
        this.CaptionBarColor = System.Drawing.Color.FromArgb(46, 125, 185);
        this.CaptionForeColor = System.Drawing.Color.White;

        // Create main tab control
        mainTabControl = new TabControlAdv
        {
            Location = new System.Drawing.Point(10, 10),
            Size = new System.Drawing.Size(1180, 700),
            Dock = DockStyle.Fill,
            TabStyle = typeof(TabRendererMetro),
            ThemeName = "Metro"
        };

        // Create Fleet Management tab
        fleetTab = new TabPageAdv("Fleet Management");

        // Create Routes tab
        routesTab = new TabPageAdv("Routes");

        // Create Maintenance tab
        maintenanceTab = new TabPageAdv("Maintenance");

        // Create Students tab
        studentsTab = new TabPageAdv("Students");

        // Create Reports tab
        reportsTab = new TabPageAdv("Reports");

        // Create Analytics tab with Enhanced Dashboard Analytics
        analyticsTab = new TabPageAdv("Analytics");

        // Create AI Assistant tab
        aiAssistantTab = new TabPageAdv("AI Assistant");

        // Create Settings tab
        settingsTab = new TabPageAdv("Settings");

        // Create GEE Maps tab
        geeMapsTab = new TabPageAdv("GEE Maps");

        // Add tabs to control (Analytics first for prominence)
        mainTabControl.TabPages.Add(analyticsTab);
        mainTabControl.TabPages.Add(fleetTab);
        mainTabControl.TabPages.Add(routesTab);
        mainTabControl.TabPages.Add(maintenanceTab);
        mainTabControl.TabPages.Add(studentsTab);
        mainTabControl.TabPages.Add(reportsTab);
        mainTabControl.TabPages.Add(aiAssistantTab);
        mainTabControl.TabPages.Add(settingsTab);
        mainTabControl.TabPages.Add(geeMapsTab);

        // Add tab control to form
        this.Controls.Add(mainTabControl);

        subtitleLabel = new Label
        {
            Text = "Loading fleet information...",
            Location = new System.Drawing.Point(20, 50),
            Size = new System.Drawing.Size(700, 30),
            Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular),
            ForeColor = System.Drawing.Color.FromArgb(102, 102, 102),
            AutoSize = false,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };

        this.Controls.Add(subtitleLabel);
    }

    #endregion
}

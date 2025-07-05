using System;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Themes;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms;

namespace BusBuddy
{
    public class MainForm : SfRibbonForm
    {
        private SfRibbonControlAdv ribbon;
        private SfStatusBar statusBar;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Enable High DPI
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.EnableHighDpi = true;
            this.UseCompatibleTextRendering = true;
            this.Text = "Bus Buddy";
            this.MinimumSize = new System.Drawing.Size(1024, 700);

            // Apply Metro/Office2019 theme
            SfSkinManager.ApplyVisualStyle(this, VisualTheme.Office2019DarkGray);

            // Ribbon
            ribbon = new SfRibbonControlAdv();
            ribbon.Dock = DockStyle.Top;
            ribbon.ThemeName = "Office2019DarkGray";
            ribbon.DpiAware = true;
            // TODO: Add ribbon tabs and items here
            this.Controls.Add(ribbon);

            // Status Bar
            statusBar = new SfStatusBar();
            statusBar.Dock = DockStyle.Bottom;
            statusBar.ThemeName = "Office2019DarkGray";
            statusBar.ShowGrip = false;
            statusBar.Items.Add(new ToolStripStatusLabel("Ready"));
            this.Controls.Add(statusBar);
        }
    }
}

using Syncfusion.Windows.Forms.Tools;

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

        // Note: Tab control initialization is handled in Dashboard.cs InitializeDashboard() method
    }

    #endregion
}

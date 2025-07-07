namespace Bus_Buddy.Forms;

partial class StudentManagementForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

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
        if (disposing)
        {
            _selectedStudent = null;
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
        this.panelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
        this.labelTitle = new Syncfusion.Windows.Forms.Tools.AutoLabel();
        this.lblStudentCount = new Syncfusion.Windows.Forms.Tools.AutoLabel();
        this.lblActiveCount = new Syncfusion.Windows.Forms.Tools.AutoLabel();
        this.panelControls = new Syncfusion.Windows.Forms.Tools.GradientPanel();
        this.btnAddStudent = new Syncfusion.WinForms.Controls.SfButton();
        this.btnEditStudent = new Syncfusion.WinForms.Controls.SfButton();
        this.btnViewDetails = new Syncfusion.WinForms.Controls.SfButton();
        this.btnDeleteStudent = new Syncfusion.WinForms.Controls.SfButton();
        this.btnRefresh = new Syncfusion.WinForms.Controls.SfButton();
        this.lblSearch = new Syncfusion.Windows.Forms.Tools.AutoLabel();
        this.txtSearch = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
        this.lblGrade = new Syncfusion.Windows.Forms.Tools.AutoLabel();
        this.cmbGradeFilter = new Syncfusion.WinForms.ListView.SfComboBox();
        this.lblRoute = new Syncfusion.Windows.Forms.Tools.AutoLabel();
        this.cmbRouteFilter = new Syncfusion.WinForms.ListView.SfComboBox();
        this.lblActive = new Syncfusion.Windows.Forms.Tools.AutoLabel();
        this.cmbActiveFilter = new Syncfusion.WinForms.ListView.SfComboBox();
        this.panelGrid = new Syncfusion.Windows.Forms.Tools.GradientPanel();
        this.studentDataGrid = new Syncfusion.WinForms.DataGrid.SfDataGrid();
        ((System.ComponentModel.ISupportInitialize)(this.panelHeader)).BeginInit();
        this.panelHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.panelControls)).BeginInit();
        this.panelControls.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.cmbGradeFilter)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.cmbRouteFilter)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.cmbActiveFilter)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.panelGrid)).BeginInit();
        this.panelGrid.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.studentDataGrid)).BeginInit();
        this.SuspendLayout();
        // 
        // panelHeader
        // 
        this.panelHeader.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250))))));
        this.panelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.panelHeader.Controls.Add(this.labelTitle);
        this.panelHeader.Controls.Add(this.lblStudentCount);
        this.panelHeader.Controls.Add(this.lblActiveCount);
        this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelHeader.Location = new System.Drawing.Point(0, 0);
        this.panelHeader.Name = "panelHeader";
        this.panelHeader.Padding = new System.Windows.Forms.Padding(20, 15, 20, 15);
        this.panelHeader.Size = new System.Drawing.Size(1600, 80);
        this.panelHeader.TabIndex = 0;
        // 
        // labelTitle
        // 
        this.labelTitle.AutoSize = true;
        this.labelTitle.BackColor = System.Drawing.Color.Transparent;
        this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
        this.labelTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
        this.labelTitle.Location = new System.Drawing.Point(20, 25);
        this.labelTitle.Name = "labelTitle";
        this.labelTitle.Size = new System.Drawing.Size(597, 46);
        this.labelTitle.TabIndex = 0;
        this.labelTitle.Text = "🎓 Student Transportation Management";
        // 
        // lblStudentCount
        // 
        this.lblStudentCount.BackColor = System.Drawing.Color.Transparent;
        this.lblStudentCount.Font = new System.Drawing.Font("Segoe UI", 10F);
        this.lblStudentCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
        this.lblStudentCount.Location = new System.Drawing.Point(450, 25);
        this.lblStudentCount.Name = "lblStudentCount";
        this.lblStudentCount.Size = new System.Drawing.Size(150, 20);
        this.lblStudentCount.TabIndex = 1;
        this.lblStudentCount.Text = "Total Students: 0";
        // 
        // lblActiveCount
        // 
        this.lblActiveCount.BackColor = System.Drawing.Color.Transparent;
        this.lblActiveCount.Font = new System.Drawing.Font("Segoe UI", 10F);
        this.lblActiveCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
        this.lblActiveCount.Location = new System.Drawing.Point(450, 45);
        this.lblActiveCount.Name = "lblActiveCount";
        this.lblActiveCount.Size = new System.Drawing.Size(150, 20);
        this.lblActiveCount.TabIndex = 2;
        this.lblActiveCount.Text = "Active: 0";
        // 
        // panelControls
        // 
        this.panelControls.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
        this.panelControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.panelControls.Controls.Add(this.btnAddStudent);
        this.panelControls.Controls.Add(this.btnEditStudent);
        this.panelControls.Controls.Add(this.btnViewDetails);
        this.panelControls.Controls.Add(this.btnDeleteStudent);
        this.panelControls.Controls.Add(this.btnRefresh);
        this.panelControls.Controls.Add(this.lblSearch);
        this.panelControls.Controls.Add(this.txtSearch);
        this.panelControls.Controls.Add(this.lblGrade);
        this.panelControls.Controls.Add(this.cmbGradeFilter);
        this.panelControls.Controls.Add(this.lblRoute);
        this.panelControls.Controls.Add(this.cmbRouteFilter);
        this.panelControls.Controls.Add(this.lblActive);
        this.panelControls.Controls.Add(this.cmbActiveFilter);
        this.panelControls.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelControls.Location = new System.Drawing.Point(0, 80);
        this.panelControls.Name = "panelControls";
        this.panelControls.Padding = new System.Windows.Forms.Padding(20, 15, 20, 15);
        this.panelControls.Size = new System.Drawing.Size(1600, 120);
        this.panelControls.TabIndex = 1;
        // 
        // btnAddStudent
        // 
        this.btnAddStudent.AccessibleName = "Add Student";
        this.btnAddStudent.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.btnAddStudent.Location = new System.Drawing.Point(20, 15);
        this.btnAddStudent.Name = "btnAddStudent";
        this.btnAddStudent.Size = new System.Drawing.Size(140, 40);
        this.btnAddStudent.TabIndex = 0;
        this.btnAddStudent.Text = "➕ Add Student";
        // 
        // btnEditStudent
        // 
        this.btnEditStudent.AccessibleName = "Edit Student";
        this.btnEditStudent.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.btnEditStudent.Location = new System.Drawing.Point(170, 15);
        this.btnEditStudent.Name = "btnEditStudent";
        this.btnEditStudent.Size = new System.Drawing.Size(140, 40);
        this.btnEditStudent.TabIndex = 1;
        this.btnEditStudent.Text = "✏️ Edit Student";
        // 
        // btnViewDetails
        // 
        this.btnViewDetails.AccessibleName = "View Details";
        this.btnViewDetails.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.btnViewDetails.Location = new System.Drawing.Point(320, 15);
        this.btnViewDetails.Name = "btnViewDetails";
        this.btnViewDetails.Size = new System.Drawing.Size(140, 40);
        this.btnViewDetails.TabIndex = 2;
        this.btnViewDetails.Text = "👁️ View Details";
        // 
        // btnDeleteStudent
        // 
        this.btnDeleteStudent.AccessibleName = "Delete Student";
        this.btnDeleteStudent.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.btnDeleteStudent.Location = new System.Drawing.Point(470, 15);
        this.btnDeleteStudent.Name = "btnDeleteStudent";
        this.btnDeleteStudent.Size = new System.Drawing.Size(120, 40);
        this.btnDeleteStudent.TabIndex = 3;
        this.btnDeleteStudent.Text = "🗑️ Delete";
        // 
        // btnRefresh
        // 
        this.btnRefresh.AccessibleName = "Refresh Data";
        this.btnRefresh.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
        this.btnRefresh.Location = new System.Drawing.Point(600, 15);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(120, 40);
        this.btnRefresh.TabIndex = 4;
        this.btnRefresh.Text = "🔄 Refresh";
        // 
        // lblSearch
        // 
        this.lblSearch.BackColor = System.Drawing.Color.Transparent;
        this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblSearch.Location = new System.Drawing.Point(20, 75);
        this.lblSearch.Name = "lblSearch";
        this.lblSearch.Size = new System.Drawing.Size(60, 20);
        this.lblSearch.TabIndex = 5;
        this.lblSearch.Text = "Search:";
        // 
        // txtSearch
        // 
        this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.txtSearch.Location = new System.Drawing.Point(85, 70);
        this.txtSearch.Name = "txtSearch";
        this.txtSearch.Size = new System.Drawing.Size(200, 30);
        this.txtSearch.TabIndex = 6;
        // 
        // lblGrade
        // 
        this.lblGrade.BackColor = System.Drawing.Color.Transparent;
        this.lblGrade.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblGrade.Location = new System.Drawing.Point(300, 75);
        this.lblGrade.Name = "lblGrade";
        this.lblGrade.Size = new System.Drawing.Size(50, 20);
        this.lblGrade.TabIndex = 7;
        this.lblGrade.Text = "Grade:";
        // 
        // cmbGradeFilter
        // 
        this.cmbGradeFilter.DropDownStyle = Syncfusion.WinForms.ListView.Enums.DropDownStyle.DropDownList;
        this.cmbGradeFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cmbGradeFilter.Location = new System.Drawing.Point(355, 70);
        this.cmbGradeFilter.Name = "cmbGradeFilter";
        this.cmbGradeFilter.Size = new System.Drawing.Size(100, 30);
        this.cmbGradeFilter.TabIndex = 8;
        // 
        // lblRoute
        // 
        this.lblRoute.BackColor = System.Drawing.Color.Transparent;
        this.lblRoute.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblRoute.Location = new System.Drawing.Point(470, 75);
        this.lblRoute.Name = "lblRoute";
        this.lblRoute.Size = new System.Drawing.Size(50, 20);
        this.lblRoute.TabIndex = 9;
        this.lblRoute.Text = "Route:";
        // 
        // cmbRouteFilter
        // 
        this.cmbRouteFilter.DropDownStyle = Syncfusion.WinForms.ListView.Enums.DropDownStyle.DropDownList;
        this.cmbRouteFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cmbRouteFilter.Location = new System.Drawing.Point(525, 70);
        this.cmbRouteFilter.Name = "cmbRouteFilter";
        this.cmbRouteFilter.Size = new System.Drawing.Size(150, 30);
        this.cmbRouteFilter.TabIndex = 10;
        // 
        // lblActive
        // 
        this.lblActive.BackColor = System.Drawing.Color.Transparent;
        this.lblActive.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblActive.Location = new System.Drawing.Point(690, 75);
        this.lblActive.Name = "lblActive";
        this.lblActive.Size = new System.Drawing.Size(50, 20);
        this.lblActive.TabIndex = 11;
        this.lblActive.Text = "Status:";
        // 
        // cmbActiveFilter
        // 
        this.cmbActiveFilter.DropDownStyle = Syncfusion.WinForms.ListView.Enums.DropDownStyle.DropDownList;
        this.cmbActiveFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cmbActiveFilter.Location = new System.Drawing.Point(745, 70);
        this.cmbActiveFilter.Name = "cmbActiveFilter";
        this.cmbActiveFilter.Size = new System.Drawing.Size(100, 30);
        this.cmbActiveFilter.TabIndex = 12;
        // 
        // panelGrid
        // 
        this.panelGrid.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
        this.panelGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.panelGrid.Controls.Add(this.studentDataGrid);
        this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelGrid.Location = new System.Drawing.Point(0, 200);
        this.panelGrid.Name = "panelGrid";
        this.panelGrid.Padding = new System.Windows.Forms.Padding(10);
        this.panelGrid.Size = new System.Drawing.Size(1600, 800);
        this.panelGrid.TabIndex = 2;
        // 
        // studentDataGrid
        // 
        this.studentDataGrid.AutoGenerateColumns = false;
        this.studentDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.studentDataGrid.Location = new System.Drawing.Point(10, 10);
        this.studentDataGrid.Name = "studentDataGrid";
        this.studentDataGrid.Size = new System.Drawing.Size(1580, 780);
        this.studentDataGrid.TabIndex = 0;
        // 
        // StudentManagementForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1600, 1000);
        this.Controls.Add(this.panelGrid);
        this.Controls.Add(this.panelControls);
        this.Controls.Add(this.panelHeader);
        this.MinimumSize = new System.Drawing.Size(1200, 700);
        this.Name = "StudentManagementForm";
        this.Text = "Bus Buddy - Student Management";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        this.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
        this.ForeColor = System.Drawing.Color.Black;
        ((System.ComponentModel.ISupportInitialize)(this.panelHeader)).EndInit();
        this.panelHeader.ResumeLayout(false);
        this.panelHeader.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.panelControls)).EndInit();
        this.panelControls.ResumeLayout(false);
        this.panelControls.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.txtSearch)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.cmbGradeFilter)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.cmbRouteFilter)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.cmbActiveFilter)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.panelGrid)).EndInit();
        this.panelGrid.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.studentDataGrid)).EndInit();
        this.ResumeLayout(false);
    }

    #endregion
    private Syncfusion.Windows.Forms.Tools.GradientPanel panelHeader;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelTitle;
    private Syncfusion.Windows.Forms.Tools.AutoLabel lblStudentCount;
    private Syncfusion.Windows.Forms.Tools.AutoLabel lblActiveCount;
    private Syncfusion.Windows.Forms.Tools.GradientPanel panelControls;
    private Syncfusion.WinForms.Controls.SfButton btnAddStudent;
    private Syncfusion.WinForms.Controls.SfButton btnEditStudent;
    private Syncfusion.WinForms.Controls.SfButton btnViewDetails;
    private Syncfusion.WinForms.Controls.SfButton btnDeleteStudent;
    private Syncfusion.WinForms.Controls.SfButton btnRefresh;
    private Syncfusion.Windows.Forms.Tools.AutoLabel lblSearch;
    private Syncfusion.Windows.Forms.Tools.TextBoxExt txtSearch;
    private Syncfusion.Windows.Forms.Tools.AutoLabel lblGrade;
    private Syncfusion.WinForms.ListView.SfComboBox cmbGradeFilter;
    private Syncfusion.Windows.Forms.Tools.AutoLabel lblRoute;
    private Syncfusion.WinForms.ListView.SfComboBox cmbRouteFilter;
    private Syncfusion.Windows.Forms.Tools.AutoLabel lblActive;
    private Syncfusion.WinForms.ListView.SfComboBox cmbActiveFilter;
    private Syncfusion.Windows.Forms.Tools.GradientPanel panelGrid;
    private Syncfusion.WinForms.DataGrid.SfDataGrid studentDataGrid;
}

using System;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Bus_Buddy.Models;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Data;
// Only available Syncfusion controls for 30.1.37:
// DataGrid, Input, SmithChart, plus core/themes

namespace BusBuddy.Forms
{
    public class ReportInfo
    {
        public string ReportName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public partial class ReportsForm : Form
    {
        private Syncfusion.WinForms.DataGrid.SfDataGrid? reportsGrid;
        private System.ComponentModel.BindingList<ReportInfo>? reportsData;
        // Placeholder: Add SmithChart for analytics if needed

        public ReportsForm()
        {
            // No designer file, so skip InitializeComponent
            InitializeReportsLayout();
            SetupEventHandlers();
        }

        private void InitializeReportsLayout()
        {
            // DataGrid for report list
            reportsGrid = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            reportsGrid.Location = new System.Drawing.Point(10, 10);
            reportsGrid.Size = new System.Drawing.Size(800, 480);
            reportsGrid.Name = "reportsGrid";
            reportsGrid.AllowFiltering = true;
            reportsGrid.AllowSorting = true;
            reportsGrid.AutoGenerateColumns = false;
            reportsGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "ReportName", HeaderText = "Report" });
            reportsGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "Category", HeaderText = "Category" });
            reportsGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "CreatedDate", HeaderText = "Created", Format = "d" });
            reportsGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "CreatedBy", HeaderText = "Created By" });
            reportsGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn() { MappingName = "Status", HeaderText = "Status" });

            // Sample data for demonstration
            reportsData = new System.ComponentModel.BindingList<ReportInfo>
            {
                new ReportInfo { ReportName = "Ridership Summary", Category = "Ridership", CreatedDate = DateTime.Today.AddDays(-2), CreatedBy = "Admin", Status = "Complete" },
                new ReportInfo { ReportName = "Attendance Detail", Category = "Attendance", CreatedDate = DateTime.Today.AddDays(-1), CreatedBy = "User1", Status = "In Progress" },
                new ReportInfo { ReportName = "Maintenance Log", Category = "Maintenance", CreatedDate = DateTime.Today, CreatedBy = "User2", Status = "Complete" },
                new ReportInfo { ReportName = "Incident Report", Category = "Incidents", CreatedDate = DateTime.Today.AddDays(-3), CreatedBy = "Admin", Status = "Complete" },
                new ReportInfo { ReportName = "Fuel Usage", Category = "Fuel", CreatedDate = DateTime.Today.AddDays(-5), CreatedBy = "User3", Status = "Draft" }
            };
            reportsGrid.DataSource = reportsData;
            this.Controls.Add(reportsGrid);

            // Placeholder: Add SmithChart for analytics visualization if needed
            // var smithChart = new Syncfusion.WinForms.SmithChart.SfSmithChart();
            // smithChart.Location = new System.Drawing.Point(820, 10);
            // smithChart.Size = new System.Drawing.Size(300, 300);
            // this.Controls.Add(smithChart);

            // Set form properties
            this.Text = "Reports";
            this.ClientSize = new System.Drawing.Size(850, 500);
        }

        private void SetupEventHandlers()
        {
            // Handle double-click to view/edit report
            if (reportsGrid != null)
            {
                reportsGrid.CellDoubleClick += ReportsGrid_CellDoubleClick;
            }
        }

        private void ReportsGrid_CellDoubleClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
        {
            if (reportsGrid?.SelectedItem is ReportInfo selectedReport)
            {
                MessageBox.Show($"Opening report: {selectedReport.ReportName}", "Report Viewer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                // TODO: Implement actual report viewing/editing when ReportViewer is available
            }
        }

        // Method to refresh data from database/service
        public void RefreshReports()
        {
            // TODO: Replace with actual data loading logic
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            if (reportsData != null)
            {
                reportsData.Clear();
                // Add sample data (replace with actual database call)
                reportsData.Add(new ReportInfo { ReportName = "Daily Ridership", Category = "Ridership", CreatedDate = DateTime.Today, CreatedBy = "System", Status = "Auto-Generated" });
                reportsData.Add(new ReportInfo { ReportName = "Weekly Maintenance", Category = "Maintenance", CreatedDate = DateTime.Today.AddDays(-1), CreatedBy = "Scheduler", Status = "Complete" });
            }
        }
    }
}

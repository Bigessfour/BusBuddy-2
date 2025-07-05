using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Bus_Buddy.Models;
using Bus_Buddy.Data.Repositories;

namespace BusBuddy.Forms
{
    /// <summary>
    /// Professional Bus Reports Form using actual Bus entities and BusRepository
    /// Demonstrates integration with existing sophisticated models and repositories
    /// </summary>
    public partial class BusReportsForm : Form
    {
        private SfDataGrid? busReportsGrid;
        private BindingList<BusReportViewModel>? busReportsData;
        private readonly BusRepository _busRepository;
        private Button refreshButton = null!;
        private Button exportButton = null!;
        private Label statusLabel = null!;

        public BusReportsForm(BusRepository busRepository)
        {
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
            // No designer file, so skip InitializeComponent
            InitializeBusReportsLayout();
            SetupEventHandlers();
            _ = LoadBusReportDataAsync();
        }

        private void InitializeBusReportsLayout()
        {
            this.Text = "Bus Fleet Reports - Bus Buddy";
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Status label
            statusLabel = new Label
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(400, 25),
                Text = "Loading bus fleet data...",
                ForeColor = System.Drawing.Color.Blue
            };
            this.Controls.Add(statusLabel);

            // Refresh button
            refreshButton = new Button
            {
                Location = new System.Drawing.Point(850, 10),
                Size = new System.Drawing.Size(80, 25),
                Text = "Refresh",
                UseVisualStyleBackColor = true
            };
            this.Controls.Add(refreshButton);

            // Export button
            exportButton = new Button
            {
                Location = new System.Drawing.Point(940, 10),
                Size = new System.Drawing.Size(50, 25),
                Text = "Export",
                UseVisualStyleBackColor = true
            };
            this.Controls.Add(exportButton);

            // DataGrid for bus reports
            busReportsGrid = new SfDataGrid
            {
                Location = new System.Drawing.Point(10, 45),
                Size = new System.Drawing.Size(980, 545),
                Name = "busReportsGrid",
                AllowFiltering = true,
                AllowSorting = true,
                AutoGenerateColumns = false
            };

            // Configure columns to match Bus entity properties
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "BusNumber", HeaderText = "Bus #", Width = 80 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "LicenseNumber", HeaderText = "License", Width = 100 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "Model", HeaderText = "Model", Width = 120 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "SeatingCapacity", HeaderText = "Capacity", Width = 80 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "Age", HeaderText = "Age (Years)", Width = 90 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "CurrentOdometer", HeaderText = "Mileage", Width = 100, Format = "N0" });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "InspectionStatus", HeaderText = "Inspection", Width = 100 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "NeedsAttention", HeaderText = "Needs Attention", Width = 100 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "Status", HeaderText = "Status", Width = 80 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "DateLastInspection", HeaderText = "Last Inspection", Width = 120, Format = "d" });

            this.Controls.Add(busReportsGrid);
        }

        private void SetupEventHandlers()
        {
            if (refreshButton != null)
                refreshButton.Click += async (s, e) => await RefreshBusReportsAsync();

            if (exportButton != null)
                exportButton.Click += new EventHandler(ExportButton_Click);

            if (busReportsGrid != null)
                busReportsGrid.CellDoubleClick += BusReportsGrid_CellDoubleClick;
        }

        private async Task LoadBusReportDataAsync()
        {
            try
            {
                if (statusLabel != null)
                    statusLabel.Text = "Loading bus fleet data...";

                // Use existing repository methods to get comprehensive bus data
                var buses = await _busRepository.GetAllAsync();
                var busReportViewModels = buses.Select(bus => new BusReportViewModel
                {
                    VehicleId = bus.VehicleId,
                    BusNumber = bus.BusNumber,
                    LicenseNumber = bus.LicenseNumber,
                    Model = bus.Model,
                    SeatingCapacity = bus.SeatingCapacity,
                    Age = bus.Age, // Uses computed property from Bus model
                    CurrentOdometer = bus.CurrentOdometer ?? 0,
                    InspectionStatus = bus.InspectionStatus, // Uses computed property
                    NeedsAttention = bus.NeedsAttention ? "Yes" : "No",
                    Status = bus.Status,
                    DateLastInspection = bus.DateLastInspection,
                    PurchaseDate = bus.PurchaseDate,
                    FuelType = bus.FuelType
                }).ToList();

                busReportsData = new BindingList<BusReportViewModel>(busReportViewModels);
                if (busReportsGrid != null)
                    busReportsGrid.DataSource = busReportsData;

                if (statusLabel != null)
                    statusLabel.Text = $"Loaded {busReportViewModels.Count} buses - Fleet Report Generated";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bus data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (statusLabel != null)
                    statusLabel.Text = "Error loading data";
            }
        }

        private async Task RefreshBusReportsAsync()
        {
            await LoadBusReportDataAsync();
        }

        private void ExportButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Export functionality will be implemented with Syncfusion's export features",
                "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: Implement actual export using Syncfusion export features
        }

        private void BusReportsGrid_CellDoubleClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
        {
            if (busReportsGrid?.SelectedItem is BusReportViewModel selectedBus)
            {
                MessageBox.Show($"Bus Details:\n" +
                    $"Bus Number: {selectedBus.BusNumber}\n" +
                    $"Model: {selectedBus.Model}\n" +
                    $"Capacity: {selectedBus.SeatingCapacity}\n" +
                    $"Age: {selectedBus.Age} years\n" +
                    $"Mileage: {selectedBus.CurrentOdometer:N0}\n" +
                    $"Inspection Status: {selectedBus.InspectionStatus}\n" +
                    $"Needs Attention: {selectedBus.NeedsAttention}\n" +
                    $"Status: {selectedBus.Status}",
                    "Bus Details", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // TODO: Open detailed bus management form when available
            }
        }

        /// <summary>
        /// Additional methods leveraging existing repository capabilities
        /// </summary>
        public void LoadMaintenanceDueBuses()
        {
            MessageBox.Show("Maintenance due bus filtering is not implemented. Please add this feature to BusRepository if needed.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void LoadInspectionDueBuses()
        {
            MessageBox.Show("Inspection due bus filtering is not implemented. Please add this feature to BusRepository if needed.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    /// <summary>
    /// ViewModel for Bus reports - flattens Bus entity for reporting display
    /// Preserves all important Bus properties while being optimized for grid display
    /// </summary>
    public class BusReportViewModel
    {
        public int VehicleId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int SeatingCapacity { get; set; }
        public int Age { get; set; }
        public int CurrentOdometer { get; set; }
        public string InspectionStatus { get; set; } = string.Empty;
        public string NeedsAttention { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? DateLastInspection { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? FuelType { get; set; } = string.Empty;
    }
}

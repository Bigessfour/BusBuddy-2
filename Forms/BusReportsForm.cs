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
    public partial class BusReportsForm : SfForm
    {
        private SfDataGrid? busReportsGrid;
        private BindingList<BusReportViewModel>? busReportsData;
        private readonly BusRepository _busRepository;
        private Button refreshButton;
        private Button exportButton;
        private Label statusLabel;

        public BusReportsForm(BusRepository busRepository)
        {
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
            InitializeComponent();
            InitializeBusReportsLayout();
            SetupEventHandlers();
            LoadBusReportDataAsync();
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
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "LicensePlate", HeaderText = "License", Width = 100 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "Model", HeaderText = "Model", Width = 120 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "Capacity", HeaderText = "Capacity", Width = 80 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "Age", HeaderText = "Age (Years)", Width = 90 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "Mileage", HeaderText = "Mileage", Width = 100, Format = "N0" });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "InspectionStatus", HeaderText = "Inspection", Width = 100 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "MaintenanceStatus", HeaderText = "Maintenance", Width = 100 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "IsActive", HeaderText = "Active", Width = 60 });
            busReportsGrid.Columns.Add(new GridTextColumn() { MappingName = "LastInspection", HeaderText = "Last Inspection", Width = 120, Format = "d" });

            this.Controls.Add(busReportsGrid);
        }

        private void SetupEventHandlers()
        {
            if (refreshButton != null)
                refreshButton.Click += async (s, e) => await RefreshBusReportsAsync();

            if (exportButton != null)
                exportButton.Click += ExportButton_Click;

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
                    BusId = bus.BusId,
                    BusNumber = bus.BusNumber,
                    LicensePlate = bus.LicensePlate,
                    Model = bus.Model,
                    Capacity = bus.Capacity,
                    Age = bus.Age, // Uses computed property from Bus model
                    Mileage = bus.Mileage,
                    InspectionStatus = bus.InspectionStatus, // Uses computed property
                    MaintenanceStatus = bus.MaintenanceStatus,
                    IsActive = bus.IsActive ? "Yes" : "No",
                    LastInspection = bus.LastInspection,
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

        private void ExportButton_Click(object sender, EventArgs e)
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
                    $"Capacity: {selectedBus.Capacity}\n" +
                    $"Age: {selectedBus.Age} years\n" +
                    $"Mileage: {selectedBus.Mileage:N0}\n" +
                    $"Inspection Status: {selectedBus.InspectionStatus}\n" +
                    $"Maintenance Status: {selectedBus.MaintenanceStatus}",
                    "Bus Details", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // TODO: Open detailed bus management form when available
            }
        }

        /// <summary>
        /// Additional methods leveraging existing repository capabilities
        /// </summary>
        public async Task LoadMaintenanceDueBusesAsync()
        {
            try
            {
                var maintenanceDueBuses = await _busRepository.GetBusesNeedingMaintenanceAsync();
                var viewModels = maintenanceDueBuses.Select(bus => new BusReportViewModel
                {
                    BusId = bus.BusId,
                    BusNumber = bus.BusNumber,
                    LicensePlate = bus.LicensePlate,
                    Model = bus.Model,
                    Capacity = bus.Capacity,
                    Age = bus.Age,
                    Mileage = bus.Mileage,
                    InspectionStatus = bus.InspectionStatus,
                    MaintenanceStatus = bus.MaintenanceStatus,
                    IsActive = bus.IsActive ? "Yes" : "No",
                    LastInspection = bus.LastInspection,
                    PurchaseDate = bus.PurchaseDate,
                    FuelType = bus.FuelType
                }).ToList();

                busReportsData = new BindingList<BusReportViewModel>(viewModels);
                if (busReportsGrid != null)
                    busReportsGrid.DataSource = busReportsData;

                if (statusLabel != null)
                    statusLabel.Text = $"Showing {viewModels.Count} buses needing maintenance";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance due buses: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task LoadInspectionDueBusesAsync()
        {
            try
            {
                var inspectionDueBuses = await _busRepository.GetBusesNeedingInspectionAsync();
                var viewModels = inspectionDueBuses.Select(bus => new BusReportViewModel
                {
                    BusId = bus.BusId,
                    BusNumber = bus.BusNumber,
                    LicensePlate = bus.LicensePlate,
                    Model = bus.Model,
                    Capacity = bus.Capacity,
                    Age = bus.Age,
                    Mileage = bus.Mileage,
                    InspectionStatus = bus.InspectionStatus,
                    MaintenanceStatus = bus.MaintenanceStatus,
                    IsActive = bus.IsActive ? "Yes" : "No",
                    LastInspection = bus.LastInspection,
                    PurchaseDate = bus.PurchaseDate,
                    FuelType = bus.FuelType
                }).ToList();

                busReportsData = new BindingList<BusReportViewModel>(viewModels);
                if (busReportsGrid != null)
                    busReportsGrid.DataSource = busReportsData;

                if (statusLabel != null)
                    statusLabel.Text = $"Showing {viewModels.Count} buses needing inspection";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inspection due buses: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// ViewModel for Bus reports - flattens Bus entity for reporting display
    /// Preserves all important Bus properties while being optimized for grid display
    /// </summary>
    public class BusReportViewModel
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int Age { get; set; }
        public decimal Mileage { get; set; }
        public string InspectionStatus { get; set; } = string.Empty;
        public string MaintenanceStatus { get; set; } = string.Empty;
        public string IsActive { get; set; } = string.Empty;
        public DateTime? LastInspection { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string FuelType { get; set; } = string.Empty;
    }
}

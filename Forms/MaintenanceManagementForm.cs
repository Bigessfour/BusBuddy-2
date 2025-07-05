using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Bus_Buddy.Utilities;
using System.ComponentModel;

namespace Bus_Buddy.Forms;

/// <summary>
/// Maintenance Management Form - Complete implementation using Syncfusion WinForms
/// Provides CRUD operations for vehicle maintenance records with search and filtering capabilities
/// </summary>
public partial class MaintenanceManagementForm : SfForm
{
    #region Fields and Services
    private readonly IBusService _busService;
    private readonly IMaintenanceService _maintenanceService;
    private readonly ILogger<MaintenanceManagementForm> _logger;

    // Data Collections
    private BindingList<Maintenance> _maintenanceRecords = null!;
    private BindingList<Maintenance> _filteredRecords = null!;
    private List<Bus> _vehicles = null!;

    // Currently selected maintenance record
    private Maintenance? _selectedRecord;
    #endregion

    #region Constructor
    public MaintenanceManagementForm(
        IBusService busService,
        IMaintenanceService maintenanceService,
        ILogger<MaintenanceManagementForm> logger)
    {
        _busService = busService;
        _maintenanceService = maintenanceService;
        _logger = logger;

        _logger.LogInformation("Initializing Maintenance Management Form");

        InitializeDataCollections();
        InitializeComponent();
        SetupSyncfusionStyling();
        ConfigureDataGrid();
        SetupFilters();

        // Load data asynchronously after form is shown
        Load += async (s, e) => await LoadMaintenanceDataAsync();
    }
    #endregion

    #region Initialization
    private void InitializeDataCollections()
    {
        _maintenanceRecords = new BindingList<Maintenance>();
        _filteredRecords = new BindingList<Maintenance>();
        _vehicles = new List<Bus>();
    }

    private void SetupSyncfusionStyling()
    {
        try
        {
            // Apply Syncfusion Office2019 theme
            Style.TitleBar.BackColor = Color.FromArgb(230, 126, 34);
            Style.TitleBar.ForeColor = Color.White;
            Style.BackColor = Color.FromArgb(248, 249, 250);

            _logger.LogDebug("Syncfusion styling applied successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Some styling options may not be available");
        }
    }

    private void ConfigureDataGrid()
    {
        try
        {
            // Apply standardized configuration
            SyncfusionLayoutManager.ConfigureSfDataGrid(dataGridMaintenance, true, true);
            SyncfusionAdvancedManager.ApplyAdvancedConfiguration(dataGridMaintenance);
            VisualEnhancementManager.ApplyEnhancedGridVisuals(dataGridMaintenance);
            SyncfusionLayoutManager.ApplyGridStyling(dataGridMaintenance);

            // Configure data source and columns
            dataGridMaintenance.AutoGenerateColumns = false;
            dataGridMaintenance.DataSource = _filteredRecords;
            SetupDataGridColumns();

            // Setup selection event
            dataGridMaintenance.SelectionChanged += DataGridMaintenance_SelectionChanged;

            _logger.LogInformation("Data grid configured successfully with standardized configuration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring data grid");
            ShowError($"Data grid configuration failed: {ex.Message}");
        }
    }

    private void SetupDataGridColumns()
    {
        dataGridMaintenance.Columns.Clear();

        // Maintenance ID Column
        dataGridMaintenance.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Maintenance.MaintenanceId),
            HeaderText = "ID",
            Width = 80,
            AllowEditing = false
        });

        // Date Column
        dataGridMaintenance.Columns.Add(new Syncfusion.WinForms.DataGrid.GridDateTimeColumn()
        {
            MappingName = nameof(Maintenance.Date),
            HeaderText = "Date",
            Width = 120,
            Format = "MM/dd/yyyy"
        });

        // Vehicle Column
        dataGridMaintenance.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = "Vehicle.BusNumber",
            HeaderText = "Vehicle",
            Width = 100
        });

        // Maintenance Type Column
        dataGridMaintenance.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Maintenance.MaintenanceCompleted),
            HeaderText = "Maintenance Type",
            Width = 180
        });

        // Description Column
        dataGridMaintenance.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Maintenance.Description),
            HeaderText = "Description",
            Width = 250
        });

        // Vendor Column
        dataGridMaintenance.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Maintenance.Vendor),
            HeaderText = "Vendor",
            Width = 150
        });

        // Priority Column
        dataGridMaintenance.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Maintenance.Priority),
            HeaderText = "Priority",
            Width = 100
        });

        // Repair Cost Column
        dataGridMaintenance.Columns.Add(new Syncfusion.WinForms.DataGrid.GridNumericColumn()
        {
            MappingName = nameof(Maintenance.RepairCost),
            HeaderText = "Cost",
            Width = 120,
            Format = "C"
        });

        // Created Date Column
        dataGridMaintenance.Columns.Add(new Syncfusion.WinForms.DataGrid.GridDateTimeColumn()
        {
            MappingName = nameof(Maintenance.CreatedDate),
            HeaderText = "Created",
            Width = 120,
            Format = "MM/dd/yyyy"
        });
    }

    private void ApplyDataGridTheme()
    {
        try
        {
            // Apply Office2019 theme colors to match application style
            if (dataGridMaintenance.Style != null)
            {
                dataGridMaintenance.Style.HeaderStyle.BackColor = Color.FromArgb(230, 126, 34);
                dataGridMaintenance.Style.HeaderStyle.TextColor = Color.White;
                dataGridMaintenance.Style.SelectionStyle.BackColor = Color.FromArgb(230, 126, 34, 50);
                dataGridMaintenance.Style.SelectionStyle.TextColor = Color.Black;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Some data grid styling options may not be available");
        }
    }

    private void SetupFilters()
    {
        try
        {
            // Populate priority filter
            cmbPriorityFilter.Items.Clear();
            cmbPriorityFilter.Items.Add("All Priorities");
            cmbPriorityFilter.Items.Add("High");
            cmbPriorityFilter.Items.Add("Normal");
            cmbPriorityFilter.Items.Add("Low");

            cmbPriorityFilter.SelectedIndex = 0; // Default to "All Priorities"

            // Set default date filter to 30 days ago
            dtpDateFilter.Value = DateTime.Today.AddDays(-30);

            _logger.LogDebug("Filters setup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up filters");
        }
    }
    #endregion

    #region Data Loading
    private async Task LoadMaintenanceDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading maintenance records from database");
            UpdateStatus("Loading maintenance records...");

            // Load vehicles first for the filter dropdown
            await LoadVehiclesAsync();

            // Load maintenance records using the maintenance service
            var maintenanceRecords = await _maintenanceService.GetAllMaintenanceRecordsAsync();

            _maintenanceRecords.Clear();
            foreach (var record in maintenanceRecords)
            {
                _maintenanceRecords.Add(record);
            }

            ApplyFilters();

            UpdateStatus($"Loaded {_maintenanceRecords.Count} maintenance records");
            _logger.LogInformation("Loaded {Count} maintenance records successfully", _maintenanceRecords.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading maintenance records");
            ShowError($"Failed to load maintenance records: {ex.Message}");
            UpdateStatus("Error loading maintenance records");
        }
    }

    private async Task LoadVehiclesAsync()
    {
        try
        {
            var vehicles = await _busService.GetAllBusEntitiesAsync();
            _vehicles = vehicles;

            // Update vehicle filter dropdown
            cmbVehicleFilter.Items.Clear();
            cmbVehicleFilter.Items.Add("All Vehicles");

            foreach (var vehicle in _vehicles)
            {
                cmbVehicleFilter.Items.Add($"{vehicle.BusNumber} - {vehicle.Make} {vehicle.Model}");
            }

            cmbVehicleFilter.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading vehicles for filter");
        }
    }

    private void ApplyFilters()
    {
        try
        {
            var filtered = _maintenanceRecords.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.ToLower();
                filtered = filtered.Where(m =>
                    (m.MaintenanceCompleted?.ToLower().Contains(searchTerm) == true) ||
                    (m.Description?.ToLower().Contains(searchTerm) == true) ||
                    (m.Vendor?.ToLower().Contains(searchTerm) == true) ||
                    (m.Vehicle?.BusNumber?.ToLower().Contains(searchTerm) == true));
            }

            // Apply vehicle filter
            if (cmbVehicleFilter.SelectedIndex > 0 && cmbVehicleFilter.SelectedItem != null)
            {
                var selectedVehicleText = cmbVehicleFilter.SelectedItem.ToString();
                var busNumber = selectedVehicleText?.Split(' ')[0]; // Extract bus number
                if (!string.IsNullOrEmpty(busNumber))
                {
                    filtered = filtered.Where(m => m.Vehicle?.BusNumber == busNumber);
                }
            }

            // Apply priority filter
            if (cmbPriorityFilter.SelectedIndex > 0 && cmbPriorityFilter.SelectedItem != null)
            {
                var selectedPriority = cmbPriorityFilter.SelectedItem.ToString();
                if (selectedPriority != "All Priorities")
                {
                    filtered = filtered.Where(m => m.Priority == selectedPriority);
                }
            }

            // Apply date filter
            filtered = filtered.Where(m => m.Date >= dtpDateFilter.Value.Date);

            // Update filtered collection
            _filteredRecords.Clear();
            foreach (var record in filtered.OrderByDescending(m => m.Date))
            {
                _filteredRecords.Add(record);
            }

            UpdateStatus($"Showing {_filteredRecords.Count} of {_maintenanceRecords.Count} maintenance records");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying filters");
            ShowError($"Filter error: {ex.Message}");
        }
    }
    #endregion

    #region Event Handlers
    private void BtnAdd_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Opening Maintenance Edit Form for new record");

            var maintenanceEditLogger = ServiceContainer.GetService<ILogger<MaintenanceEditForm>>();
            using var maintenanceEditForm = new MaintenanceEditForm(maintenanceEditLogger, _maintenanceService, _busService);
            if (maintenanceEditForm.ShowDialog() == DialogResult.OK && maintenanceEditForm.IsDataSaved)
            {
                // Refresh the grid data
                _ = LoadMaintenanceDataAsync();
                _logger.LogInformation("Maintenance record added successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening add maintenance form");
            ShowError($"Failed to open add maintenance form: {ex.Message}");
        }
    }

    private void BtnEdit_Click(object sender, EventArgs e)
    {
        try
        {
            if (_selectedRecord == null)
            {
                ShowInfo("Please select a maintenance record to edit.");
                return;
            }

            _logger.LogInformation("Opening Maintenance Edit Form for record {MaintenanceId}", _selectedRecord.MaintenanceId);

            var maintenanceEditLogger = ServiceContainer.GetService<ILogger<MaintenanceEditForm>>();
            using var maintenanceEditForm = new MaintenanceEditForm(maintenanceEditLogger, _maintenanceService, _busService, _selectedRecord);
            if (maintenanceEditForm.ShowDialog() == DialogResult.OK && maintenanceEditForm.IsDataSaved)
            {
                // Refresh the grid data
                _ = LoadMaintenanceDataAsync();
                _logger.LogInformation("Maintenance record updated successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening edit maintenance form");
            ShowError($"Failed to open edit maintenance form: {ex.Message}");
        }
    }

    private async void BtnDelete_Click(object sender, EventArgs e)
    {
        try
        {
            if (_selectedRecord == null)
            {
                ShowInfo("Please select a maintenance record to delete.");
                return;
            }

            var result = Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Are you sure you want to delete this maintenance record?\n\nVehicle: {_selectedRecord.Vehicle?.BusNumber}\nType: {_selectedRecord.MaintenanceCompleted}\nDate: {_selectedRecord.Date:MM/dd/yyyy}\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _logger.LogInformation("Deleting maintenance record {MaintenanceId}", _selectedRecord.MaintenanceId);
                UpdateStatus("Deleting maintenance record...");

                await _maintenanceService.DeleteMaintenanceRecordAsync(_selectedRecord.MaintenanceId);

                await LoadMaintenanceDataAsync();
                _selectedRecord = null;

                ShowSuccess("Maintenance record deleted successfully");
                _logger.LogInformation("Maintenance record deleted successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting maintenance record");
            ShowError($"Delete maintenance record failed: {ex.Message}");
            UpdateStatus("Error deleting maintenance record");
        }
    }

    private async void BtnRefresh_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Refreshing maintenance data");
            await LoadMaintenanceDataAsync();
            ShowInfo("Maintenance data refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing maintenance data");
            ShowError($"Refresh failed: {ex.Message}");
        }
    }

    private void BtnViewDetails_Click(object sender, EventArgs e)
    {
        try
        {
            if (_selectedRecord == null)
            {
                ShowInfo("Please select a maintenance record to view details.");
                return;
            }

            _logger.LogInformation("Viewing details for maintenance record {MaintenanceId}", _selectedRecord.MaintenanceId);

            // Create detailed info message
            var details = $@"Maintenance Record Details

Record ID: {_selectedRecord.MaintenanceId}
Vehicle: {_selectedRecord.Vehicle?.BusNumber} - {_selectedRecord.Vehicle?.Make} {_selectedRecord.Vehicle?.Model}
Date: {_selectedRecord.Date:MM/dd/yyyy}

Maintenance Information:
Type: {_selectedRecord.MaintenanceCompleted}
Description: {_selectedRecord.Description ?? "No description provided"}
Priority: {_selectedRecord.Priority}

Vendor Information:
Vendor: {_selectedRecord.Vendor ?? "Not specified"}
Repair Cost: {_selectedRecord.RepairCost:C}

Additional Information:
Notes: {_selectedRecord.Notes ?? "No additional notes"}
Created: {_selectedRecord.CreatedDate:MM/dd/yyyy HH:mm}
Created By: {_selectedRecord.CreatedBy ?? "System"}

{(_selectedRecord.UpdatedDate.HasValue ? $"Last Updated: {_selectedRecord.UpdatedDate:MM/dd/yyyy HH:mm}" : "")}
{(_selectedRecord.UpdatedBy != null ? $"Updated By: {_selectedRecord.UpdatedBy}" : "")}";

            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, details,
                $"Maintenance Record Details - {_selectedRecord.MaintenanceCompleted}",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing maintenance record details");
            ShowError($"Failed to view maintenance record details: {ex.Message}");
        }
    }

    private void TxtSearch_TextChanged(object sender, EventArgs e)
    {
        try
        {
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying search filter");
        }
    }

    private void CmbVehicleFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying vehicle filter");
        }
    }

    private void CmbPriorityFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying priority filter");
        }
    }

    private void DtpDateFilter_ValueChanged(object sender, EventArgs e)
    {
        try
        {
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying date filter");
        }
    }

    private void DataGridMaintenance_CellDoubleClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
    {
        try
        {
            if (dataGridMaintenance.SelectedItem is Maintenance maintenance)
            {
                _selectedRecord = maintenance;
                BtnEdit_Click(sender, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling cell double click");
        }
    }

    private void DataGridMaintenance_SelectionChanged(object sender, EventArgs e)
    {
        try
        {
            if (dataGridMaintenance.SelectedItem is Maintenance maintenance)
            {
                _selectedRecord = maintenance;
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
                btnViewDetails.Enabled = true;
            }
            else
            {
                _selectedRecord = null;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
                btnViewDetails.Enabled = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling selection change");
        }
    }
    #endregion

    #region Utility Methods
    private void ShowError(string message)
    {
        _logger.LogError("Maintenance Management Error: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Maintenance Management Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ShowInfo(string message)
    {
        _logger.LogInformation("Maintenance Management Info: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Maintenance Management",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowSuccess(string message)
    {
        _logger.LogInformation("Maintenance Management Success: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Success",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void UpdateStatus(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => statusLabel.Text = message);
        }
        else
        {
            statusLabel.Text = message;
        }
    }

    #endregion
}

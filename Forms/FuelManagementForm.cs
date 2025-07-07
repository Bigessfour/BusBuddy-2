using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.Windows.Forms;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Bus_Buddy.Utilities;
using System.ComponentModel;

namespace Bus_Buddy.Forms;

/// <summary>
/// Fuel Management Form - Complete implementation using Syncfusion WinForms
/// Provides CRUD operations for vehicle fuel records with search and filtering capabilities
/// </summary>
public partial class FuelManagementForm : MetroForm
{
    #region Fields and Services
    private readonly IBusService _busService;
    private readonly IFuelService _fuelService;
    private readonly ILogger<FuelManagementForm> _logger;

    // Data Collections
    private BindingList<Fuel> _fuelRecords = null!;
    private BindingList<Fuel> _filteredRecords = null!;
    private List<Bus> _vehicles = null!;

    // Currently selected fuel record
    private Fuel? _selectedRecord;
    #endregion

    #region Constructor
    public FuelManagementForm(
        IBusService busService,
        IFuelService fuelService,
        ILogger<FuelManagementForm> logger)
    {
        _busService = busService;
        _fuelService = fuelService;
        _logger = logger;

        _logger.LogInformation("Initializing Fuel Management Form");

        InitializeDataCollections();
        InitializeComponent();
        SetupSyncfusionStyling();
        ConfigureDataGrid();
        SetupFilters();

        // Load data asynchronously after form is shown
        Load += async (s, e) => await LoadFuelDataAsync();
    }
    #endregion

    #region Initialization
    private void InitializeDataCollections()
    {
        _fuelRecords = new BindingList<Fuel>();
        _filteredRecords = new BindingList<Fuel>();
        _vehicles = new List<Bus>();
    }

    private void SetupSyncfusionStyling()
    {
        try
        {
            // Apply Syncfusion Office2019 theme
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.MetroColor = Color.FromArgb(230, 126, 34);
            this.CaptionForeColor = Color.White;

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
            SyncfusionLayoutManager.ConfigureSfDataGrid(dataGridFuel, true, true);
            SyncfusionAdvancedManager.ApplyAdvancedConfiguration(dataGridFuel);
            VisualEnhancementManager.ApplyEnhancedGridVisuals(dataGridFuel);
            SyncfusionLayoutManager.ApplyGridStyling(dataGridFuel);

            // Configure data source and columns
            dataGridFuel.AutoGenerateColumns = false;
            dataGridFuel.DataSource = _filteredRecords;
            SetupDataGridColumns();

            // Setup selection event
            dataGridFuel.SelectionChanged += DataGridFuel_SelectionChanged;

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
        dataGridFuel.Columns.Clear();

        // Fuel ID Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Fuel.FuelId),
            HeaderText = "ID",
            Width = 80,
            AllowEditing = false
        });

        // Date Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridDateTimeColumn()
        {
            MappingName = nameof(Fuel.FuelDate),
            HeaderText = "Date",
            Width = 120,
            Format = "MM/dd/yyyy"
        });

        // Vehicle Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = "Vehicle.BusNumber",
            HeaderText = "Vehicle",
            Width = 100
        });

        // Fuel Type Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Fuel.FuelType),
            HeaderText = "Fuel Type",
            Width = 180
        });

        // Gallons Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridNumericColumn()
        {
            MappingName = nameof(Fuel.Gallons),
            HeaderText = "Gallons",
            Width = 100,
        });

        // Cost Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridNumericColumn()
        {
            MappingName = nameof(Fuel.TotalCost),
            HeaderText = "Cost",
            Width = 120,
            Format = "C"
        });

        // Odometer Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridNumericColumn()
        {
            MappingName = nameof(Fuel.VehicleOdometerReading),
            HeaderText = "Odometer",
            Width = 120,
        });
    }

    private void ApplyDataGridTheme()
    {
        try
        {
            // Apply Office2019 theme colors to match application style
            if (dataGridFuel.Style != null)
            {
                dataGridFuel.Style.HeaderStyle.BackColor = Color.FromArgb(230, 126, 34);
                dataGridFuel.Style.HeaderStyle.TextColor = Color.White;
                dataGridFuel.Style.SelectionStyle.BackColor = Color.FromArgb(230, 126, 34, 50);
                dataGridFuel.Style.SelectionStyle.TextColor = Color.Black;
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
    private async Task LoadFuelDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading fuel records from database");
            UpdateStatus("Loading fuel records...");

            // Load vehicles first for the filter dropdown
            await LoadVehiclesAsync();

            // Load fuel records using the fuel service
            var fuelRecords = await _fuelService.GetAllFuelRecordsAsync();

            _fuelRecords.Clear();
            foreach (var record in fuelRecords)
            {
                _fuelRecords.Add(record);
            }

            ApplyFilters();

            UpdateStatus($"Loaded {_fuelRecords.Count} fuel records");
            _logger.LogInformation("Loaded {Count} fuel records successfully", _fuelRecords.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading fuel records");
            ShowError($"Failed to load fuel records: {ex.Message}");
            UpdateStatus("Error loading fuel records");
        }
    }

    private async Task LoadVehiclesAsync()
    {
        try
        {
            var vehicles = await _busService.GetAllBusEntitiesAsync();
            _vehicles = vehicles;

            // Update vehicle filter dropdown
            var vehicleList = new List<string> { "All Vehicles" };
            vehicleList.AddRange(_vehicles.Select(v => $"{v.BusNumber} - {v.Make} {v.Model}"));
            cmbVehicleFilter.DataSource = vehicleList;
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
            var filtered = _fuelRecords.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.ToLower();
                filtered = filtered.Where(m =>
                    (m.FuelType?.ToLower().Contains(searchTerm) == true) ||
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

            // Apply date filter
            if (dtpDateFilter.Value.HasValue)
            {
                filtered = filtered.Where(m => m.FuelDate >= dtpDateFilter.Value.Value.Date);
            }

            // Update filtered collection
            _filteredRecords.Clear();
            foreach (var record in filtered.OrderByDescending(m => m.FuelDate))
            {
                _filteredRecords.Add(record);
            }

            UpdateStatus($"Showing {_filteredRecords.Count} of {_fuelRecords.Count} fuel records");
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
            _logger.LogInformation("Opening Fuel Edit Form for new record");

            var fuelEditLogger = ServiceContainer.GetService<ILogger<FuelEditForm>>();
            using var fuelEditForm = new FuelEditForm(fuelEditLogger, _fuelService, _busService);
            if (fuelEditForm.ShowDialog() == DialogResult.OK && fuelEditForm.IsDataSaved)
            {
                // Refresh the grid data
                _ = LoadFuelDataAsync();
                _logger.LogInformation("Fuel record added successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening add fuel form");
            ShowError($"Failed to open add fuel form: {ex.Message}");
        }
    }

    private void BtnEdit_Click(object sender, EventArgs e)
    {
        try
        {
            if (_selectedRecord == null)
            {
                ShowInfo("Please select a fuel record to edit.");
                return;
            }

            _logger.LogInformation("Opening Fuel Edit Form for record {FuelId}", _selectedRecord.FuelId);

            var fuelEditLogger = ServiceContainer.GetService<ILogger<FuelEditForm>>();
            using var fuelEditForm = new FuelEditForm(fuelEditLogger, _fuelService, _busService, _selectedRecord);
            if (fuelEditForm.ShowDialog() == DialogResult.OK && fuelEditForm.IsDataSaved)
            {
                // Refresh the grid data
                _ = LoadFuelDataAsync();
                _logger.LogInformation("Fuel record updated successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening edit fuel form");
            ShowError($"Failed to open edit fuel form: {ex.Message}");
        }
    }

    private async void BtnDelete_Click(object sender, EventArgs e)
    {
        try
        {
            if (_selectedRecord == null)
            {
                ShowInfo("Please select a fuel record to delete.");
                return;
            }

            var result = Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Are you sure you want to delete this fuel record?\n\nVehicle: {_selectedRecord.Vehicle?.BusNumber}\nType: {_selectedRecord.FuelType}\nDate: {_selectedRecord.FuelDate:MM/dd/yyyy}\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _logger.LogInformation("Deleting fuel record {FuelId}", _selectedRecord.FuelId);
                UpdateStatus("Deleting fuel record...");

                await _fuelService.DeleteFuelRecordAsync(_selectedRecord.FuelId);

                await LoadFuelDataAsync();
                _selectedRecord = null;

                ShowSuccess("Fuel record deleted successfully");
                _logger.LogInformation("Fuel record deleted successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fuel record");
            ShowError($"Delete fuel record failed: {ex.Message}");
            UpdateStatus("Error deleting fuel record");
        }
    }

    private async void BtnRefresh_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Refreshing fuel data");
            await LoadFuelDataAsync();
            ShowInfo("Fuel data refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing fuel data");
            ShowError($"Refresh failed: {ex.Message}");
        }
    }

    private void BtnViewDetails_Click(object sender, EventArgs e)
    {
        try
        {
            if (_selectedRecord == null)
            {
                ShowInfo("Please select a fuel record to view details.");
                return;
            }

            _logger.LogInformation("Viewing details for fuel record {FuelId}", _selectedRecord.FuelId);

            // Create detailed info message
            var details = $@"Fuel Record Details

Record ID: {_selectedRecord.FuelId}
Vehicle: {_selectedRecord.Vehicle?.BusNumber} - {_selectedRecord.Vehicle?.Make} {_selectedRecord.Vehicle?.Model}
Date: {_selectedRecord.FuelDate:MM/dd/yyyy}

Fuel Information:
Type: {_selectedRecord.FuelType}
Gallons: {_selectedRecord.Gallons}
Cost: {_selectedRecord.TotalCost:C}
Odometer: {_selectedRecord.VehicleOdometerReading}
Location: {_selectedRecord.FuelLocation}
Price Per Gallon: {_selectedRecord.PricePerGallon:C}

Additional Information:
Notes: {_selectedRecord.Notes ?? "No additional notes"}";

            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, details,
                $"Fuel Record Details - {_selectedRecord.FuelType}",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing fuel record details");
            ShowError($"Failed to view fuel record details: {ex.Message}");
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

    private void DtpDateFilter_ValueChanged(object sender, Syncfusion.WinForms.Input.Events.DateTimeValueChangedEventArgs e)
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

    private void DataGridFuel_CellDoubleClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
    {
        try
        {
            if (dataGridFuel.SelectedItem is Fuel fuel)
            {
                _selectedRecord = fuel;
                BtnEdit_Click(sender, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling cell double click");
        }
    }

    private void DataGridFuel_SelectionChanged(object sender, Syncfusion.WinForms.DataGrid.Events.SelectionChangedEventArgs e)
    {
        try
        {
            if (dataGridFuel.SelectedItem is Fuel fuel)
            {
                _selectedRecord = fuel;
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
        _logger.LogError("Fuel Management Error: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Fuel Management Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ShowInfo(string message)
    {
        _logger.LogInformation("Fuel Management Info: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Fuel Management",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowSuccess(string message)
    {
        _logger.LogInformation("Fuel Management Success: {Message}", message);
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

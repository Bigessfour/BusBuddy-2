using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Bus_Buddy.Utilities;
using System.ComponentModel;

namespace Bus_Buddy.Forms;

/// <summary>
/// Fuel Management Form - Complete implementation using Syncfusion WinForms
/// Provides CRUD operations for fuel records with analytics and filtering capabilities
/// </summary>
public partial class FuelManagementForm : SfForm
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
            Style.TitleBar.BackColor = Color.FromArgb(41, 128, 185);
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

        // Fuel Location Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Fuel.FuelLocation),
            HeaderText = "Location",
            Width = 150
        });

        // Fuel Type Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Fuel.FuelType),
            HeaderText = "Fuel Type",
            Width = 100
        });

        // Gallons Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridNumericColumn()
        {
            MappingName = nameof(Fuel.Gallons),
            HeaderText = "Gallons",
            Width = 100,
            Format = "N2"
        });

        // Price Per Gallon Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridNumericColumn()
        {
            MappingName = nameof(Fuel.PricePerGallon),
            HeaderText = "Price/Gal",
            Width = 100,
            Format = "C3"
        });

        // Total Cost Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridNumericColumn()
        {
            MappingName = nameof(Fuel.TotalCost),
            HeaderText = "Total Cost",
            Width = 120,
            Format = "C"
        });

        // Notes Column
        dataGridFuel.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
        {
            MappingName = nameof(Fuel.Notes),
            HeaderText = "Notes",
            Width = 200
        });
    }

    private void ApplyDataGridTheme()
    {
        try
        {
            // Apply Office2019 theme colors to match application style
            if (dataGridFuel.Style != null)
            {
                dataGridFuel.Style.HeaderStyle.BackColor = Color.FromArgb(41, 128, 185);
                dataGridFuel.Style.HeaderStyle.TextColor = Color.White;
                dataGridFuel.Style.SelectionStyle.BackColor = Color.FromArgb(41, 128, 185, 50);
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
            // Populate fuel type filter
            cmbFuelTypeFilter.Items.Clear();
            cmbFuelTypeFilter.Items.Add("All Types");
            cmbFuelTypeFilter.Items.Add("Gasoline");
            cmbFuelTypeFilter.Items.Add("Diesel");
            cmbFuelTypeFilter.Items.Add("Unleaded");
            cmbFuelTypeFilter.Items.Add("Premium");

            cmbFuelTypeFilter.SelectedIndex = 0; // Default to "All Types"

            // Set default date range to last 30 days
            dtpStartDate.Value = DateTime.Today.AddDays(-30);
            dtpEndDate.Value = DateTime.Today;

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

            // Load fuel records
            var fuelRecords = await _fuelService.GetAllFuelRecordsAsync();

            _fuelRecords.Clear();
            foreach (var record in fuelRecords)
            {
                _fuelRecords.Add(record);
            }

            ApplyFilters();
            UpdateSummaryLabels();

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
            var filtered = _fuelRecords.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.ToLower();
                filtered = filtered.Where(f =>
                    (f.FuelLocation?.ToLower().Contains(searchTerm) == true) ||
                    (f.Notes?.ToLower().Contains(searchTerm) == true) ||
                    (f.Vehicle?.BusNumber?.ToLower().Contains(searchTerm) == true));
            }

            // Apply vehicle filter
            if (cmbVehicleFilter.SelectedIndex > 0 && cmbVehicleFilter.SelectedItem != null)
            {
                var selectedVehicleText = cmbVehicleFilter.SelectedItem.ToString();
                var busNumber = selectedVehicleText?.Split(' ')[0]; // Extract bus number
                if (!string.IsNullOrEmpty(busNumber))
                {
                    filtered = filtered.Where(f => f.Vehicle?.BusNumber == busNumber);
                }
            }

            // Apply fuel type filter
            if (cmbFuelTypeFilter.SelectedIndex > 0 && cmbFuelTypeFilter.SelectedItem != null)
            {
                var selectedFuelType = cmbFuelTypeFilter.SelectedItem.ToString();
                if (selectedFuelType != "All Types")
                {
                    filtered = filtered.Where(f => f.FuelType == selectedFuelType);
                }
            }

            // Apply date range filter
            filtered = filtered.Where(f =>
                f.FuelDate >= dtpStartDate.Value.Date &&
                f.FuelDate <= dtpEndDate.Value.Date);

            // Update filtered collection
            _filteredRecords.Clear();
            foreach (var record in filtered.OrderByDescending(f => f.FuelDate))
            {
                _filteredRecords.Add(record);
            }

            UpdateStatus($"Showing {_filteredRecords.Count} of {_fuelRecords.Count} fuel records");
            UpdateSummaryLabels(); // Update summary
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying filters");
            ShowError($"Filter error: {ex.Message}");
        }
    }

    private void UpdateSummaryLabels()
    {
        try
        {
            var totalCost = _filteredRecords.Sum(f => f.TotalCost ?? 0);
            var totalGallons = _filteredRecords.Sum(f => f.Gallons ?? 0);

            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    lblTotalCost.Text = $"Total Cost: {totalCost:C}";
                    lblTotalGallons.Text = $"Total Gallons: {totalGallons:N1}";
                });
            }
            else
            {
                lblTotalCost.Text = $"Total Cost: {totalCost:C}";
                lblTotalGallons.Text = $"Total Gallons: {totalGallons:N1}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating summary labels");
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
                $"Are you sure you want to delete this fuel record?\n\nVehicle: {_selectedRecord.Vehicle?.BusNumber}\nDate: {_selectedRecord.FuelDate:MM/dd/yyyy}\nAmount: {_selectedRecord.Gallons:N2} gallons\nCost: {_selectedRecord.TotalCost:C}\n\nThis action cannot be undone.",
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
Location: {_selectedRecord.FuelLocation ?? "Not specified"}
Gallons: {_selectedRecord.Gallons:N3}
Price per Gallon: {_selectedRecord.PricePerGallon:C3}
Total Cost: {_selectedRecord.TotalCost:C}

Additional Information:
Notes: {_selectedRecord.Notes ?? "No notes"}

{(_selectedRecord.Vehicle?.MilesPerGallon.HasValue == true ? $"Vehicle MPG: {_selectedRecord.Vehicle.MilesPerGallon:N1}" : "")}";

            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, details,
                $"Fuel Record Details - {_selectedRecord.Vehicle?.BusNumber}",
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

    private void CmbFuelTypeFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying fuel type filter");
        }
    }

    private void DtpStartDate_ValueChanged(object sender, EventArgs e)
    {
        try
        {
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying start date filter");
        }
    }

    private void DtpEndDate_ValueChanged(object sender, EventArgs e)
    {
        try
        {
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying end date filter");
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

    private void DataGridFuel_SelectionChanged(object sender, EventArgs e)
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

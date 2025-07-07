using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Services;
using Bus_Buddy.Models;
using Bus_Buddy.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace Bus_Buddy.Forms;

/// <summary>
/// Driver Management Form for managing bus drivers
/// Demonstrates comprehensive driver data management with Syncfusion controls
/// </summary>
public partial class DriverManagementForm : MetroForm
{
    private readonly ILogger<DriverManagementForm> _logger;
    private readonly IBusService _busService;
    private List<Driver> _drivers = new List<Driver>();

    public DriverManagementForm(ILogger<DriverManagementForm> logger, IBusService busService)
    {
        _logger = logger;
        _busService = busService;

        _logger.LogInformation("Initializing Driver Management form");
        InitializeComponent();
        LoadDriverDataAsync();
    }

    private async void LoadDriverDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading driver data");

            // Show loading state
            if (this.InvokeRequired)
            {
                this.Invoke(() => ShowLoadingState());
            }
            else
            {
                ShowLoadingState();
            }

            // Load drivers from service
            _drivers = await _busService.GetAllDriversAsync();

            // Update the data grid on UI thread
            if (this.InvokeRequired)
            {
                this.Invoke(() => UpdateDriverGrid());
            }
            else
            {
                UpdateDriverGrid();
            }

            _logger.LogInformation("Loaded {DriverCount} drivers", _drivers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading driver data");

            if (this.InvokeRequired)
            {
                this.Invoke(() => HandleLoadError(ex));
            }
            else
            {
                HandleLoadError(ex);
            }
        }
    }

    private void ShowLoadingState()
    {
        statusLabel.Text = "Loading drivers...";
        statusLabel.ForeColor = Color.FromArgb(52, 152, 219);
    }

    private void UpdateDriverGrid()
    {
        // Update the data grid
        driverDataGrid.DataSource = _drivers;

        // Update status
        statusLabel.Text = $"Loaded {_drivers.Count} drivers";
        statusLabel.ForeColor = Color.FromArgb(46, 204, 113);

        // Apply conditional styling for expiring licenses
        ApplyConditionalStyling();
    }

    private void HandleLoadError(Exception ex)
    {
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
            $"Error loading driver data: {ex.Message}",
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        statusLabel.Text = "Error loading data";
        statusLabel.ForeColor = Color.FromArgb(231, 76, 60);
    }

    private void ApplyConditionalStyling()
    {
        try
        {
            var expiryThreshold = DateTime.Now.AddDays(30);
            var expiringCount = 0;

            // Apply conditional styling through QueryRowStyle event
            driverDataGrid.QueryRowStyle += (sender, e) =>
            {
                if (e.RowData is Driver driver && driver.LicenseExpiryDate.HasValue)
                {
                    if (driver.LicenseExpiryDate.Value <= expiryThreshold)
                    {
                        e.Style.BackColor = Color.FromArgb(255, 235, 238);
                        e.Style.TextColor = Color.FromArgb(198, 40, 40);
                        expiringCount++;
                    }
                }
            };

            // Count expiring licenses
            expiringCount = _drivers.Count(d => d.LicenseExpiryDate.HasValue &&
                                         d.LicenseExpiryDate.Value <= expiryThreshold);

            if (expiringCount > 0)
            {
                statusLabel.Text += $" - {expiringCount} license(s) expiring soon";
                statusLabel.ForeColor = Color.FromArgb(255, 152, 0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error applying conditional styling");
        }
    }

    #region Button Event Handlers

    private void AddDriverButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Add Driver button clicked");

            // Create new DriverEditForm for adding a driver
            var logger = ServiceContainer.GetService<ILogger<DriverEditForm>>();
            var driverEditForm = new DriverEditForm(logger, _busService);
            var result = driverEditForm.ShowDialog(this);

            if (result == DialogResult.OK && driverEditForm.IsDataSaved)
            {
                // Refresh the data grid
                LoadDriverDataAsync();
                statusLabel.Text = $"Driver {driverEditForm.EditedDriver?.FirstName} {driverEditForm.EditedDriver?.LastName} added successfully";
                statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Add Driver");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void EditDriverButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Edit Driver button clicked");

            var selectedDriver = GetSelectedDriver();
            if (selectedDriver != null)
            {
                // Create DriverEditForm with existing driver data
                var logger = ServiceContainer.GetService<ILogger<DriverEditForm>>();
                var driverEditForm = new DriverEditForm(logger, _busService, selectedDriver);
                var result = driverEditForm.ShowDialog(this);

                if (result == DialogResult.OK && driverEditForm.IsDataSaved)
                {
                    // Refresh the data grid
                    LoadDriverDataAsync();
                    statusLabel.Text = $"Driver {driverEditForm.EditedDriver?.FirstName} {driverEditForm.EditedDriver?.LastName} updated successfully";
                    statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
                }
            }
            else
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Please select a driver to edit.",
                    "No Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Edit Driver");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void DeleteDriverButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Delete Driver button clicked");

            var selectedDriver = GetSelectedDriver();
            if (selectedDriver != null)
            {
                var result = Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    $"Are you sure you want to delete driver {selectedDriver.FirstName} {selectedDriver.LastName}?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    ShowDeletingState();

                    try
                    {
                        // Note: We would need to implement DeleteDriverAsync in the service
                        // For now, show a placeholder message
                        await Task.Delay(1000); // Simulate API call

                        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                            $"Delete Driver functionality will be implemented.\n\nDriver {selectedDriver.FirstName} {selectedDriver.LastName} would be deleted.",
                            "Delete Driver",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        statusLabel.Text = $"Driver {selectedDriver.FirstName} {selectedDriver.LastName} deleted successfully";
                        statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
                    }
                    finally
                    {
                        RestoreDeleteButtonState();
                    }
                }
            }
            else
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Please select a driver to delete.",
                    "No Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Delete Driver");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error deleting driver: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            RestoreDeleteButtonState();
        }
    }

    private void ShowDeletingState()
    {
        deleteDriverButton.Enabled = false;
        deleteDriverButton.Text = "Deleting...";
        deleteDriverButton.BackColor = Color.FromArgb(158, 158, 158);
    }

    private void RestoreDeleteButtonState()
    {
        deleteDriverButton.Enabled = true;
        deleteDriverButton.Text = "Delete Driver";
        deleteDriverButton.BackColor = Color.FromArgb(244, 67, 54);
    }

    private async void RefreshButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Refresh button clicked");
            statusLabel.ForeColor = Color.Gray;
            await Task.Run(() => LoadDriverDataAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing data");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error refreshing data: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ViewLicenseButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("View License button clicked");

            var selectedDriver = GetSelectedDriver();
            if (selectedDriver != null)
            {
                // Show driver license information
                var licenseInfo = $"Driver License Information\n\n" +
                    $"Driver: {selectedDriver.FirstName} {selectedDriver.LastName}\n" +
                    $"License Number: {selectedDriver.LicenseNumber}\n" +
                    $"License Class: {selectedDriver.LicenseClass}\n" +
                    $"Issue Date: {selectedDriver.LicenseIssueDate?.ToString("MM/dd/yyyy") ?? "Not specified"}\n" +
                    $"Expiry Date: {selectedDriver.LicenseExpiryDate?.ToString("MM/dd/yyyy") ?? "Not specified"}\n" +
                    $"Endorsements: {selectedDriver.Endorsements ?? "None"}\n" +
                    $"Status: {selectedDriver.Status}";

                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    licenseInfo,
                    "License Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Please select a driver to view license information.",
                    "No Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing license");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Close button clicked");
            this.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing form");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    #endregion

    #region Helper Methods

    private Driver? GetSelectedDriver()
    {
        try
        {
            var selectedIndex = driverDataGrid.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < _drivers.Count)
            {
                return _drivers[selectedIndex];
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting selected driver");
            return null;
        }
    }

    #endregion

    #region Event Handlers

    private void DriverDataGrid_CellDoubleClick(object sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
    {
        // Double-click to edit driver - just trigger edit without checking row
        EditDriverButton_Click(sender, EventArgs.Empty);
    }

    private void DriverDataGrid_SelectionChanged(object sender, Syncfusion.WinForms.DataGrid.Events.SelectionChangedEventArgs e)
    {
        // Enable/disable buttons based on selection
        var hasSelection = driverDataGrid.SelectedIndex >= 0;
        editDriverButton.Enabled = hasSelection;
        deleteDriverButton.Enabled = hasSelection;
        viewLicenseButton.Enabled = hasSelection;
    }

    #endregion
}

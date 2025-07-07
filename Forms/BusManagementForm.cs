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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace Bus_Buddy.Forms;

/// <summary>
/// Bus Management Form for managing fleet vehicles
/// Demonstrates how model management forms integrate with the dashboard
/// </summary>
public partial class BusManagementForm : MetroForm
{
    private readonly ILogger<BusManagementForm> _logger;
    private readonly IBusService _busService;
    private List<BusInfo> _buses = new List<BusInfo>();

    public BusManagementForm(ILogger<BusManagementForm> logger, IBusService busService)
    {
        _logger = logger;
        _busService = busService;

        _logger.LogInformation("Initializing Bus Management form");
        InitializeComponent();
        // Load data asynchronously after form is shown
        Load += async (s, e) => await LoadBusDataAsync();
    }

    private void ConfigureDataGrid()
    {
        if (busDataGrid != null)
        {
            // Note: Basic configuration now handled by the designer file.
            // This method reserved for bus-specific customizations only

            _logger.LogInformation("Bus-specific data grid customizations applied");
        }
    }

    private async Task LoadBusDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading bus data");

            // Load buses from service
            _buses = await _busService.GetAllBusesAsync();

            // Update UI on main thread
            if (this.InvokeRequired)
            {
                this.Invoke(() => UpdateBusGridAndStatus());
            }
            else
            {
                UpdateBusGridAndStatus();
            }

            _logger.LogInformation("Loaded {BusCount} buses", _buses.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bus data");

            // Handle UI updates on main thread
            if (this.InvokeRequired)
            {
                this.Invoke(() => HandleBusLoadError(ex));
            }
            else
            {
                HandleBusLoadError(ex);
            }
        }
    }

    private void UpdateBusGridAndStatus()
    {
        // Update the data grid
        if (busDataGrid != null)
        {
            busDataGrid.DataSource = _buses;
        }

        // Update status with success styling
        if (statusLabel != null)
        {
            statusLabel.Text = $"Loaded {_buses.Count} buses";
            statusLabel.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);

            // Count buses needing attention for status
            var maintenanceNeeded = _buses.Count(b => b.Status.Contains("Maintenance") || b.Status.Contains("Out of Service"));
            if (maintenanceNeeded > 0)
            {
                statusLabel.Text += $" - {maintenanceNeeded} bus(es) need attention";
                statusLabel.ForeColor = System.Drawing.Color.FromArgb(255, 152, 0); // Orange for warnings
            }
        }
    }

    private void HandleBusLoadError(Exception ex)
    {
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
            $"Error loading bus data: {ex.Message}", "Data Load Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);

        if (statusLabel != null)
        {
            statusLabel.Text = "Error loading data";
            statusLabel.ForeColor = System.Drawing.Color.FromArgb(231, 76, 60);
        }
    }

    #region Button Event Handlers

    private void AddBusButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Add Bus button clicked");

            // Open Add Bus dialog using the service container
            var busEditForm = ServiceContainer.GetService<BusEditForm>();
            var result = busEditForm.ShowDialog(this);

            if (result == DialogResult.OK && busEditForm.IsDataSaved)
            {
                // Refresh the data grid asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadBusDataAsync();
                        this.Invoke(() =>
                        {
                            if (statusLabel != null)
                            {
                                statusLabel.Text = $"Bus {busEditForm.EditedBus?.BusNumber} added successfully";
                                statusLabel.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing after bus add");
                        this.Invoke(() =>
                        {
                            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                                $"Bus added but refresh failed: {ex.Message}", "Refresh Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        });
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Add Bus");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, $"Error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void EditBusButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Edit Bus button clicked");

            if (busDataGrid.SelectedItem != null)
            {
                var selectedBusInfo = busDataGrid.SelectedItem as BusInfo;

                if (selectedBusInfo != null)
                {
                    // Visual feedback during operation
                    if (editBusButton != null)
                    {
                        editBusButton.Enabled = false;
                        editBusButton.Text = "Loading...";
                    }

                    try
                    {
                        // Get the full Bus entity from the service
                        var busEntity = await _busService.GetBusEntityByIdAsync(selectedBusInfo.BusId);

                        if (busEntity != null)
                        {
                            // Create BusEditForm with dependency injection
                            var logger = ServiceContainer.GetService<ILogger<BusEditForm>>();
                            var busEditForm = new BusEditForm(logger, _busService, busEntity);
                            var result = busEditForm.ShowDialog(this);

                            if (result == DialogResult.OK && busEditForm.IsDataSaved)
                            {
                                // Refresh the data grid
                                await LoadBusDataAsync();
                                if (statusLabel != null)
                                {
                                    statusLabel.Text = $"Bus {busEditForm.EditedBus?.BusNumber} updated successfully";
                                    statusLabel.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
                                }
                            }
                        }
                        else
                        {
                            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                                "Could not load bus details for editing.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    finally
                    {
                        if (editBusButton != null)
                        {
                            editBusButton.Enabled = true;
                            editBusButton.Text = "Edit Bus";
                        }
                    }
                }
            }
            else
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Please select a bus to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Edit Bus");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, $"Error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            // Restore button state
            if (editBusButton != null)
            {
                editBusButton.Enabled = true;
                editBusButton.Text = "Edit Bus";
            }
        }
    }

    private async void DeleteBusButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Delete Bus button clicked");

            if (busDataGrid.SelectedItem != null)
            {
                var selectedBus = busDataGrid.SelectedItem as BusInfo;

                if (selectedBus != null)
                {
                    var result = Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                        $"Are you sure you want to delete bus {selectedBus.BusNumber}?\n\nThis action cannot be undone.",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if (result == DialogResult.Yes)
                    {
                        // Visual feedback during operation
                        if (deleteBusButton != null)
                        {
                            deleteBusButton.Enabled = false;
                            deleteBusButton.Text = "Deleting...";
                        }

                        try
                        {
                            bool success = await _busService.DeleteBusEntityAsync(selectedBus.BusId);

                            if (success)
                            {
                                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                                    $"Bus {selectedBus.BusNumber} has been deleted successfully.",
                                    "Delete Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Refresh the data grid
                                await LoadBusDataAsync();
                                if (statusLabel != null)
                                {
                                    statusLabel.Text = $"Bus {selectedBus.BusNumber} deleted successfully";
                                    statusLabel.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
                                }
                            }
                            else
                            {
                                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                                    $"Failed to delete bus {selectedBus.BusNumber}. Please try again.",
                                    "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        finally
                        {
                            if (deleteBusButton != null)
                            {
                                deleteBusButton.Enabled = true;
                                deleteBusButton.Text = "Delete Bus";
                            }
                        }
                    }
                }
            }
            else
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Please select a bus to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Delete Bus");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, $"Error deleting bus: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            // Restore button state
            if (deleteBusButton != null)
            {
                deleteBusButton.Enabled = true;
                deleteBusButton.Text = "Delete Bus";
            }
        }
    }

    private async void RefreshButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Refresh button clicked for Bus Management");

            // Visual feedback during refresh
            if (refreshButton != null)
            {
                refreshButton.Enabled = false;
                refreshButton.Text = "Refreshing...";
            }

            if (statusLabel != null)
            {
                statusLabel.Text = "Refreshing bus data...";
                statusLabel.ForeColor = System.Drawing.Color.FromArgb(52, 152, 219);
            }

            await LoadBusDataAsync();

            if (statusLabel != null)
            {
                statusLabel.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing bus data");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, $"Error refreshing data: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (statusLabel != null)
            {
                statusLabel.Text = "Refresh failed";
                statusLabel.ForeColor = System.Drawing.Color.FromArgb(231, 76, 60);
            }
        }
        finally
        {
            if (refreshButton != null)
            {
                refreshButton.Enabled = true;
                refreshButton.Text = "Refresh";
            }
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
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, $"Error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion
}

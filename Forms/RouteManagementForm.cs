using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using Syncfusion.WinForms.DataGrid.Events;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Services;
using Bus_Buddy.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace Bus_Buddy.Forms;

/// <summary>
/// Route Management Form for managing bus routes
/// Demonstrates route planning and management functionality
/// </summary>
public partial class RouteManagementForm : MetroForm
{
    private readonly ILogger<RouteManagementForm> _logger;
    private readonly IBusService _busService;
    private List<Route> _routes = new List<Route>();

    public RouteManagementForm(ILogger<RouteManagementForm> logger, IBusService busService)
    {
        _logger = logger;
        _busService = busService;

        _logger.LogInformation("Initializing Route Management form");
        InitializeComponent();
        InitializeRouteManagement();
    }

    private void InitializeRouteManagement()
    {
        // Apply Syncfusion theme integration
        try
        {
            // Set Office2016 visual style using SkinManager
            Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(this, Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not apply Office2016 theme, using default styling");
        }

        // Set Syncfusion MetroForm styles
        this.MetroColor = System.Drawing.Color.FromArgb(255, 87, 34);
        this.CaptionBarColor = System.Drawing.Color.FromArgb(255, 87, 34);
        this.CaptionForeColor = System.Drawing.Color.White;
        this.Text = "Route Management - Bus Routes";

        // Enable high DPI scaling for this form
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

        // Configure data grid columns
        ConfigureDataGridColumns();

        // Setup SfDataGrid event handlers
        routeDataGrid.SelectionChanged += RouteDataGrid_SelectionChanged;
        routeDataGrid.CellDoubleClick += RouteDataGrid_CellDoubleClick;
        routeDataGrid.QueryRowStyle += RouteDataGrid_QueryRowStyle;

        _logger.LogInformation("Route Management form initialized successfully");

        // Load route data asynchronously with proper UI thread marshaling
        _ = Task.Run(async () =>
        {
            try
            {
                await LoadRouteDataAsync();

                // Update UI on main thread
                this.Invoke(() =>
                {
                    statusLabel.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial data load");

                // Update UI on main thread for error state
                this.Invoke(() =>
                {
                    statusLabel.Text = "Failed to load initial data";
                    statusLabel.ForeColor = System.Drawing.Color.FromArgb(231, 76, 60);
                });
            }
        });
    }

    private void ConfigureDataGridColumns()
    {
        // Clear auto-generated columns
        routeDataGrid.AutoGenerateColumns = false;
        routeDataGrid.Columns.Clear();

        // Apply Office2016 styling to the grid
        routeDataGrid.Style.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(255, 87, 34);
        routeDataGrid.Style.HeaderStyle.TextColor = System.Drawing.Color.White;
        routeDataGrid.Style.HeaderStyle.Font.Bold = true;
        routeDataGrid.Style.BorderColor = System.Drawing.Color.FromArgb(227, 227, 227);
        routeDataGrid.Style.SelectionStyle.BackColor = System.Drawing.Color.FromArgb(255, 87, 34, 50);
        routeDataGrid.Style.SelectionStyle.TextColor = System.Drawing.Color.Black;

        // Add custom columns using Syncfusion GridTextColumn

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "RouteId",
            HeaderText = "Route ID",
            Width = 80,
            AllowEditing = false
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "RouteName",
            HeaderText = "Route Name",
            Width = 200,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "Description",
            HeaderText = "Description",
            Width = 300,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "AMVehicleId",
            HeaderText = "AM Vehicle ID",
            Width = 100,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "PMVehicleId",
            HeaderText = "PM Vehicle ID",
            Width = 100,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "AMDriverId",
            HeaderText = "AM Driver ID",
            Width = 100,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "PMDriverId",
            HeaderText = "PM Driver ID",
            Width = 100,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "AMBeginMiles",
            HeaderText = "AM Begin Miles",
            Width = 100,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "AMEndMiles",
            HeaderText = "AM End Miles",
            Width = 100,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "AMRiders",
            HeaderText = "AM Riders",
            Width = 80,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "PMBeginMiles",
            HeaderText = "PM Begin Miles",
            Width = 100,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "PMEndMiles",
            HeaderText = "PM End Miles",
            Width = 100,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "PMRiders",
            HeaderText = "PM Riders",
            Width = 80,
            AllowSorting = true
        });

        routeDataGrid.Columns.Add(new GridCheckBoxColumn()
        {
            MappingName = "IsActive",
            HeaderText = "Active",
            Width = 60,
            AllowSorting = true
        });

        // Enable advanced grid features
        routeDataGrid.AllowSorting = true;
        routeDataGrid.AllowFiltering = false; // Disable built-in filtering for custom implementation
        routeDataGrid.AllowResizingColumns = true;
        routeDataGrid.ShowRowHeader = false;
        routeDataGrid.AutoSizeColumnsMode = AutoSizeColumnsMode.None;
    }

    private async Task LoadRouteDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading route data");

            // Load routes from service
            _routes = await _busService.GetAllRouteEntitiesAsync();

            // Update UI on main thread
            if (this.InvokeRequired)
            {
                this.Invoke(() => UpdateGridAndStatus());
            }
            else
            {
                UpdateGridAndStatus();
            }

            _logger.LogInformation("Loaded {RouteCount} routes", _routes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading route data");

            // Handle UI updates on main thread
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

    private void UpdateGridAndStatus()
    {
        // Update the data grid
        routeDataGrid.DataSource = _routes;

        // Update status with success styling
        statusLabel.Text = $"Loaded {_routes.Count} routes";
        statusLabel.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);

        // Count inactive routes for status
        var inactiveCount = _routes.Count(r => r.IsActive == false);
        if (inactiveCount > 0)
        {
            statusLabel.Text += $" - {inactiveCount} inactive route(s)";
            statusLabel.ForeColor = System.Drawing.Color.FromArgb(255, 152, 0); // Orange for warnings
        }
    }

    private void HandleLoadError(Exception ex)
    {
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
            $"Error loading route data: {ex.Message}", "Data Load Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
        statusLabel.Text = "Error loading data";
        statusLabel.ForeColor = System.Drawing.Color.FromArgb(231, 76, 60);
    }

    private void RouteDataGrid_QueryRowStyle(object sender, QueryRowStyleEventArgs e)
    {
        if (e.RowData is Route route)
        {
            // Apply styling based on route status
            if (route.IsActive == false)
            {
                // Inactive routes - subtle gray styling
                e.Style.BackColor = Color.FromArgb(248, 248, 248);
                e.Style.TextColor = Color.FromArgb(128, 128, 128);
            }
            else if (!route.AMVehicleId.HasValue || !route.PMVehicleId.HasValue)
            {
                // Routes missing vehicle assignments - light orange warning
                e.Style.BackColor = Color.FromArgb(255, 245, 230);
                e.Style.TextColor = Color.FromArgb(184, 134, 11);
            }
            else if (!route.AMDriverId.HasValue || !route.PMDriverId.HasValue)
            {
                // Routes missing driver assignments - light yellow warning
                e.Style.BackColor = Color.FromArgb(254, 252, 232);
                e.Style.TextColor = Color.FromArgb(161, 98, 7);
            }
        }
    }

    #region Button Event Handlers

    private async void AddRouteButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Add Route button clicked");

            // Get services from ServiceContainer
            var logger = ServiceContainer.GetService<ILogger<RouteEditForm>>();
            var busService = ServiceContainer.GetService<IBusService>();

            if (logger == null || busService == null)
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Required services not available", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var addForm = new RouteEditForm(logger, busService);

            if (addForm.ShowDialog() == DialogResult.OK && addForm.IsDataSaved)
            {
                // Refresh the grid to show the new route
                await LoadRouteDataAsync();
                _logger.LogInformation("New route added successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Add Route");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void EditRouteButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Edit Route button clicked");

            if (routeDataGrid.CurrentItem != null && routeDataGrid.CurrentItem is Route selectedRoute)
            {
                // Get services from ServiceContainer
                var logger = ServiceContainer.GetService<ILogger<RouteEditForm>>();
                var busService = ServiceContainer.GetService<IBusService>();

                if (logger == null || busService == null)
                {
                    Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                        "Required services not available", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var editForm = new RouteEditForm(logger, busService, selectedRoute);

                if (editForm.ShowDialog() == DialogResult.OK && editForm.IsDataSaved)
                {
                    // Refresh the grid to show updated route data
                    await LoadRouteDataAsync();
                    _logger.LogInformation("Route {RouteId} updated successfully", selectedRoute.RouteId);
                }
            }
            else
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Please select a route to edit.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Edit Route");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteRouteButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Delete Route button clicked");

            if (routeDataGrid.CurrentItem != null && routeDataGrid.CurrentItem is Route selectedRoute)
            {
                var result = Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    $"Are you sure you want to delete route '{selectedRoute.RouteName}'?\n\nThis action cannot be undone.",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                        $"Delete Route functionality will be implemented here.\n\nRoute '{selectedRoute.RouteName}' would be deleted.",
                        "Delete Route", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Please select a route to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Delete Route");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error deleting route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ViewStopsButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("View Stops button clicked");

            if (routeDataGrid.CurrentItem != null && routeDataGrid.CurrentItem is Route selectedRoute)
            {
                // Show route stops information
                var stopsInfo = $"Route Stops for '{selectedRoute.RouteName}'\n\n" +
                    $"Route ID: {selectedRoute.RouteId}\n" +
                    $"Description: {selectedRoute.Description}\n" +
                    $"Status: {(selectedRoute.IsActive ? "Active" : "Inactive")}\n\n" +
                    "This will show detailed stop information including:\n" +
                    "• Stop locations and addresses\n" +
                    "• Pickup times\n" +
                    "• Student assignments\n" +
                    "• Special instructions";

                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    stopsInfo, "Route Stops", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Please select a route to view stops.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing route stops");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void RefreshButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Refresh button clicked");

            // Visual feedback during refresh
            refreshButton.Enabled = false;
            refreshButton.Text = "Refreshing...";
            statusLabel.Text = "Refreshing route data...";
            statusLabel.ForeColor = System.Drawing.Color.FromArgb(52, 152, 219);

            // Perform refresh operation
            await LoadRouteDataAsync();

            statusLabel.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing data");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error refreshing data: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            statusLabel.Text = "Refresh failed";
            statusLabel.ForeColor = System.Drawing.Color.FromArgb(231, 76, 60);
        }
        finally
        {
            refreshButton.Enabled = true;
            refreshButton.Text = "Refresh";
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
                $"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion

    private void RouteDataGrid_CellDoubleClick(object sender, CellClickEventArgs e)
    {
        // Double-click to edit route if there's a current item selected
        if (routeDataGrid.CurrentItem != null)
        {
            EditRouteButton_Click(sender, EventArgs.Empty);
        }
    }

    private void RouteDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Enable/disable buttons based on selection
        var hasSelection = routeDataGrid.CurrentItem != null;
        editRouteButton.Enabled = hasSelection;
        deleteRouteButton.Enabled = hasSelection;
        viewStopsButton.Enabled = hasSelection;
    }
}

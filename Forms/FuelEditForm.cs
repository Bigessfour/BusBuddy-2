using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Bus_Buddy.Data.UnitOfWork;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Bus_Buddy.Utilities;
using System.Collections.Generic;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// Fuel record edit form using Syncfusion v30.1.37 components
    /// Provides CRUD operations for fuel data
    /// </summary>
    public partial class FuelEditForm : MetroForm
    {
        private readonly ILogger<FuelEditForm> _logger;
        private readonly IFuelService _fuelService;
        private readonly IBusService _busService;

        // Data Management
        private Fuel? _currentFuel;
        private bool _isEditMode = false;

        public Fuel? EditedFuel { get; private set; }
        public bool IsDataSaved { get; private set; }

        public FuelEditForm(ILogger<FuelEditForm> logger, IFuelService fuelService, IBusService busService, Fuel? fuel = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fuelService = fuelService ?? throw new ArgumentNullException(nameof(fuelService));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));

            _currentFuel = fuel ?? new Fuel();
            _isEditMode = fuel != null;

            InitializeComponent();
            InitializeFormStyling();
            LoadFormData();

            if (_isEditMode && _currentFuel != null)
            {
                LoadFuelData();
            }

            _logger.LogInformation("FuelEditForm initialized in {Mode} mode", _isEditMode ? "Edit" : "Add");
        }

        private void InitializeFormStyling()
        {
            // Title label
            titleLabel.Text = _isEditMode ? "Edit Fuel Record" : "Add New Fuel Record";

            // Fuel Type ComboBox
            fuelTypeComboBox.DataSource = new List<string> {
                "Gasoline", "Diesel"
            };

            // Save button
            saveButton.Text = _isEditMode ? "Update" : "Save";

            // Apply visual enhancements
            ApplyMetroTheme();
        }

        private async void LoadFormData()
        {
            try
            {
                // Load buses for selection
                var buses = await _busService.GetAllBusesAsync();
                busComboBox.DataSource = buses.ToList();
                busComboBox.DisplayMember = "BusNumber";
                busComboBox.ValueMember = "BusId";

                _logger.LogDebug("Form data loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading form data");
                MessageBox.Show("Error loading form data. Please try again.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFuelData()
        {
            if (_currentFuel == null) return;

            busComboBox.SelectedValue = _currentFuel.VehicleFueledId;
            dateEdit.Value = _currentFuel.FuelDate;
            odometerTextBox.Text = _currentFuel.VehicleOdometerReading.ToString();
            fuelTypeComboBox.Text = _currentFuel.FuelType;
            costTextBox.Text = _currentFuel.TotalCost.ToString();
            notesTextBox.Text = _currentFuel.Notes;
            gallonsTextBox.Text = _currentFuel.Gallons.ToString();
            pricePerGallonTextBox.Text = _currentFuel.PricePerGallon.ToString();
            locationTextBox.Text = _currentFuel.FuelLocation;
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput()) return;

                UpdateFuelFromForm();

                if (_isEditMode && _currentFuel != null)
                {
                    EditedFuel = await _fuelService.UpdateFuelRecordAsync(_currentFuel);
                    _logger.LogInformation("Fuel record updated: ID {FuelId}", _currentFuel.FuelId);
                }
                else if (_currentFuel != null)
                {
                    EditedFuel = await _fuelService.CreateFuelRecordAsync(_currentFuel);
                    _logger.LogInformation("New fuel record created");
                }

                IsDataSaved = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving fuel record");
                MessageBox.Show($"Error saving fuel record: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateFuelFromForm()
        {
            if (_currentFuel == null) return;

            _currentFuel.VehicleFueledId = (int)busComboBox.SelectedValue;
            _currentFuel.FuelDate = dateEdit.Value!.Value;
            _currentFuel.VehicleOdometerReading = int.Parse(odometerTextBox.Text);
            _currentFuel.FuelType = fuelTypeComboBox.Text;
            _currentFuel.TotalCost = decimal.Parse(costTextBox.Text);
            _currentFuel.Notes = notesTextBox.Text;
            _currentFuel.Gallons = decimal.Parse(gallonsTextBox.Text);
            _currentFuel.PricePerGallon = decimal.Parse(pricePerGallonTextBox.Text);
            _currentFuel.FuelLocation = locationTextBox.Text;
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateInput()
        {
            if (busComboBox.SelectedValue == null)
            {
                MessageBoxAdv.Show(this, "Please select a bus.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (dateEdit.Value == null)
            {
                MessageBoxAdv.Show(this, "Please select a date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(odometerTextBox.Text, out int odometer) || odometer < 0)
            {
                MessageBoxAdv.Show(this, "Please enter a valid odometer reading.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(fuelTypeComboBox.Text))
            {
                MessageBoxAdv.Show(this, "Please select a fuel type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(locationTextBox.Text))
            {
                MessageBoxAdv.Show(this, "Please enter a location.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!decimal.TryParse(costTextBox.Text, out decimal cost) || cost < 0)
            {
                MessageBoxAdv.Show(this, "Please enter a valid cost.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!decimal.TryParse(gallonsTextBox.Text, out decimal gallons) || gallons < 0)
            {
                MessageBoxAdv.Show(this, "Please enter a valid number of gallons.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!decimal.TryParse(pricePerGallonTextBox.Text, out decimal price) || price < 0)
            {
                MessageBoxAdv.Show(this, "Please enter a valid price per gallon.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void ApplyMetroTheme()
        {
            try
            {
                // Apply Syncfusion Metro styling
                BackColor = Color.FromArgb(255, 255, 255);
                VisualEnhancementManager.ApplyEnhancedTheme(this);

                _logger.LogDebug("Metro theme applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error applying metro theme");
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Set focus to the first input control
            if (busComboBox != null)
            {
                busComboBox.Focus();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of Syncfusion controls
                titleLabel?.Dispose();
                busLabel?.Dispose();
                dateLabel?.Dispose();
                odometerLabel?.Dispose();
                fuelTypeLabel?.Dispose();
                locationLabel?.Dispose();
                costLabel?.Dispose();
                notesLabel?.Dispose();
                gallonsLabel?.Dispose();
                pricePerGallonLabel?.Dispose();
                busComboBox?.Dispose();
                fuelTypeComboBox?.Dispose();
                dateEdit?.Dispose();
                odometerTextBox?.Dispose();
                costTextBox?.Dispose();
                notesTextBox?.Dispose();
                gallonsTextBox?.Dispose();
                pricePerGallonTextBox?.Dispose();
                locationTextBox?.Dispose();
                saveButton?.Dispose();
                cancelButton?.Dispose();
                mainPanel?.Dispose();
                formTableLayout?.Dispose();
                buttonPanel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

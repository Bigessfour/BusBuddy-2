using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Services;
using Bus_Buddy.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bus_Buddy.Forms;

/// <summary>
/// Driver Add/Edit Form for creating and editing driver records
/// Demonstrates comprehensive data entry form with validation using Syncfusion controls
/// </summary>
public partial class DriverEditForm : MetroForm
{
    private readonly ILogger<DriverEditForm> _logger;
    private readonly IBusService _busService;
    private Driver? _currentDriver;
    private bool _isEditMode;

    public Driver? EditedDriver { get; private set; }
    public bool IsDataSaved { get; private set; }

    public DriverEditForm(ILogger<DriverEditForm> logger, IBusService busService, Driver? driver = null)
    {
        _logger = logger;
        _busService = busService;
        _currentDriver = driver;
        _isEditMode = driver != null;

        _logger.LogInformation("Initializing Driver Edit form in {Mode} mode", _isEditMode ? "Edit" : "Add");

        InitializeComponent();

        if (_isEditMode)
        {
            this.Text = "Edit Driver";
        }
        else
        {
            this.Text = "Add New Driver";
        }

        if (_isEditMode && _currentDriver != null)
        {
            LoadDriverData(_currentDriver);
        }
    }

    private void LoadDriverData(Driver driver)
    {
        try
        {
            _logger.LogInformation("Loading driver data for editing: {DriverName}", driver.DriverName);

            firstNameTextBox.Text = driver.FirstName;
            lastNameTextBox.Text = driver.LastName;
            driverNameTextBox.Text = driver.DriverName;
            phoneTextBox.Text = driver.DriverPhone ?? string.Empty;
            emailTextBox.Text = driver.DriverEmail ?? string.Empty;
            addressTextBox.Text = driver.Address ?? string.Empty;
            cityTextBox.Text = driver.City ?? string.Empty;
            stateComboBox.Text = driver.State ?? string.Empty;
            zipTextBox.Text = driver.Zip ?? string.Empty;

            licenseNumberTextBox.Text = driver.LicenseNumber ?? string.Empty;
            licenseTypeComboBox.Text = driver.DriversLicenceType;
            licenseClassComboBox.Text = driver.LicenseClass ?? string.Empty;

            if (driver.LicenseIssueDate.HasValue)
                licenseIssueDatePicker.Value = driver.LicenseIssueDate.Value;

            if (driver.LicenseExpiryDate.HasValue)
                licenseExpiryDatePicker.Value = driver.LicenseExpiryDate.Value;

            endorsementsTextBox.Text = driver.Endorsements ?? string.Empty;

            if (driver.HireDate.HasValue)
                hireDatePicker.Value = driver.HireDate.Value;

            statusComboBox.Text = driver.Status;
            trainingCompleteCheckBox.Checked = driver.TrainingComplete;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading driver data");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error loading driver data: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private bool ValidateForm()
    {
        var errors = new List<string>();

        // Required field validation
        if (string.IsNullOrWhiteSpace(firstNameTextBox.Text))
            errors.Add("First Name is required");

        if (string.IsNullOrWhiteSpace(lastNameTextBox.Text))
            errors.Add("Last Name is required");

        if (string.IsNullOrWhiteSpace(driverNameTextBox.Text))
            errors.Add("Driver Name is required");

        if (string.IsNullOrWhiteSpace(licenseTypeComboBox.Text))
            errors.Add("License Type is required");

        if (string.IsNullOrWhiteSpace(licenseNumberTextBox.Text))
            errors.Add("License Number is required");

        // Email validation
        if (!string.IsNullOrWhiteSpace(emailTextBox.Text))
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(emailTextBox.Text);
                if (addr.Address != emailTextBox.Text)
                    errors.Add("Email address format is invalid");
            }
            catch
            {
                errors.Add("Email address format is invalid");
            }
        }

        // Phone validation (basic)
        if (!string.IsNullOrWhiteSpace(phoneTextBox.Text))
        {
            var phone = phoneTextBox.Text.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if (phone.Length < 10)
                errors.Add("Phone number must be at least 10 digits");
        }

        // Date validation
        if (licenseIssueDatePicker.Value > DateTime.Now)
            errors.Add("License issue date cannot be in the future");

        if (licenseExpiryDatePicker.Value <= licenseIssueDatePicker.Value)
            errors.Add("License expiry date must be after issue date");

        if (hireDatePicker.Value > DateTime.Now)
            errors.Add("Hire date cannot be in the future");

        // Zip code validation
        if (!string.IsNullOrWhiteSpace(zipTextBox.Text))
        {
            var zip = zipTextBox.Text.Replace("-", "");
            if (zip.Length != 5 && zip.Length != 9)
                errors.Add("Zip code must be 5 or 9 digits");
        }

        if (errors.Count > 0)
        {
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Please correct the following errors:\n\n{string.Join("\n", errors)}",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private Driver CreateDriverFromForm()
    {
        var driver = _currentDriver ?? new Driver();

        driver.FirstName = firstNameTextBox.Text.Trim();
        driver.LastName = lastNameTextBox.Text.Trim();
        driver.DriverName = driverNameTextBox.Text.Trim();
        driver.DriverPhone = string.IsNullOrWhiteSpace(phoneTextBox.Text) ? null : phoneTextBox.Text.Trim();
        driver.DriverEmail = string.IsNullOrWhiteSpace(emailTextBox.Text) ? null : emailTextBox.Text.Trim();
        driver.Address = string.IsNullOrWhiteSpace(addressTextBox.Text) ? null : addressTextBox.Text.Trim();
        driver.City = string.IsNullOrWhiteSpace(cityTextBox.Text) ? null : cityTextBox.Text.Trim();
        driver.State = string.IsNullOrWhiteSpace(stateComboBox.Text) ? null : stateComboBox.Text.Trim();
        driver.Zip = string.IsNullOrWhiteSpace(zipTextBox.Text) ? null : zipTextBox.Text.Trim();

        driver.LicenseNumber = string.IsNullOrWhiteSpace(licenseNumberTextBox.Text) ? null : licenseNumberTextBox.Text.Trim();
        driver.DriversLicenceType = licenseTypeComboBox.Text.Trim();
        driver.LicenseClass = string.IsNullOrWhiteSpace(licenseClassComboBox.Text) ? null : licenseClassComboBox.Text.Trim();
        driver.LicenseIssueDate = licenseIssueDatePicker.Value;
        driver.LicenseExpiryDate = licenseExpiryDatePicker.Value;
        driver.Endorsements = string.IsNullOrWhiteSpace(endorsementsTextBox.Text) ? null : endorsementsTextBox.Text.Trim();

        driver.HireDate = hireDatePicker.Value;
        driver.Status = statusComboBox.Text;
        driver.TrainingComplete = trainingCompleteCheckBox.Checked;

        return driver;
    }

    #region Button Event Handlers

    private async void SaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (!ValidateForm())
                return;

            _logger.LogInformation("Attempting to save driver data");

            // Show saving state with visual feedback
            ShowSavingState();

            var driver = CreateDriverFromForm();

            bool success;
            if (_isEditMode)
            {
                success = await _busService.UpdateDriverEntityAsync(driver);
                _logger.LogInformation("Updated driver: {DriverName}", driver.DriverName);
            }
            else
            {
                await _busService.AddDriverEntityAsync(driver);
                success = true;
                _logger.LogInformation("Added new driver: {DriverName}", driver.DriverName);
            }

            if (success || !_isEditMode)
            {
                EditedDriver = driver;
                IsDataSaved = true;

                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    $"Driver {driver.DriverName} has been {(_isEditMode ? "updated" : "added")} successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "Failed to save driver data. Please try again.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving driver data");

            if (this.InvokeRequired)
            {
                this.Invoke(() => HandleSaveError(ex));
            }
            else
            {
                HandleSaveError(ex);
            }
        }
        finally
        {
            RestoreSaveButtonState();
        }
    }

    private void ShowSavingState()
    {
        saveButton.Enabled = false;
        saveButton.Text = "Saving...";
        saveButton.BackColor = Color.FromArgb(158, 158, 158);
    }

    private void RestoreSaveButtonState()
    {
        saveButton.Enabled = true;
        saveButton.Text = "Save";
        saveButton.BackColor = Color.FromArgb(76, 175, 80);
    }

    private void HandleSaveError(Exception ex)
    {
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
            $"Error saving driver data: {ex.Message}",
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Cancel button clicked");

            if (HasUnsavedChanges())
            {
                var result = Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                    "You have unsaved changes. Are you sure you want to cancel?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                    return;
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cancel operation");
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Error: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    #endregion

    private bool HasUnsavedChanges()
    {
        if (!_isEditMode)
        {
            // For new drivers, check if any data has been entered
            return !string.IsNullOrWhiteSpace(firstNameTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(lastNameTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(driverNameTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(phoneTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(emailTextBox.Text);
        }

        if (_currentDriver == null) return false;

        // For existing drivers, compare with original data
        return firstNameTextBox.Text != _currentDriver.FirstName ||
               lastNameTextBox.Text != _currentDriver.LastName ||
               driverNameTextBox.Text != _currentDriver.DriverName ||
               phoneTextBox.Text != (_currentDriver.DriverPhone ?? string.Empty) ||
               emailTextBox.Text != (_currentDriver.DriverEmail ?? string.Empty) ||
               statusComboBox.Text != _currentDriver.Status ||
               trainingCompleteCheckBox.Checked != _currentDriver.TrainingComplete;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (this.DialogResult == DialogResult.None && HasUnsavedChanges())
        {
            var result = Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                "You have unsaved changes. Are you sure you want to close?",
                "Unsaved Changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        base.OnFormClosing(e);
    }
}

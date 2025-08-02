using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core;
using Microsoft.EntityFrameworkCore;
using BusBuddy.WPF;
using Serilog;

namespace BusBuddy.WPF.ViewModels.Student
{
    /// <summary>
    /// ViewModel for the StudentForm - handles adding and editing students
    /// Includes address validation and route assignment functionality
    /// </summary>
    public class StudentFormViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<StudentFormViewModel>();

        private readonly BusBuddyContext _context;
        private readonly AddressService _addressService;
        private Core.Models.Student _student;
        private string _formTitle = "Add New Student";
        private string _addressValidationMessage = string.Empty;
        private Brush _addressValidationColor = Brushes.Gray;
        private bool _isEditMode;

        public StudentFormViewModel() : this(new Core.Models.Student())
        {
        }

        public StudentFormViewModel(Core.Models.Student? student = null)
        {
            _context = new BusBuddyContext();
            _addressService = new AddressService();
            // For MVP, we'll do simple validation directly in the ViewModel
            // TODO: Inject AddressValidationService when UnitOfWork is available

            _student = student ?? new Core.Models.Student
            {
                Active = true,
                EnrollmentDate = DateTime.Today,
                CreatedDate = DateTime.Now
            };

            _isEditMode = student != null && student.StudentId > 0;
            _formTitle = _isEditMode ? "Edit Student" : "Add New Student";

            AvailableRoutes = new ObservableCollection<string>();
            AvailableBusStops = new ObservableCollection<string>();

            InitializeCommands();
            _ = LoadDataAsync();
        }

        #region Properties

        /// <summary>
        /// Student being edited or added
        /// </summary>
        public Core.Models.Student Student
        {
            get => _student;
            set => SetProperty(ref _student, value);
        }

        /// <summary>
        /// Form title (Add New Student or Edit Student)
        /// </summary>
        public string FormTitle
        {
            get => _formTitle;
            set => SetProperty(ref _formTitle, value);
        }

        /// <summary>
        /// Address validation message to display to user
        /// </summary>
        public string AddressValidationMessage
        {
            get => _addressValidationMessage;
            set => SetProperty(ref _addressValidationMessage, value);
        }

        /// <summary>
        /// Color for address validation message (green for success, red for error)
        /// </summary>
        public Brush AddressValidationColor
        {
            get => _addressValidationColor;
            set => SetProperty(ref _addressValidationColor, value);
        }

        /// <summary>
        /// Available route names for assignment
        /// </summary>
        public ObservableCollection<string> AvailableRoutes { get; }

        /// <summary>
        /// Available bus stop names for assignment
        /// </summary>
        public ObservableCollection<string> AvailableBusStops { get; }

        /// <summary>
        /// Whether form is in edit mode (vs add mode)
        /// </summary>
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        #endregion

        #region Commands

        public ICommand ValidateAddressCommand { get; private set; } = null!;
        public ICommand SaveCommand { get; private set; } = null!;
        public ICommand CancelCommand { get; private set; } = null!;

        #endregion

        #region Command Initialization

        private void InitializeCommands()
        {
            ValidateAddressCommand = new RelayCommand(async () => await ValidateAddressAsync());
            SaveCommand = new RelayCommand(async () => await SaveStudentAsync(), CanSaveStudent);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        #endregion

        #region Command Handlers

        /// <summary>
        /// Validate the student's address using simple regex patterns
        /// </summary>
        private async Task ValidateAddressAsync()
        {
            try
            {
                Logger.Information("Validating address for student");

                if (string.IsNullOrWhiteSpace(Student.HomeAddress))
                {
                    AddressValidationMessage = "Please enter an address before validating.";
                    AddressValidationColor = Brushes.Orange;
                    return;
                }

                // Use the AddressService for validation
                var addressValidation = _addressService.ValidateAddress(Student.HomeAddress);

                // Also validate components if available
                var componentValidation = _addressService.ValidateAddressComponents(
                    Student.HomeAddress, Student.City, Student.State, Student.Zip);

                bool isValid = addressValidation.IsValid && componentValidation.IsValid;
                string errorMessage = !addressValidation.IsValid ? addressValidation.Error : componentValidation.Error;

                if (isValid)
                {
                    AddressValidationMessage = "✓ Address format is valid.";
                    AddressValidationColor = Brushes.Green;
                    Logger.Information("Address validation successful");
                }
                else
                {
                    AddressValidationMessage = $"✗ Address validation failed: {errorMessage}";
                    AddressValidationColor = Brushes.Red;
                    Logger.Warning("Address validation failed: {Error}", errorMessage);
                }

                await Task.CompletedTask; // Make method async-compatible
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error validating address");
                AddressValidationMessage = "✗ Error validating address. Please check format and try again.";
                AddressValidationColor = Brushes.Red;
            }
        }

        /// <summary>
        /// Save the student to the database
        /// </summary>
        private async Task SaveStudentAsync()
        {
            try
            {
                Logger.Information("Saving student {StudentName}", Student.StudentName);

                // Validate required fields
                if (!IsValidStudent())
                {
                    return;
                }

                // Set audit fields
                if (IsEditMode)
                {
                    Student.UpdatedDate = DateTime.Now;
                    Student.UpdatedBy = Environment.UserName;
                    _context.Students.Update(Student);
                }
                else
                {
                    Student.CreatedDate = DateTime.Now;
                    Student.CreatedBy = Environment.UserName;
                    _context.Students.Add(Student);
                }

                await _context.SaveChangesAsync();

                Logger.Information("Successfully saved student {StudentId} - {StudentName}",
                    Student.StudentId, Student.StudentName);

                // TODO: Close form and return to student list
                ExecuteCancel();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error saving student");
                // TODO: Show error message to user
            }
        }

        private bool CanSaveStudent()
        {
            return !string.IsNullOrWhiteSpace(Student?.StudentName) &&
                   !string.IsNullOrWhiteSpace(Student?.Grade);
        }

        private void ExecuteCancel()
        {
            try
            {
                Logger.Information("Cancel command executed");
                // TODO: Close form without saving
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing cancel command");
            }
        }

        #endregion

        #region Data Operations

        /// <summary>
        /// Load available routes and bus stops for the form
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                Logger.Information("Loading form data");

                // Load available routes (from seed data or existing routes)
                var routes = new[] { "Route A", "Route B", "Route C", "Route D" };
                AvailableRoutes.Clear();
                foreach (var route in routes)
                {
                    AvailableRoutes.Add(route);
                }

                // Load available bus stops
                var busStops = new[]
                {
                    "Oak & 1st", "Maple & Main", "Pine & Center", "Elm & 2nd",
                    "Cedar & Park", "Birch & State", "Walnut & Lincoln", "Cherry & Washington",
                    "Spruce & Adams", "Hickory & Jefferson", "Poplar & Monroe", "Ash & Madison",
                    "Sycamore & Jackson", "Willow & Van Buren", "Dogwood & Harrison"
                };

                AvailableBusStops.Clear();
                foreach (var stop in busStops)
                {
                    AvailableBusStops.Add(stop);
                }

                Logger.Information("Loaded {RouteCount} routes and {StopCount} bus stops",
                    AvailableRoutes.Count, AvailableBusStops.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading form data");
            }
        }

        /// <summary>
        /// Validate required student fields and address format
        /// </summary>
        private bool IsValidStudent()
        {
            if (string.IsNullOrWhiteSpace(Student.StudentName))
            {
                AddressValidationMessage = "✗ Student name is required.";
                AddressValidationColor = Brushes.Red;
                return false;
            }

            if (string.IsNullOrWhiteSpace(Student.Grade))
            {
                AddressValidationMessage = "✗ Grade is required.";
                AddressValidationColor = Brushes.Red;
                return false;
            }

            // Validate address if provided
            if (!string.IsNullOrWhiteSpace(Student.HomeAddress))
            {
                var addressValidation = _addressService.ValidateAddress(Student.HomeAddress);
                if (!addressValidation.IsValid)
                {
                    AddressValidationMessage = $"✗ Address validation failed: {addressValidation.Error}";
                    AddressValidationColor = Brushes.Red;
                    return false;
                }
            }

            AddressValidationMessage = "✓ Student information is valid.";
            AddressValidationColor = Brushes.Green;
            return true;
        }

        /// <summary>
        /// Simple address validation using regex patterns (MVP implementation)
        /// </summary>
        private (bool IsValid, string? ErrorMessage) ValidateAddressComponents(string street, string city, string state, string zipCode)
        {
            var validationMessages = new List<string>();

            // Validate street address
            if (string.IsNullOrWhiteSpace(street))
            {
                validationMessages.Add("Street address is required");
            }
            else if (!Regex.IsMatch(street.Trim(), @"^\d+\s+[\w\s\.,#-]+$"))
            {
                validationMessages.Add("Street address must start with a number followed by street name");
            }

            // Validate city
            if (string.IsNullOrWhiteSpace(city))
            {
                validationMessages.Add("City is required");
            }
            else if (!Regex.IsMatch(city.Trim(), @"^[A-Za-z\s\.-]+$"))
            {
                validationMessages.Add("City name can only contain letters, spaces, periods, and hyphens");
            }

            // Validate state
            if (string.IsNullOrWhiteSpace(state))
            {
                validationMessages.Add("State is required");
            }
            else if (!IsValidState(state))
            {
                validationMessages.Add("State must be a valid 2-letter US state abbreviation");
            }

            // Validate ZIP code
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                validationMessages.Add("ZIP code is required");
            }
            else if (!IsValidZipCode(zipCode))
            {
                validationMessages.Add("ZIP code must be 5 digits or 5+4 format (e.g., 12345 or 12345-6789)");
            }

            if (validationMessages.Any())
            {
                return (false, string.Join("; ", validationMessages));
            }

            return (true, null);
        }

        /// <summary>
        /// Validates ZIP code format (5-digit or 9-digit)
        /// </summary>
        private bool IsValidZipCode(string zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                return false;
            }

            // 5-digit or 5+4 format
            return Regex.IsMatch(zipCode.Trim(), @"^\d{5}(-\d{4})?$");
        }

        /// <summary>
        /// Validates US state abbreviation
        /// </summary>
        private bool IsValidState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                return false;
            }

            var validStates = new HashSet<string>
            {
                "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
                "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
                "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
                "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
                "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY",
                "DC"  // District of Columbia
            };

            return validStates.Contains(state.ToUpperInvariant());
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

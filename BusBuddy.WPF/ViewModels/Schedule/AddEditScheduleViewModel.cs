using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.Core.Services;
using BusBuddy.WPF.Views.Schedule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;
using System.Windows;

namespace BusBuddy.WPF.ViewModels.Schedule
{
    public partial class AddEditScheduleViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;
        private readonly IRouteService _routeService;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;
        private readonly BusBuddy.Core.Models.Schedule? _originalSchedule;
        private readonly ILogger _logger;

        public ObservableCollection<Route> Routes { get; } = new();
        public ObservableCollection<Bus> Buses { get; } = new();
        public ObservableCollection<Driver> Drivers { get; } = new();
        public ObservableCollection<string> StatusOptions { get; } = new()
        {
            "Scheduled", "InProgress", "Completed", "Cancelled", "Delayed"
        };

        [ObservableProperty]
        private string _dialogTitle = "Add Schedule";

        [ObservableProperty]
        private Route? _selectedRoute;

        [ObservableProperty]
        private Bus? _selectedBus;

        [ObservableProperty]
        private Driver? _selectedDriver;

        [ObservableProperty]
        private DateTime _scheduleDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _departureTime = DateTime.Today.AddHours(8);

        [ObservableProperty]
        private DateTime _arrivalTime = DateTime.Today.AddHours(9);

        [ObservableProperty]
        private string _selectedStatus = "Scheduled";

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasValidationErrors;

        public ICommand SaveCommand { get; }
        public ICommand LoadDataCommand { get; }
        public ICommand CancelCommand { get; }

        public bool IsEditMode => _originalSchedule != null;

        public AddEditScheduleViewModel(
            IScheduleService scheduleService,
            IRouteService routeService,
            IBusService busService,
            IDriverService driverService,
            BusBuddy.Core.Models.Schedule? schedule = null)
        {
            _scheduleService = scheduleService;
            _routeService = routeService;
            _busService = busService;
            _driverService = driverService;
            _originalSchedule = schedule;
            _logger = Log.ForContext<AddEditScheduleViewModel>()
                .ForContext("Module", "ScheduleManagement")
                .ForContext("Component", "AddEditScheduleViewModel")
                .ForContext("IsEditMode", IsEditMode);

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            CancelCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(Cancel);

            if (_originalSchedule != null)
            {
                DialogTitle = "Edit Schedule";
                LoadScheduleData();
                _logger.Information("AddEditScheduleViewModel initialized for editing schedule {ScheduleId}", _originalSchedule.ScheduleId);
            }
            else
            {
                _logger.Information("AddEditScheduleViewModel initialized for adding new schedule");
            }
        }

        private async Task LoadDataAsync()
        {
            _logger.Information("Loading reference data for schedule dialog");

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                _logger.Information("Loading reference data for schedule dialog");

                // Load routes
                var routes = await _routeService.GetAllActiveRoutesAsync();
                Routes.Clear();
                foreach (var route in routes)
                {
                    Routes.Add(route);
                }
                _logger.Debug("Loaded {RouteCount} routes", Routes.Count);

                // Load buses
                var buses = await _busService.GetAllBusesAsync();
                Buses.Clear();
                foreach (var bus in buses)
                {
                    Buses.Add(bus);
                }
                _logger.Debug("Loaded {BusCount} buses", Buses.Count);

                // Load drivers
                var drivers = await _driverService.GetAllDriversAsync();
                Drivers.Clear();
                foreach (var driver in drivers)
                {
                    Drivers.Add(driver);
                }
                _logger.Debug("Loaded {DriverCount} drivers", Drivers.Count);

                // Select current items if editing
                if (_originalSchedule != null)
                {
                    SelectedRoute = Routes.FirstOrDefault(r => r.RouteId == _originalSchedule.RouteId);
                    SelectedBus = Buses.FirstOrDefault(b => b.VehicleId == _originalSchedule.BusId);
                    SelectedDriver = Drivers.FirstOrDefault(d => d.DriverId == _originalSchedule.DriverId);

                    _logger.Debug("Selected existing values: Route={RouteId}, Bus={BusId}, Driver={DriverId}",
                        SelectedRoute?.RouteId, SelectedBus?.VehicleId, SelectedDriver?.DriverId);
                }

                _logger.Information("Successfully loaded all reference data");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load reference data");
                ErrorMessage = "Failed to load reference data. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadScheduleData()
        {
            if (_originalSchedule == null) return;

            _logger.Debug("Loading schedule data for editing");

            ScheduleDate = _originalSchedule.ScheduleDate;
            DepartureTime = _originalSchedule.DepartureTime;
            ArrivalTime = _originalSchedule.ArrivalTime;
            SelectedStatus = _originalSchedule.Status;
            Notes = _originalSchedule.Notes;

            _logger.Debug("Loaded schedule data: Date={ScheduleDate}, Departure={DepartureTime}, Arrival={ArrivalTime}, Status={Status}",
                ScheduleDate, DepartureTime, ArrivalTime, SelectedStatus);
        }

        private async Task SaveAsync()
        {
            _logger.Information("Attempting to save schedule");

            try
            {
                ErrorMessage = string.Empty;

                _logger.Information("Attempting to save schedule");

                if (!ValidateInput())
                {
                    _logger.Warning("Schedule validation failed");
                    HasValidationErrors = true;
                    return;
                }

                HasValidationErrors = false;
                var schedule = CreateScheduleFromInput();

                if (IsEditMode)
                {
                    schedule.ScheduleId = _originalSchedule!.ScheduleId;
                    schedule.CreatedDate = _originalSchedule.CreatedDate;
                    schedule.UpdatedDate = DateTime.UtcNow;

                    _logger.Information("Updating existing schedule {ScheduleId}", schedule.ScheduleId);
                    await _scheduleService.UpdateScheduleAsync(schedule);
                    _logger.Information("Successfully updated schedule {ScheduleId}", schedule.ScheduleId);
                }
                else
                {
                    schedule.CreatedDate = DateTime.UtcNow;
                    _logger.Information("Creating new schedule");
                    await _scheduleService.AddScheduleAsync(schedule);
                    _logger.Information("Successfully created new schedule");
                }

                // Close dialog with success
                if (Application.Current.Windows.OfType<AddEditScheduleDialog>().FirstOrDefault() is AddEditScheduleDialog dialog)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save schedule");
                ErrorMessage = "Failed to save schedule. Please try again.";
                MessageBox.Show("Failed to save schedule. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            var validationErrors = new List<string>();

            if (SelectedRoute == null)
            {
                validationErrors.Add("Please select a route");
            }

            if (SelectedBus == null)
            {
                validationErrors.Add("Please select a bus");
            }

            if (SelectedDriver == null)
            {
                validationErrors.Add("Please select a driver");
            }

            if (DepartureTime >= ArrivalTime)
            {
                validationErrors.Add("Departure time must be before arrival time");
            }

            if (ScheduleDate < DateTime.Today)
            {
                validationErrors.Add("Schedule date cannot be in the past");
            }

            if (validationErrors.Any())
            {
                ErrorMessage = string.Join(Environment.NewLine, validationErrors);
                _logger.Warning("Validation errors: {ValidationErrors}", string.Join(", ", validationErrors));
                return false;
            }

            return true;
        }

        private void Cancel()
        {
            _logger.Information("Schedule dialog cancelled");

            if (Application.Current.Windows.OfType<AddEditScheduleDialog>().FirstOrDefault() is AddEditScheduleDialog dialog)
            {
                dialog.DialogResult = false;
                dialog.Close();
            }
        }

        private BusBuddy.Core.Models.Schedule CreateScheduleFromInput()
        {
            // Combine date with time
            var departureDateTime = ScheduleDate.Date.Add(DepartureTime.TimeOfDay);
            var arrivalDateTime = ScheduleDate.Date.Add(ArrivalTime.TimeOfDay);

            var schedule = new BusBuddy.Core.Models.Schedule
            {
                RouteId = SelectedRoute!.RouteId,
                BusId = SelectedBus!.VehicleId,
                DriverId = SelectedDriver!.DriverId,
                ScheduleDate = ScheduleDate,
                DepartureTime = departureDateTime,
                ArrivalTime = arrivalDateTime,
                Status = SelectedStatus,
                Notes = Notes
            };

            _logger.Debug("Created schedule from input: Route={RouteId}, Bus={BusId}, Driver={DriverId}, Date={ScheduleDate}",
                schedule.RouteId, schedule.BusId, schedule.DriverId, schedule.ScheduleDate);

            return schedule;
        }
    }
}

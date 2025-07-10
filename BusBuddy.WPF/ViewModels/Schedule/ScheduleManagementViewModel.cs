using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BusBuddy.WPF.ViewModels
{
    public partial class ScheduleManagementViewModel : BaseInDevelopmentViewModel
    {
        private readonly IScheduleService _scheduleService;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;

        [ObservableProperty]
        private ObservableCollection<Schedule> _schedules;

        [ObservableProperty]
        private ObservableCollection<Bus> _buses;

        [ObservableProperty]
        private ObservableCollection<Driver> _drivers;

        [ObservableProperty]
        private Schedule _selectedSchedule;

        public IAsyncRelayCommand LoadSchedulesCommand { get; }
        public IAsyncRelayCommand AddScheduleCommand { get; }
        public IAsyncRelayCommand UpdateScheduleCommand { get; }
        public IAsyncRelayCommand DeleteScheduleCommand { get; }

        public ScheduleManagementViewModel(
            IScheduleService scheduleService,
            IBusService busService,
            IDriverService driverService,
            ILogger<ScheduleManagementViewModel>? logger = null)
            : base(logger)
        {
            _scheduleService = scheduleService;
            _busService = busService;
            _driverService = driverService;

            Schedules = new ObservableCollection<Schedule>();
            Buses = new ObservableCollection<Bus>();
            Drivers = new ObservableCollection<Driver>();
            SelectedSchedule = new Schedule();

            LoadSchedulesCommand = new AsyncRelayCommand(LoadDataAsync);
            AddScheduleCommand = new AsyncRelayCommand(AddScheduleAsync);
            UpdateScheduleCommand = new AsyncRelayCommand(UpdateScheduleAsync, CanUpdateOrDelete);
            DeleteScheduleCommand = new AsyncRelayCommand(DeleteScheduleAsync, CanUpdateOrDelete);

            // Set as in-development
            IsInDevelopment = true;

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await LoadSchedulesAsync();
                await LoadBusesAsync();
                await LoadDriversAsync();

                Logger?.LogInformation("Loaded schedules, buses, and drivers data");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading schedule data");
            }
        }

        private async Task LoadSchedulesAsync()
        {
            try
            {
                var schedules = await _scheduleService.GetSchedulesAsync();
                Schedules.Clear();
                foreach (var s in schedules)
                {
                    Schedules.Add(s);
                }
                Logger?.LogInformation("Loaded {ScheduleCount} schedules", Schedules.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading schedules");
            }
        }

        private async Task LoadBusesAsync()
        {
            try
            {
                var buses = await _busService.GetAllBusEntitiesAsync();
                Buses.Clear();
                foreach (var bus in buses)
                {
                    Buses.Add(bus);
                }
                Logger?.LogInformation("Loaded {BusCount} buses", Buses.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading buses");
            }
        }

        private async Task LoadDriversAsync()
        {
            try
            {
                var drivers = await _driverService.GetAllDriversAsync();
                Drivers.Clear();
                foreach (var driver in drivers)
                {
                    Drivers.Add(driver);
                }
                Logger?.LogInformation("Loaded {DriverCount} drivers", Drivers.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading drivers");
            }
        }

        private async Task AddScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                try
                {
                    // Set default values for a new schedule
                    SelectedSchedule.ScheduleDate = DateTime.Now;
                    await _scheduleService.AddScheduleAsync(SelectedSchedule);
                    await LoadSchedulesAsync();
                    Logger?.LogInformation("Added new schedule with ID {ScheduleId}", SelectedSchedule.ScheduleId);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error adding schedule");
                }
            }
        }

        private async Task UpdateScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                try
                {
                    await _scheduleService.UpdateScheduleAsync(SelectedSchedule);
                    await LoadSchedulesAsync();
                    Logger?.LogInformation("Updated schedule with ID {ScheduleId}", SelectedSchedule.ScheduleId);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error updating schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
            }
        }

        private async Task DeleteScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                try
                {
                    await _scheduleService.DeleteScheduleAsync(SelectedSchedule.ScheduleId);
                    await LoadSchedulesAsync();
                    Logger?.LogInformation("Deleted schedule with ID {ScheduleId}", SelectedSchedule.ScheduleId);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error deleting schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
            }
        }

        private bool CanUpdateOrDelete()
        {
            return SelectedSchedule != null && SelectedSchedule.ScheduleId != 0;
        }

        partial void OnSelectedScheduleChanged(Schedule value)
        {
            (UpdateScheduleCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (DeleteScheduleCommand as IRelayCommand)?.NotifyCanExecuteChanged();

            if (value != null)
            {
                Logger?.LogDebug("Selected schedule changed to ID {ScheduleId}", value.ScheduleId);
            }
        }
    }
}

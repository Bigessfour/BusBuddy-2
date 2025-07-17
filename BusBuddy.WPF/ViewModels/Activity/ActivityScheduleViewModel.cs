using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Data.Repositories;
using BusBuddy.Core.Data.Interfaces;
using BusBuddy.Core.Data.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BusBuddy.WPF.ViewModels.Activity
{
    public partial class ActivityScheduleViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? validationError;

        private static readonly ILogger Logger = Log.ForContext<ActivityScheduleViewModel>();
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActivityScheduleRepository _activityScheduleRepo;

        [ObservableProperty]
        private ObservableCollection<ActivitySchedule> activitySchedules = new();

        [ObservableProperty]
        private ActivitySchedule? selectedSchedule;

        [ObservableProperty]
        private ObservableCollection<Bus> availableBuses = new();

        [ObservableProperty]
        private ObservableCollection<Driver> availableDrivers = new();

        public ActivityScheduleViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _activityScheduleRepo = unitOfWork.ActivitySchedules;

            // Fire and forget async loading in constructor
            _ = Task.Run(async () =>
            {
                await LoadSchedulesAsync();
                await LoadAvailableBusesAsync();
                await LoadAvailableDriversAsync();
            });
        }

        public bool ValidateSchedule(ActivitySchedule schedule)
        {
            if (schedule == null)
            {
                ValidationError = "Schedule cannot be null.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(schedule.TripType))
            {
                ValidationError = "Trip type is required.";
                return false;
            }
            if (schedule.ScheduledDate == default)
            {
                ValidationError = "Scheduled date is required.";
                return false;
            }
            if (schedule.ScheduledVehicleId <= 0)
            {
                ValidationError = "Bus selection is required.";
                return false;
            }
            if (schedule.ScheduledDriverId <= 0)
            {
                ValidationError = "Driver selection is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(schedule.ScheduledDestination))
            {
                ValidationError = "Destination is required.";
                return false;
            }
            if (schedule.ScheduledLeaveTime == default)
            {
                ValidationError = "Leave time is required.";
                return false;
            }
            if (schedule.ScheduledEventTime == default)
            {
                ValidationError = "Event time is required.";
                return false;
            }
            ValidationError = null;
            return true;
        }

        [RelayCommand]
        public void OpenScheduleDialog(bool isEdit)
        {
            // If adding, create a new ActivitySchedule instance
            if (!isEdit)
            {
                SelectedSchedule = new ActivitySchedule();
            }
            // If editing, SelectedSchedule is already set
            // Show dialog (actual dialog invocation is handled in the View)
            Logger.Information("Opening ActivityScheduleDialog for {Mode}", isEdit ? "Edit" : "Add");
        }

        [RelayCommand]
        public async Task LoadAvailableBusesAsync()
        {
            try
            {
                var buses = await _unitOfWork.Buses.GetActiveVehiclesAsync();
                AvailableBuses = new ObservableCollection<Bus>(buses);
                Logger.Information("Loaded {Count} buses for selection", AvailableBuses.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load available buses");
            }
        }

        [RelayCommand]
        public async Task LoadAvailableDriversAsync()
        {
            try
            {
                var drivers = await _unitOfWork.Drivers.GetActiveDriversAsync();
                AvailableDrivers = new ObservableCollection<Driver>(drivers);
                Logger.Information("Loaded {Count} drivers for selection", AvailableDrivers.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load available drivers");
            }
        }

        [RelayCommand]
        public async Task LoadSchedulesAsync()
        {
            try
            {
                var list = await _activityScheduleRepo.GetAllAsync();
                ActivitySchedules = new ObservableCollection<ActivitySchedule>(list);
                Logger.Information("Loaded {Count} activity schedules", ActivitySchedules.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load activity schedules");
            }
        }

        [RelayCommand]
        public async Task AddScheduleAsync(ActivitySchedule newSchedule)
        {
            if (!ValidateSchedule(newSchedule))
            {
                Logger.Warning("Validation failed: {Error}", ValidationError);
                return;
            }
            try
            {
                await _activityScheduleRepo.AddAsync(newSchedule);
                await _unitOfWork.SaveChangesAsync();
                ActivitySchedules.Add(newSchedule);
                Logger.Information("Added new activity schedule {Id}", newSchedule.ActivityScheduleId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to add activity schedule");
            }
        }

        [RelayCommand]
        public async Task UpdateScheduleAsync(ActivitySchedule schedule)
        {
            if (!ValidateSchedule(schedule))
            {
                Logger.Warning("Validation failed: {Error}", ValidationError);
                return;
            }
            try
            {
                _activityScheduleRepo.Update(schedule);
                await _unitOfWork.SaveChangesAsync();
                Logger.Information("Updated activity schedule {Id}", schedule.ActivityScheduleId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update activity schedule");
            }
        }

        [RelayCommand]
        public async Task DeleteScheduleAsync(ActivitySchedule schedule)
        {
            try
            {
                await _activityScheduleRepo.RemoveByIdAsync(schedule.ActivityScheduleId);
                await _unitOfWork.SaveChangesAsync();
                ActivitySchedules.Remove(schedule);
                Logger.Information("Deleted activity schedule {Id}", schedule.ActivityScheduleId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to delete activity schedule");
            }
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels
{
    public partial class ScheduleManagementViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;

        [ObservableProperty]
        private ObservableCollection<Activity> _schedules;

        [ObservableProperty]
        private ObservableCollection<Bus> _buses;

        [ObservableProperty]
        private ObservableCollection<Driver> _drivers;

        [ObservableProperty]
        private Activity _selectedSchedule;

        public IAsyncRelayCommand LoadSchedulesCommand { get; }
        public IAsyncRelayCommand AddScheduleCommand { get; }
        public IAsyncRelayCommand UpdateScheduleCommand { get; }
        public IAsyncRelayCommand DeleteScheduleCommand { get; }

        public ScheduleManagementViewModel(IScheduleService scheduleService, IBusService busService, IDriverService driverService)
        {
            _scheduleService = scheduleService;
            _busService = busService;
            _driverService = driverService;

            Schedules = new ObservableCollection<Activity>();
            Buses = new ObservableCollection<Bus>();
            Drivers = new ObservableCollection<Driver>();
            SelectedSchedule = new Activity();

            LoadSchedulesCommand = new AsyncRelayCommand(LoadDataAsync);
            AddScheduleCommand = new AsyncRelayCommand(AddScheduleAsync);
            UpdateScheduleCommand = new AsyncRelayCommand(UpdateScheduleAsync, CanUpdateOrDelete);
            DeleteScheduleCommand = new AsyncRelayCommand(DeleteScheduleAsync, CanUpdateOrDelete);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadSchedulesAsync();
            await LoadBusesAsync();
            await LoadDriversAsync();
        }

        private async Task LoadSchedulesAsync()
        {
            var schedules = await _scheduleService.GetAllSchedulesAsync();
            Schedules.Clear();
            foreach (var s in schedules)
            {
                Schedules.Add(s);
            }
        }

        private async Task LoadBusesAsync()
        {
            var buses = await _busService.GetAllBusEntitiesAsync();
            Buses.Clear();
            foreach (var bus in buses)
            {
                Buses.Add(bus);
            }
        }

        private async Task LoadDriversAsync()
        {
            var drivers = await _driverService.GetAllDriversAsync();
            Drivers.Clear();
            foreach (var driver in drivers)
            {
                Drivers.Add(driver);
            }
        }

        private async Task AddScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                // Set default values for a new schedule
                SelectedSchedule.Date = DateTime.Now;
                SelectedSchedule.ActivityType = "Scheduled Route";
                await _scheduleService.AddScheduleAsync(SelectedSchedule);
                await LoadSchedulesAsync();
            }
        }

        private async Task UpdateScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                await _scheduleService.UpdateScheduleAsync(SelectedSchedule);
                await LoadSchedulesAsync();
            }
        }

        private async Task DeleteScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                await _scheduleService.DeleteScheduleAsync(SelectedSchedule.ActivityId);
                await LoadSchedulesAsync();
            }
        }

        private bool CanUpdateOrDelete()
        {
            return SelectedSchedule != null && SelectedSchedule.ActivityId != 0;
        }

        partial void OnSelectedScheduleChanged(Activity value)
        {
            (UpdateScheduleCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (DeleteScheduleCommand as IRelayCommand)?.NotifyCanExecuteChanged();
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using System.Windows.Input;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.ViewModels
{
    public class Schedule : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string? Route { get; set; }
        public string? BusNumber { get; set; }
        public string? DriverName { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
// ...existing Schedule class...

public interface IScheduleService
{
    ObservableCollection<BusBuddy.WPF.ViewModels.Schedule> GetSchedules();
    ObservableCollection<string> GetAvailableBuses();
    ObservableCollection<string> GetAvailableDrivers();
    void AddSchedule(BusBuddy.WPF.ViewModels.Schedule schedule);
    void UpdateSchedule(BusBuddy.WPF.ViewModels.Schedule schedule);
    void DeleteSchedule(BusBuddy.WPF.ViewModels.Schedule schedule);
    Task AutoGenerateSchedulesAsync();
}

public class ScheduleService : IScheduleService
{
    private ObservableCollection<BusBuddy.WPF.ViewModels.Schedule> _schedules = new();
    private ObservableCollection<string> _buses;
    private ObservableCollection<string> _drivers;
    private ObservableCollection<string> _routes;

    // Use the real EF DbContext
    private readonly BusBuddyDbContext _dbContext;
    private readonly BusBuddy.Core.Services.IActivityLogService? _logService;

    public ScheduleService(BusBuddyDbContext dbContext, BusBuddy.Core.Services.IActivityLogService? logService = null)
    {
        _dbContext = dbContext;
        _logService = logService;

        // Populate buses, drivers, and routes from the database
        _buses = new ObservableCollection<string>(_dbContext.Vehicles.Select(b => b.BusNumber).ToList());
        _drivers = new ObservableCollection<string>(_dbContext.Drivers.Select(d => d.DriverName).ToList());
        _routes = new ObservableCollection<string>(_dbContext.Routes.Select(r => r.RouteName).ToList());
    }

    public ObservableCollection<BusBuddy.WPF.ViewModels.Schedule> GetSchedules() => _schedules;
    public ObservableCollection<string> GetAvailableBuses() => _buses;
    public ObservableCollection<string> GetAvailableDrivers() => _drivers;
    public void AddSchedule(BusBuddy.WPF.ViewModels.Schedule schedule) => _schedules.Add(schedule);
    public void UpdateSchedule(BusBuddy.WPF.ViewModels.Schedule schedule)
    {
        var existing = _schedules.FirstOrDefault(s => s.Id == schedule.Id);
        if (existing != null)
        {
            existing.Route = schedule.Route;
            existing.BusNumber = schedule.BusNumber;
            existing.DriverName = schedule.DriverName;
            existing.DepartureTime = schedule.DepartureTime;
            existing.ArrivalTime = schedule.ArrivalTime;
        }
    }
    public void DeleteSchedule(BusBuddy.WPF.ViewModels.Schedule schedule) => _schedules.Remove(schedule);

    // Auto-generate schedules by matching available routes and drivers
    public async Task AutoGenerateSchedulesAsync()
    {
        _schedules.Clear();
        int id = 1;
        var now = DateTime.Now.Date.AddHours(7);
        int count = Math.Min(_routes.Count, Math.Min(_buses.Count, _drivers.Count));
        for (int i = 0; i < count; i++)
        {
            var schedule = new BusBuddy.WPF.ViewModels.Schedule
            {
                Id = id++,
                Route = _routes[i],
                BusNumber = _buses[i],
                DriverName = _drivers[i],
                DepartureTime = now.AddMinutes(i * 15),
                ArrivalTime = now.AddMinutes(i * 15 + 45)
            };
            _schedules.Add(schedule);
            await SaveScheduleToDatabaseAsync(schedule);
        }
    }

    // Implements actual EF/database save logic
    private async Task SaveScheduleToDatabaseAsync(BusBuddy.WPF.ViewModels.Schedule schedule)
    {
        try
        {
            // Map WPF Schedule to EF Activity entity
            var entity = new Activity
            {
                Date = schedule.DepartureTime.Date,
                ActivityType = "Route",
                Destination = schedule.Route ?? string.Empty,
                LeaveTime = schedule.DepartureTime.TimeOfDay,
                EventTime = schedule.ArrivalTime.TimeOfDay,
                AssignedVehicleId = GetBusIdByNumber(schedule.BusNumber),
                DriverId = GetDriverIdByName(schedule.DriverName),
                Status = "Scheduled"
            };

            _dbContext.Activities.Add(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            if (_logService != null)
            {
                await _logService.LogAsync("Schedule Save Failed", "System", ex.ToString());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleService] Error saving schedule: {ex}");
            }
        }
    }

    // Helper methods for lookup (implement as needed)
    private int GetBusIdByNumber(string? busNumber)
    {
        var bus = _dbContext.Vehicles.FirstOrDefault(b => b.BusNumber == busNumber);
        return bus?.VehicleId ?? 0;
    }

    private int GetDriverIdByName(string? driverName)
    {
        var driver = _dbContext.Drivers.FirstOrDefault(d => d.DriverName == driverName);
        return driver?.DriverId ?? 0;
    }
}

public class ScheduleManagementViewModel : INotifyPropertyChanged
{
    private readonly IScheduleService _service;
    public ObservableCollection<BusBuddy.WPF.ViewModels.Schedule> Schedules { get; }
    public ObservableCollection<string> AvailableBuses { get; }
    public ObservableCollection<string> AvailableDrivers { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand AutoGenerateCommand { get; }
    private BusBuddy.WPF.ViewModels.Schedule? _selectedSchedule;
    public BusBuddy.WPF.ViewModels.Schedule? SelectedSchedule
    {
        get => _selectedSchedule;
        set { _selectedSchedule = value; OnPropertyChanged(); }
    }
    public ScheduleManagementViewModel(IScheduleService service)
    {
        _service = service;
        Schedules = _service.GetSchedules();
        AvailableBuses = _service.GetAvailableBuses();
        AvailableDrivers = _service.GetAvailableDrivers();
        AddCommand = new RelayCommand(AddSchedule);
        EditCommand = new RelayCommand(EditSchedule);
        DeleteCommand = new RelayCommand(DeleteSchedule);
        AutoGenerateCommand = new RelayCommand(async () => await AutoGenerateSchedulesAsync());
    }

    private async Task AutoGenerateSchedulesAsync()
    {
        await _service.AutoGenerateSchedulesAsync();
    }
    private void AddSchedule()
    {
        var newSchedule = new BusBuddy.WPF.ViewModels.Schedule
        {
            Id = Schedules.Count + 1,
            Route = string.Empty,
            BusNumber = AvailableBuses.FirstOrDefault() ?? string.Empty,
            DriverName = AvailableDrivers.FirstOrDefault() ?? string.Empty,
            DepartureTime = DateTime.Now,
            ArrivalTime = DateTime.Now.AddHours(1)
        };
        _service.AddSchedule(newSchedule);
    }
    private void EditSchedule()
    {
        if (SelectedSchedule != null)
            _service.UpdateSchedule(SelectedSchedule);
    }
    private void DeleteSchedule()
    {
        if (SelectedSchedule != null)
            _service.DeleteSchedule(SelectedSchedule);
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

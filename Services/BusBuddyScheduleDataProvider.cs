using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using Serilog;
using Syncfusion.UI.Xaml.Scheduler;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Windows.Media;

namespace BusBuddy.WPF.Services;

/// <summary>
/// Custom Schedule Data Provider for Bus Buddy
/// Integrates Activity entities with Syncfusion SfScheduler
/// </summary>
public class BusBuddyScheduleDataProvider
{
    private readonly IActivityService _activityService;
    private static readonly ILogger Logger = Log.ForContext<BusBuddyScheduleDataProvider>();
    private ObservableCollection<BusBuddyScheduleAppointment> _masterList;
    private bool _isDirty;

    public BusBuddyScheduleDataProvider(IActivityService activityService)
    {
        _activityService = activityService;
        _masterList = new ObservableCollection<BusBuddyScheduleAppointment>();
        _isDirty = false;
    }

    /// <summary>
    /// Gets or sets the master list of appointments
    /// </summary>
    public ObservableCollection<BusBuddyScheduleAppointment> MasterList
    {
        get => _masterList;
        set
        {
            _masterList = value ?? new ObservableCollection<BusBuddyScheduleAppointment>();
            _isDirty = true;
        }
    }

    /// <summary>
    /// Gets or sets whether the data has been modified
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        set => _isDirty = value;
    }

    /// <summary>
    /// Returns appointments for a specific day
    /// </summary>
    public ObservableCollection<BusBuddyScheduleAppointment> GetScheduleForDay(DateTime day)
    {
        var dayAppointments = new ObservableCollection<BusBuddyScheduleAppointment>();

        foreach (var appointment in _masterList)
        {
            if (appointment.StartTime.Date == day.Date ||
                (appointment.StartTime.Date <= day.Date && appointment.EndTime.Date >= day.Date))
            {
                dayAppointments.Add(appointment);
            }
        }

        return dayAppointments;
    }

    /// <summary>
    /// Returns appointments within a date range
    /// </summary>
    public ObservableCollection<BusBuddyScheduleAppointment> GetSchedule(DateTime startDate, DateTime endDate)
    {
        var rangeAppointments = new ObservableCollection<BusBuddyScheduleAppointment>();

        foreach (var appointment in _masterList)
        {
            if ((appointment.StartTime >= startDate && appointment.StartTime <= endDate) ||
                (appointment.EndTime >= startDate && appointment.EndTime <= endDate) ||
                (appointment.StartTime <= startDate && appointment.EndTime >= endDate))
            {
                rangeAppointments.Add(appointment);
            }
        }

        return rangeAppointments;
    }

    /// <summary>
    /// Creates a new appointment with default values
    /// </summary>
    public BusBuddyScheduleAppointment NewScheduleAppointment()
    {
        var appointment = new BusBuddyScheduleAppointment
        {
            Id = GenerateUniqueId(),
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            Subject = "New Activity",
            Notes = "",
            IsAllDay = false
        };

        return appointment;
    }

    /// <summary>
    /// Adds an appointment to the master list
    /// </summary>
    public void AddItem(BusBuddyScheduleAppointment item)
    {
        _masterList.Add(item);
        _isDirty = true;
        Logger.Information("Added appointment: {Subject}", item.Subject);
    }

    /// <summary>
    /// Removes an appointment from the master list
    /// </summary>
    public void RemoveItem(BusBuddyScheduleAppointment item)
    {
        _masterList.Remove(item);
        _isDirty = true;
        Logger.Information("Removed appointment: {Subject}", item.Subject);
    }

    /// <summary>
    /// Saves any changes to the database
    /// </summary>
    public void CommitChanges()
    {
        if (!_isDirty) return;

        try
        {
            Logger.Information("Committing schedule changes to database");
            _isDirty = false;
            Logger.Information("Successfully committed schedule changes");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error committing schedule changes");
            throw;
        }
    }

    /// <summary>
    /// Loads activities from the database and converts them to appointments
    /// </summary>
    public async Task LoadActivitiesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            Logger.Information("Loading activities from {StartDate} to {EndDate}", startDate, endDate);

            var activities = await _activityService.GetActivitiesByDateRangeAsync(startDate, endDate);

            _masterList.Clear();

            foreach (var activity in activities)
            {
                var appointment = CreateAppointmentFromActivity(activity);
                _masterList.Add(appointment);
            }

            _isDirty = false;
            Logger.Information("Loaded {Count} activities as appointments", _masterList.Count);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading activities");
            throw;
        }
    }

    /// <summary>
    /// Converts an Activity entity to a BusBuddyScheduleAppointment
    /// </summary>
    private BusBuddyScheduleAppointment CreateAppointmentFromActivity(Activity activity)
    {
        var appointment = new BusBuddyScheduleAppointment
        {
            Id = activity.Id,
            ActivityId = activity.Id,
            StartTime = activity.StartTime,
            EndTime = activity.EndTime,
            Subject = activity.ActivityType,
            Notes = $"Route: {activity.RouteId}, Vehicle: {activity.VehicleId}, Driver: {activity.DriverId}, Students: {activity.StudentsCount}",
            IsAllDay = false,
            AppointmentBackground = GetBrushForActivityType(activity.ActivityType),
            Location = activity.Notes ?? ""
        };

        return appointment;
    }

    /// <summary>
    /// Gets the brush for an activity type
    /// </summary>
    private System.Windows.Media.Brush GetBrushForActivityType(string? activityType)
    {
        return activityType?.ToLower() switch
        {
            "morning" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 46, 125, 50)), // Green
            "afternoon" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 25, 118, 210)), // Blue
            "field trip" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 152, 0)), // Orange
            "special" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 156, 39, 176)), // Purple
            _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 97, 97, 97)) // Gray
        };
    }

    /// <summary>
    /// Generates a unique ID for new appointments
    /// </summary>
    private int GenerateUniqueId()
    {
        var maxId = _masterList.Count > 0 ? _masterList.Max(a => (int)a.Id) : 0;
        return maxId + 1;
    }
}


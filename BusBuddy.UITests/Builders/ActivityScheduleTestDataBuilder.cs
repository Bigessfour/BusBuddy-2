using BusBuddy.Core.Models;

namespace BusBuddy.UITests.Builders;

/// <summary>
/// Test data builder for ActivitySchedule objects
/// Provides fluent API for creating test activity schedules with realistic data
/// </summary>
public class ActivityScheduleTestDataBuilder
{
    private readonly ActivitySchedule _activitySchedule;

    public ActivityScheduleTestDataBuilder()
    {
        _activitySchedule = new ActivitySchedule
        {
            ActivityScheduleId = 1,
            ScheduledDate = DateTime.Today,
            TripType = "Field Trip",
            ScheduledVehicleId = 1,
            ScheduledDestination = "Science Museum",
            ScheduledLeaveTime = new TimeSpan(9, 0, 0), // 9:00 AM
            ScheduledEventTime = new TimeSpan(10, 0, 0), // 10:00 AM
            ScheduledRiders = 45
        };
    }

    public ActivityScheduleTestDataBuilder WithId(int id)
    {
        _activitySchedule.ActivityScheduleId = id;
        return this;
    }

    public ActivityScheduleTestDataBuilder WithDate(DateTime date)
    {
        _activitySchedule.ScheduledDate = date;
        return this;
    }

    public ActivityScheduleTestDataBuilder WithTripType(string tripType)
    {
        _activitySchedule.TripType = tripType;
        return this;
    }

    public ActivityScheduleTestDataBuilder WithVehicleId(int vehicleId)
    {
        _activitySchedule.ScheduledVehicleId = vehicleId;
        return this;
    }

    public ActivityScheduleTestDataBuilder WithDestination(string destination)
    {
        _activitySchedule.ScheduledDestination = destination;
        return this;
    }

    public ActivityScheduleTestDataBuilder WithLeaveTime(TimeSpan leaveTime)
    {
        _activitySchedule.ScheduledLeaveTime = leaveTime;
        return this;
    }

    public ActivityScheduleTestDataBuilder WithEventTime(TimeSpan eventTime)
    {
        _activitySchedule.ScheduledEventTime = eventTime;
        return this;
    }

    public ActivityScheduleTestDataBuilder WithRiders(int riders)
    {
        _activitySchedule.ScheduledRiders = riders;
        return this;
    }

    public ActivityScheduleTestDataBuilder AsFieldTrip()
    {
        _activitySchedule.TripType = "Field Trip";
        _activitySchedule.ScheduledDestination = "Science Museum";
        _activitySchedule.ScheduledRiders = 45;
        return this;
    }

    public ActivityScheduleTestDataBuilder AsSportsEvent()
    {
        _activitySchedule.TripType = "Sports Event";
        _activitySchedule.ScheduledDestination = "Regional Stadium";
        _activitySchedule.ScheduledRiders = 30;
        return this;
    }

    public ActivityScheduleTestDataBuilder AsAcademicCompetition()
    {
        _activitySchedule.TripType = "Academic Competition";
        _activitySchedule.ScheduledDestination = "University Campus";
        _activitySchedule.ScheduledRiders = 25;
        return this;
    }

    public ActivityScheduleTestDataBuilder ForToday()
    {
        _activitySchedule.ScheduledDate = DateTime.Today;
        return this;
    }

    public ActivityScheduleTestDataBuilder ForTomorrow()
    {
        _activitySchedule.ScheduledDate = DateTime.Today.AddDays(1);
        return this;
    }

    public ActivityScheduleTestDataBuilder ForNextWeek()
    {
        _activitySchedule.ScheduledDate = DateTime.Today.AddDays(7);
        return this;
    }

    public ActivitySchedule Build() => _activitySchedule;

    // Static factory methods for common scenarios
    public static ActivitySchedule CreateTodayFieldTrip(int id = 1)
    {
        return new ActivityScheduleTestDataBuilder()
            .WithId(id)
            .ForToday()
            .AsFieldTrip()
            .WithLeaveTime(new TimeSpan(8, 30, 0))
            .WithEventTime(new TimeSpan(9, 30, 0))
            .Build();
    }

    public static ActivitySchedule CreateTomorrowSportsEvent(int id = 2)
    {
        return new ActivityScheduleTestDataBuilder()
            .WithId(id)
            .ForTomorrow()
            .AsSportsEvent()
            .WithLeaveTime(new TimeSpan(14, 0, 0)) // 2:00 PM
            .WithEventTime(new TimeSpan(15, 30, 0)) // 3:30 PM
            .Build();
    }

    public static ActivitySchedule CreateAcademicCompetition(int id = 3)
    {
        return new ActivityScheduleTestDataBuilder()
            .WithId(id)
            .ForNextWeek()
            .AsAcademicCompetition()
            .WithLeaveTime(new TimeSpan(7, 0, 0)) // 7:00 AM
            .WithEventTime(new TimeSpan(9, 0, 0)) // 9:00 AM
            .Build();
    }

    public static List<ActivitySchedule> CreateWeeklySchedule(int count = 7)
    {
        var activities = new List<ActivitySchedule>();
        var tripTypes = new[] { "Field Trip", "Sports Event", "Academic Competition", "Special Event", "Regular Route" };
        var destinations = new[] { "Science Museum", "Regional Stadium", "University Campus", "Community Center", "Library" };

        for (int i = 1; i <= count; i++)
        {
            var typeIndex = (i - 1) % tripTypes.Length;
            activities.Add(new ActivityScheduleTestDataBuilder()
                .WithId(i)
                .WithDate(DateTime.Today.AddDays(i - 1))
                .WithTripType(tripTypes[typeIndex])
                .WithDestination(destinations[typeIndex])
                .WithVehicleId((i % 5) + 1)
                .WithLeaveTime(new TimeSpan(8 + (i % 3), 0, 0))
                .WithEventTime(new TimeSpan(9 + (i % 3), 30, 0))
                .WithRiders(20 + (i * 5))
                .Build());
        }
        return activities;
    }
}

using BusBuddy.Core.Models;

namespace BusBuddy.UITests.Builders;

/// <summary>
/// Test data builder for Activity entities
/// Creates Activity objects for testing dashboard components and activity management
/// </summary>
public class ActivityTestDataBuilder
{
    private Activity _activity;

    public ActivityTestDataBuilder()
    {
        _activity = new Activity
        {
            ActivityId = 1,
            Date = DateTime.Today,
            ActivityType = "Field Trip",
            Destination = "Science Museum",
            LeaveTime = TimeSpan.Parse("08:00"),
            EventTime = TimeSpan.Parse("09:30"),
            RequestedBy = "Principal Johnson",
            AssignedVehicleId = 1,
            DriverId = 1,
            StudentsCount = 25,
            Status = "Scheduled",
            ReturnTime = TimeSpan.Parse("15:00"),
            ExpectedPassengers = 25,
            Notes = "Bring permission slips"
        };
    }

    public ActivityTestDataBuilder WithId(int id)
    {
        _activity.ActivityId = id;
        return this;
    }

    public ActivityTestDataBuilder WithDate(DateTime date)
    {
        _activity.Date = date;
        return this;
    }

    public ActivityTestDataBuilder WithActivityType(string activityType)
    {
        _activity.ActivityType = activityType;
        return this;
    }

    public ActivityTestDataBuilder WithVehicleId(int vehicleId)
    {
        _activity.AssignedVehicleId = vehicleId;
        return this;
    }

    public ActivityTestDataBuilder WithDriverId(int driverId)
    {
        _activity.DriverId = driverId;
        return this;
    }

    public ActivityTestDataBuilder WithDestination(string destination)
    {
        _activity.Destination = destination;
        return this;
    }

    public ActivityTestDataBuilder WithLeaveTime(TimeSpan leaveTime)
    {
        _activity.LeaveTime = leaveTime;
        return this;
    }

    public ActivityTestDataBuilder WithEventTime(TimeSpan eventTime)
    {
        _activity.EventTime = eventTime;
        return this;
    }

    public ActivityTestDataBuilder WithReturnTime(TimeSpan returnTime)
    {
        _activity.ReturnTime = returnTime;
        return this;
    }

    public ActivityTestDataBuilder WithStudentsCount(int studentsCount)
    {
        _activity.StudentsCount = studentsCount;
        _activity.ExpectedPassengers = studentsCount;
        return this;
    }

    public ActivityTestDataBuilder WithRequestedBy(string requestedBy)
    {
        _activity.RequestedBy = requestedBy;
        return this;
    }

    public ActivityTestDataBuilder WithStatus(string status)
    {
        _activity.Status = status;
        return this;
    }

    public ActivityTestDataBuilder WithNotes(string notes)
    {
        _activity.Notes = notes;
        return this;
    }

    public ActivityTestDataBuilder AsFieldTrip()
    {
        _activity.ActivityType = "Field Trip";
        _activity.Destination = "Science Museum";
        _activity.LeaveTime = TimeSpan.Parse("08:00");
        _activity.EventTime = TimeSpan.Parse("09:30");
        _activity.ReturnTime = TimeSpan.Parse("15:00");
        return this;
    }

    public ActivityTestDataBuilder AsSportsEvent()
    {
        _activity.ActivityType = "Sports Event";
        _activity.Destination = "Regional Stadium";
        _activity.LeaveTime = TimeSpan.Parse("14:00");
        _activity.EventTime = TimeSpan.Parse("16:00");
        _activity.ReturnTime = TimeSpan.Parse("20:00");
        return this;
    }

    public ActivityTestDataBuilder AsAcademicCompetition()
    {
        _activity.ActivityType = "Academic Competition";
        _activity.Destination = "University Campus";
        _activity.LeaveTime = TimeSpan.Parse("07:30");
        _activity.EventTime = TimeSpan.Parse("09:00");
        _activity.ReturnTime = TimeSpan.Parse("16:30");
        return this;
    }

    public ActivityTestDataBuilder ForToday()
    {
        _activity.Date = DateTime.Today;
        return this;
    }

    public ActivityTestDataBuilder ForTomorrow()
    {
        _activity.Date = DateTime.Today.AddDays(1);
        return this;
    }

    public ActivityTestDataBuilder ForNextWeek()
    {
        _activity.Date = DateTime.Today.AddDays(7);
        return this;
    }

    public Activity Build()
    {
        return _activity;
    }

    /// <summary>
    /// Creates a list of test activities for dashboard testing
    /// </summary>
    public static List<Activity> CreateTestActivities(int count = 5)
    {
        var activities = new List<Activity>();
        var activityTypes = new[] { "Field Trip", "Sports Event", "Academic Competition", "Special Event", "Regular Route" };
        var destinations = new[] { "Science Museum", "Regional Stadium", "University Campus", "City Hall", "Downtown District" };
        var requestors = new[] { "Principal Johnson", "Coach Martinez", "Ms. Anderson", "Mr. Thompson", "Dr. Wilson" };

        for (int i = 0; i < count; i++)
        {
            var activity = new ActivityTestDataBuilder()
                .WithId(i + 1)
                .WithDate(DateTime.Today.AddDays(i))
                .WithActivityType(activityTypes[i % activityTypes.Length])
                .WithDestination(destinations[i % destinations.Length])
                .WithRequestedBy(requestors[i % requestors.Length])
                .WithVehicleId((i % 2) + 1) // Use only vehicle IDs 1-2 to match test expectations
                .WithDriverId((i % 2) + 1)  // Use only driver IDs 1-2 to match test expectations
                .WithStudentsCount(20 + (i * 5))
                .WithLeaveTime(TimeSpan.Parse($"{8 + i}:00"))
                .WithEventTime(TimeSpan.Parse($"{9 + i}:30"))
                .WithReturnTime(TimeSpan.Parse($"{15 + i}:00"))
                .Build();

            activities.Add(activity);
        }

        return activities;
    }

    /// <summary>
    /// Creates a field trip activity for today for testing
    /// </summary>
    public static Activity CreateTodayFieldTrip(int id = 77)
    {
        return new ActivityTestDataBuilder()
            .WithId(id)
            .ForToday()
            .AsFieldTrip()
            .WithStudentsCount(30)
            .WithRequestedBy("Ms. Davis")
            .WithVehicleId(2)
            .WithDriverId(2)
            .Build();
    }

    /// <summary>
    /// Creates activities for a week's schedule
    /// </summary>
    public static List<Activity> CreateWeeklySchedule(int activitiesPerDay = 2)
    {
        var activities = new List<Activity>();
        var activityTypes = new[] { "Field Trip", "Sports Event", "Academic Competition" };

        for (int day = 0; day < 7; day++)
        {
            for (int activity = 0; activity < activitiesPerDay; activity++)
            {
                var id = (day * activitiesPerDay) + activity + 1;
                var activityEntity = new ActivityTestDataBuilder()
                    .WithId(id)
                    .WithDate(DateTime.Today.AddDays(day))
                    .WithActivityType(activityTypes[activity % activityTypes.Length])
                    .WithVehicleId((id % 2) + 1) // Use only vehicle IDs 1-2
                    .WithDriverId((id % 2) + 1)  // Use only driver IDs 1-2
                    .WithStudentsCount(15 + (activity * 10))
                    .Build();

                activities.Add(activityEntity);
            }
        }

        return activities;
    }
}

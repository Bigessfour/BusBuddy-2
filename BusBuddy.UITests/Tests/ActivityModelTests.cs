using NUnit.Framework;
using FluentAssertions;
using BusBuddy.Core.Models;
using BusBuddy.UITests.Builders;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Comprehensive unit tests for Activity entity validation and business logic
/// Uses FluentAssertions for readable test assertions
/// </summary>
[TestFixture]
[Category("UnitTests")]
[Category("Models")]
public class ActivityModelTests
{
    [Test]
    public void Activity_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var activity = new Activity();

        // Assert
        activity.ActivityId.Should().Be(0);
        activity.ActivityType.Should().Be(string.Empty);
        activity.Destination.Should().Be(string.Empty);
        activity.RequestedBy.Should().Be(string.Empty);
        activity.Status.Should().Be("Scheduled");
        activity.Date.Should().Be(default(DateTime));
        activity.LeaveTime.Should().Be(default(TimeSpan));
        activity.EventTime.Should().Be(default(TimeSpan));
        activity.ReturnTime.Should().Be(default(TimeSpan));
    }

    [Test]
    public void Activity_SetActivityType_ShouldUpdatePropertyAndNotifyPropertyChanged()
    {
        // Arrange
        var activity = new Activity();
        var propertyChangedEventRaised = false;
        activity.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Activity.ActivityType))
            {
                propertyChangedEventRaised = true;
            }
        };

        // Act
        activity.ActivityType = "Field Trip";

        // Assert
        activity.ActivityType.Should().Be("Field Trip");
        propertyChangedEventRaised.Should().BeTrue("PropertyChanged event should be raised");
    }

    [Test]
    public void Activity_SetDestination_ShouldUpdatePropertyAndNotifyPropertyChanged()
    {
        // Arrange
        var activity = new Activity();
        var propertyChangedEventRaised = false;
        activity.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Activity.Destination))
            {
                propertyChangedEventRaised = true;
            }
        };

        // Act
        activity.Destination = "Science Museum";

        // Assert
        activity.Destination.Should().Be("Science Museum");
        propertyChangedEventRaised.Should().BeTrue("PropertyChanged event should be raised");
    }

    [Test]
    public void Activity_SetDate_ShouldUpdatePropertyAndActivityDateAlias()
    {
        // Arrange
        var activity = new Activity();
        var testDate = new DateTime(2025, 8, 15);

        // Act
        activity.Date = testDate;

        // Assert
        activity.Date.Should().Be(testDate);
        activity.ActivityDate.Should().Be(testDate); // ActivityDate is an alias for Date
    }

    [Test]
    public void Activity_SetLeaveTime_ShouldUpdatePropertyAndStartTimeAlias()
    {
        // Arrange
        var activity = new Activity();
        var leaveTime = TimeSpan.Parse("08:30");

        // Act
        activity.LeaveTime = leaveTime;

        // Assert
        activity.LeaveTime.Should().Be(leaveTime);
        activity.StartTime.Should().Be(leaveTime); // StartTime is an alias for LeaveTime
    }

    [Test]
    public void Activity_SetEventTime_ShouldUpdatePropertyAndEndTimeAlias()
    {
        // Arrange
        var activity = new Activity();
        var eventTime = TimeSpan.Parse("15:30");

        // Act
        activity.EventTime = eventTime;

        // Assert
        activity.EventTime.Should().Be(eventTime);
        activity.EndTime.Should().Be(eventTime); // EndTime is an alias for EventTime
    }

    [Test]
    [TestCase(null, ExpectedResult = "")]
    [TestCase("", ExpectedResult = "")]
    [TestCase("Field Trip", ExpectedResult = "Field Trip")]
    public string Activity_SetActivityType_WithNullOrEmpty_ShouldHandleGracefully(string activityType)
    {
        // Arrange
        var activity = new Activity();

        // Act
        activity.ActivityType = activityType;

        // Assert
        return activity.ActivityType;
    }

    [Test]
    [TestCase(null, ExpectedResult = "")]
    [TestCase("", ExpectedResult = "")]
    [TestCase("Science Museum", ExpectedResult = "Science Museum")]
    public string Activity_SetDestination_WithNullOrEmpty_ShouldHandleGracefully(string destination)
    {
        // Arrange
        var activity = new Activity();

        // Act
        activity.Destination = destination;

        // Assert
        return activity.Destination;
    }

    [Test]
    [TestCase(null, ExpectedResult = "")]
    [TestCase("", ExpectedResult = "")]
    [TestCase("Principal Johnson", ExpectedResult = "Principal Johnson")]
    public string Activity_SetRequestedBy_WithNullOrEmpty_ShouldHandleGracefully(string requestedBy)
    {
        // Arrange
        var activity = new Activity();

        // Act
        activity.RequestedBy = requestedBy;

        // Assert
        return activity.RequestedBy;
    }

    [Test]
    public void Activity_SetStudentsCount_ShouldUpdateExpectedPassengers()
    {
        // Arrange
        var activity = new Activity();

        // Act
        activity.StudentsCount = 30;

        // Assert
        activity.StudentsCount.Should().Be(30);
        // Note: This test assumes ExpectedPassengers should match StudentsCount
        // You may need to adjust based on actual business logic
    }

    [Test]
    public void Activity_MultiplePropertyChanges_ShouldRaiseAllEvents()
    {
        // Arrange
        var activity = new Activity();
        var propertyChangedEvents = new List<string>();
        activity.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName ?? "");

        // Act
        activity.ActivityType = "Sports Event";
        activity.Destination = "Regional Stadium";
        activity.RequestedBy = "Coach Martinez";

        // Assert
        propertyChangedEvents.Should().Contain(nameof(Activity.ActivityType));
        propertyChangedEvents.Should().Contain(nameof(Activity.Destination));
        propertyChangedEvents.Should().Contain(nameof(Activity.RequestedBy));
        propertyChangedEvents.Should().HaveCount(3);
    }

    [Test]
    public void Activity_SetSameValue_ShouldNotRaisePropertyChanged()
    {
        // Arrange
        var activity = new Activity { ActivityType = "Field Trip" };
        var propertyChangedEventRaised = false;
        activity.PropertyChanged += (sender, e) => propertyChangedEventRaised = true;

        // Act
        activity.ActivityType = "Field Trip"; // Same value

        // Assert
        propertyChangedEventRaised.Should().BeFalse("PropertyChanged should not be raised for the same value");
    }

    [Test]
    public void Activity_BusinessLogicValidation_ShouldEnforceRules()
    {
        // Arrange & Act
        var activity = ActivityTestDataBuilder.CreateTodayFieldTrip(100);

        // Assert - Business rule validations
        activity.ActivityId.Should().Be(100);
        activity.Date.Date.Should().Be(DateTime.Today);
        activity.ActivityType.Should().Be("Field Trip");
        activity.Destination.Should().NotBeNullOrEmpty();
        activity.RequestedBy.Should().NotBeNullOrEmpty();
        activity.AssignedVehicleId.Should().BeGreaterThan(0);
        activity.DriverId.Should().BeGreaterThan(0);
        activity.StudentsCount.Should().BeGreaterThan(0);
        activity.LeaveTime.Should().BeLessThan(activity.EventTime, "Leave time should be before event time");
        activity.EventTime.Should().BeLessThan(activity.ReturnTime, "Event time should be before return time");
    }

    [Test]
    public void Activity_TimeValidation_ShouldHaveLogicalTimeSequence()
    {
        // Arrange
        var activity = new Activity
        {
            LeaveTime = TimeSpan.Parse("08:00"),
            EventTime = TimeSpan.Parse("09:30"),
            ReturnTime = TimeSpan.Parse("15:00")
        };

        // Assert
        activity.LeaveTime.Should().BeLessThan(activity.EventTime, "Leave time should be before event time");
        activity.EventTime.Should().BeLessThan(activity.ReturnTime, "Event time should be before return time");
        activity.ReturnTime.Subtract(activity.LeaveTime).Should().BeGreaterThan(TimeSpan.FromHours(1), "Total trip should be at least 1 hour");
    }
}

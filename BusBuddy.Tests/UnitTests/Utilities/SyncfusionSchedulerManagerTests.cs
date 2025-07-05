using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Utilities;
using Bus_Buddy.Models;
using System.Windows.Forms;
using System;
using System.Collections.Generic;

namespace BusBuddy.Tests.UnitTests.Utilities
{
    /// <summary>
    /// Tests for SyncfusionSchedulerManager - Advanced scheduling component management
    /// This covers complex Syncfusion scheduler operations and appointment management
    /// </summary>
    [TestFixture]
    public class SyncfusionSchedulerManagerTests
    {
        private SyncfusionSchedulerManager _manager = null!;
        private Form _testForm = null!;
        private List<ActivitySchedule> _testSchedules = null!;

        [SetUp]
        public void SetUp()
        {
            _manager = new SyncfusionSchedulerManager();
            _testForm = new Form();
            _testSchedules = CreateTestSchedules();
        }

        private static List<ActivitySchedule> CreateTestSchedules()
        {
            return new List<ActivitySchedule>
            {
                new ActivitySchedule
                {
                    ActivityScheduleId = 1,
                    ScheduledDate = DateTime.Today.AddDays(1),
                    ScheduledLeaveTime = TimeSpan.FromHours(9),
                    ScheduledEventTime = TimeSpan.FromHours(17),
                    TripType = "Sports Trip",
                    ScheduledDestination = "Stadium",
                    ScheduledVehicleId = 1,
                    ScheduledDriverId = 1,
                    RequestedBy = "Test User",
                    Status = "Scheduled",
                    Notes = "Test schedule 1"
                },
                new ActivitySchedule
                {
                    ActivityScheduleId = 2,
                    ScheduledDate = DateTime.Today.AddDays(2),
                    ScheduledLeaveTime = TimeSpan.FromHours(10),
                    ScheduledEventTime = TimeSpan.FromHours(18),
                    TripType = "Activity Trip",
                    ScheduledDestination = "Museum",
                    ScheduledVehicleId = 2,
                    ScheduledDriverId = 2,
                    RequestedBy = "Test User 2",
                    Status = "Confirmed",
                    Notes = "Test schedule 2"
                }
            };
        }

        [Test]
        public void Constructor_ShouldCreateInstance()
        {
            // Act & Assert
            _manager.Should().NotBeNull();
        }

        [Test]
        public void InitializeScheduler_WithValidForm_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _manager.InitializeScheduler(_testForm));
        }

        [Test]
        public void InitializeScheduler_WithNullForm_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _manager.InitializeScheduler(null!));
        }

        [Test]
        public void LoadScheduleData_WithValidData_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _manager.LoadScheduleData(_testSchedules));
        }

        [Test]
        public void LoadScheduleData_WithNullData_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _manager.LoadScheduleData(null!));
        }

        [Test]
        public void LoadScheduleData_WithEmptyData_ShouldNotThrow()
        {
            // Arrange
            var emptySchedules = new List<ActivitySchedule>();

            // Act & Assert
            Assert.DoesNotThrow(() => _manager.LoadScheduleData(emptySchedules));
        }

        [Test]
        public void SetViewMode_WithValidModes_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _manager.SetViewMode(SchedulerViewMode.Day));
            Assert.DoesNotThrow(() => _manager.SetViewMode(SchedulerViewMode.Week));
            Assert.DoesNotThrow(() => _manager.SetViewMode(SchedulerViewMode.Month));
            Assert.DoesNotThrow(() => _manager.SetViewMode(SchedulerViewMode.Year));
        }

        [Test]
        public void AddAppointment_WithValidSchedule_ShouldNotThrow()
        {
            // Arrange
            var schedule = _testSchedules[0];

            // Act & Assert
            Assert.DoesNotThrow(() => _manager.AddAppointment(schedule));
        }

        [Test]
        public void AddAppointment_WithNullSchedule_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _manager.AddAppointment(null!));
        }

        [Test]
        public void UpdateAppointment_WithValidSchedule_ShouldNotThrow()
        {
            // Arrange
            var schedule = _testSchedules[0];

            // Act & Assert
            Assert.DoesNotThrow(() => _manager.UpdateAppointment(schedule));
        }

        [Test]
        public void RemoveAppointment_WithValidId_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _manager.RemoveAppointment(1));
        }

        [Test]
        public void RemoveAppointment_WithInvalidId_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _manager.RemoveAppointment(-1));
        }

        [Test]
        public void GetAppointmentsForDate_WithValidDate_ShouldNotThrow()
        {
            // Arrange
            var testDate = DateTime.Today;

            // Act & Assert
            Assert.DoesNotThrow(() => _manager.GetAppointmentsForDate(testDate));
        }

        [Test]
        public void GetAppointmentsForDateRange_WithValidRange_ShouldNotThrow()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(7);

            // Act & Assert
            Assert.DoesNotThrow(() => _manager.GetAppointmentsForDateRange(startDate, endDate));
        }

        [Test]
        public void GetAppointmentsForDateRange_WithInvalidRange_ShouldThrowArgumentException()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(-7); // End before start

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _manager.GetAppointmentsForDateRange(startDate, endDate));
        }

        [Test]
        public void SetTimeScale_WithValidScale_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _manager.SetTimeScale(TimeSpan.FromMinutes(30)));
            Assert.DoesNotThrow(() => _manager.SetTimeScale(TimeSpan.FromHours(1)));
        }

        [Test]
        public void SetTimeScale_WithInvalidScale_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _manager.SetTimeScale(TimeSpan.Zero));
            Assert.Throws<ArgumentException>(() => _manager.SetTimeScale(TimeSpan.FromMinutes(-30)));
        }

        [Test]
        public void ApplyCustomStyling_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _manager.ApplyCustomStyling());
        }

        [Test]
        public void EnableDragAndDrop_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _manager.EnableDragAndDrop(true));
            Assert.DoesNotThrow(() => _manager.EnableDragAndDrop(false));
        }

        [Test]
        public void SetWorkingHours_WithValidHours_ShouldNotThrow()
        {
            // Arrange
            var startTime = TimeSpan.FromHours(9);
            var endTime = TimeSpan.FromHours(17);

            // Act & Assert
            Assert.DoesNotThrow(() => _manager.SetWorkingHours(startTime, endTime));
        }

        [Test]
        public void SetWorkingHours_WithInvalidHours_ShouldThrowArgumentException()
        {
            // Arrange
            var startTime = TimeSpan.FromHours(17);
            var endTime = TimeSpan.FromHours(9); // End before start

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _manager.SetWorkingHours(startTime, endTime));
        }

        [Test]
        public void ExportSchedule_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var filePath = System.IO.Path.GetTempFileName();

            // Act & Assert
            Assert.DoesNotThrow(() => _manager.ExportSchedule(filePath, ExportFormat.PDF));

            // Cleanup
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        [Test]
        public void NavigateToDate_WithValidDate_ShouldNotThrow()
        {
            // Arrange
            var targetDate = DateTime.Today.AddDays(30);

            // Act & Assert
            Assert.DoesNotThrow(() => _manager.NavigateToDate(targetDate));
        }

        [Test]
        public void RefreshScheduler_ShouldNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _manager.RefreshScheduler());
        }

        [Test]
        public void GetCurrentViewDate_ShouldReturnDate()
        {
            // Act
            var currentDate = _manager.GetCurrentViewDate();

            // Assert
            currentDate.Should().BeBefore(DateTime.MaxValue);
            currentDate.Should().BeAfter(DateTime.MinValue);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                _testForm?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TearDown warning: {ex.Message}");
                // Ignore disposal errors in test cleanup
            }
        }
    }
}

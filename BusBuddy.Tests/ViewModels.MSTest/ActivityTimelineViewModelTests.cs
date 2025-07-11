using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.WPF.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BusBuddy.Tests.ViewModels
{
    [TestClass]
    public class ActivityTimelineViewModelTests
    {
        private Mock<IActivityLogService> _mockLogService;
        private ActivityTimelineViewModel _viewModel;

        [TestInitialize]
        public void Initialize()
        {
            _mockLogService = new Mock<IActivityLogService>();
            _viewModel = new ActivityTimelineViewModel(_mockLogService.Object);
        }

        [TestMethod]
        public void ViewModel_Initialization_ShouldSetUpDefaultProperties()
        {
            // Assert that the ViewModel is initialized with appropriate default values
            Assert.IsNotNull(_viewModel.DateRanges);
            Assert.IsTrue(_viewModel.DateRanges.Count > 0);
            Assert.IsNotNull(_viewModel.EventTypes);
            Assert.IsTrue(_viewModel.EventTypes.Count > 0);
            Assert.IsNotNull(_viewModel.SelectedEventTypes);
            Assert.IsTrue(_viewModel.SelectedEventTypes.Count > 0);
            Assert.IsNotNull(_viewModel.TimelineEvents);
            Assert.IsNotNull(_viewModel.RefreshCommand);
        }

        [TestMethod]
        public void ViewModel_SelectingPresetDateRange_ShouldUpdateDateValues()
        {
            // Initial state
            var originalStartDate = _viewModel.StartDate;
            var originalEndDate = _viewModel.EndDate;

            // Act - select "Today" date range
            var todayOption = _viewModel.DateRanges.First(d => d.Range == DateRange.Today);
            _viewModel.SelectedDateRange = todayOption;

            // Assert
            Assert.AreEqual(DateTime.Now.Date, _viewModel.StartDate.Date);
            Assert.AreNotEqual(originalStartDate, _viewModel.StartDate);
            Assert.AreNotEqual(originalEndDate, _viewModel.EndDate);
        }

        [TestMethod]
        public void ViewModel_SelectingCustomDateRange_ShouldSetIsCustomDateRangeTrue()
        {
            // Act - select "Custom Range" date range
            var customOption = _viewModel.DateRanges.First(d => d.Range == DateRange.Custom);
            _viewModel.SelectedDateRange = customOption;

            // Assert
            Assert.IsTrue(_viewModel.IsCustomDateRange);
        }

        [TestMethod]
        public async Task RefreshTimelineAsync_ShouldPopulateTimelineEvents()
        {
            // Arrange
            var logs = new List<ActivityLog>
            {
                new ActivityLog { Id = 1, Timestamp = DateTime.Now, Action = "Created Entity", User = "Test User" },
                new ActivityLog { Id = 2, Timestamp = DateTime.Now.AddHours(-1), Action = "Updated Entity", User = "Test User" },
                new ActivityLog { Id = 3, Timestamp = DateTime.Now.AddHours(-2), Action = "Deleted Entity", User = "Test User" }
            };

            _mockLogService.Setup(s => s.GetLogsAsync(It.IsAny<int>()))
                .ReturnsAsync(logs);

            // Act
            await Task.Run(() => _viewModel.RefreshCommand.Execute(null));

            // Assert
            Assert.AreEqual(3, _viewModel.TimelineEvents.Count);
            Assert.IsFalse(_viewModel.HasNoData);
        }

        [TestMethod]
        public async Task RefreshTimelineAsync_WithNoData_ShouldSetHasNoDataTrue()
        {
            // Arrange
            _mockLogService.Setup(s => s.GetLogsAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ActivityLog>());

            // Act
            await Task.Run(() => _viewModel.RefreshCommand.Execute(null));

            // Assert
            Assert.AreEqual(0, _viewModel.TimelineEvents.Count);
            Assert.IsTrue(_viewModel.HasNoData);
        }

        [TestMethod]
        public void DetermineEventType_ShouldMapActionsToCorrectTypes()
        {
            // Access the private method using reflection
            var method = typeof(ActivityTimelineViewModel).GetMethod("DetermineEventType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Test with various action strings
            string createType = (string)method.Invoke(_viewModel, new object[] { "Created User" });
            string updateType = (string)method.Invoke(_viewModel, new object[] { "Updated Entity" });
            string deleteType = (string)method.Invoke(_viewModel, new object[] { "Deleted Record" });
            string readType = (string)method.Invoke(_viewModel, new object[] { "Viewed Report" });
            string loginType = (string)method.Invoke(_viewModel, new object[] { "User Login" });
            string errorType = (string)method.Invoke(_viewModel, new object[] { "Error Processing" });
            string systemType = (string)method.Invoke(_viewModel, new object[] { "System Startup" });

            // Assert
            Assert.AreEqual("Create", createType);
            Assert.AreEqual("Update", updateType);
            Assert.AreEqual("Delete", deleteType);
            Assert.AreEqual("Read", readType);
            Assert.AreEqual("Login", loginType);
            Assert.AreEqual("Error", errorType);
            Assert.AreEqual("System", systemType);
        }

        [TestMethod]
        public void EventTypeOption_ShouldHaveAppropriateProperties()
        {
            // Check the first event type option
            var firstOption = _viewModel.EventTypes[0];

            Assert.IsNotNull(firstOption.EventType);
            Assert.IsNotNull(firstOption.DisplayName);
            Assert.IsNotNull(firstOption.Color);
            Assert.IsInstanceOfType(firstOption.Color, typeof(SolidColorBrush));
        }
    }
}

using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.WPF.Services;
using BusBuddy.WPF.ViewModels;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Tests.ViewModels
{
    /// <summary>
    /// Unit tests for the DriverManagementViewModel
    /// </summary>
    [TestFixture]
    public class DriverManagementViewModelTests
    {
        private Mock<IDriverService> _mockDriverService;
        private Mock<IDriverAvailabilityService> _mockAvailabilityService;
        private Mock<IActivityLogService> _mockActivityLogService;
        private DriverManagementViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Set up mocks
            _mockDriverService = new Mock<IDriverService>();
            _mockAvailabilityService = new Mock<IDriverAvailabilityService>();
            _mockActivityLogService = new Mock<IActivityLogService>();

            // Set up test data
            SetupMockData();

            // Create the view model
            _viewModel = new DriverManagementViewModel(
                _mockDriverService.Object,
                _mockAvailabilityService.Object,
                _mockActivityLogService.Object);

            // Wait for initialization to complete
            _viewModel.Initialized.Wait();
        }

        private void SetupMockData()
        {
            // Mock driver data
            var drivers = new List<Driver>
            {
                new Driver
                {
                    DriverId = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    LicenseExpiryDate = DateTime.Today.AddDays(60)
                },
                new Driver
                {
                    DriverId = 2,
                    FirstName = "Jane",
                    LastName = "Doe",
                    LicenseExpiryDate = DateTime.Today.AddDays(20)
                },
                new Driver
                {
                    DriverId = 3,
                    FirstName = "Bob",
                    LastName = "Johnson",
                    LicenseExpiryDate = DateTime.Today.AddDays(-10)
                }
            };

            _mockDriverService.Setup(m => m.GetAllDriversAsync())
                .ReturnsAsync(drivers);

            // Mock availability data
            var availabilities = new List<DriverAvailabilityInfo>
            {
                new DriverAvailabilityInfo
                {
                    DriverId = 1,
                    DriverName = "John Smith",
                    AvailableDates = new List<DateTime> { DateTime.Today, DateTime.Today.AddDays(1) }
                },
                new DriverAvailabilityInfo
                {
                    DriverId = 2,
                    DriverName = "Jane Doe",
                    AvailableDates = new List<DateTime> { DateTime.Today.AddDays(2), DateTime.Today.AddDays(3) }
                }
            };

            _mockAvailabilityService.Setup(m => m.GetDriverAvailabilitiesAsync())
                .ReturnsAsync(availabilities);

            // Set up activity logging to do nothing
            _mockActivityLogService.Setup(m => m.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public void LoadDriversCommand_LoadsAllDrivers()
        {
            // Arrange - done in setup

            // Act - initialization already calls LoadDriversAsync

            // Assert
            _viewModel.Drivers.Count.Should().Be(3);
            _viewModel.Drivers.Should().Contain(d => d.DriverId == 1);
            _viewModel.Drivers.Should().Contain(d => d.DriverId == 2);
            _viewModel.Drivers.Should().Contain(d => d.DriverId == 3);
        }

        [Test]
        public void GenerateLicenseStatusReport_CreatesCorrectStatuses()
        {
            // Arrange - done in setup

            // Act
            _viewModel.GenerateLicenseStatusReportCommand.Execute(null);

            // Assert
            _viewModel.LicenseStatusReport.Count.Should().Be(3);

            var expiredDriver = _viewModel.LicenseStatusReport.FirstOrDefault(r => r.DriverName == "Bob Johnson");
            expiredDriver.Should().NotBeNull();
            expiredDriver.LicenseStatus.Should().Be("Expired");

            var expiringSoonDriver = _viewModel.LicenseStatusReport.FirstOrDefault(r => r.DriverName == "Jane Doe");
            expiringSoonDriver.Should().NotBeNull();
            expiringSoonDriver.LicenseStatus.Should().Be("Expiring Soon");

            var currentDriver = _viewModel.LicenseStatusReport.FirstOrDefault(r => r.DriverName == "John Smith");
            currentDriver.Should().NotBeNull();
            currentDriver.LicenseStatus.Should().Be("Current");
        }

        [Test]
        public async Task AddDriverCommand_CallsDriverService()
        {
            // Arrange
            var newDriver = new Driver { FirstName = "New", LastName = "Driver" };
            _viewModel.SelectedDriver = newDriver;

            // Act
            _viewModel.AddDriverCommand.Execute(null);

            // Allow async operations to complete
            await Task.Delay(100);

            // Assert
            _mockDriverService.Verify(m => m.AddDriverAsync(newDriver), Times.Once);
            _mockDriverService.Verify(m => m.GetAllDriversAsync(), Times.Exactly(2)); // Initial load + refresh
        }

        [Test]
        public async Task UpdateDriverCommand_CallsDriverService()
        {
            // Arrange
            var driver = new Driver { DriverId = 1, FirstName = "Updated", LastName = "Driver" };
            _viewModel.SelectedDriver = driver;

            // Act
            _viewModel.UpdateDriverCommand.Execute(null);

            // Allow async operations to complete
            await Task.Delay(100);

            // Assert
            _mockDriverService.Verify(m => m.UpdateDriverAsync(driver), Times.Once);
            _mockDriverService.Verify(m => m.GetAllDriversAsync(), Times.Exactly(2)); // Initial load + refresh
        }

        [Test]
        public async Task DeleteDriverCommand_CallsDriverService()
        {
            // Arrange
            var driver = new Driver { DriverId = 1, FirstName = "John", LastName = "Smith" };
            _viewModel.SelectedDriver = driver;

            // Act
            _viewModel.DeleteDriverCommand.Execute(null);

            // Allow async operations to complete
            await Task.Delay(100);

            // Assert
            _mockDriverService.Verify(m => m.DeleteDriverAsync(1), Times.Once);
            _mockDriverService.Verify(m => m.GetAllDriversAsync(), Times.Exactly(2)); // Initial load + refresh
        }

        [Test]
        public void CanUpdateOrDelete_ReturnsFalseForNewDriver()
        {
            // Arrange
            var newDriver = new Driver { DriverId = 0, FirstName = "New", LastName = "Driver" };
            _viewModel.SelectedDriver = newDriver;

            // Act & Assert
            _viewModel.UpdateDriverCommand.CanExecute(null).Should().BeFalse();
            _viewModel.DeleteDriverCommand.CanExecute(null).Should().BeFalse();
        }

        [Test]
        public void CanUpdateOrDelete_ReturnsTrueForExistingDriver()
        {
            // Arrange
            var existingDriver = new Driver { DriverId = 1, FirstName = "John", LastName = "Smith" };
            _viewModel.SelectedDriver = existingDriver;

            // Act & Assert
            _viewModel.UpdateDriverCommand.CanExecute(null).Should().BeTrue();
            _viewModel.DeleteDriverCommand.CanExecute(null).Should().BeTrue();
        }
    }

    // Helper class removed as it's now defined in the WPF project
    // public class DriverAvailabilityInfo
    // {
    //     public int DriverId { get; set; }
    //     public string DriverName { get; set; } = string.Empty;
    //     public List<DateTime> AvailableDates { get; set; } = new();
    // }
}

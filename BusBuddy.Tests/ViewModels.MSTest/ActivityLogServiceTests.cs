using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Tests.ViewModels
{
    [TestClass]
    public class ActivityLogServiceTests
    {
        private BusBuddyDbContext _dbContext;
        private Mock<ILogger<ActivityLogService>> _mockLogger;
        private ActivityLogService _service;

        [TestInitialize]
        public void Initialize()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
                .UseInMemoryDatabase(databaseName: $"ActivityLogTestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new BusBuddyDbContext(options);
            _mockLogger = new Mock<ILogger<ActivityLogService>>();
            _service = new ActivityLogService(_dbContext, _mockLogger.Object);
        }

        [TestMethod]
        public async Task LogAsync_Should_CreateLogEntry()
        {
            // Arrange
            string action = "Test Action";
            string user = "Test User";
            string details = "Test Details";

            // Act
            await _service.LogAsync(action, user, details);

            // Assert
            var logs = await _dbContext.ActivityLogs.ToListAsync();
            Assert.AreEqual(1, logs.Count);
            var log = logs.First();
            Assert.AreEqual(action, log.Action);
            Assert.AreEqual(user, log.User);
            Assert.AreEqual(details, log.Details);
        }

        [TestMethod]
        public async Task LogAsync_Should_TruncateLongDetails()
        {
            // Arrange
            string action = "Test Action";
            string user = "Test User";
            string details = new string('X', 1500); // Create a string that exceeds the 1000 char limit

            // Act
            await _service.LogAsync(action, user, details);

            // Assert
            var logs = await _dbContext.ActivityLogs.ToListAsync();
            Assert.AreEqual(1, logs.Count);
            var log = logs.First();
            Assert.IsTrue(log.Details.Length <= 1000);
            Assert.IsTrue(log.Details.EndsWith("[...]"));
        }

        [TestMethod]
        public async Task GetLogsAsync_Should_ReturnOrderedByTimestampDescending()
        {
            // Arrange
            await CreateTestLogsAsync(5);

            // Act
            var result = await _service.GetLogsAsync();

            // Assert
            var logs = result.ToList();
            Assert.AreEqual(5, logs.Count);

            // Verify logs are ordered by timestamp descending
            for (int i = 0; i < logs.Count - 1; i++)
            {
                Assert.IsTrue(logs[i].Timestamp >= logs[i + 1].Timestamp);
            }
        }

        [TestMethod]
        public async Task GetLogsByDateRangeAsync_Should_ReturnLogsInRange()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(-5);
            var midDate = DateTime.Now.AddDays(-3);
            var endDate = DateTime.Now.AddDays(-1);

            // Create logs with different dates
            await _service.LogAsync("Outside Before", "Test User", "Before range");
            _dbContext.ActivityLogs.First().Timestamp = startDate.AddDays(-1);

            await _service.LogAsync("Inside Start", "Test User", "At start of range");
            _dbContext.ActivityLogs.Skip(1).First().Timestamp = startDate;

            await _service.LogAsync("Inside Middle", "Test User", "In middle of range");
            _dbContext.ActivityLogs.Skip(2).First().Timestamp = midDate;

            await _service.LogAsync("Inside End", "Test User", "At end of range");
            _dbContext.ActivityLogs.Skip(3).First().Timestamp = endDate;

            await _service.LogAsync("Outside After", "Test User", "After range");
            _dbContext.ActivityLogs.Skip(4).First().Timestamp = endDate.AddDays(1);

            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetLogsByDateRangeAsync(startDate, endDate);

            // Assert
            var logs = result.ToList();
            Assert.AreEqual(3, logs.Count);
            Assert.IsTrue(logs.All(l => l.Timestamp >= startDate && l.Timestamp <= endDate));
        }

        [TestMethod]
        public async Task LogEntityActionAsync_Should_CreateFormattedLogEntry()
        {
            // Arrange
            var testEntity = new TestEntity { Id = 42, Name = "Test Entity", Description = "A test entity" };

            // Act
            await _service.LogEntityActionAsync("Created", "Test User", testEntity);

            // Assert
            var logs = await _dbContext.ActivityLogs.ToListAsync();
            Assert.AreEqual(1, logs.Count);
            var log = logs.First();

            Assert.AreEqual("Created TestEntity #42", log.Action);
            Assert.AreEqual("Test User", log.User);
            Assert.IsTrue(log.Details.Contains("TestEntity"));
            Assert.IsTrue(log.Details.Contains("42"));
            Assert.IsTrue(log.Details.Contains("Test Entity"));
        }

        private async Task CreateTestLogsAsync(int count)
        {
            for (int i = 0; i < count; i++)
            {
                await _service.LogAsync($"Test Action {i}", "Test User", $"Test Details {i}");
                // Ensure timestamps are different
                await Task.Delay(5);
            }
        }

        // Test entity class for testing entity logging
        private class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}

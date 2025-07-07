using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Bus_Buddy.Services;
using BusBuddy.Services;

namespace Bus_Buddy.Services
{
    /// <summary>
    /// Production Fleet Monitoring Service
    /// Provides real-time monitoring, automated alerts, and proactive maintenance scheduling
    /// </summary>
    public class FleetMonitoringService : BackgroundService
    {
        private readonly ILogger<FleetMonitoringService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly BusBuddyAIReportingService _aiReportingService;

        // Monitoring configuration
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _alertCheckInterval = TimeSpan.FromMinutes(15);

        // Alert thresholds
        private const int MaintenanceWarningDays = 30;
        private const int MaintenanceCriticalDays = 7;
        private const double UtilizationThreshold = 85.0;

        public FleetMonitoringService(
            ILogger<FleetMonitoringService> logger,
            IServiceProvider serviceProvider,
            BusBuddyAIReportingService aiReportingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _aiReportingService = aiReportingService ?? throw new ArgumentNullException(nameof(aiReportingService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Fleet Monitoring Service started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await PerformMonitoringCycle();
                    await Task.Delay(_monitoringInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Fleet Monitoring Service stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fleet Monitoring Service encountered an error");
            }
        }

        private async Task PerformMonitoringCycle()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var busService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IBusService>(scope.ServiceProvider);
                var maintenanceService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IMaintenanceService>(scope.ServiceProvider);

                _logger.LogDebug("Starting fleet monitoring cycle");

                // Get current fleet status
                var buses = await busService.GetAllBusesAsync();
                var busList = buses.ToList();

                // Check for maintenance alerts
                await CheckMaintenanceAlerts(busList, maintenanceService);

                // Check fleet utilization
                await CheckFleetUtilization(busList);

                // Generate AI insights for any critical issues
                await GenerateProactiveInsights(busList);

                _logger.LogDebug($"Fleet monitoring cycle completed. Monitored {busList.Count} buses");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during fleet monitoring cycle");
            }
        }

        private async Task CheckMaintenanceAlerts(List<BusInfo> buses, IMaintenanceService maintenanceService)
        {
            var alerts = new List<FleetAlert>();

            foreach (var bus in buses)
            {
                var daysSinceLastMaintenance = (DateTime.Now - bus.LastMaintenance).Days;

                // Critical maintenance alert (overdue)
                if (daysSinceLastMaintenance > 90)
                {
                    alerts.Add(new FleetAlert
                    {
                        BusId = bus.BusId,
                        BusNumber = bus.BusNumber,
                        AlertType = FleetAlertType.MaintenanceOverdue,
                        Priority = AlertPriority.Critical,
                        Message = $"Bus #{bus.BusNumber} maintenance is {daysSinceLastMaintenance} days overdue",
                        CreatedAt = DateTime.Now,
                        RequiresAction = true
                    });
                }
                // Warning maintenance alert
                else if (daysSinceLastMaintenance > 60)
                {
                    alerts.Add(new FleetAlert
                    {
                        BusId = bus.BusId,
                        BusNumber = bus.BusNumber,
                        AlertType = FleetAlertType.MaintenanceDue,
                        Priority = AlertPriority.Medium,
                        Message = $"Bus #{bus.BusNumber} maintenance due in {90 - daysSinceLastMaintenance} days",
                        CreatedAt = DateTime.Now,
                        RequiresAction = false
                    });
                }
            }

            // Process alerts
            if (alerts.Any())
            {
                await ProcessAlerts(alerts);
            }
        }

        private async Task CheckFleetUtilization(List<BusInfo> buses)
        {
            var activeBuses = buses.Count(b => b.Status == "Active");
            var totalBuses = buses.Count;

            if (totalBuses > 0)
            {
                var utilizationPercentage = (double)activeBuses / totalBuses * 100;

                if (utilizationPercentage > UtilizationThreshold)
                {
                    var alert = new FleetAlert
                    {
                        BusId = 0, // Fleet-wide alert
                        BusNumber = "FLEET",
                        AlertType = FleetAlertType.HighUtilization,
                        Priority = AlertPriority.Medium,
                        Message = $"Fleet utilization at {utilizationPercentage:F1}% - consider expanding capacity",
                        CreatedAt = DateTime.Now,
                        RequiresAction = false
                    };

                    await ProcessAlerts(new List<FleetAlert> { alert });
                }
            }
        }

        private async Task GenerateProactiveInsights(List<BusInfo> buses)
        {
            try
            {
                // Only generate insights if there are significant issues
                var criticalIssues = buses.Count(b => (DateTime.Now - b.LastMaintenance).Days > 90);
                var inactiveBuses = buses.Count(b => b.Status != "Active");

                if (criticalIssues > 0 || inactiveBuses > buses.Count * 0.2) // More than 20% inactive
                {
                    var fleetSummary = $"Fleet Health Check: {buses.Count} total buses, " +
                                     $"{criticalIssues} requiring immediate maintenance, " +
                                     $"{inactiveBuses} currently inactive. " +
                                     $"Fleet utilization: {(double)(buses.Count - inactiveBuses) / buses.Count * 100:F1}%";

                    var query = "Analyze this fleet health data and provide prioritized action recommendations for " +
                               "optimizing fleet availability and reducing maintenance costs.";

                    var response = await _aiReportingService.GenerateReportAsync(query, fleetSummary);

                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        var insightAlert = new FleetAlert
                        {
                            BusId = 0,
                            BusNumber = "AI_INSIGHT",
                            AlertType = FleetAlertType.AIInsight,
                            Priority = AlertPriority.Low,
                            Message = $"AI Fleet Optimization Insight: {response.Content}",
                            CreatedAt = DateTime.Now,
                            RequiresAction = false
                        };

                        await ProcessAlerts(new List<FleetAlert> { insightAlert });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate proactive AI insights");
            }
        }

        private Task ProcessAlerts(List<FleetAlert> alerts)
        {
            foreach (var alert in alerts)
            {
                // Log the alert
                LogAlert(alert);

                // Store alert in database (if needed)
                // await StoreAlert(alert);

                // Send notifications (if configured)
                // await SendNotification(alert);
            }

            return Task.CompletedTask;
        }

        private void LogAlert(FleetAlert alert)
        {
            var logLevel = alert.Priority switch
            {
                AlertPriority.Critical => LogLevel.Error,
                AlertPriority.High => LogLevel.Warning,
                AlertPriority.Medium => LogLevel.Information,
                AlertPriority.Low => LogLevel.Debug,
                _ => LogLevel.Information
            };

            _logger.Log(logLevel, "Fleet Alert: {AlertType} - {Message} (Bus: {BusNumber})",
                       alert.AlertType, alert.Message, alert.BusNumber);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fleet Monitoring Service stopping");
            await base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Fleet alert data structure
    /// </summary>
    public class FleetAlert
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public FleetAlertType AlertType { get; set; }
        public AlertPriority Priority { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool RequiresAction { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
    }

    /// <summary>
    /// Types of fleet alerts
    /// </summary>
    public enum FleetAlertType
    {
        MaintenanceOverdue,
        MaintenanceDue,
        InspectionRequired,
        InsuranceExpiring,
        HighUtilization,
        LowUtilization,
        VehicleBreakdown,
        AIInsight,
        SystemIssue
    }

    /// <summary>
    /// Alert priority levels
    /// </summary>
    public enum AlertPriority
    {
        Critical = 4,
        High = 3,
        Medium = 2,
        Low = 1
    }
}

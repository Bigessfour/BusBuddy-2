using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BusBuddy.Core.Services
{
    /// <summary>
    /// Production Fleet Monitoring Service
    /// Provides real-time monitoring, automated alerts, and proactive maintenance scheduling
    /// </summary>
    public class FleetMonitoringService : BackgroundService
    {
        private static readonly ILogger Logger = Log.ForContext<FleetMonitoringService>();
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
            IServiceProvider serviceProvider,
            BusBuddyAIReportingService aiReportingService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _aiReportingService = aiReportingService ?? throw new ArgumentNullException(nameof(aiReportingService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Information("Fleet Monitoring Service started");

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
                Logger.Information("Fleet Monitoring Service stopped");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Fleet Monitoring Service encountered an error");
            }
        }

        private async Task PerformMonitoringCycle()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var busService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IBusService>(scope.ServiceProvider);
                var maintenanceService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IMaintenanceService>(scope.ServiceProvider);

                Logger.Debug("Starting fleet monitoring cycle");

                // Get current fleet status
                var buses = await busService.GetAllBusesAsync();
                var busList = buses.ToList();

                // Check for maintenance alerts
                await CheckMaintenanceAlerts(busList, maintenanceService);

                // Check fleet utilization
                await CheckFleetUtilization(busList);

                // Generate AI insights for any critical issues
                await GenerateProactiveInsights(busList);

                Logger.Debug("Fleet monitoring cycle completed. Monitored {BusCount} buses", busList.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during fleet monitoring cycle");
            }
        }

        private async Task CheckMaintenanceAlerts(List<Bus> buses, IMaintenanceService maintenanceService)
        {
            var alerts = new List<FleetAlert>();

            foreach (var bus in buses)
            {
                var lastMaintenance = bus.DateLastInspection ?? DateTime.MinValue;
                var daysSinceLastMaintenance = (DateTime.Now - lastMaintenance).Days;

                // Critical maintenance alert (overdue)
                if (daysSinceLastMaintenance > 90)
                {
                    alerts.Add(new FleetAlert
                    {
                        BusId = bus.VehicleId,
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
                        BusId = bus.VehicleId,
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
            if (alerts.Count > 0)
            {
                await ProcessAlerts(alerts);
            }
        }

        private async Task CheckFleetUtilization(List<Bus> buses)
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

        private async Task GenerateProactiveInsights(List<Bus> buses)
        {
            try
            {
                // Only generate insights if there are significant issues
                var criticalIssues = buses.Count(b => b.DateLastInspection.HasValue && (DateTime.Now - b.DateLastInspection.Value).Days > 90);
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
                Logger.Warning(ex, "Failed to generate proactive AI insights");
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
            var logMethod = alert.Priority switch
            {
                AlertPriority.Critical => (Action<string, object, object, object>)Logger.Error,
                AlertPriority.High => (Action<string, object, object, object>)Logger.Warning,
                AlertPriority.Medium => (Action<string, object, object, object>)Logger.Information,
                AlertPriority.Low => (Action<string, object, object, object>)Logger.Debug,
                _ => (Action<string, object, object, object>)Logger.Information
            };

            logMethod("Fleet Alert: {AlertType} - {Message} (Bus: {BusNumber})",
                     alert.AlertType, alert.Message, alert.BusNumber);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Fleet Monitoring Service stopping");
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

using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BusBuddy.WPF.ViewModels
{
    public class ActivityLogViewModel : BaseInDevelopmentViewModel
    {
        private readonly IActivityLogService _activityLogService;
        public ObservableCollection<ActivityLog> Logs { get; } = new();

        public ActivityLogViewModel(IActivityLogService activityLogService, ILogger<ActivityLogViewModel>? logger = null)
            : base(logger)
        {
            _activityLogService = activityLogService;
            _ = LoadLogsAsync();

            // Set as in-development
            IsInDevelopment = true;
        }

        public async Task LoadLogsAsync()
        {
            try
            {
                Logs.Clear();
                var logs = await _activityLogService.GetLogsAsync(200);
                foreach (var log in logs)
                    Logs.Add(log);

                Logger?.LogInformation("Loaded {LogCount} activity logs", Logs.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading activity logs");
            }
        }
    }
}

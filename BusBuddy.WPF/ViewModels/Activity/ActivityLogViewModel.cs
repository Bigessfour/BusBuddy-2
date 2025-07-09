using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using System.Collections.ObjectModel;

namespace BusBuddy.WPF.ViewModels
{
    public class ActivityLogViewModel : BaseViewModel
    {
        private readonly IActivityLogService _activityLogService;
        public ObservableCollection<ActivityLog> Logs { get; } = new();

        public ActivityLogViewModel(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
            _ = LoadLogsAsync();
        }

        public async Task LoadLogsAsync()
        {
            Logs.Clear();
            var logs = await _activityLogService.GetLogsAsync(200);
            foreach (var log in logs)
                Logs.Add(log);
        }
    }
}

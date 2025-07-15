using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.ViewModels.Schedule;
using BusBuddy.WPF.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.ViewModels
{
    public class NavigationItem
    {
        public required string Name { get; set; }
        public required string ViewModelName { get; set; }

        /// <summary>
        /// Gets or sets whether this navigation item is for a deprecated module.
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// Gets or sets a tooltip to display for this navigation item.
        /// </summary>
        public string? Tooltip { get; set; }
    }

    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        private object? _currentViewModel;

        private readonly ILogger<MainViewModel>? _logger;
        private readonly ILazyViewModelService _lazyViewModelService;
        private readonly DashboardViewModel _dashboardViewModel; // Keep dashboard eager for immediate display
        private readonly LoadingViewModel _loadingViewModel; // Keep loading view eager

        public ObservableCollection<NavigationItem> NavigationItems { get; }

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            LoadingViewModel loadingViewModel,
            ILazyViewModelService lazyViewModelService,
            ILogger<MainViewModel>? logger = null)
        {
            _dashboardViewModel = dashboardViewModel;
            _loadingViewModel = loadingViewModel;
            _lazyViewModelService = lazyViewModelService;
            _logger = logger;

            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { Name = "Dashboard", ViewModelName = "Dashboard" },
                new NavigationItem { Name = "Buses", ViewModelName = "Buses" },
                new NavigationItem { Name = "Drivers", ViewModelName = "Drivers" },
                new NavigationItem { Name = "Routes", ViewModelName = "Routes" },
                new NavigationItem { Name = "Schedule", ViewModelName = "Schedule" },
                new NavigationItem { Name = "Students", ViewModelName = "Students" },
                new NavigationItem { Name = "Maintenance", ViewModelName = "Maintenance" },
                new NavigationItem { Name = "Fuel", ViewModelName = "Fuel" },
                new NavigationItem { Name = "Activity", ViewModelName = "Activity" },
                new NavigationItem { Name = "Student List", ViewModelName = "StudentList" },
                new NavigationItem { Name = "Settings", ViewModelName = "Settings" }
            };

            // Start with loading view for smooth startup - no data loading yet
            CurrentViewModel = _loadingViewModel;

            _logger?.LogInformation("MainViewModel initialized with {Count} navigation items using lazy loading", NavigationItems.Count);
        }

        partial void OnCurrentViewModelChanged(object? value)
        {
            var viewName = value?.GetType().Name?.Replace("ViewModel", "") ?? "Unknown";
            _logger?.LogInformation("UI View model changed - CurrentView is now {ViewName} ({ViewModelType})",
                viewName, value?.GetType().Name ?? "null");
        }

        /// <summary>
        /// Navigate to a specific view model
        /// </summary>
        [RelayCommand]
        private async Task NavigateTo(string viewModelName)
        {
            _logger?.LogInformation("UI Navigation initiated to {ViewModelName} via button click", viewModelName);

            object? previousViewModel = CurrentViewModel;
            var startTime = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Show loading state for non-cached ViewModels
                if (previousViewModel != _loadingViewModel)
                {
                    var stats = _lazyViewModelService.GetCacheStats();
                    if (stats.CachedCount == 0 && viewModelName != "Dashboard" && viewModelName != "Loading")
                    {
                        CurrentViewModel = _loadingViewModel;
                    }
                }

                CurrentViewModel = viewModelName switch
                {
                    "Dashboard" => _dashboardViewModel,
                    "Buses" => await _lazyViewModelService.GetViewModelAsync<BusManagementViewModel>(),
                    "Drivers" => await _lazyViewModelService.GetViewModelAsync<DriverManagementViewModel>(),
                    "Routes" => await _lazyViewModelService.GetViewModelAsync<RouteManagementViewModel>(),
                    "Schedule" => await _lazyViewModelService.GetViewModelAsync<ScheduleManagementViewModel>(),
                    "Students" => await _lazyViewModelService.GetViewModelAsync<StudentManagementViewModel>(),
                    "Maintenance" => await _lazyViewModelService.GetViewModelAsync<MaintenanceTrackingViewModel>(),
                    "Fuel" => await _lazyViewModelService.GetViewModelAsync<FuelManagementViewModel>(),
                    "Activity" => await _lazyViewModelService.GetViewModelAsync<ActivityLogViewModel>(),
                    "Settings" => await _lazyViewModelService.GetViewModelAsync<SettingsViewModel>(),
                    "StudentList" => await _lazyViewModelService.GetViewModelAsync<StudentListViewModel>(),
                    "Loading" => _loadingViewModel,
                    "Error" => _loadingViewModel, // Use loading view for errors too
                    _ => _dashboardViewModel
                };

                startTime.Stop();
                _logger?.LogInformation("UI View transition completed to {ViewModel} from {PreviousViewModel} in {ElapsedMs}ms",
                    CurrentViewModel?.GetType().Name,
                    previousViewModel?.GetType().Name ?? "null",
                    startTime.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                startTime.Stop();
                _logger?.LogError(ex, "Error navigating to {ViewModelName} after {ElapsedMs}ms", viewModelName, startTime.ElapsedMilliseconds);
                CurrentViewModel = _loadingViewModel;
            }
        }

        /// <summary>
        /// Preload essential ViewModels in the background
        /// </summary>
        public async Task PreloadEssentialViewModelsAsync()
        {
            try
            {
                _logger?.LogInformation("Starting background preload of essential ViewModels");
                await _lazyViewModelService.PreloadEssentialViewModelsAsync();
                _logger?.LogInformation("Background preload completed");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during background ViewModel preload");
            }
        }
    }
}

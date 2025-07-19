using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.ViewModels.Schedule;
using BusBuddy.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BusBuddy.WPF.ViewModels
{
    public class NavigationItem
    {
        public required string Name { get; set; }
        public required string ViewModelName { get; set; }
        public required string Icon { get; set; }

        /// <summary>
        /// Gets or sets whether this navigation item is for a deprecated module.
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// Gets or sets whether this navigation item is for a new module.
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets a tooltip to display for this navigation item.
        /// </summary>
        public string? Tooltip { get; set; }

        /// <summary>
        /// Gets or sets a description to display for this navigation item.
        /// </summary>
        public string? Description { get; set; }
    }

    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        private object? _currentViewModel;

        [ObservableProperty]
        private NavigationItem? _selectedNavigationItem;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isInitialLoading;

        [ObservableProperty]
        private string _currentViewTitle = "Dashboard";

        [ObservableProperty]
        private int _notificationCount = 0;

        [ObservableProperty]
        private bool _hasNotifications = false;

        [ObservableProperty]
        private string _currentViewName = "Dashboard";

        /// <summary>
        /// Gets whether the application is running in debug mode
        /// </summary>
        public bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return System.Diagnostics.Debugger.IsAttached;
#endif
            }
        }

        private readonly ILogger _logger;
        private readonly ILazyViewModelService _lazyViewModelService;
        private readonly DashboardViewModel _dashboardViewModel; // Keep dashboard eager for immediate display
        private readonly LoadingViewModel _loadingViewModel; // Keep loading view eager
        private readonly INavigationService? _navigationService; // Optional navigation service for centralized navigation

        public ObservableCollection<NavigationItem> NavigationItems { get; }

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            LoadingViewModel loadingViewModel,
            ILazyViewModelService lazyViewModelService,
            INavigationService? navigationService = null)
        {
            _dashboardViewModel = dashboardViewModel;
            _loadingViewModel = loadingViewModel;
            _lazyViewModelService = lazyViewModelService;
            _navigationService = navigationService;
            _logger = Log.ForContext<MainViewModel>();

            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem {
                    Name = "Dashboard",
                    ViewModelName = "Dashboard",
                    Icon = "📊",
                    Description = "Fleet Overview & Analytics",
                    Tooltip = "View real-time fleet performance and key metrics"
                },
                new NavigationItem {
                    Name = "Buses",
                    ViewModelName = "Buses",
                    Icon = "🚌",
                    Description = "Fleet Management",
                    Tooltip = "Manage bus inventory, maintenance schedules, and assignments"
                },
                new NavigationItem {
                    Name = "Drivers",
                    ViewModelName = "Drivers",
                    Icon = "👨‍💼",
                    Description = "Driver Management",
                    Tooltip = "Manage driver licenses, assignments, and performance"
                },
                new NavigationItem {
                    Name = "Routes",
                    ViewModelName = "Routes",
                    Icon = "🗺️",
                    Description = "Route Planning",
                    Tooltip = "Create and optimize bus routes and stops"
                },
                new NavigationItem {
                    Name = "Schedule",
                    ViewModelName = "Schedule",
                    Icon = "📅",
                    Description = "Schedule Management",
                    Tooltip = "Manage bus schedules and timetables"
                },
                new NavigationItem {
                    Name = "Sports Scheduling",
                    ViewModelName = "SportsSchedule",
                    Icon = "🏐",
                    Description = "Sports Trips & Events",
                    Tooltip = "Manage sports trips, events, and activity schedules",
                    IsNew = true
                },
                new NavigationItem {
                    Name = "Students",
                    ViewModelName = "Students",
                    Icon = "👨‍🎓",
                    Description = "Student Management",
                    Tooltip = "Manage student information and bus assignments"
                },
                new NavigationItem {
                    Name = "Maintenance",
                    ViewModelName = "Maintenance",
                    Icon = "🔧",
                    Description = "Maintenance Tracking",
                    Tooltip = "Track maintenance schedules and service history"
                },
                new NavigationItem {
                    Name = "Fuel",
                    ViewModelName = "Fuel",
                    Icon = "⛽",
                    Description = "Fuel Management",
                    Tooltip = "Monitor fuel consumption and costs"
                },
                new NavigationItem {
                    Name = "Activity",
                    ViewModelName = "Activity",
                    Icon = "📝",
                    Description = "Activity Logging",
                    Tooltip = "View system activity and audit trails"
                },
                new NavigationItem {
                    Name = "Student List",
                    ViewModelName = "StudentList",
                    Icon = "📋",
                    Description = "Student Rosters",
                    Tooltip = "View and manage student rosters and assignments"
                },
                new NavigationItem {
                    Name = "Settings",
                    ViewModelName = "Settings",
                    Icon = "⚙️",
                    Description = "System Settings",
                    Tooltip = "Configure application settings and preferences"
                }
            };

            // Start with loading view for smooth startup - no data loading yet
            CurrentViewModel = _loadingViewModel;

            _logger.Information("MainViewModel initialized with {Count} navigation items using lazy loading", NavigationItems.Count);
        }

        partial void OnCurrentViewModelChanged(object? value)
        {
            var viewName = value?.GetType().Name?.Replace("ViewModel", "") ?? "Unknown";
            _logger.Information("UI View model changed - CurrentView is now {ViewName} ({ViewModelType})",
                viewName, value?.GetType().Name ?? "null");
        }

        /// <summary>
        /// Navigate to a specific view model
        /// </summary>
        [RelayCommand]
        public async Task NavigateTo(string viewModelName)
        {
            _logger.Information("UI Navigation initiated to {ViewModelName} via button click", viewModelName);

            // Update current view name for menu checkboxes
            CurrentViewName = viewModelName;

            // Use NavigationService if available, otherwise use legacy method
            if (_navigationService != null)
            {
                try
                {
                    // Map legacy view model names to NavigationService names
                    var navigationName = viewModelName switch
                    {
                        "Buses" => "BusManagement",
                        "Drivers" => "DriverManagement",
                        "Routes" => "RouteManagement",
                        "Schedule" => "ScheduleManagement",
                        "SportsSchedule" => "ScheduleManagement",
                        "Students" => "StudentManagement",
                        "Maintenance" => "Maintenance",
                        "Fuel" => "FuelManagement",
                        "Activity" => "ActivityLog",
                        "Settings" => "Settings",
                        "StudentList" => "StudentManagement",
                        "Dashboard" => "Dashboard",
                        _ => "Dashboard"
                    };

                    _navigationService.NavigateTo(navigationName);
                    _logger.Information("Navigation completed using NavigationService to {NavigationName}", navigationName);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error navigating using NavigationService, falling back to legacy method");
                }
            }

            // Legacy navigation method (fallback)
            await NavigateToLegacy(viewModelName);
        }

        /// <summary>
        /// Command for menu-based navigation with parameter
        /// </summary>
        [RelayCommand]
        public async Task NavigateToCommand(string viewName)
        {
            await NavigateTo(viewName);
        }

        /// <summary>
        /// Legacy navigation method for backward compatibility
        /// </summary>
        private async Task NavigateToLegacy(string viewModelName)
        {
            _logger.Information("UI Navigation initiated to {ViewModelName} via button click", viewModelName);

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
                    "SportsSchedule" => await _lazyViewModelService.GetViewModelAsync<BusBuddy.WPF.ViewModels.ScheduleManagement.ScheduleViewModel>(),
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

                // Initialize dashboard data if navigating to Dashboard
                if (viewModelName == "Dashboard" && CurrentViewModel == _dashboardViewModel)
                {
                    try
                    {
                        await _dashboardViewModel.InitializeAsync();
                        _logger.Information("Dashboard data initialized after navigation");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error initializing dashboard data after navigation");
                    }
                }

                startTime.Stop();
                _logger.Information("UI View transition completed to {ViewModel} from {PreviousViewModel} in {ElapsedMs}ms",
                    CurrentViewModel?.GetType().Name,
                    previousViewModel?.GetType().Name ?? "null",
                    startTime.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                startTime.Stop();
                _logger.Error(ex, "Error navigating to {ViewModelName} after {ElapsedMs}ms", viewModelName, startTime.ElapsedMilliseconds);
                CurrentViewModel = _loadingViewModel;
            }
        }

        /// <summary>
        /// Command to navigate to a specific view based on NavigationItem
        /// </summary>
        [RelayCommand]
        private async Task Navigate(NavigationItem navigationItem)
        {
            if (navigationItem != null)
            {
                await NavigateTo(navigationItem.ViewModelName);
            }
        }

        /// <summary>
        /// Public method to navigate to Dashboard view - used by startup orchestration
        /// </summary>
        public async Task NavigateToDashboardAsync()
        {
            _logger.Information("UI Navigation to Dashboard initiated from startup orchestration");

            // Use NavigationService if available
            if (_navigationService != null)
            {
                try
                {
                    _navigationService.NavigateTo("Dashboard");
                    _logger.Information("Dashboard navigation completed using NavigationService");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error navigating to Dashboard using NavigationService, falling back to legacy method");
                }
            }

            // Legacy fallback
            CurrentViewModel = _dashboardViewModel;

            // Initialize dashboard data after navigation
            try
            {
                await _dashboardViewModel.InitializeAsync();
                _logger.Information("Dashboard initialization completed after navigation");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error initializing dashboard after navigation");
            }
        }

        /// <summary>
        /// Synchronous version for backward compatibility
        /// </summary>
        public void NavigateToDashboard()
        {
            _logger.Information("UI Navigation to Dashboard initiated from startup orchestration (sync)");

            // Use NavigationService if available
            if (_navigationService != null)
            {
                try
                {
                    _navigationService.NavigateTo("Dashboard");
                    _logger.Information("Dashboard navigation completed using NavigationService (sync)");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error navigating to Dashboard using NavigationService (sync), falling back to legacy method");
                }
            }

            // Legacy fallback
            CurrentViewModel = _dashboardViewModel;

            // Initialize dashboard data in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _dashboardViewModel.InitializeAsync();
                    _logger.Information("Dashboard initialization completed after navigation (background)");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error initializing dashboard after navigation (background)");
                }
            });
        }

        /// <summary>
        /// Preload essential ViewModels in the background
        /// </summary>
        public async Task PreloadEssentialViewModelsAsync()
        {
            try
            {
                _logger.Information("Starting background preload of essential ViewModels");
                await _lazyViewModelService.PreloadEssentialViewModelsAsync();
                _logger.Information("Background preload completed");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during background ViewModel preload");
            }
        }
    }
}

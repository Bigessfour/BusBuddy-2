using System;
using System.Collections.Generic;
using System.Linq;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.ViewModels.Schedule;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.Services
{
    /// <summary>
    /// Centralized navigation service implementation for BusBuddy WPF
    /// Manages view model creation, navigation history, and view transitions
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NavigationService> _logger;
        private readonly List<string> _navigationHistory;
        private object? _currentViewModel;
        private string _currentViewTitle;

        public NavigationService(IServiceProvider serviceProvider, ILogger<NavigationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _navigationHistory = new List<string>();
            _currentViewTitle = "Bus Buddy Dashboard";
        }

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnNavigationChanged();
            }
        }

        public string CurrentViewTitle
        {
            get => _currentViewTitle;
            private set
            {
                _currentViewTitle = value;
                OnNavigationChanged();
            }
        }

        public IReadOnlyList<string> NavigationHistory => _navigationHistory.AsReadOnly();

        public bool CanNavigateBack => _navigationHistory.Count > 0;

        public event EventHandler<NavigationEventArgs>? NavigationChanged;

        public void NavigateTo(string viewName)
        {
            NavigateTo(viewName, null);
        }

        public void NavigateTo(string viewName, object? parameters)
        {
            try
            {
                _logger.LogInformation("Navigating to view: {ViewName}", viewName);

                // Add current view to history if we have one
                if (_currentViewModel != null)
                {
                    var currentViewName = GetViewNameFromViewModel(_currentViewModel);
                    if (!string.IsNullOrEmpty(currentViewName))
                    {
                        _navigationHistory.Add(currentViewName);
                    }
                }

                // Create new view model
                var viewModel = CreateViewModelForView(viewName, parameters);
                var viewTitle = GetViewTitleForView(viewName);

                // Update current state
                CurrentViewModel = viewModel;
                CurrentViewTitle = viewTitle;

                _logger.LogInformation("Successfully navigated to: {ViewName}", viewName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to navigate to view: {ViewName}", viewName);
                throw;
            }
        }

        public bool NavigateBack()
        {
            if (!CanNavigateBack)
            {
                _logger.LogWarning("Cannot navigate back - no history available");
                return false;
            }

            try
            {
                // Get the last view from history
                var previousViewName = _navigationHistory.Last();
                _navigationHistory.RemoveAt(_navigationHistory.Count - 1);

                // Navigate without adding to history
                var viewModel = CreateViewModelForView(previousViewName, null);
                var viewTitle = GetViewTitleForView(previousViewName);

                CurrentViewModel = viewModel;
                CurrentViewTitle = viewTitle;

                _logger.LogInformation("Successfully navigated back to: {ViewName}", previousViewName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to navigate back");
                return false;
            }
        }

        public void ClearHistory()
        {
            _navigationHistory.Clear();
            _logger.LogInformation("Navigation history cleared");
        }

        private object? CreateViewModelForView(string viewName, object? parameters)
        {
            return viewName switch
            {
                "Dashboard" => _serviceProvider.GetRequiredService<DashboardViewModel>(),
                "BusManagement" => _serviceProvider.GetRequiredService<BusManagementViewModel>(),
                "DriverManagement" => _serviceProvider.GetRequiredService<DriverManagementViewModel>(),
                "RouteManagement" => _serviceProvider.GetRequiredService<RouteManagementViewModel>(),
                "ScheduleManagement" => _serviceProvider.GetRequiredService<ScheduleManagementViewModel>(),
                "StudentManagement" => _serviceProvider.GetRequiredService<StudentManagementViewModel>(),
                "Maintenance" => _serviceProvider.GetRequiredService<MaintenanceTrackingViewModel>(),
                "FuelManagement" => _serviceProvider.GetRequiredService<FuelManagementViewModel>(),
                "ActivityLog" => _serviceProvider.GetRequiredService<ActivityLogViewModel>(),
                "Settings" => _serviceProvider.GetRequiredService<SettingsViewModel>(),
                _ => throw new ArgumentException($"Unknown view name: {viewName}")
            };
        }

        private string GetViewTitleForView(string viewName)
        {
            return viewName switch
            {
                "Dashboard" => "🚌 Bus Buddy Dashboard",
                "BusManagement" => "🚌 Bus Management",
                "DriverManagement" => "👨‍💼 Driver Management",
                "RouteManagement" => "🗺️ Route Management",
                "ScheduleManagement" => "📅 Schedule Management",
                "StudentManagement" => "🎓 Student Management",
                "Maintenance" => "🔧 Maintenance Tracking",
                "FuelManagement" => "⛽ Fuel Management",
                "ActivityLog" => "📝 Activity Log",
                "Settings" => "⚙️ Settings",
                _ => $"Bus Buddy - {viewName}"
            };
        }

        private string GetViewNameFromViewModel(object viewModel)
        {
            return viewModel switch
            {
                DashboardViewModel => "Dashboard",
                BusManagementViewModel => "BusManagement",
                DriverManagementViewModel => "DriverManagement",
                RouteManagementViewModel => "RouteManagement",
                ScheduleManagementViewModel => "ScheduleManagement",
                StudentManagementViewModel => "StudentManagement",
                MaintenanceTrackingViewModel => "Maintenance",
                FuelManagementViewModel => "FuelManagement",
                ActivityLogViewModel => "ActivityLog",
                SettingsViewModel => "Settings",
                _ => string.Empty
            };
        }

        private void OnNavigationChanged()
        {
            NavigationChanged?.Invoke(this, new NavigationEventArgs(
                _currentViewModel != null ? GetViewNameFromViewModel(_currentViewModel) : string.Empty,
                null,
                _currentViewModel,
                _currentViewTitle));
        }

        /// <summary>
        /// Get the current navigation state for debugging or logging purposes
        /// </summary>
        public string GetCurrentNavigationState()
        {
            var currentView = _currentViewModel != null ? GetViewNameFromViewModel(_currentViewModel) : "None";
            return $"Current: {currentView}, History: [{string.Join(", ", _navigationHistory)}]";
        }

        /// <summary>
        /// Check if we can navigate to a specific view
        /// </summary>
        public bool CanNavigateTo(string viewName)
        {
            return viewName switch
            {
                "Dashboard" or "BusManagement" or "DriverManagement" or "RouteManagement" or
                "ScheduleManagement" or "StudentManagement" or "Maintenance" or "FuelManagement" or
                "ActivityLog" or "Settings" => true,
                _ => false
            };
        }

        /// <summary>
        /// Navigate to Dashboard (convenience method)
        /// </summary>
        public void NavigateToDashboard()
        {
            NavigateTo("Dashboard");
        }
    }
}

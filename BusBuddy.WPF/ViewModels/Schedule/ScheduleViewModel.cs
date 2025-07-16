using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.Core.Services;
using BusBuddy.WPF.Views.Schedule;
using BusBuddy.WPF.Models;
using BusBuddy.WPF.ViewModels.Schedule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Serilog;
using System.Windows;

namespace BusBuddy.WPF.ViewModels.ScheduleManagement
{
    public class ScheduleViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;
        private readonly IRouteService _routeService;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ObservableCollection<BusBuddy.Core.Models.Schedule> Schedules { get; } = new();

        private BusBuddy.Core.Models.Schedule? _selectedSchedule;
        public BusBuddy.Core.Models.Schedule? SelectedSchedule
        {
            get => _selectedSchedule;
            set
            {
                if (SetProperty(ref _selectedSchedule, value))
                {
                    _logger.Debug("Schedule selection changed to {ScheduleId}", value?.ScheduleId);
                    OnPropertyChanged(nameof(CanEditSchedule));
                    OnPropertyChanged(nameof(CanDeleteSchedule));
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool CanEditSchedule => SelectedSchedule != null;
        public bool CanDeleteSchedule => SelectedSchedule != null;

        public ICommand LoadSchedulesCommand { get; }
        public ICommand AddScheduleCommand { get; }
        public ICommand EditScheduleCommand { get; }
        public ICommand DeleteScheduleCommand { get; }
        public ICommand RefreshCommand { get; }

        public ScheduleViewModel(
            IScheduleService scheduleService,
            IRouteService routeService,
            IBusService busService,
            IDriverService driverService,
            IServiceProvider serviceProvider)
        {
            _scheduleService = scheduleService;
            _routeService = routeService;
            _busService = busService;
            _driverService = driverService;
            _serviceProvider = serviceProvider;
            _logger = Log.ForContext<ScheduleViewModel>()
                .ForContext("Module", "ScheduleManagement")
                .ForContext("Component", "ScheduleViewModel");

            LoadSchedulesCommand = new AsyncRelayCommand(LoadSchedulesAsync);
            AddScheduleCommand = new AsyncRelayCommand(AddScheduleAsync);
            EditScheduleCommand = new AsyncRelayCommand(EditScheduleAsync, () => CanEditSchedule);
            DeleteScheduleCommand = new AsyncRelayCommand(DeleteScheduleAsync, () => CanDeleteSchedule);
            RefreshCommand = new AsyncRelayCommand(LoadSchedulesAsync);

            _logger.Information("ScheduleViewModel initialized");
        }
        private async Task LoadSchedulesAsync()
        {
            _logger.Information("Starting to load schedules");

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                _logger.Information("Loading schedules...");

                Schedules.Clear();
                var schedules = await _scheduleService.GetSchedulesAsync();

                foreach (var schedule in schedules)
                {
                    Schedules.Add(schedule);
                }

                _logger.Information("Successfully loaded {ScheduleCount} schedules", schedules.Count());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load schedules");
                ErrorMessage = "Failed to load schedules. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddScheduleAsync()
        {
            _logger.Information("Starting to add new schedule");

            try
            {
                _logger.Information("Opening add schedule dialog");

                var viewModel = new AddEditScheduleViewModel(
                    _scheduleService,
                    _routeService,
                    _busService,
                    _driverService);

                var dialog = new AddEditScheduleDialog
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow
                };

                // Load data first
                if (viewModel.LoadDataCommand is AsyncRelayCommand loadCommand)
                {
                    await loadCommand.ExecuteAsync(null);
                }

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    _logger.Information("Schedule added successfully, refreshing list");
                    await LoadSchedulesAsync();
                }
                else
                {
                    _logger.Debug("Add schedule dialog cancelled");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add schedule");
                ErrorMessage = "Failed to add schedule. Please try again.";
            }
        }

        private async Task EditScheduleAsync()
        {
            if (SelectedSchedule == null)
            {
                _logger.Warning("Edit schedule attempted with no schedule selected");
                return;
            }

            _logger.Information("Opening edit schedule dialog for schedule {ScheduleId}", SelectedSchedule.ScheduleId);

            try
            {
                _logger.Information("Opening edit schedule dialog for schedule {ScheduleId}", SelectedSchedule.ScheduleId);

                var viewModel = new AddEditScheduleViewModel(
                    _scheduleService,
                    _routeService,
                    _busService,
                    _driverService,
                    SelectedSchedule);

                var dialog = new AddEditScheduleDialog
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow
                };

                // Load data first
                if (viewModel.LoadDataCommand is AsyncRelayCommand loadCommand)
                {
                    await loadCommand.ExecuteAsync(null);
                }

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    _logger.Information("Schedule {ScheduleId} updated successfully, refreshing list", SelectedSchedule.ScheduleId);
                    await LoadSchedulesAsync();
                }
                else
                {
                    _logger.Debug("Edit schedule dialog cancelled for schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to edit schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                ErrorMessage = "Failed to edit schedule. Please try again.";
            }
        }

        private async Task DeleteScheduleAsync()
        {
            if (SelectedSchedule == null)
            {
                _logger.Warning("Delete schedule attempted with no schedule selected");
                return;
            }

            _logger.Information("Deleting schedule with ID: {ScheduleId}", SelectedSchedule.ScheduleId);

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete this schedule?\n\nRoute: {SelectedSchedule.Route?.RouteName}\nDate: {SelectedSchedule.ScheduleDate:yyyy-MM-dd}",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _logger.Information("Deleting schedule {ScheduleId}", SelectedSchedule.ScheduleId);

                    await _scheduleService.DeleteScheduleAsync(SelectedSchedule.ScheduleId);
                    Schedules.Remove(SelectedSchedule);

                    _logger.Information("Successfully deleted schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
                else
                {
                    _logger.Debug("Delete schedule cancelled for schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                ErrorMessage = "Failed to delete schedule. Please try again.";
            }
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using System.Windows.Input;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.ViewModels
{
    using BusBuddy.Core.Services;
    using BusBuddy.Core.Models;
    using System.Linq;
    using System.Threading.Tasks;

    public class ScheduleManagementViewModel : INotifyPropertyChanged
    {
        private readonly IScheduleService _service;
        public ObservableCollection<Activity> Schedules { get; } = new();
        public ObservableCollection<string> AvailableBuses { get; } = new();
        public ObservableCollection<string> AvailableDrivers { get; } = new();
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AutoGenerateCommand { get; }
        private Activity? _selectedSchedule;
        public Activity? SelectedSchedule
        {
            get => _selectedSchedule;
            set { _selectedSchedule = value; OnPropertyChanged(); }
        }

        public ScheduleManagementViewModel(IScheduleService service)
        {
            _service = service;
            LoadSchedulesAsync();
            AddCommand = new BusBuddy.WPF.RelayCommand(_ => AddScheduleAsyncWrapper());
            EditCommand = new BusBuddy.WPF.RelayCommand(_ => EditScheduleAsyncWrapper());
            DeleteCommand = new BusBuddy.WPF.RelayCommand(_ => DeleteScheduleAsyncWrapper());
            AutoGenerateCommand = new BusBuddy.WPF.RelayCommand(_ => AutoGenerateSchedulesAsyncWrapper());
        }

        private async void LoadSchedulesAsync()
        {
            Schedules.Clear();
            var schedules = await _service.GetAllSchedulesAsync();
            foreach (var s in schedules)
                Schedules.Add(s);
            // Optionally populate AvailableBuses and AvailableDrivers
        }

        private async void AddScheduleAsyncWrapper() => await AddScheduleAsync();
        private async void EditScheduleAsyncWrapper() => await EditScheduleAsync();
        private async void DeleteScheduleAsyncWrapper() => await DeleteScheduleAsync();
        private void AutoGenerateSchedulesAsyncWrapper() => AutoGenerateSchedulesAsync();

        private async Task AddScheduleAsync()
        {
            var newSchedule = new Activity
            {
                Date = DateTime.Now,
                ActivityType = "Route",
                Destination = "",
                LeaveTime = DateTime.Now.TimeOfDay,
                EventTime = DateTime.Now.AddHours(1).TimeOfDay,
                Status = "Scheduled"
            };
            var created = await _service.AddScheduleAsync(newSchedule);
            Schedules.Add(created);
        }

        private async Task EditScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                await _service.UpdateScheduleAsync(SelectedSchedule);
                // Optionally reload schedules
            }
        }

        private async Task DeleteScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                await _service.DeleteScheduleAsync(SelectedSchedule.ActivityId);
                Schedules.Remove(SelectedSchedule);
            }
        }

        private void AutoGenerateSchedulesAsync()
        {
            // Implement auto-generation logic using real service if needed
            // For now, just reload all schedules
            LoadSchedulesAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels.ScheduleManagement
{
    public class ScheduleViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;
        public ObservableCollection<BusBuddy.Core.Models.Schedule> Schedules { get; } = new();
        private BusBuddy.Core.Models.Schedule? _selectedSchedule;
        public BusBuddy.Core.Models.Schedule? SelectedSchedule
        {
            get => _selectedSchedule;
            set => SetProperty(ref _selectedSchedule, value);
        }

        public ICommand LoadSchedulesCommand { get; }
        public ICommand AddScheduleCommand { get; }
        public ICommand EditScheduleCommand { get; }
        public ICommand DeleteScheduleCommand { get; }

        public ScheduleViewModel(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
            LoadSchedulesCommand = new AsyncRelayCommand(LoadSchedulesAsync);
            AddScheduleCommand = new AsyncRelayCommand(AddScheduleAsync);
            EditScheduleCommand = new AsyncRelayCommand(EditScheduleAsync);
            DeleteScheduleCommand = new AsyncRelayCommand(DeleteScheduleAsync);
        }

        private async Task LoadSchedulesAsync()
        {
            Schedules.Clear();
            var schedules = await _scheduleService.GetSchedulesAsync();
            foreach (var s in schedules)
                Schedules.Add(s);
        }

        private async Task AddScheduleAsync()
        {
            // Show dialog and add logic here
            await Task.CompletedTask;
        }

        private async Task EditScheduleAsync()
        {
            // Show dialog and edit logic here
            await Task.CompletedTask;
        }

        private async Task DeleteScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                await _scheduleService.DeleteScheduleAsync(SelectedSchedule.ScheduleId);
                Schedules.Remove(SelectedSchedule);
            }
        }
    }
}

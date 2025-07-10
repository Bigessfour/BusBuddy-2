using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BusBuddy.WPF.ViewModels
{
    public partial class StudentManagementViewModel : BaseInDevelopmentViewModel
    {
        private readonly IStudentService _studentService;
        private readonly IBusService _busService;
        private readonly IRouteService _routeService;

        public ObservableCollection<Student> Students { get; } = new();
        public ObservableCollection<Bus> Buses { get; private set; } = new();
        public ObservableCollection<Route> Routes { get; private set; } = new();

        [ObservableProperty]
        private Student? _selectedStudent;

        public StudentManagementViewModel(
            IStudentService studentService,
            IBusService busService,
            IRouteService routeService,
            ILogger<StudentManagementViewModel> logger) : base(logger)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));

            // Load data asynchronously in sequence to avoid DbContext concurrency issues
            _ = InitializeAsync();

            // Set as in-development
            IsInDevelopment = true;
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Load data sequentially to avoid concurrent DbContext access
                await LoadDataAsync();
                await LoadStudentsAsync();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error during ViewModel initialization");
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var buses = await _busService.GetAllBusEntitiesAsync();
                Buses = new ObservableCollection<Bus>(buses);
                OnPropertyChanged(nameof(Buses));

                var routes = await _routeService.GetAllActiveRoutesAsync();
                Routes = new ObservableCollection<Route>(routes);
                OnPropertyChanged(nameof(Routes));

                Logger?.LogInformation("Loaded {BusCount} buses and {RouteCount} routes", Buses.Count, Routes.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading buses and routes");
            }
        }

        private async Task LoadStudentsAsync()
        {
            try
            {
                Students.Clear();
                var students = await _studentService.GetAllStudentsAsync();
                foreach (var s in students)
                    Students.Add(s);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading students");
            }
        }

        [RelayCommand]
        private async Task AddStudentAsync()
        {
            try
            {
                var newStudent = new Student
                {
                    StudentName = "New Student",
                    Grade = "K",
                    Active = true
                };
                var created = await _studentService.AddStudentAsync(newStudent);
                Students.Add(created);
                Logger?.LogInformation("Added new student with ID {StudentId}", created.StudentId);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error adding student");
            }
        }

        [RelayCommand]
        private async Task EditStudentAsync()
        {
            if (SelectedStudent == null) return;

            try
            {
                await _studentService.UpdateStudentAsync(SelectedStudent);
                Logger?.LogInformation("Updated student with ID {StudentId}", SelectedStudent.StudentId);
                // Refresh data
                await LoadStudentsAsync();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error updating student {StudentId}", SelectedStudent.StudentId);
            }
        }

        [RelayCommand]
        private async Task DeleteStudentAsync()
        {
            if (SelectedStudent == null) return;

            try
            {
                await _studentService.DeleteStudentAsync(SelectedStudent.StudentId);
                Students.Remove(SelectedStudent);
                Logger?.LogInformation("Deleted student with ID {StudentId}", SelectedStudent.StudentId);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error deleting student {StudentId}", SelectedStudent.StudentId);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadStudentsAsync();
            await LoadDataAsync();
        }

        // Stub for XAML binding: DeleteCommand
        public System.Windows.Input.ICommand DeleteCommand => DeleteStudentCommand;

        // Stub for XAML binding: AvailableBuses
        public System.Collections.IEnumerable AvailableBuses => Buses;

        // Stub for XAML binding: AvailableRoutes
        public System.Collections.IEnumerable AvailableRoutes => Routes;
    }
}

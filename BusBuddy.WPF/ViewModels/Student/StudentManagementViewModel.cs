using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public class StudentManagementViewModel : INotifyPropertyChanged
    {
        private readonly IStudentService _studentService;
        private readonly IBusService _busService;
        private readonly IRouteService _routeService;

        public ObservableCollection<Student> Students { get; } = new();
        public ObservableCollection<Bus> Buses { get; private set; } = new();
        public ObservableCollection<Route> Routes { get; private set; } = new();

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        private Student? _selectedStudent;
        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set { _selectedStudent = value; OnPropertyChanged(); }
        }

        public StudentManagementViewModel(IStudentService studentService, IBusService busService, IRouteService routeService)
        {
            _studentService = studentService;
            _busService = busService;
            _routeService = routeService;

            LoadStudents();
            LoadDataAsync();

            AddCommand = new RelayCommand(_ => AddStudentAsyncWrapper());
            EditCommand = new RelayCommand(_ => EditStudentAsyncWrapper());
            DeleteCommand = new RelayCommand(_ => DeleteStudentAsyncWrapper());
        }

        private async void LoadDataAsync()
        {
            var buses = await _busService.GetAllBusEntitiesAsync();
            Buses = new ObservableCollection<Bus>(buses);
            OnPropertyChanged(nameof(Buses));

            var routes = await _routeService.GetAllActiveRoutesAsync();
            Routes = new ObservableCollection<Route>(routes);
            OnPropertyChanged(nameof(Routes));

            Log.Information("Loaded {BusCount} buses and {RouteCount} routes", Buses.Count, Routes.Count);
        }

        // ICommand async wrappers
        private async void AddStudentAsyncWrapper() => await AddStudentAsync();
        private async void EditStudentAsyncWrapper() => await EditStudentAsync();
        private async void DeleteStudentAsyncWrapper() => await DeleteStudentAsync();

        private async void LoadStudents()
        {
            Students.Clear();
            var students = await _studentService.GetAllStudentsAsync();
            foreach (var s in students)
                Students.Add(s);
        }

        private async Task AddStudentAsync()
        {
            var newStudent = new Student
            {
                StudentName = "New Student",
                Grade = "K",
                Active = true
            };
            var created = await _studentService.AddStudentAsync(newStudent);
            Students.Add(created);
        }

        private async Task EditStudentAsync()
        {
            if (SelectedStudent != null)
            {
                await _studentService.UpdateStudentAsync(SelectedStudent);
                // Optionally reload students
            }
        }

        private async Task DeleteStudentAsync()
        {
            if (SelectedStudent != null)
            {
                await _studentService.DeleteStudentAsync(SelectedStudent.StudentId);
                Students.Remove(SelectedStudent);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Removed duplicate legacy StudentManagementViewModel. Only the refactored version remains.
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public class Student : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Grade { get; set; }
        public string? AssignedBus { get; set; }
        public string? AssignedRoute { get; set; }
        public string? Status { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public interface IStudentService
    {
        ObservableCollection<Student> GetStudents();
        ObservableCollection<string> GetAvailableBuses();
        ObservableCollection<string> GetAvailableRoutes();
        void AddStudent(Student student);
        void UpdateStudent(Student student);
        void DeleteStudent(Student student);
    }

    public class StudentService : IStudentService
    {
        private ObservableCollection<Student> _students = new();
        private ObservableCollection<string> _buses = new() { "Bus 1", "Bus 2", "Bus 3" };
        private ObservableCollection<string> _routes = new() { "Route 101", "Route 102", "Route 103" };
        public ObservableCollection<Student> GetStudents() => _students;
        public ObservableCollection<string> GetAvailableBuses() => _buses;
        public ObservableCollection<string> GetAvailableRoutes() => _routes;
        public void AddStudent(Student student) => _students.Add(student);
        public void UpdateStudent(Student student)
        {
            var existing = _students.FirstOrDefault(s => s.Id == student.Id);
            if (existing != null)
            {
                existing.Name = student.Name;
                existing.Grade = student.Grade;
                existing.AssignedBus = student.AssignedBus;
                existing.AssignedRoute = student.AssignedRoute;
                existing.Status = student.Status;
            }
        }
        public void DeleteStudent(Student student) => _students.Remove(student);
    }

    public class StudentManagementViewModel : INotifyPropertyChanged
    {
        private readonly IStudentService _service;
        public ObservableCollection<Student> Students { get; }
        public ObservableCollection<string> AvailableBuses { get; }
        public ObservableCollection<string> AvailableRoutes { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        private Student? _selectedStudent;
        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set { _selectedStudent = value; OnPropertyChanged(); }
        }
        public StudentManagementViewModel(IStudentService service)
        {
            _service = service;
            Students = _service.GetStudents();
            AvailableBuses = _service.GetAvailableBuses();
            AvailableRoutes = _service.GetAvailableRoutes();
            AddCommand = new RelayCommand(AddStudent);
            EditCommand = new RelayCommand(EditStudent);
            DeleteCommand = new RelayCommand(DeleteStudent);
        }
        private void AddStudent()
        {
            var newStudent = new Student
            {
                Id = Students.Count + 1,
                Name = "New Student",
                Grade = "K",
                AssignedBus = AvailableBuses.FirstOrDefault() ?? string.Empty,
                AssignedRoute = AvailableRoutes.FirstOrDefault() ?? string.Empty,
                Status = "Active"
            };
            _service.AddStudent(newStudent);
        }
        private void EditStudent()
        {
            if (SelectedStudent != null)
                _service.UpdateStudent(SelectedStudent);
        }
        private void DeleteStudent()
        {
            if (SelectedStudent != null)
                _service.DeleteStudent(SelectedStudent);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

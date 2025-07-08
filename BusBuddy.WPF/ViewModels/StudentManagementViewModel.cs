using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{

    using BusBuddy.Core.Models;
    using BusBuddy.Core.Services;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class StudentManagementViewModel : INotifyPropertyChanged
    {
        private readonly IStudentService _service;
        public ObservableCollection<Student> Students { get; } = new();
        public ObservableCollection<string> AvailableBuses { get; } = new();
        public ObservableCollection<string> AvailableRoutes { get; } = new();
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
            LoadStudents();
            // TODO: Load real buses and routes from service if available
            AddCommand = new BusBuddy.WPF.RelayCommand(_ => AddStudentAsyncWrapper());
            EditCommand = new BusBuddy.WPF.RelayCommand(_ => EditStudentAsyncWrapper());
            DeleteCommand = new BusBuddy.WPF.RelayCommand(_ => DeleteStudentAsyncWrapper());
        }

        // ICommand async wrappers
        private async void AddStudentAsyncWrapper() => await AddStudentAsync();
        private async void EditStudentAsyncWrapper() => await EditStudentAsync();
        private async void DeleteStudentAsyncWrapper() => await DeleteStudentAsync();

        private async void LoadStudents()
        {
            Students.Clear();
            var students = await _service.GetAllStudentsAsync();
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
            var created = await _service.AddStudentAsync(newStudent);
            Students.Add(created);
        }

        private async Task EditStudentAsync()
        {
            if (SelectedStudent != null)
            {
                await _service.UpdateStudentAsync(SelectedStudent);
                // Optionally reload students
            }
        }

        private async Task DeleteStudentAsync()
        {
            if (SelectedStudent != null)
            {
                await _service.DeleteStudentAsync(SelectedStudent.StudentId);
                Students.Remove(SelectedStudent);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Removed duplicate legacy StudentManagementViewModel. Only the refactored version remains.
}

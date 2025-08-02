using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core;
using Microsoft.EntityFrameworkCore;
using BusBuddy.WPF;
using Serilog;

namespace BusBuddy.WPF.ViewModels.Student
{
    /// <summary>
    /// ViewModel for the StudentsView - manages student list display and operations
    /// Implements MVP pattern with basic CRUD operations
    /// </summary>
    public class StudentsViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<StudentsViewModel>();

        private readonly BusBuddyContext _context;
        private readonly AddressService _addressService;
        private Core.Models.Student? _selectedStudent;
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public StudentsViewModel()
        {
            _context = new BusBuddyContext();
            _addressService = new AddressService();
            Students = new ObservableCollection<Core.Models.Student>();

            InitializeCommands();
            _ = LoadStudentsAsync();
        }

        #region Properties

        /// <summary>
        /// Collection of all students for display in the data grid
        /// </summary>
        public ObservableCollection<Core.Models.Student> Students { get; }

        /// <summary>
        /// Currently selected student in the data grid
        /// </summary>
        public Core.Models.Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    OnPropertyChanged(nameof(HasSelectedStudent));
                }
            }
        }

        /// <summary>
        /// Whether a student is currently selected
        /// </summary>
        public bool HasSelectedStudent => SelectedStudent != null;

        /// <summary>
        /// Total number of students
        /// </summary>
        public int TotalStudents => Students.Count;

        /// <summary>
        /// Number of active students
        /// </summary>
        public int ActiveStudents => Students.Count(s => s.Active);

        /// <summary>
        /// Whether data is currently being loaded
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Status message for user feedback
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #endregion

        #region Commands

        public ICommand AddStudentCommand { get; private set; } = null!;
        public ICommand EditStudentCommand { get; private set; } = null!;
        public ICommand DeleteStudentCommand { get; private set; } = null!;
        public ICommand RefreshCommand { get; private set; } = null!;
        public ICommand ExportCommand { get; private set; } = null!;
        public ICommand ValidateAddressCommand { get; private set; } = null!;

        #endregion

        #region Command Initialization

        private void InitializeCommands()
        {
            AddStudentCommand = new RelayCommand(ExecuteAddStudent);
            EditStudentCommand = new RelayCommand(ExecuteEditStudent, CanExecuteEditStudent);
            DeleteStudentCommand = new RelayCommand(ExecuteDeleteStudent, CanExecuteDeleteStudent);
            RefreshCommand = new RelayCommand(async () => await LoadStudentsAsync());
            ExportCommand = new RelayCommand(ExecuteExport);
            ValidateAddressCommand = new RelayCommand(ExecuteValidateAddress, CanExecuteValidateAddress);
        }

        #endregion

        #region Command Handlers

        private void ExecuteAddStudent()
        {
            try
            {
                // TODO: Open StudentForm dialog for adding new student
                Logger.Information("Add student command executed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing add student command");
            }
        }

        private void ExecuteEditStudent()
        {
            try
            {
                if (SelectedStudent != null)
                {
                    // TODO: Open StudentForm dialog for editing selected student
                    Logger.Information("Edit student command executed for student {StudentId}", SelectedStudent.StudentId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing edit student command");
            }
        }

        private bool CanExecuteEditStudent() => HasSelectedStudent;

        private async void ExecuteDeleteStudent()
        {
            try
            {
                if (SelectedStudent != null)
                {
                    // TODO: Show confirmation dialog before deleting
                    await DeleteStudentAsync(SelectedStudent);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing delete student command");
            }
        }

        private bool CanExecuteDeleteStudent() => HasSelectedStudent;

        private void ExecuteExport()
        {
            try
            {
                // TODO: Implement CSV export functionality
                Logger.Information("Export command executed - {StudentCount} students", Students.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing export command");
            }
        }

        private void ExecuteValidateAddress()
        {
            try
            {
                if (SelectedStudent?.HomeAddress == null)
                {
                    StatusMessage = "No address to validate";
                    return;
                }

                var validation = _addressService.ValidateAddress(SelectedStudent.HomeAddress);
                StatusMessage = validation.IsValid
                    ? "Address format is valid"
                    : $"Address validation failed: {validation.Error}";

                Logger.Information("Address validation performed for student {StudentId}: {IsValid}",
                    SelectedStudent.StudentId, validation.IsValid);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error validating address";
                Logger.Error(ex, "Error executing validate address command");
            }
        }

        private bool CanExecuteValidateAddress() => HasSelectedStudent && !string.IsNullOrWhiteSpace(SelectedStudent?.HomeAddress);

        #endregion

        #region Data Operations

        /// <summary>
        /// Load all students from the database
        /// </summary>
        public async Task LoadStudentsAsync()
        {
            try
            {
                IsLoading = true;
                Logger.Information("Loading students from database");

                var students = await _context.Students
                    .OrderBy(s => s.StudentName)
                    .ToListAsync();

                Students.Clear();
                foreach (var student in students)
                {
                    Students.Add(student);
                }

                Logger.Information("Loaded {StudentCount} students", Students.Count);
                OnPropertyChanged(nameof(TotalStudents));
                OnPropertyChanged(nameof(ActiveStudents));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading students");
                // TODO: Show error message to user
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Delete a student from the database
        /// </summary>
        private async Task DeleteStudentAsync(Core.Models.Student student)
        {
            try
            {
                Logger.Information("Deleting student {StudentId} - {StudentName}", student.StudentId, student.StudentName);

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                Students.Remove(student);
                SelectedStudent = null;

                Logger.Information("Successfully deleted student {StudentId}", student.StudentId);
                OnPropertyChanged(nameof(TotalStudents));
                OnPropertyChanged(nameof(ActiveStudents));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error deleting student {StudentId}", student.StudentId);
                // TODO: Show error message to user
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

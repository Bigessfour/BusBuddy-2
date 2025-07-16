using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Models;
using BusBuddy.WPF;
using BusBuddy.WPF.Views;
using Serilog;
using Serilog.Context;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public class StudentListViewModel : BaseViewModel, IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);
        private bool _disposed = false;
        public ObservableCollection<Core.Models.Student> Students { get; } = new();
        public ICommand OpenStudentDetailCommand { get; }

        // Property to track initialization completion
        public Task Initialized { get; private set; }

        public StudentListViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "Construction"))
            {
                Logger.Information("StudentListViewModel constructor starting");

                OpenStudentDetailCommand = new BusBuddy.WPF.RelayCommand(o => OpenStudentDetail(o));

                Logger.Information("StudentListViewModel constructor completed, initiating LoadStudentsAsync");
                Initialized = LoadStudentsAsync();
            }
        }

        private async Task LoadStudentsAsync()
        {
            await LoadDataAsync(async () =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];

                using (LogContext.PushProperty("CorrelationId", correlationId))
                using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
                using (LogContext.PushProperty("OperationType", "LoadStudents"))
                {
                    Logger.Information("LoadStudentsAsync called");

                    await _loadSemaphore.WaitAsync();
                    try
                    {
                        Logger.Information("Acquiring load semaphore");

                        Logger.Information("Getting all students from UnitOfWork");
                        var students = await _unitOfWork.Students.GetAllAsync();

                        if (students != null)
                        {
                            Logger.Information("Retrieved {StudentCount} students", students.Count());

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Logger.Information("Updating UI with student data");
                                Students.Clear();
                                foreach (var student in students)
                                {
                                    Students.Add(student);
                                }
                                Logger.Information("Student data updated in UI");
                            });
                        }
                        else
                        {
                            Logger.Warning("Retrieved null student collection");
                        }
                    }
                    finally
                    {
                        _loadSemaphore.Release();
                        Logger.Information("Released load semaphore");
                    }
                }
            });

            Logger.Information("LoadStudentsAsync completed");
        }

        private void OpenStudentDetail(object? parameter)
        {
            try
            {
                using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
                using (LogContext.PushProperty("OperationType", "OpenStudentDetail"))
                {
                    Logger.Information("OpenStudentDetail called");

                    if (parameter is Core.Models.Student student)
                    {
                        Logger.Information("Opening detail view for student ID: {StudentId}", student.StudentId);

                        var viewModel = new StudentDetailViewModel(student, _unitOfWork);
                        var view = new StudentDetailView
                        {
                            DataContext = viewModel
                        };
                        viewModel.CloseAction = () => view.Close();
                        view.ShowDialog();

                        Logger.Information("Student detail view closed, refreshing student list");
                        _ = LoadStudentsAsync(); // Refresh the list after the dialog is closed
                    }
                    else
                    {
                        Logger.Warning("OpenStudentDetail called with null or invalid parameter");
                    }
                }
            }
            catch (Exception ex)
            {
                using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
                using (LogContext.PushProperty("OperationType", "OpenStudentDetail"))
                using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                {
                    Logger.Error(ex, "Error in OpenStudentDetail: {ErrorMessage}", ex.Message);
                }

                // Consider showing an error message to the user
                MessageBox.Show(
                    $"An error occurred while opening student details: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "Dispose"))
            {
                Logger.Information("Disposing StudentListViewModel");
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
                using (LogContext.PushProperty("OperationType", "Dispose"))
                {
                    Logger.Information("Disposing resources in StudentListViewModel");
                    _loadSemaphore?.Dispose();
                    _disposed = true;
                }
            }
        }
    }
}

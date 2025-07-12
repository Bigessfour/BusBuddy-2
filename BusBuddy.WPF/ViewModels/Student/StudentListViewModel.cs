using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Models;
using BusBuddy.WPF;
using BusBuddy.WPF.Views;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<StudentListViewModel>? _logger;
        private readonly SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);
        private bool _disposed = false;
        public ObservableCollection<Core.Models.Student> Students { get; } = new();
        public ICommand OpenStudentDetailCommand { get; }

        // Property to track initialization completion
        public Task Initialized { get; private set; }

        protected override ILogger? GetLogger() => _logger;

        public StudentListViewModel(IUnitOfWork unitOfWork, ILogger<StudentListViewModel>? logger = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger;

            _logger?.LogInformation("StudentListViewModel constructor starting");

            OpenStudentDetailCommand = new BusBuddy.WPF.RelayCommand(o => OpenStudentDetail(o));

            _logger?.LogInformation("StudentListViewModel constructor completed, initiating LoadStudentsAsync");
            Initialized = LoadStudentsAsync();
        }

        private async Task LoadStudentsAsync()
        {
            _logger?.LogInformation("LoadStudentsAsync called");
            await LoadDataAsync(async () =>
            {
                _logger?.LogInformation("Acquiring load semaphore");
                await _loadSemaphore.WaitAsync();
                try
                {
                    _logger?.LogInformation("Getting all students from UnitOfWork");
                    var students = await _unitOfWork.Students.GetAllAsync();

                    if (students != null)
                    {
                        _logger?.LogInformation("Retrieved {0} students", students.Count());

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _logger?.LogInformation("Updating UI with student data");
                            Students.Clear();
                            foreach (var student in students)
                            {
                                Students.Add(student);
                            }
                            _logger?.LogInformation("Student data updated in UI");
                        });
                    }
                    else
                    {
                        _logger?.LogWarning("Retrieved null student collection");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error loading students: {0}", ex.Message);
                    throw; // Re-throw to be caught by LoadDataAsync
                }
                finally
                {
                    _loadSemaphore.Release();
                    _logger?.LogInformation("Released load semaphore");
                }
            });
            _logger?.LogInformation("LoadStudentsAsync completed");
        }

        private void OpenStudentDetail(object? parameter)
        {
            try
            {
                _logger?.LogInformation("OpenStudentDetail called");
                if (parameter is Core.Models.Student student)
                {
                    _logger?.LogInformation("Opening detail view for student ID: {0}", student.StudentId);
                    var viewModel = new StudentDetailViewModel(student, _unitOfWork);
                    var view = new StudentDetailView
                    {
                        DataContext = viewModel
                    };
                    viewModel.CloseAction = () => view.Close();
                    view.ShowDialog();
                    _logger?.LogInformation("Student detail view closed, refreshing student list");
                    _ = LoadStudentsAsync(); // Refresh the list after the dialog is closed
                }
                else
                {
                    _logger?.LogWarning("OpenStudentDetail called with null or invalid parameter");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in OpenStudentDetail: {0}", ex.Message);
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
            _logger?.LogInformation("Disposing StudentListViewModel");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _logger?.LogInformation("Disposing resources in StudentListViewModel");
                _loadSemaphore?.Dispose();
                _disposed = true;
            }
        }
    }
}

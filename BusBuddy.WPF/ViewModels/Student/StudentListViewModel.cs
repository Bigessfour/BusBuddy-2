using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.WPF.ViewModels.Student;
using BusBuddy.WPF.Views.Student;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BusBuddy.WPF.ViewModels
{
    public partial class StudentListViewModel : BaseViewModel
    {
        private static readonly new ILogger Logger = Log.ForContext<StudentListViewModel>();
        private readonly IStudentService _studentService;
        private readonly IRouteService _routeService;

        public ObservableCollection<BusBuddy.Core.Models.Student> Students { get; } = new();
        public ObservableCollection<Route> Routes { get; } = new();
        public ObservableCollection<string> AvailableGrades { get; } = new();
        public ObservableCollection<object> AvailableRoutes { get; } = new();

        [ObservableProperty]
        private ObservableCollection<BusBuddy.Core.Models.Student> _filteredStudents = new();

        [ObservableProperty]
        private BusBuddy.Core.Models.Student? _selectedStudent;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedGradeFilter = "All Grades";

        [ObservableProperty]
        private string _selectedRouteFilter = "All Routes";

        // Statistics properties
        [ObservableProperty]
        private int _totalStudents = 0;

        [ObservableProperty]
        private int _activeStudents = 0;

        [ObservableProperty]
        private int _studentsWithRoutes = 0;

        [ObservableProperty]
        private int _studentsWithoutRoutes = 0;

        public StudentListViewModel(IStudentService studentService, IRouteService routeService)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));

            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "Construction"))
            {
                Logger.Information("StudentListViewModel constructor starting");

                // Initialize data asynchronously
                _ = InitializeAsync();

                Logger.Information("StudentListViewModel constructor completed");
            }
        }

        private async Task InitializeAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                IsLoading = true;

                // Load routes first for filtering
                await LoadRoutesAsync();

                // Load students
                await LoadStudentsAsync();

                // Apply initial filters
                ApplyFilters();

                // Update statistics
                UpdateStatistics();

                stopwatch.Stop();
                Logger.Information("StudentListViewModel initialization completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during StudentListViewModel initialization");
                stopwatch.Stop();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadRoutesAsync()
        {
            try
            {
                Logger.Information("Loading routes for student list filtering");

                var routes = await _routeService.GetAllActiveRoutesAsync();
                Routes.Clear();
                foreach (var route in routes)
                {
                    Routes.Add(route);
                }

                // Update available routes for filtering
                AvailableRoutes.Clear();
                AvailableRoutes.Add("All Routes");
                foreach (var route in routes)
                {
                    AvailableRoutes.Add(route);
                }

                Logger.Information("Loaded {RouteCount} routes for filtering", routes.Count());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading routes for student list");
            }
        }

        private async Task LoadStudentsAsync()
        {
            try
            {
                Logger.Information("Loading students for student list");

                var students = await _studentService.GetAllStudentsAsync();
                Students.Clear();
                foreach (var student in students)
                {
                    Students.Add(student);
                }

                // Update available grades for filtering
                UpdateFilterOptions();

                Logger.Information("Loaded {StudentCount} students", students.Count());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading students for student list");
            }
        }

        private void UpdateFilterOptions()
        {
            try
            {
                Logger.Debug("Updating filter options for student list");

                // Update grade options
                AvailableGrades.Clear();
                AvailableGrades.Add("All Grades");

                var grades = Students
                    .Where(s => !string.IsNullOrEmpty(s.Grade))
                    .Select(s => s.Grade)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                foreach (var grade in grades)
                {
                    if (!string.IsNullOrEmpty(grade))
                    {
                        AvailableGrades.Add(grade);
                    }
                }

                Logger.Information("Updated filter options: {GradeCount} grades", AvailableGrades.Count - 1);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating filter options");
            }
        }

        private void ApplyFilters()
        {
            var filterStopwatch = Stopwatch.StartNew();

            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "ApplyFilters"))
            using (LogContext.PushProperty("SearchText", SearchText))
            using (LogContext.PushProperty("SelectedGradeFilter", SelectedGradeFilter))
            using (LogContext.PushProperty("SelectedRouteFilter", SelectedRouteFilter))
            {
                try
                {
                    Logger.Information("Applying filters to {TotalStudents} students", Students.Count);

                    var filtered = Students.AsEnumerable();
                    var originalCount = filtered.Count();

                    // Apply text search
                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        var searchLower = SearchText.ToLower();
                        filtered = filtered.Where(s =>
                            s.StudentName.ToLower().Contains(searchLower) ||
                            (s.StudentNumber?.ToLower().Contains(searchLower) ?? false) ||
                            (s.ParentGuardian?.ToLower().Contains(searchLower) ?? false) ||
                            (s.Grade?.ToLower().Contains(searchLower) ?? false) ||
                            (s.School?.ToLower().Contains(searchLower) ?? false) ||
                            (s.AMRoute?.ToLower().Contains(searchLower) ?? false) ||
                            (s.PMRoute?.ToLower().Contains(searchLower) ?? false) ||
                            (s.BusStop?.ToLower().Contains(searchLower) ?? false));

                        var afterTextSearch = filtered.Count();
                        Logger.Debug("Text search '{SearchText}' filtered {OriginalCount} to {AfterTextSearch} students",
                            SearchText, originalCount, afterTextSearch);
                    }

                    // Apply grade filter
                    if (SelectedGradeFilter != "All Grades")
                    {
                        var beforeGradeFilter = filtered.Count();
                        filtered = filtered.Where(s => s.Grade == SelectedGradeFilter);
                        var afterGradeFilter = filtered.Count();

                        Logger.Debug("Grade filter '{SelectedGradeFilter}' filtered {BeforeGradeFilter} to {AfterGradeFilter} students",
                            SelectedGradeFilter, beforeGradeFilter, afterGradeFilter);
                    }

                    // Apply route filter
                    if (SelectedRouteFilter != "All Routes")
                    {
                        var beforeRouteFilter = filtered.Count();
                        filtered = filtered.Where(s => s.AMRoute == SelectedRouteFilter || s.PMRoute == SelectedRouteFilter);
                        var afterRouteFilter = filtered.Count();

                        Logger.Debug("Route filter '{SelectedRouteFilter}' filtered {BeforeRouteFilter} to {AfterRouteFilter} students",
                            SelectedRouteFilter, beforeRouteFilter, afterRouteFilter);
                    }

                    FilteredStudents.Clear();
                    foreach (var student in filtered)
                    {
                        FilteredStudents.Add(student);
                    }

                    filterStopwatch.Stop();
                    Logger.Information("Applied filters successfully in {ElapsedMs}ms: {OriginalCount} → {FilteredCount} students",
                        filterStopwatch.ElapsedMilliseconds, originalCount, FilteredStudents.Count);
                }
                catch (Exception ex)
                {
                    filterStopwatch.Stop();

                    using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                    {
                        Logger.Error(ex, "Error applying filters after {ElapsedMs}ms: {ErrorMessage}",
                            filterStopwatch.ElapsedMilliseconds, ex.Message);
                    }
                }
            }
        }

        private void UpdateStatistics()
        {
            var statisticsStopwatch = Stopwatch.StartNew();

            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "UpdateStatistics"))
            {
                try
                {
                    Logger.Debug("Updating student statistics");

                    var previousStats = new
                    {
                        TotalStudents = this.TotalStudents,
                        ActiveStudents = this.ActiveStudents,
                        StudentsWithRoutes = this.StudentsWithRoutes,
                        StudentsWithoutRoutes = this.StudentsWithoutRoutes
                    };

                    TotalStudents = Students.Count;
                    ActiveStudents = Students.Count(s => s.Active);
                    StudentsWithRoutes = Students.Count(s => !string.IsNullOrEmpty(s.AMRoute) || !string.IsNullOrEmpty(s.PMRoute));
                    StudentsWithoutRoutes = TotalStudents - StudentsWithRoutes;

                    statisticsStopwatch.Stop();

                    using (LogContext.PushProperty("TotalStudents", TotalStudents))
                    using (LogContext.PushProperty("ActiveStudents", ActiveStudents))
                    using (LogContext.PushProperty("StudentsWithRoutes", StudentsWithRoutes))
                    using (LogContext.PushProperty("StudentsWithoutRoutes", StudentsWithoutRoutes))
                    using (LogContext.PushProperty("InactiveStudents", TotalStudents - ActiveStudents))
                    using (LogContext.PushProperty("RouteAssignmentPercentage", TotalStudents > 0 ? (StudentsWithRoutes * 100.0 / TotalStudents) : 0))
                    {
                        Logger.Information("Updated student statistics in {ElapsedMs}ms: {TotalStudents} total, {ActiveStudents} active, {StudentsWithRoutes} with routes, {StudentsWithoutRoutes} without routes",
                            statisticsStopwatch.ElapsedMilliseconds, TotalStudents, ActiveStudents, StudentsWithRoutes, StudentsWithoutRoutes);
                    }

                    // Log significant changes
                    if (previousStats.TotalStudents != TotalStudents)
                    {
                        Logger.Information("Total student count changed from {PreviousTotal} to {NewTotal}",
                            previousStats.TotalStudents, TotalStudents);
                    }

                    if (previousStats.ActiveStudents != ActiveStudents)
                    {
                        Logger.Information("Active student count changed from {PreviousActive} to {NewActive}",
                            previousStats.ActiveStudents, ActiveStudents);
                    }
                }
                catch (Exception ex)
                {
                    statisticsStopwatch.Stop();

                    using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                    {
                        Logger.Error(ex, "Error updating statistics after {ElapsedMs}ms: {ErrorMessage}",
                            statisticsStopwatch.ElapsedMilliseconds, ex.Message);
                    }
                }
            }
        }
        partial void OnSearchTextChanged(string value)
        {
            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "PropertyChange"))
            using (LogContext.PushProperty("PropertyName", nameof(SearchText)))
            using (LogContext.PushProperty("NewValue", value))
            {
                Logger.Debug("SearchText changed to '{NewValue}', applying filters", value);
                ApplyFilters();
            }
        }

        partial void OnSelectedGradeFilterChanged(string value)
        {
            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "PropertyChange"))
            using (LogContext.PushProperty("PropertyName", nameof(SelectedGradeFilter)))
            using (LogContext.PushProperty("NewValue", value))
            {
                Logger.Debug("SelectedGradeFilter changed to '{NewValue}', applying filters", value);
                ApplyFilters();
            }
        }

        partial void OnSelectedRouteFilterChanged(string value)
        {
            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "PropertyChange"))
            using (LogContext.PushProperty("PropertyName", nameof(SelectedRouteFilter)))
            using (LogContext.PushProperty("NewValue", value))
            {
                Logger.Debug("SelectedRouteFilter changed to '{NewValue}', applying filters", value);
                ApplyFilters();
            }
        }

        partial void OnSelectedStudentChanged(BusBuddy.Core.Models.Student? value)
        {
            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "PropertyChange"))
            using (LogContext.PushProperty("PropertyName", nameof(SelectedStudent)))
            using (LogContext.PushProperty("StudentId", value?.StudentId))
            using (LogContext.PushProperty("StudentName", value?.StudentName))
            {
                if (value != null)
                {
                    Logger.Debug("SelectedStudent changed to {StudentName} (ID: {StudentId})",
                        value.StudentName, value.StudentId);
                }
                else
                {
                    Logger.Debug("SelectedStudent cleared (set to null)");
                }
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                IsLoading = true;
                await LoadStudentsAsync();
                await LoadRoutesAsync();
                ApplyFilters();
                UpdateStatistics();
                Logger.Information("Student list refreshed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error refreshing student list");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    FileName = $"StudentList_{DateTime.Now:yyyyMMdd}",
                    DefaultExt = ".csv",
                    Filter = "CSV Files (*.csv)|*.csv"
                };

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    Logger.Information("Exporting {StudentCount} students to {FilePath}", FilteredStudents.Count, dialog.FileName);

                    var csv = new StringBuilder();
                    csv.AppendLine("Student Name,Student Number,Grade,School,AM Route,PM Route,Bus Stop,Parent/Guardian,Home Phone,Active,Special Needs");

                    foreach (var student in FilteredStudents)
                    {
                        csv.AppendLine($"\"{student.StudentName}\"," +
                                      $"\"{student.StudentNumber ?? ""}\"," +
                                      $"\"{student.Grade ?? ""}\"," +
                                      $"\"{student.School ?? ""}\"," +
                                      $"\"{student.AMRoute ?? ""}\"," +
                                      $"\"{student.PMRoute ?? ""}\"," +
                                      $"\"{student.BusStop ?? ""}\"," +
                                      $"\"{student.ParentGuardian ?? ""}\"," +
                                      $"\"{student.HomePhone ?? ""}\"," +
                                      $"{student.Active}," +
                                      $"{student.SpecialNeeds}");
                    }

                    await File.WriteAllTextAsync(dialog.FileName, csv.ToString());

                    MessageBox.Show($"Successfully exported {FilteredStudents.Count} students to {dialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error exporting student list");
                MessageBox.Show($"Error exporting student list: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Print()
        {
            try
            {
                Logger.Information("Printing student list with {StudentCount} students", FilteredStudents.Count);

                // TODO: Implement actual printing functionality
                MessageBox.Show("Print functionality will be implemented in a future update.",
                    "Print", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error printing student list");
                MessageBox.Show($"Error printing student list: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ViewDetails()
        {
            if (SelectedStudent == null)
            {
                Logger.Warning("ViewDetails command executed but no student is selected");
                return;
            }

            var commandStopwatch = Stopwatch.StartNew();

            using (LogContext.PushProperty("ViewModelType", nameof(StudentListViewModel)))
            using (LogContext.PushProperty("OperationType", "Command"))
            using (LogContext.PushProperty("CommandName", nameof(ViewDetails)))
            using (LogContext.PushProperty("StudentId", SelectedStudent.StudentId))
            using (LogContext.PushProperty("StudentName", SelectedStudent.StudentName))
            {
                try
                {
                    Logger.Information("Executing ViewDetails command for student {StudentName} (ID: {StudentId})",
                        SelectedStudent.StudentName, SelectedStudent.StudentId);

                    // TODO: Implement proper student detail view
                    var studentDetails = $"Student Details:\n\n" +
                                       $"Name: {SelectedStudent.StudentName}\n" +
                                       $"Number: {SelectedStudent.StudentNumber}\n" +
                                       $"Grade: {SelectedStudent.Grade}\n" +
                                       $"School: {SelectedStudent.School}\n" +
                                       $"AM Route: {SelectedStudent.AMRoute}\n" +
                                       $"PM Route: {SelectedStudent.PMRoute}\n" +
                                       $"Parent: {SelectedStudent.ParentGuardian}\n" +
                                       $"Phone: {SelectedStudent.HomePhone}";

                    MessageBox.Show(studentDetails, "Student Details", MessageBoxButton.OK, MessageBoxImage.Information);

                    commandStopwatch.Stop();
                    Logger.Information("ViewDetails command completed successfully in {ElapsedMs}ms",
                        commandStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    commandStopwatch.Stop();

                    using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                    {
                        Logger.Error(ex, "Error executing ViewDetails command after {ElapsedMs}ms: {ErrorMessage}",
                            commandStopwatch.ElapsedMilliseconds, ex.Message);
                    }

                    MessageBox.Show($"Error viewing student details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void EditStudent()
        {
            if (SelectedStudent == null) return;

            try
            {
                Logger.Information("Editing student {StudentId}", SelectedStudent.StudentId);

                // TODO: Implement proper student edit dialog
                MessageBox.Show($"Edit functionality for student '{SelectedStudent.StudentName}' will be implemented in a future update.",
                    "Edit Student", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error editing student");
                MessageBox.Show($"Error editing student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddStudent()
        {
            try
            {
                Logger.Information("Adding new student");

                // TODO: Implement proper student add dialog
                MessageBox.Show("Add student functionality will be implemented in a future update.",
                    "Add Student", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error adding student");
                MessageBox.Show($"Error adding student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AssignRoute()
        {
            if (SelectedStudent == null) return;

            try
            {
                Logger.Information("Assigning route to student {StudentId}", SelectedStudent.StudentId);

                // TODO: Implement proper route assignment dialog
                MessageBox.Show($"Route assignment for student '{SelectedStudent.StudentName}' will be implemented in a future update.",
                    "Assign Route", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error assigning route");
                MessageBox.Show($"Error assigning route: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void CopyToClipboard()
        {
            if (SelectedStudent == null) return;

            try
            {
                var studentInfo = $"Name: {SelectedStudent.StudentName}\n" +
                                 $"Number: {SelectedStudent.StudentNumber}\n" +
                                 $"Grade: {SelectedStudent.Grade}\n" +
                                 $"School: {SelectedStudent.School}\n" +
                                 $"AM Route: {SelectedStudent.AMRoute}\n" +
                                 $"PM Route: {SelectedStudent.PMRoute}\n" +
                                 $"Parent: {SelectedStudent.ParentGuardian}\n" +
                                 $"Phone: {SelectedStudent.HomePhone}";

                Clipboard.SetText(studentInfo);
                Logger.Information("Student information copied to clipboard");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error copying to clipboard");
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SendEmail()
        {
            if (SelectedStudent == null) return;

            try
            {
                Logger.Information("Sending email for student {StudentId}", SelectedStudent.StudentId);

                // TODO: Implement email functionality
                MessageBox.Show("Email functionality will be implemented in a future update.",
                    "Email", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error sending email");
                MessageBox.Show($"Error sending email: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void PrintStudent()
        {
            if (SelectedStudent == null) return;

            try
            {
                Logger.Information("Printing student information for {StudentId}", SelectedStudent.StudentId);

                // TODO: Implement student-specific printing
                MessageBox.Show("Student print functionality will be implemented in a future update.",
                    "Print Student", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error printing student");
                MessageBox.Show($"Error printing student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void GenerateReport()
        {
            if (SelectedStudent == null) return;

            try
            {
                Logger.Information("Generating report for student {StudentId}", SelectedStudent.StudentId);

                // TODO: Implement report generation
                MessageBox.Show("Report generation functionality will be implemented in a future update.",
                    "Generate Report", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error generating report");
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void BulkActions()
        {
            try
            {
                Logger.Information("Opening bulk actions dialog");

                // TODO: Implement bulk actions functionality
                MessageBox.Show("Bulk actions functionality will be implemented in a future update.",
                    "Bulk Actions", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error opening bulk actions");
                MessageBox.Show($"Error opening bulk actions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Reports()
        {
            try
            {
                Logger.Information("Opening reports dialog");

                // TODO: Implement reports functionality
                MessageBox.Show("Reports functionality will be implemented in a future update.",
                    "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error opening reports");
                MessageBox.Show($"Error opening reports: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Compatibility commands for XAML binding
        public RelayCommand OpenStudentDetailCommand => new RelayCommand(parameter => ViewDetails());
        public RelayCommand EditCommand => new RelayCommand(parameter => EditStudent());
        // PrintCommand is auto-generated by [RelayCommand] attribute on Print method
    }
}

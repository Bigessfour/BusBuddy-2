using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.WPF.Utilities;
using BusBuddy.WPF.ViewModels.Student;
using BusBuddy.WPF.Controls;
using StudentViews = BusBuddy.WPF.Views.Student;
using Serilog;
using Serilog.Context;

// Disable async method without await operator warnings
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace BusBuddy.WPF.ViewModels
{
    public partial class StudentManagementViewModel : BaseInDevelopmentViewModel
    {
        private static readonly new ILogger Logger = Log.ForContext<StudentManagementViewModel>();

        // ...existing fields, properties, and constructor...

        [RelayCommand]
        private async Task ImportStudentsAsync()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var lines = System.IO.File.ReadAllLines(dialog.FileName);
                    if (lines.Length < 2)
                    {
                        System.Windows.MessageBox.Show("CSV file is empty or missing data.", "Import Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var fields = ParseCsvLine(lines[i]);
                        if (fields.Length < 26) continue;
                        var student = new BusBuddy.Core.Models.Student
                        {
                            StudentName = fields[0],
                            StudentNumber = fields[1] ?? string.Empty,
                            Grade = fields[2] ?? string.Empty,
                            School = fields[3] ?? string.Empty,
                            DateOfBirth = DateTime.TryParse(fields[4], out var dob) ? dob : (DateTime?)null,
                            Gender = fields[5] ?? string.Empty,
                            Active = fields[6].Equals("Yes", StringComparison.OrdinalIgnoreCase),
                            SpecialNeeds = fields[7].Equals("Yes", StringComparison.OrdinalIgnoreCase),
                            HomeAddress = fields[8] ?? string.Empty,
                            City = fields[9] ?? string.Empty,
                            State = fields[10] ?? string.Empty,
                            Zip = fields[11] ?? string.Empty,
                            BusStop = fields[12] ?? string.Empty,
                            ParentGuardian = fields[13] ?? string.Empty,
                            HomePhone = fields[14] ?? string.Empty,
                            EmergencyPhone = fields[15] ?? string.Empty,
                            AlternativeContact = fields[16] ?? string.Empty,
                            AlternativePhone = fields[17] ?? string.Empty,
                            AMRoute = fields[18] ?? string.Empty,
                            PMRoute = fields[19] ?? string.Empty,
                            TransportationNotes = fields[20] ?? string.Empty,
                            MedicalNotes = fields[21] ?? string.Empty,
                            Allergies = fields[22] ?? string.Empty,
                            Medications = fields[23] ?? string.Empty,
                            DoctorName = fields[24] ?? string.Empty,
                            DoctorPhone = fields[25] ?? string.Empty
                        };
                        var created = await _studentService.AddStudentAsync(student);
                        Students.Add(created);
                    }
                    ApplyFilters();
                    System.Windows.MessageBox.Show("Import complete.", "Import", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error importing students: {ex.Message}", "Import Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void ExportStudents()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = "students.csv"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new System.IO.StreamWriter(dialog.FileName))
                    {
                        writer.WriteLine("StudentName,StudentNumber,Grade,School,DateOfBirth,Gender,Active,SpecialNeeds,HomeAddress,City,State,Zip,BusStop,ParentGuardian,HomePhone,EmergencyPhone,AlternativeContact,AlternativePhone,AMRoute,PMRoute,TransportationNotes,MedicalNotes,Allergies,Medications,DoctorName,DoctorPhone");
                        foreach (var s in Students)
                        {
                            string[] fields = new string[]
                            {
                                s.StudentName,
                                s.StudentNumber ?? string.Empty,
                                s.Grade ?? string.Empty,
                                s.School ?? string.Empty,
                                s.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
                                s.Gender ?? string.Empty,
                                s.Active ? "Yes" : "No",
                                s.SpecialNeeds ? "Yes" : "No",
                                s.HomeAddress ?? string.Empty,
                                s.City ?? string.Empty,
                                s.State ?? string.Empty,
                                s.Zip ?? string.Empty,
                                s.BusStop ?? string.Empty,
                                s.ParentGuardian ?? string.Empty,
                                s.HomePhone ?? string.Empty,
                                s.EmergencyPhone ?? string.Empty,
                                s.AlternativeContact ?? string.Empty,
                                s.AlternativePhone ?? string.Empty,
                                s.AMRoute ?? string.Empty,
                                s.PMRoute ?? string.Empty,
                                s.TransportationNotes ?? string.Empty,
                                s.MedicalNotes ?? string.Empty,
                                s.Allergies ?? string.Empty,
                                s.Medications ?? string.Empty,
                                s.DoctorName ?? string.Empty,
                                s.DoctorPhone ?? string.Empty
                            };
                            var csvLine = string.Join(",", fields.Select(f => $"\"{f.Replace("\"", "\"\"")}\""));
                            writer.WriteLine(csvLine);
                        }
                    }

                    System.Windows.MessageBox.Show("Students exported successfully.", "Export Complete", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error exporting students: {ex.Message}", "Export Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new System.Collections.Generic.List<string>();
            bool inQuotes = false;
            var value = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        value.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(value.ToString());
                    value.Clear();
                }
                else
                {
                    value.Append(c);
                }
            }
            result.Add(value.ToString());
            return result.ToArray();
        }

        private readonly IStudentService _studentService;
        private readonly IBusService _busService;
        private readonly IRouteService _routeService;

        public ObservableCollection<BusBuddy.Core.Models.Student> Students { get; } = new();
        public ObservableCollection<Bus> Buses { get; private set; } = new();
        public ObservableCollection<Route> Routes { get; private set; } = new();

        [ObservableProperty]
        private BusBuddy.Core.Models.Student? _selectedStudent;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedStatusFilter = "All Students";

        [ObservableProperty]
        private string _selectedGradeFilter = "All Grades";

        [ObservableProperty]
        private string _selectedSchoolFilter = "All Schools";

        [ObservableProperty]
        private ObservableCollection<BusBuddy.Core.Models.Student> _filteredStudents = new();

        [ObservableProperty]
        private SearchCriteria? _advancedSearchCriteria = null;

        // Available filter options
        public ObservableCollection<string> AvailableGrades { get; } = new();
        public ObservableCollection<string> AvailableSchools { get; } = new();

        // Statistics properties
        [ObservableProperty]
        private int _totalStudents = 0;

        [ObservableProperty]
        private int _activeStudents = 0;

        [ObservableProperty]
        private int _inactiveStudents = 0;

        [ObservableProperty]
        private int _studentsWithRoutes = 0;

        [ObservableProperty]
        private int _studentsWithoutRoutes = 0;

        [ObservableProperty]
        private ObservableCollection<GradeData> _gradeDistribution = new();

        [ObservableProperty]
        private ObservableCollection<StatusData> _statusDistribution = new();

        // Statistics visibility toggle
        [ObservableProperty]
        private bool _showStatistics = false;

        [RelayCommand]
        private void ToggleStatistics()
        {
            ShowStatistics = !ShowStatistics;
            if (ShowStatistics)
            {
                UpdateStatistics();
            }
        }

        public StudentManagementViewModel(
            IStudentService studentService,
            IBusService busService,
            IRouteService routeService) : base()
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));

#if DEBUG
            DebugConfig.WriteStudent("ENTER StudentManagementViewModel constructor");
#endif

            // Load data asynchronously in sequence to avoid DbContext concurrency issues
            _ = InitializeAsync();

            // Set as in-development - Set to false when development is complete
            IsInDevelopment = false;

#if DEBUG
            DebugConfig.WriteStudent("EXIT StudentManagementViewModel constructor");
#endif
        }

        private async Task InitializeAsync()
        {
#if DEBUG
            var stopwatch = Stopwatch.StartNew();
            DebugConfig.WriteStudent("ENTER InitializeAsync");
#endif
            try
            {
                IsBusy = true;

                // Load data sequentially to avoid concurrent DbContext access
                await LoadDataAsync();
                await LoadStudentsAsync();

                IsBusy = false;

#if DEBUG
                stopwatch.Stop();
                DebugConfig.WritePerformance($"InitializeAsync completed in {stopwatch.ElapsedMilliseconds}ms");
#endif
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during ViewModel initialization");
#if DEBUG
                DebugConfig.WriteStudent($"ERROR in InitializeAsync: {ex.Message}");
#endif
                IsBusy = false;
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using (LogContext.PushProperty("OperationType", "DataLoad"))
                {
                    Logger.Information("UI Loading buses and routes for student management");

                    var buses = await _busService.GetAllBusesAsync();
                    Buses = new ObservableCollection<Bus>(buses);
                    OnPropertyChanged(nameof(Buses));

                    var routes = await _routeService.GetAllActiveRoutesAsync();
                    Routes = new ObservableCollection<Route>(routes);
                    OnPropertyChanged(nameof(Routes));

                    Logger.Information("UI Loaded {BusCount} buses and {RouteCount} routes", Buses.Count, Routes.Count);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading buses and routes");
            }
        }

        private async Task LoadStudentsAsync()
        {
#if DEBUG
            DebugConfig.WriteStudent("ENTER LoadStudentsAsync");
#endif
            try
            {
                using (LogContext.PushProperty("OperationType", "StudentLoad"))
                {
                    Logger.Information("UI Loading students for management view");

                    Students.Clear();
                    var students = await _studentService.GetAllStudentsAsync();
                    foreach (var s in students)
                        Students.Add(s);

                    // Update available filter options
                    UpdateFilterOptions();

                    // Apply current filter
                    ApplyFilters();

                    // Update statistics
                    UpdateStatistics();

                    Logger.Information("UI Loaded {StudentCount} students", Students.Count);
                }
#if DEBUG
                DebugConfig.WriteStudent($"DATA: Loaded {Students.Count} students");
#endif
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading students");
#if DEBUG
                DebugConfig.WriteStudent($"ERROR in LoadStudentsAsync: {ex.Message}");
#endif
            }
        }

        private void UpdateFilterOptions()
        {
            try
            {
                using (LogContext.PushProperty("OperationType", "FilterUpdate"))
                {
                    Logger.Debug("UI Updating filter options for student management");

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

                    // Update school options
                    AvailableSchools.Clear();
                    AvailableSchools.Add("All Schools");

                    var schools = Students
                        .Where(s => !string.IsNullOrEmpty(s.School))
                        .Select(s => s.School)
                        .Distinct()
                        .OrderBy(s => s)
                        .ToList();

                    foreach (var school in schools)
                    {
                        if (!string.IsNullOrEmpty(school))
                        {
                            AvailableSchools.Add(school);
                        }
                    }

                    Logger.Information("UI Updated filter options: {GradeCount} grades, {SchoolCount} schools",
                        AvailableGrades.Count - 1, AvailableSchools.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating filter options");
            }
        }

        private void ApplyFilters()
        {
#if DEBUG
            DebugConfig.WriteStudent($"ENTER ApplyFilters: SearchText='{SearchText}', Status='{SelectedStatusFilter}', Grade='{SelectedGradeFilter}', School='{SelectedSchoolFilter}'");
#endif
            var filtered = Students.AsEnumerable();

            // If advanced search is active, use those criteria
            if (AdvancedSearchCriteria != null)
            {
                filtered = ApplyAdvancedSearchCriteria(filtered);
            }
            else
            {
                // Apply simple text search
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
                        (s.BusStop?.ToLower().Contains(searchLower) ?? false) ||
                        (s.HomeAddress?.ToLower().Contains(searchLower) ?? false) ||
                        (s.City?.ToLower().Contains(searchLower) ?? false));
                }

                // Apply status filter
                filtered = SelectedStatusFilter switch
                {
                    "Active Students" => filtered.Where(s => s.Active),
                    "Inactive Students" => filtered.Where(s => !s.Active),
                    _ => filtered // "All Students"
                };

                // Apply grade filter
                if (SelectedGradeFilter != "All Grades")
                {
                    filtered = filtered.Where(s => s.Grade == SelectedGradeFilter);
                }

                // Apply school filter
                if (SelectedSchoolFilter != "All Schools")
                {
                    filtered = filtered.Where(s => s.School == SelectedSchoolFilter);
                }
            }

            FilteredStudents.Clear();
            foreach (var student in filtered)
            {
                FilteredStudents.Add(student);
            }

#if DEBUG
            DebugConfig.WriteStudent($"FILTER: Applied filters, {FilteredStudents.Count} students match criteria");
#endif
        }
        private IEnumerable<BusBuddy.Core.Models.Student> ApplyAdvancedSearchCriteria(IEnumerable<BusBuddy.Core.Models.Student> students)
        {
            var filtered = students;
            var criteria = AdvancedSearchCriteria;

            if (criteria == null)
                return filtered;

            // Apply name filter
            if (!string.IsNullOrWhiteSpace(criteria.StudentName))
            {
                var searchName = criteria.StudentName.ToLower();
                filtered = filtered.Where(s => s.StudentName.ToLower().Contains(searchName));
            }

            // Apply student number filter
            if (!string.IsNullOrWhiteSpace(criteria.StudentNumber))
            {
                var searchNumber = criteria.StudentNumber.ToLower();
                filtered = filtered.Where(s => s.StudentNumber != null && s.StudentNumber.ToLower().Contains(searchNumber));
            }

            // Apply grade filter
            if (!string.IsNullOrEmpty(criteria.Grade))
            {
                filtered = filtered.Where(s => s.Grade == criteria.Grade);
            }

            // Apply school filter
            if (!string.IsNullOrEmpty(criteria.School))
            {
                filtered = filtered.Where(s => s.School == criteria.School);
            }

            // Apply active status filter
            filtered = criteria.ActiveStatus switch
            {
                FilterStatus.Active => filtered.Where(s => s.Active),
                FilterStatus.Inactive => filtered.Where(s => !s.Active),
                _ => filtered // All
            };

            // Apply special needs filter
            filtered = criteria.SpecialNeedsStatus switch
            {
                FilterStatus.Yes => filtered.Where(s => s.SpecialNeeds),
                FilterStatus.No => filtered.Where(s => !s.SpecialNeeds),
                _ => filtered // All
            };

            // Apply AM route filter
            if (!string.IsNullOrEmpty(criteria.AMRoute))
            {
                filtered = filtered.Where(s => s.AMRoute == criteria.AMRoute);
            }

            // Apply PM route filter
            if (!string.IsNullOrEmpty(criteria.PMRoute))
            {
                filtered = filtered.Where(s => s.PMRoute == criteria.PMRoute);
            }

            // Apply bus stop filter
            if (!string.IsNullOrWhiteSpace(criteria.BusStop))
            {
                var searchStop = criteria.BusStop.ToLower();
                filtered = filtered.Where(s => s.BusStop != null && s.BusStop.ToLower().Contains(searchStop));
            }

            // Apply route assignment filter
            filtered = criteria.RouteAssignmentStatus switch
            {
                FilterStatus.WithRoute => filtered.Where(s => !string.IsNullOrEmpty(s.AMRoute) || !string.IsNullOrEmpty(s.PMRoute)),
                FilterStatus.WithoutRoute => filtered.Where(s => string.IsNullOrEmpty(s.AMRoute) && string.IsNullOrEmpty(s.PMRoute)),
                _ => filtered // All
            };

            // Apply city filter
            if (!string.IsNullOrWhiteSpace(criteria.City))
            {
                var searchCity = criteria.City.ToLower();
                filtered = filtered.Where(s => s.City != null && s.City.ToLower().Contains(searchCity));
            }

            // Apply zip filter
            if (!string.IsNullOrWhiteSpace(criteria.Zip))
            {
                var searchZip = criteria.Zip.ToLower();
                filtered = filtered.Where(s => s.Zip != null && s.Zip.ToLower().Contains(searchZip));
            }

            return filtered;
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedStatusFilterChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedGradeFilterChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedSchoolFilterChanged(string value)
        {
            ApplyFilters();
        }

        [RelayCommand]
        private async Task AdvancedSearchAsync()
        {
#if DEBUG
            DebugConfig.WriteStudent("ENTER AdvancedSearchAsync");
#endif
            try
            {
                IsBusy = true;

                // Create and show the advanced search dialog
                var dialog = new StudentViews.AdvancedSearchDialog();
                var viewModel = new AdvancedSearchViewModel(dialog, Routes, Students);
                dialog.DataContext = viewModel;
                dialog.Owner = System.Windows.Application.Current.MainWindow;

                var result = dialog.ShowDialog();
                if (result == true && viewModel.DialogResult)
                {
                    // Apply advanced search criteria
                    AdvancedSearchCriteria = viewModel.SearchCriteria;

                    // Reset simple filters
                    SearchText = string.Empty;
                    SelectedStatusFilter = "All Students";
                    SelectedGradeFilter = "All Grades";
                    SelectedSchoolFilter = "All Schools";

                    // Apply the filters
                    ApplyFilters();

                    Logger.Information("Applied advanced search criteria");
#if DEBUG
                    DebugConfig.WriteStudent("DATA: Applied advanced search criteria");
#endif
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in advanced search");
#if DEBUG
                DebugConfig.WriteStudent($"ERROR in AdvancedSearchAsync: {ex.Message}");
#endif
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task ClearAdvancedSearch()
        {
            AdvancedSearchCriteria = null;
            await Task.Delay(1); // Just to satisfy the async requirement
            ApplyFilters();

            Logger.Information("Cleared advanced search criteria");
#if DEBUG
            DebugConfig.WriteStudent("DATA: Cleared advanced search criteria");
#endif
        }

        [RelayCommand]
        private async Task AddStudentAsync()
        {
#if DEBUG
            DebugConfig.WriteStudent("ENTER AddStudentAsync");
#endif
            try
            {
                IsBusy = true;

                // Create and show the student edit dialog
                var dialog = new StudentViews.StudentEditDialog();
                var viewModel = new StudentEditViewModel(dialog);
                viewModel.LoadStudent(); // Load as new student
                dialog.DataContext = viewModel;
                dialog.Owner = System.Windows.Application.Current.MainWindow;

                var result = dialog.ShowDialog();
                if (result == true && viewModel.DialogResult)
                {
                    var newStudent = viewModel.SaveToStudent();
                    var created = await _studentService.AddStudentAsync(newStudent);
                    Students.Add(created);
                    ApplyFilters();
                    SelectedStudent = created;

                    Logger.Information("Added new student with ID {StudentId}", created.StudentId);
#if DEBUG
                    DebugConfig.WriteStudent($"DATA: Created student #{created.StudentId} '{created.StudentName}'");
#endif
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error adding student");
#if DEBUG
                DebugConfig.WriteStudent($"ERROR in AddStudentAsync: {ex.Message}");
#endif
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task EditStudentAsync()
        {
            if (SelectedStudent == null)
            {
#if DEBUG
                DebugConfig.WriteStudent("CANCEL EditStudentAsync: No student selected");
#endif
                return;
            }

#if DEBUG
            DebugConfig.WriteStudent($"ENTER EditStudentAsync: Student #{SelectedStudent.StudentId}");
#endif
            try
            {
                IsBusy = true;

                // Create and show the student edit dialog
                var dialog = new StudentViews.StudentEditDialog();
                var viewModel = new StudentEditViewModel(dialog);
                viewModel.LoadStudent(SelectedStudent); // Load existing student
                dialog.DataContext = viewModel;
                dialog.Owner = System.Windows.Application.Current.MainWindow;

                var result = dialog.ShowDialog();
                if (result == true && viewModel.DialogResult)
                {
                    var updatedStudent = viewModel.SaveToStudent();
                    await _studentService.UpdateStudentAsync(updatedStudent);

                    Logger.Information("Updated student with ID {StudentId}", updatedStudent.StudentId);
#if DEBUG
                    DebugConfig.WriteStudent($"DATA: Updated student #{updatedStudent.StudentId} '{updatedStudent.StudentName}'");
#endif

                    // Refresh the student in our collection
                    var index = Students.IndexOf(SelectedStudent);
                    if (index >= 0)
                    {
                        Students[index] = updatedStudent;
                        SelectedStudent = updatedStudent;
                    }

                    // Refresh filtered view
                    ApplyFilters();
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating student {StudentId}", SelectedStudent?.StudentId);
#if DEBUG
                DebugConfig.WriteStudent($"ERROR in EditStudentAsync: {ex.Message}");
#endif
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteStudentAsync()
        {
            if (SelectedStudent == null)
            {
#if DEBUG
                DebugConfig.WriteStudent("CANCEL DeleteStudentAsync: No student selected");
#endif
                return;
            }

            var studentId = SelectedStudent.StudentId;
            var studentName = SelectedStudent.StudentName;

#if DEBUG
            DebugConfig.WriteStudent($"ENTER DeleteStudentAsync: Student #{studentId} '{studentName}'");
#endif

            try
            {
                IsBusy = true;

                await _studentService.DeleteStudentAsync(SelectedStudent.StudentId);
                Students.Remove(SelectedStudent);
                ApplyFilters();
                SelectedStudent = null;

                Logger.Information("Deleted student with ID {StudentId}", studentId);
#if DEBUG
                DebugConfig.WriteStudent($"DATA: Deleted student #{studentId} '{studentName}'");
#endif

                IsBusy = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error deleting student {StudentId}", studentId);
#if DEBUG
                DebugConfig.WriteStudent($"ERROR in DeleteStudentAsync: {ex.Message}");
#endif
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
#if DEBUG
            DebugConfig.WriteStudent("ENTER RefreshAsync");
#endif
            try
            {
                IsBusy = true;

                await LoadStudentsAsync();
                await LoadDataAsync();
                UpdateStatistics();

#if DEBUG
                DebugConfig.WriteStudent("DATA: Refresh completed successfully");
#endif
                IsBusy = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error refreshing data");
#if DEBUG
                DebugConfig.WriteStudent($"ERROR in RefreshAsync: {ex.Message}");
#endif
                IsBusy = false;
            }
        }

        // Calculate and update all statistics
        private void UpdateStatistics()
        {
            try
            {
                // Basic statistics
                TotalStudents = Students.Count;
                ActiveStudents = Students.Count(s => s.Active);
                InactiveStudents = Students.Count(s => !s.Active);
                StudentsWithRoutes = Students.Count(s => !string.IsNullOrEmpty(s.AMRoute) || !string.IsNullOrEmpty(s.PMRoute));
                StudentsWithoutRoutes = TotalStudents - StudentsWithRoutes;

                // Grade distribution
                var gradeGroups = Students
                    .Where(s => !string.IsNullOrEmpty(s.Grade))
                    .GroupBy(s => s.Grade)
                    .Select(g => new GradeData { Grade = g.Key ?? "Unknown", Count = g.Count() })
                    .OrderBy(g => g.Grade)
                    .ToList();

                GradeDistribution.Clear();
                foreach (var grade in gradeGroups)
                {
                    GradeDistribution.Add(grade);
                }

                // Status distribution
                StatusDistribution.Clear();
                StatusDistribution.Add(new StatusData { Status = "Active", Count = ActiveStudents });
                StatusDistribution.Add(new StatusData { Status = "Inactive", Count = InactiveStudents });

                Logger.Information("Student statistics updated: {TotalCount} total, {ActiveCount} active",
                    TotalStudents, ActiveStudents);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating student statistics");
            }
        }

        [RelayCommand]
        private async Task AssignRouteAsync()
        {
            if (SelectedStudent == null)
            {
#if DEBUG
                DebugConfig.WriteStudent("CANCEL AssignRouteAsync: No student selected");
#endif
                return;
            }

#if DEBUG
            DebugConfig.WriteStudent($"ENTER AssignRouteAsync: Student #{SelectedStudent.StudentId} '{SelectedStudent.StudentName}'");
#endif

            try
            {
                IsBusy = true;

                // Open the assign route dialog
                var dialog = new StudentViews.AssignRouteDialog();
                var viewModel = new ViewModels.Student.AssignRouteViewModel(
                    dialog,
                    Routes,
                    Routes.FirstOrDefault(r => r.RouteName == SelectedStudent.AMRoute),
                    Routes.FirstOrDefault(r => r.RouteName == SelectedStudent.PMRoute));
                dialog.DataContext = viewModel;
                dialog.Owner = System.Windows.Application.Current.MainWindow;

                var result = dialog.ShowDialog();
                if (result == true && viewModel.DialogResult)
                {
                    SelectedStudent.AMRoute = viewModel.SelectedAmRoute?.RouteName;
                    SelectedStudent.PMRoute = viewModel.SelectedPmRoute?.RouteName;
                    await _studentService.UpdateStudentAsync(SelectedStudent);
                    ApplyFilters();
                    Logger.Information("Assigned routes to student {StudentId}", SelectedStudent.StudentId);
#if DEBUG
                    DebugConfig.WriteStudent($"ROUTE: Assigned AM='{SelectedStudent.AMRoute}', PM='{SelectedStudent.PMRoute}' to student #{SelectedStudent.StudentId}");
#endif
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error assigning route to student {StudentId}", SelectedStudent?.StudentId);
#if DEBUG
                DebugConfig.WriteStudent($"ERROR in AssignRouteAsync: {ex.Message}");
#endif
                IsBusy = false;
            }
        }

        // Compatibility properties for XAML binding
        // These help maintain compatibility with existing XAML
        public System.Windows.Input.ICommand DeleteCommand => DeleteStudentCommand;
        public System.Windows.Input.ICommand AddCommand => AddStudentCommand;
        public System.Windows.Input.ICommand EditCommand => EditStudentCommand;

        // Compatibility properties for XAML data binding
        public System.Collections.IEnumerable AvailableBuses => Buses;
        public System.Collections.IEnumerable AvailableRoutes => Routes;
    }
}

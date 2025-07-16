using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace BusBuddy.WPF.ViewModels.Student;

/// <summary>
/// ViewModel for managing student-schedule assignments
/// Handles assignment, removal, and viewing of student schedules
/// </summary>
public partial class StudentScheduleManagementViewModel : ObservableObject
{
    private static readonly ILogger Logger = Log.ForContext<StudentScheduleManagementViewModel>();
    private readonly IStudentScheduleService _studentScheduleService;
    private readonly IScheduleService _scheduleService;
    private readonly IActivityScheduleService _activityScheduleService;

    [ObservableProperty]
    private BusBuddy.Core.Models.Student? _selectedStudent;

    [ObservableProperty]
    private ObservableCollection<StudentSchedule> _studentSchedules = new();

    [ObservableProperty]
    private ObservableCollection<StudentSchedule> _studentActivitySchedules = new();

    [ObservableProperty]
    private ObservableCollection<BusBuddy.Core.Models.Schedule> _availableSchedules = new();

    [ObservableProperty]
    private ObservableCollection<ActivitySchedule> _availableActivitySchedules = new();

    [ObservableProperty]
    private StudentSchedule? _selectedSchedule;

    [ObservableProperty]
    private BusBuddy.Core.Models.Schedule? _selectedAvailableSchedule;

    [ObservableProperty]
    private ActivitySchedule? _selectedAvailableActivitySchedule;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _assignmentType = "Regular";

    [ObservableProperty]
    private string? _pickupLocation;

    [ObservableProperty]
    private string? _dropoffLocation;

    [ObservableProperty]
    private string? _notes;

    public StudentScheduleManagementViewModel(
        IStudentScheduleService studentScheduleService,
        IScheduleService scheduleService,
        IActivityScheduleService activityScheduleService)
    {
        _studentScheduleService = studentScheduleService;
        _scheduleService = scheduleService;
        _activityScheduleService = activityScheduleService;
    }

    partial void OnSelectedStudentChanged(BusBuddy.Core.Models.Student? value)
    {
        if (value != null)
        {
            _ = LoadStudentSchedulesAsync();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    [RelayCommand]
    private async Task LoadStudentSchedulesAsync()
    {
        if (SelectedStudent == null) return;

        try
        {
            IsLoading = true;
            Logger.Information("Loading schedules for student {StudentId}", SelectedStudent.StudentId);

            // Load student's regular schedules
            var regularSchedules = await _studentScheduleService.GetSchedulesForStudentAsync(SelectedStudent.StudentId);
            StudentSchedules.Clear();
            foreach (var schedule in regularSchedules)
            {
                StudentSchedules.Add(schedule);
            }

            // Load student's activity schedules
            var activitySchedules = await _studentScheduleService.GetActivitySchedulesForStudentAsync(SelectedStudent.StudentId);
            StudentActivitySchedules.Clear();
            foreach (var schedule in activitySchedules)
            {
                StudentActivitySchedules.Add(schedule);
            }

            // Load available schedules
            var allSchedules = await _scheduleService.GetSchedulesAsync();
            AvailableSchedules.Clear();
            foreach (var schedule in allSchedules)
            {
                AvailableSchedules.Add(schedule);
            }

            // Load available activity schedules
            var allActivitySchedules = await _activityScheduleService.GetAllActivitySchedulesAsync();
            AvailableActivitySchedules.Clear();
            foreach (var activitySchedule in allActivitySchedules)
            {
                AvailableActivitySchedules.Add(activitySchedule);
            }

            ApplyFilters();

            Logger.Information("Loaded {RegularCount} regular schedules and {ActivityCount} activity schedules for student {StudentId}",
                StudentSchedules.Count, StudentActivitySchedules.Count, SelectedStudent.StudentId);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading schedules for student {StudentId}", SelectedStudent?.StudentId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AssignToScheduleAsync()
    {
        if (SelectedStudent == null || SelectedAvailableSchedule == null) return;

        try
        {
            IsLoading = true;
            Logger.Information("Assigning student {StudentId} to schedule {ScheduleId}",
                SelectedStudent.StudentId, SelectedAvailableSchedule.ScheduleId);

            await _studentScheduleService.AssignStudentToScheduleAsync(
                SelectedStudent.StudentId,
                SelectedAvailableSchedule.ScheduleId,
                PickupLocation,
                DropoffLocation,
                Notes);

            // Refresh the student schedules
            await LoadStudentSchedulesAsync();

            // Clear form
            SelectedAvailableSchedule = null;
            PickupLocation = null;
            DropoffLocation = null;
            Notes = null;

            Logger.Information("Successfully assigned student {StudentId} to schedule {ScheduleId}",
                SelectedStudent.StudentId, SelectedAvailableSchedule?.ScheduleId);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error assigning student {StudentId} to schedule {ScheduleId}",
                SelectedStudent?.StudentId, SelectedAvailableSchedule?.ScheduleId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AssignToActivityScheduleAsync()
    {
        if (SelectedStudent == null || SelectedAvailableActivitySchedule == null) return;

        try
        {
            IsLoading = true;
            Logger.Information("Assigning student {StudentId} to activity schedule {ActivityScheduleId}",
                SelectedStudent.StudentId, SelectedAvailableActivitySchedule.ActivityScheduleId);

            await _studentScheduleService.AssignStudentToActivityScheduleAsync(
                SelectedStudent.StudentId,
                SelectedAvailableActivitySchedule.ActivityScheduleId,
                PickupLocation,
                DropoffLocation,
                Notes);

            // Refresh the student schedules
            await LoadStudentSchedulesAsync();

            // Clear form
            SelectedAvailableActivitySchedule = null;
            PickupLocation = null;
            DropoffLocation = null;
            Notes = null;

            Logger.Information("Successfully assigned student {StudentId} to activity schedule {ActivityScheduleId}",
                SelectedStudent.StudentId, SelectedAvailableActivitySchedule?.ActivityScheduleId);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error assigning student {StudentId} to activity schedule {ActivityScheduleId}",
                SelectedStudent?.StudentId, SelectedAvailableActivitySchedule?.ActivityScheduleId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveFromScheduleAsync()
    {
        if (SelectedStudent == null || SelectedSchedule == null) return;

        try
        {
            IsLoading = true;
            Logger.Information("Removing student {StudentId} from schedule", SelectedStudent.StudentId);

            bool removed = false;
            if (SelectedSchedule.ScheduleId.HasValue && SelectedSchedule.ScheduleId > 0)
            {
                removed = await _studentScheduleService.RemoveStudentFromScheduleAsync(
                    SelectedStudent.StudentId, SelectedSchedule.ScheduleId.Value);
            }
            else if (SelectedSchedule.ActivityScheduleId.HasValue)
            {
                removed = await _studentScheduleService.RemoveStudentFromActivityScheduleAsync(
                    SelectedStudent.StudentId, SelectedSchedule.ActivityScheduleId.Value);
            }

            if (removed)
            {
                await LoadStudentSchedulesAsync();
                Logger.Information("Successfully removed student {StudentId} from schedule", SelectedStudent.StudentId);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error removing student {StudentId} from schedule", SelectedStudent?.StudentId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ConfirmAttendanceAsync()
    {
        if (SelectedSchedule == null) return;

        try
        {
            IsLoading = true;
            Logger.Information("Confirming attendance for student schedule {StudentScheduleId}",
                SelectedSchedule.StudentScheduleId);

            await _studentScheduleService.ConfirmStudentAttendanceAsync(
                SelectedSchedule.StudentScheduleId, true);

            // Refresh the student schedules
            await LoadStudentSchedulesAsync();

            Logger.Information("Successfully confirmed attendance for student schedule {StudentScheduleId}",
                SelectedSchedule.StudentScheduleId);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error confirming attendance for student schedule {StudentScheduleId}",
                SelectedSchedule?.StudentScheduleId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateScheduleAssignmentAsync()
    {
        if (SelectedSchedule == null) return;

        try
        {
            IsLoading = true;
            Logger.Information("Updating student schedule assignment {StudentScheduleId}",
                SelectedSchedule.StudentScheduleId);

            await _studentScheduleService.UpdateStudentAssignmentAsync(SelectedSchedule);

            // Refresh the student schedules
            await LoadStudentSchedulesAsync();

            Logger.Information("Successfully updated student schedule assignment {StudentScheduleId}",
                SelectedSchedule.StudentScheduleId);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error updating student schedule assignment {StudentScheduleId}",
                SelectedSchedule?.StudentScheduleId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilters()
    {
        // Filter functionality can be added here if needed
        // For now, we'll keep it simple
    }

    public async Task InitializeAsync(BusBuddy.Core.Models.Student student)
    {
        SelectedStudent = student;
        await LoadStudentSchedulesAsync();
    }
}

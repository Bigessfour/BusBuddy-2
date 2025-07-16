using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.WPF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Serilog;

namespace BusBuddy.WPF.ViewModels
{
    public class StudentDetailViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = Log.ForContext<StudentDetailViewModel>();
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentScheduleService _studentScheduleService;
        private readonly IScheduleService _scheduleService;
        private readonly BusBuddy.Core.Models.Student _originalStudent;

        public BusBuddy.Core.Models.Student Student { get; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand AssignToScheduleCommand { get; set; }
        public ICommand RemoveFromScheduleCommand { get; set; }
        public ICommand ViewScheduleDetailsCommand { get; set; }
        public Action? CloseAction { get; set; }

        // Schedule management properties
        public ObservableCollection<StudentSchedule> StudentSchedules { get; } = new();
        public ObservableCollection<StudentSchedule> StudentActivitySchedules { get; } = new();
        public ObservableCollection<BusBuddy.Core.Models.Schedule> AvailableSchedules { get; } = new();
        public ObservableCollection<ActivitySchedule> AvailableActivitySchedules { get; } = new();

        private StudentSchedule? _selectedSchedule;
        public StudentSchedule? SelectedSchedule
        {
            get => _selectedSchedule;
            set
            {
                _selectedSchedule = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoadingSchedules;
        public bool IsLoadingSchedules
        {
            get => _isLoadingSchedules;
            set
            {
                _isLoadingSchedules = value;
                OnPropertyChanged();
            }
        }

        public StudentDetailViewModel(BusBuddy.Core.Models.Student student, IUnitOfWork unitOfWork, IStudentScheduleService studentScheduleService, IScheduleService scheduleService)
        {
            _unitOfWork = unitOfWork;
            _studentScheduleService = studentScheduleService;
            _scheduleService = scheduleService;
            Student = student;
            // Create a copy for cancellation
            _originalStudent = new BusBuddy.Core.Models.Student
            {
                StudentId = student.StudentId,
                StudentName = student.StudentName,
                StudentNumber = student.StudentNumber,
                Grade = student.Grade,
                School = student.School,
                HomeAddress = student.HomeAddress,
                City = student.City,
                State = student.State,
                Zip = student.Zip,
                HomePhone = student.HomePhone,
                ParentGuardian = student.ParentGuardian,
                EmergencyPhone = student.EmergencyPhone,
                MedicalNotes = student.MedicalNotes,
                TransportationNotes = student.TransportationNotes,
                Active = student.Active,
                EnrollmentDate = student.EnrollmentDate,
                AMRoute = student.AMRoute,
                PMRoute = student.PMRoute,
                BusStop = student.BusStop,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                PickupAddress = student.PickupAddress,
                DropoffAddress = student.DropoffAddress,
                SpecialNeeds = student.SpecialNeeds,
                SpecialAccommodations = student.SpecialAccommodations,
                Allergies = student.Allergies,
                Medications = student.Medications,
                DoctorName = student.DoctorName,
                DoctorPhone = student.DoctorPhone,
                AlternativeContact = student.AlternativeContact,
                AlternativePhone = student.AlternativePhone,
                PhotoPermission = student.PhotoPermission,
                FieldTripPermission = student.FieldTripPermission,
            };

            SaveCommand = new BusBuddy.WPF.RelayCommand(o => SaveChanges(o));
            CancelCommand = new BusBuddy.WPF.RelayCommand(o => CancelChanges(o));
            AssignToScheduleCommand = new BusBuddy.WPF.RelayCommand(async o => await AssignToScheduleAsync());
            RemoveFromScheduleCommand = new BusBuddy.WPF.RelayCommand(async o => await RemoveFromScheduleAsync());
            ViewScheduleDetailsCommand = new BusBuddy.WPF.RelayCommand(o => ViewScheduleDetails());

            // Load student schedules
            _ = LoadStudentSchedulesAsync();
        }

        private void CancelChanges(object? obj)
        {
            // Restore original values
            Student.StudentName = _originalStudent.StudentName;
            Student.StudentNumber = _originalStudent.StudentNumber;
            Student.Grade = _originalStudent.Grade;
            Student.School = _originalStudent.School;
            Student.HomeAddress = _originalStudent.HomeAddress;
            Student.City = _originalStudent.City;
            Student.State = _originalStudent.State;
            Student.Zip = _originalStudent.Zip;
            Student.HomePhone = _originalStudent.HomePhone;
            Student.ParentGuardian = _originalStudent.ParentGuardian;
            Student.EmergencyPhone = _originalStudent.EmergencyPhone;
            Student.MedicalNotes = _originalStudent.MedicalNotes;
            Student.TransportationNotes = _originalStudent.TransportationNotes;
            Student.Active = _originalStudent.Active;
            Student.EnrollmentDate = _originalStudent.EnrollmentDate;
            Student.AMRoute = _originalStudent.AMRoute;
            Student.PMRoute = _originalStudent.PMRoute;
            Student.BusStop = _originalStudent.BusStop;
            Student.DateOfBirth = _originalStudent.DateOfBirth;
            Student.Gender = _originalStudent.Gender;
            Student.PickupAddress = _originalStudent.PickupAddress;
            Student.DropoffAddress = _originalStudent.DropoffAddress;
            Student.SpecialNeeds = _originalStudent.SpecialNeeds;
            Student.SpecialAccommodations = _originalStudent.SpecialAccommodations;
            Student.Allergies = _originalStudent.Allergies;
            Student.Medications = _originalStudent.Medications;
            Student.DoctorName = _originalStudent.DoctorName;
            Student.DoctorPhone = _originalStudent.DoctorPhone;
            Student.AlternativeContact = _originalStudent.AlternativeContact;
            Student.AlternativePhone = _originalStudent.AlternativePhone;
            Student.PhotoPermission = _originalStudent.PhotoPermission;
            Student.FieldTripPermission = _originalStudent.FieldTripPermission;

            OnPropertyChanged(nameof(Student));
            CloseAction?.Invoke();
        }

        private async void SaveChanges(object? obj)
        {
            if (Student.StudentId == 0)
            {
                await _unitOfWork.Students.AddAsync(Student);
            }
            else
            {
                _unitOfWork.Students.Update(Student);
            }
            await _unitOfWork.SaveChangesAsync();
            CloseAction?.Invoke();
        }

        private async Task LoadStudentSchedulesAsync()
        {
            try
            {
                IsLoadingSchedules = true;
                Logger.Information("Loading schedules for student {StudentId}", Student.StudentId);

                if (Student.StudentId > 0)
                {
                    // Load regular schedules
                    var regularSchedules = await _studentScheduleService.GetSchedulesForStudentAsync(Student.StudentId);
                    StudentSchedules.Clear();
                    foreach (var schedule in regularSchedules)
                    {
                        StudentSchedules.Add(schedule);
                    }

                    // Load activity schedules
                    var activitySchedules = await _studentScheduleService.GetActivitySchedulesForStudentAsync(Student.StudentId);
                    StudentActivitySchedules.Clear();
                    foreach (var schedule in activitySchedules)
                    {
                        StudentActivitySchedules.Add(schedule);
                    }

                    // Load available schedules for assignment
                    var allSchedules = await _scheduleService.GetSchedulesAsync();
                    AvailableSchedules.Clear();
                    foreach (var schedule in allSchedules)
                    {
                        AvailableSchedules.Add(schedule);
                    }

                    Logger.Information("Loaded {RegularCount} regular schedules and {ActivityCount} activity schedules for student {StudentId}",
                        StudentSchedules.Count, StudentActivitySchedules.Count, Student.StudentId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading schedules for student {StudentId}", Student.StudentId);
            }
            finally
            {
                IsLoadingSchedules = false;
            }
        }

        private async Task AssignToScheduleAsync()
        {
            await Task.Delay(1); // Just to satisfy the async requirement
            try
            {
                Logger.Information("Assigning student {StudentId} to schedule", Student.StudentId);
                // This would typically open a dialog to select schedules
                // For now, it's a placeholder for future implementation
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error assigning student {StudentId} to schedule", Student.StudentId);
            }
        }

        private async Task RemoveFromScheduleAsync()
        {
            try
            {
                if (SelectedSchedule == null) return;

                Logger.Information("Removing student {StudentId} from schedule", Student.StudentId);

                bool removed = false;
                if (SelectedSchedule.ScheduleId.HasValue && SelectedSchedule.ScheduleId > 0)
                {
                    removed = await _studentScheduleService.RemoveStudentFromScheduleAsync(Student.StudentId, SelectedSchedule.ScheduleId.Value);
                }
                else if (SelectedSchedule.ActivityScheduleId.HasValue)
                {
                    removed = await _studentScheduleService.RemoveStudentFromActivityScheduleAsync(Student.StudentId, SelectedSchedule.ActivityScheduleId.Value);
                }

                if (removed)
                {
                    await LoadStudentSchedulesAsync();
                    Logger.Information("Successfully removed student {StudentId} from schedule", Student.StudentId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error removing student {StudentId} from schedule", Student.StudentId);
            }
        }

        private void ViewScheduleDetails()
        {
            if (SelectedSchedule == null) return;

            Logger.Information("Viewing schedule details for student {StudentId}", Student.StudentId);
            // This would typically open a dialog to view schedule details
            // For now, it's a placeholder for future implementation
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

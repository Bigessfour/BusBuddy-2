using BusBuddy.Core.Models;
using BusBuddy.WPF.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Windows;

namespace BusBuddy.WPF.ViewModels.Student
{
    public partial class StudentEditViewModel : ObservableObject
    {
        private readonly ILogger<StudentEditViewModel>? _logger;
        private readonly Window _dialog;

        [ObservableProperty]
        private string _dialogTitle = "Edit Student";

        [ObservableProperty]
        private string _studentName = string.Empty;

        [ObservableProperty]
        private string? _studentNumber;

        [ObservableProperty]
        private string? _grade;

        [ObservableProperty]
        private string? _school;

        [ObservableProperty]
        private DateTime? _dateOfBirth;

        [ObservableProperty]
        private string? _gender;

        [ObservableProperty]
        private bool _active = true;

        [ObservableProperty]
        private bool _specialNeeds = false;

        [ObservableProperty]
        private string? _homeAddress;

        [ObservableProperty]
        private string? _city;

        [ObservableProperty]
        private string? _state;

        [ObservableProperty]
        private string? _zip;

        [ObservableProperty]
        private string? _busStop;

        [ObservableProperty]
        private string? _parentGuardian;

        [ObservableProperty]
        private string? _homePhone;

        [ObservableProperty]
        private string? _emergencyPhone;

        [ObservableProperty]
        private string? _alternativeContact;

        [ObservableProperty]
        private string? _alternativePhone;

        [ObservableProperty]
        private string? _amRoute;

        [ObservableProperty]
        private string? _pmRoute;

        [ObservableProperty]
        private string? _transportationNotes;

        [ObservableProperty]
        private string? _medicalNotes;

        [ObservableProperty]
        private string? _allergies;

        [ObservableProperty]
        private string? _medications;

        [ObservableProperty]
        private string? _doctorName;

        [ObservableProperty]
        private string? _doctorPhone;

        public Core.Models.Student? EditingStudent { get; private set; }
        public bool DialogResult { get; private set; } = false;

        public StudentEditViewModel(Window dialog, ILogger<StudentEditViewModel>? logger = null)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _logger = logger;

#if DEBUG
            DebugConfig.WriteStudent("ENTER StudentEditViewModel constructor");
#endif
        }

        public void LoadStudent(Core.Models.Student? student = null)
        {
#if DEBUG
            DebugConfig.WriteStudent($"ENTER LoadStudent: student={(student?.StudentId.ToString() ?? "new")}");
#endif

            EditingStudent = student;

            if (student == null)
            {
                // New student
                DialogTitle = "Add New Student";
                ResetToDefaults();
#if DEBUG
                DebugConfig.WriteStudent("DATA: Loading new student form");
#endif
            }
            else
            {
                // Edit existing student
                DialogTitle = $"Edit Student - {student.StudentName}";
                LoadFromStudent(student);
#if DEBUG
                DebugConfig.WriteStudent($"DATA: Loading existing student #{student.StudentId} '{student.StudentName}'");
#endif
            }
        }

        private void ResetToDefaults()
        {
            StudentName = "New Student";
            Grade = "K";
            Active = true;
            SpecialNeeds = false;
            // All other properties will be null/default
        }

        private void LoadFromStudent(Core.Models.Student student)
        {
            StudentName = student.StudentName;
            StudentNumber = student.StudentNumber;
            Grade = student.Grade;
            School = student.School;
            DateOfBirth = student.DateOfBirth;
            Gender = student.Gender;
            Active = student.Active;
            SpecialNeeds = student.SpecialNeeds;
            HomeAddress = student.HomeAddress;
            City = student.City;
            State = student.State;
            Zip = student.Zip;
            BusStop = student.BusStop;
            ParentGuardian = student.ParentGuardian;
            HomePhone = student.HomePhone;
            EmergencyPhone = student.EmergencyPhone;
            AlternativeContact = student.AlternativeContact;
            AlternativePhone = student.AlternativePhone;
            AmRoute = student.AMRoute;
            PmRoute = student.PMRoute;
            TransportationNotes = student.TransportationNotes;
            MedicalNotes = student.MedicalNotes;
            Allergies = student.Allergies;
            Medications = student.Medications;
            DoctorName = student.DoctorName;
            DoctorPhone = student.DoctorPhone;
        }

        public Core.Models.Student SaveToStudent()
        {
#if DEBUG
            DebugConfig.WriteStudent("ENTER SaveToStudent");
#endif

            var student = EditingStudent ?? new Core.Models.Student();

            student.StudentName = StudentName;
            student.StudentNumber = StudentNumber;
            student.Grade = Grade;
            student.School = School;
            student.DateOfBirth = DateOfBirth;
            student.Gender = Gender;
            student.Active = Active;
            student.SpecialNeeds = SpecialNeeds;
            student.HomeAddress = HomeAddress;
            student.City = City;
            student.State = State;
            student.Zip = Zip;
            student.BusStop = BusStop;
            student.ParentGuardian = ParentGuardian;
            student.HomePhone = HomePhone;
            student.EmergencyPhone = EmergencyPhone;
            student.AlternativeContact = AlternativeContact;
            student.AlternativePhone = AlternativePhone;
            student.AMRoute = AmRoute;
            student.PMRoute = PmRoute;
            student.TransportationNotes = TransportationNotes;
            student.MedicalNotes = MedicalNotes;
            student.Allergies = Allergies;
            student.Medications = Medications;
            student.DoctorName = DoctorName;
            student.DoctorPhone = DoctorPhone;

            // Set audit fields
            if (EditingStudent == null)
            {
                student.CreatedDate = DateTime.UtcNow;
                student.CreatedBy = Environment.UserName;
            }
            else
            {
                student.UpdatedDate = DateTime.UtcNow;
                student.UpdatedBy = Environment.UserName;
            }

#if DEBUG
            DebugConfig.WriteStudent($"DATA: Saved student data for '{student.StudentName}'");
#endif

            return student;
        }

        [RelayCommand]
        private void Save()
        {
#if DEBUG
            DebugConfig.WriteStudent("ENTER Save command");
#endif

            try
            {

                // Basic validation
                string error = string.Empty;
                if (string.IsNullOrWhiteSpace(StudentName))
                    error += "- Student name is required.\n";
                if (string.IsNullOrWhiteSpace(Grade))
                    error += "- Grade is required.\n";
                if (string.IsNullOrWhiteSpace(School))
                    error += "- School is required.\n";

                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show($"Please correct the following:\n\n{error}", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DialogResult = true;
                _dialog.DialogResult = true;

#if DEBUG
                DebugConfig.WriteStudent("Student edit dialog saved successfully");
#endif
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving student in dialog");
#if DEBUG
                DebugConfig.WriteStudent($"ERROR in Save: {ex.Message}");
#endif
                MessageBox.Show($"Error saving student: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
#if DEBUG
            DebugConfig.WriteStudent("CANCEL Student edit dialog");
#endif
            DialogResult = false;
            _dialog.DialogResult = false;
        }
    }
}

using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Models;
using BusBuddy.WPF;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public class StudentDetailViewModel : INotifyPropertyChanged
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BusBuddy.Core.Models.Student _originalStudent;

        public BusBuddy.Core.Models.Student Student { get; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public Action? CloseAction { get; set; }

        public StudentDetailViewModel(BusBuddy.Core.Models.Student student, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

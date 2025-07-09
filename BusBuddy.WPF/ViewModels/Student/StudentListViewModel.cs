using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Models;
using BusBuddy.WPF.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public class StudentListViewModel : INotifyPropertyChanged
    {
        private readonly IUnitOfWork _unitOfWork;
        public ObservableCollection<Student> Students { get; } = new();
        public ICommand OpenStudentDetailCommand { get; }

        public StudentListViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            OpenStudentDetailCommand = new RelayCommand(OpenStudentDetail);
            LoadStudents();
        }

        private async void LoadStudents()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            Students.Clear();
            foreach (var student in students)
            {
                Students.Add(student);
            }
        }

        private void OpenStudentDetail(object? parameter)
        {
            if (parameter is Student student)
            {
                var viewModel = new StudentDetailViewModel(student, _unitOfWork);
                var view = new StudentDetailView
                {
                    DataContext = viewModel
                };
                viewModel.CloseAction = () => view.Close();
                view.ShowDialog();
                LoadStudents(); // Refresh the list after the dialog is closed
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

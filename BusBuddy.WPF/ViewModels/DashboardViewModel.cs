using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusBuddy.WPF.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private int _totalStudents;
        public int TotalStudents
        {
            get => _totalStudents;
            set { _totalStudents = value; OnPropertyChanged(); }
        }

        private double _totalRouteMiles;
        public double TotalRouteMiles
        {
            get => _totalRouteMiles;
            set { _totalRouteMiles = value; OnPropertyChanged(); }
        }

        // Add more properties as needed for dashboard analytics

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

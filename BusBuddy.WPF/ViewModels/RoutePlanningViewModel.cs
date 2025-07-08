using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public class RoutePlanningViewModel : INotifyPropertyChanged
    {
        private bool _showDistrictBoundaries;
        public bool ShowDistrictBoundaries
        {
            get => _showDistrictBoundaries;
            set { _showDistrictBoundaries = value; OnPropertyChanged(); }
        }

        private bool _showRouteStops;
        public bool ShowRouteStops
        {
            get => _showRouteStops;
            set { _showRouteStops = value; OnPropertyChanged(); }
        }

        private bool _showStudentAddresses;
        public bool ShowStudentAddresses
        {
            get => _showStudentAddresses;
            set { _showStudentAddresses = value; OnPropertyChanged(); }
        }

        public ICommand? OptimizeRoutesCommand { get; set; }
        public ICommand? AddStopCommand { get; set; }
        public ICommand? ExportToScheduleCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

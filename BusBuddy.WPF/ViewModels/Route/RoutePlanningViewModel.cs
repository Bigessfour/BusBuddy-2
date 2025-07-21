using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.WPF.Services;

namespace BusBuddy.WPF.ViewModels
{
    public class RoutePlanningViewModel : INotifyPropertyChanged
    {

        private readonly IRoutePopulationScaffold _routePopulationScaffold;

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

        public ICommand OptimizeRoutesCommand { get; }
        public ICommand ExportAssignmentsCommand { get; }

        public ObservableCollection<Route> Routes { get; set; } = new();
        public ObservableCollection<Route> OptimizedRoutes { get; set; } = new();

        public RoutePlanningViewModel(IRoutePopulationScaffold routePopulationScaffold)
        {
            _routePopulationScaffold = routePopulationScaffold;
            OptimizeRoutesCommand = new global::BusBuddy.WPF.RelayCommand(_ => OptimizeRoutesAsync().GetAwaiter().GetResult());
            ExportAssignmentsCommand = new global::BusBuddy.WPF.RelayCommand(_ => ExportAssignmentsToCsv());
            _ = LoadRoutesAsync();
        }

        private async Task LoadRoutesAsync()
        {
            var routes = await _routePopulationScaffold.GetOptimizedRoutesAsync();
            Routes.Clear();
            foreach (var route in routes)
            {
                Routes.Add(route);
            }
            OptimizedRoutes.Clear();
            foreach (var route in routes)
            {
                OptimizedRoutes.Add(route);
            }
            OnPropertyChanged(nameof(Routes));
            OnPropertyChanged(nameof(OptimizedRoutes));
        }

        private async Task OptimizeRoutesAsync()
        {
            var optimized = await _routePopulationScaffold.GetOptimizedRoutesAsync();
            OptimizedRoutes.Clear();
            foreach (var route in optimized)
            {
                OptimizedRoutes.Add(route);
            }
            OnPropertyChanged(nameof(OptimizedRoutes));
        }

        private void ExportAssignmentsToCsv()
        {
            var csvLines = new System.Collections.Generic.List<string> { "Route Name,AM/PM,Bus Number,Driver Name" };
            foreach (var route in OptimizedRoutes)
            {
                csvLines.Add($"{route.RouteName},AM,{route.BusNumber},{route.DriverName}");
                csvLines.Add($"{route.RouteName},PM,{route.BusNumber},{route.DriverName}");
            }
            var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RouteAssignments.csv");
            System.IO.File.WriteAllLines(filePath, csvLines);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

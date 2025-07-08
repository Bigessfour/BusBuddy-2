using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using BusBuddy.Core.Models;

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


        public ICommand OptimizeRoutesCommand { get; }
        public ICommand ExportAssignmentsCommand { get; }

        public ObservableCollection<Route> Routes { get; set; } = new();
        public ObservableCollection<Route> OptimizedRoutes { get; set; } = new();

        public RoutePlanningViewModel()
        {
            // Load demo routes
            LoadRoutes();
            OptimizeRoutesCommand = new RelayCommand(OptimizeRoutes);
            ExportAssignmentsCommand = new RelayCommand(ExportAssignmentsToCsv);
        }

        private void LoadRoutes()
        {
            Routes.Clear();
            Routes.Add(new BusBuddy.Core.Models.Route { RouteId = 1, RouteName = "East Route", Distance = 12.5m, AMDriverId = 1, AMVehicleId = 1, PMDriverId = 2, PMVehicleId = 2, BusNumber = "101", DriverName = "Alice Smith" });
            Routes.Add(new BusBuddy.Core.Models.Route { RouteId = 2, RouteName = "West Route", Distance = 8.2m, AMDriverId = 2, AMVehicleId = 2, PMDriverId = 1, PMVehicleId = 1, BusNumber = "102", DriverName = "Bob Jones" });
            Routes.Add(new BusBuddy.Core.Models.Route { RouteId = 3, RouteName = "North Route", Distance = 15.0m, AMDriverId = 3, AMVehicleId = 3, PMDriverId = 3, PMVehicleId = 3, BusNumber = "103", DriverName = "Carol Lee" });
        }

        private void OptimizeRoutes()
        {
            // Simple heuristic: sort by Distance ascending
            var sorted = Routes.OrderBy(r => r.Distance ?? 0).ToList();
            OptimizedRoutes.Clear();
            foreach (var route in sorted)
                OptimizedRoutes.Add(route);
            OnPropertyChanged(nameof(OptimizedRoutes));
        }

        private void ExportAssignmentsToCsv()
        {
            var csvLines = new List<string> { "Route Name,AM/PM,Bus Number,Driver Name" };
            foreach (var route in Routes)
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

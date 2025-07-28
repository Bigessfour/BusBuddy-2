using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BusBuddy.Core.Models;

namespace BusBuddy.WPF.ViewModels.Route
{
    /// <summary>
    /// Phase 2 Route Management ViewModel
    /// Enhanced route planning and management functionality
    /// </summary>
    public class RouteManagementViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BusBuddy.Core.Models.Route> Routes { get; set; } = new();

        private BusBuddy.Core.Models.Route? _selectedRoute;
        public BusBuddy.Core.Models.Route? SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                _selectedRoute = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRouteSelected));
            }
        }

        public bool IsRouteSelected => SelectedRoute != null;

        public RouteManagementViewModel()
        {
            LoadRoutes();
        }

        private void LoadRoutes()
        {
            // TODO: Load routes from service in Phase 2
            // For now, add sample data
            Routes.Add(new BusBuddy.Core.Models.Route
            {
                RouteId = 1,
                RouteName = "Route 1 - Elementary",
                Description = "Elementary school morning route"
            });
            Routes.Add(new BusBuddy.Core.Models.Route
            {
                RouteId = 2,
                RouteName = "Route 2 - High School",
                Description = "High school afternoon route"
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

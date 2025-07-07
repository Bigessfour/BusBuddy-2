using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusBuddy.WPF.ViewModels
{
    public class DashboardAnalyticsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<RouteUsage> RouteUsages { get; set; } = new();
        public ObservableCollection<ActivityTypeStat> ActivityTypes { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RouteUsage
    {
        public string RouteName { get; set; }
        public double Miles { get; set; }
    }

    public class ActivityTypeStat
    {
        public string Type { get; set; }
        public int Count { get; set; }
    }
}

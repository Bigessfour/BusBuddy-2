using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// Phase 1 Dashboard ViewModel - Simple and functional
    /// Displays basic metrics and provides navigation to core views
    /// </summary>
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Basic dashboard data
        private int _totalDrivers;
        private int _totalVehicles;
        private int _totalActivities;
        private int _activeDrivers;

        public int TotalDrivers
        {
            get => _totalDrivers;
            set
            {
                _totalDrivers = value;
                OnPropertyChanged();
            }
        }

        public int TotalVehicles
        {
            get => _totalVehicles;
            set
            {
                _totalVehicles = value;
                OnPropertyChanged();
            }
        }

        public int TotalActivities
        {
            get => _totalActivities;
            set
            {
                _totalActivities = value;
                OnPropertyChanged();
            }
        }

        public int ActiveDrivers
        {
            get => _activeDrivers;
            set
            {
                _activeDrivers = value;
                OnPropertyChanged();
            }
        }

        // Recent activities for dashboard display
        public ObservableCollection<BusBuddy.Core.Models.Activity> RecentActivities { get; set; } = new();

        public DashboardViewModel()
        {
            _ = LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                using var context = new BusBuddyContext();

                // Load basic counts
                TotalDrivers = await context.Drivers.CountAsync();
                TotalVehicles = await context.Vehicles.CountAsync();
                TotalActivities = await context.Activities.CountAsync();
                ActiveDrivers = await context.Drivers.CountAsync(d => d.Status == "Active");

                // Load recent activities
                var recentActivities = await context.Activities
                    .OrderByDescending(a => a.Date)
                    .Take(5)
                    .ToListAsync();

                RecentActivities.Clear();
                foreach (var activity in recentActivities)
                {
                    RecentActivities.Add(activity);
                }
            }
            catch (Exception ex)
            {
                // Phase 1: Simple error handling
                MessageBox.Show($"Error loading dashboard data: {ex.Message}");
            }
        }
    }
}

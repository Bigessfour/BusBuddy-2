using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;

namespace BusBuddy.WPF.ViewModels
{
    public class FuelManagementViewModel : INotifyPropertyChanged
    {
        private readonly IFuelService _fuelService;
        public ObservableCollection<Fuel> FuelRecords { get; set; } = new();
        public ObservableCollection<FuelTrendPoint> FuelTrends { get; set; } = new();

        public FuelManagementViewModel(IFuelService fuelService)
        {
            _fuelService = fuelService;
            _ = LoadFuelRecordsAsync();
        }

        private async Task LoadFuelRecordsAsync()
        {
            FuelRecords.Clear();
            var records = await _fuelService.GetAllFuelRecordsAsync();
            foreach (var record in records)
                FuelRecords.Add(record);
            CalculateTrends();
        }

        private void CalculateTrends()
        {
            FuelTrends.Clear();
            // Simple trend: average MPG per month
            var grouped = new System.Linq.EnumerableQuery<Fuel>(FuelRecords)
                .GroupBy(f => new { f.FuelDate.Year, f.FuelDate.Month })
                .Select(g => new FuelTrendPoint
                {
                    Period = new DateTime(g.Key.Year, g.Key.Month, 1),
                    AvgMPG = g.Average(f => f.Gallons.HasValue && f.Gallons > 0 ? (f.VehicleOdometerReading / (double)f.Gallons.Value) : 0)
                });
            foreach (var pt in grouped)
                FuelTrends.Add(pt);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FuelTrendPoint
    {
        public DateTime Period { get; set; }
        public double AvgMPG { get; set; }
    }
}

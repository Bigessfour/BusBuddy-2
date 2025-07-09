using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace BusBuddy.WPF.ViewModels
{
    public class FuelManagementViewModel : BaseViewModel
    {
        private readonly IFuelService _fuelService;

        private ObservableCollection<Fuel> _fuelRecords = new();
        public ObservableCollection<Fuel> FuelRecords
        {
            get => _fuelRecords;
            set => SetProperty(ref _fuelRecords, value);
        }

        private ObservableCollection<FuelTrendPoint> _fuelTrends = new();
        public ObservableCollection<FuelTrendPoint> FuelTrends
        {
            get => _fuelTrends;
            set => SetProperty(ref _fuelTrends, value);
        }

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

        // BaseViewModel already provides PropertyChanged functionality
    }

    public class FuelTrendPoint
    {
        public DateTime Period { get; set; }
        public double AvgMPG { get; set; }
    }
}

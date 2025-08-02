using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BusBuddy.Core;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using BusBuddy.Core.Models;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// Phase 1 Vehicles ViewModel - Clean and functional
    /// </summary>
    public class VehiclesViewModel : BaseViewModel
    {
        private ObservableCollection<Bus> _vehicles = new();

        public ObservableCollection<Bus> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public async Task LoadVehiclesAsync()
        {
            try
            {
                using var context = new BusBuddyContext();
                var vehicles = await context.Vehicles.ToListAsync();
                Vehicles.Clear();
                foreach (var vehicle in vehicles)
                {
                    Vehicles.Add(vehicle);
                }
            }
            catch (System.Exception ex)
            {
                // Phase 1: Simple error handling
                MessageBox.Show($"Error loading vehicles: {ex.Message}");
            }
        }
    }
}

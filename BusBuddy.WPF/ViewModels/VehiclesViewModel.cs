using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BusBuddy.Core;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// Phase 1 Vehicles ViewModel - Clean and functional
    /// </summary>
    public class VehiclesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BusBuddy.Core.Models.Vehicle> Vehicles { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

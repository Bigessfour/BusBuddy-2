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
    /// Phase 1 Drivers ViewModel - Clean and functional
    /// </summary>
    public class DriversViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BusBuddy.Core.Models.Driver> Drivers { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadDriversAsync()
        {
            try
            {
                using var context = new BusBuddyContext();
                var drivers = await context.Drivers.ToListAsync();
                Drivers.Clear();
                foreach (var driver in drivers)
                {
                    Drivers.Add(driver);
                }
            }
            catch (System.Exception ex)
            {
                // Phase 1: Simple error handling
                MessageBox.Show($"Error loading drivers: {ex.Message}");
            }
        }
    }
}

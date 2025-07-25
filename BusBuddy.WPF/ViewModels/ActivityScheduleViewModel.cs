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
    /// Phase 1 Activity Schedule ViewModel - Clean and functional
    /// </summary>
    public class ActivityScheduleViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BusBuddy.Core.Models.Activity> Activities { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadActivitiesAsync()
        {
            try
            {
                using var context = new BusBuddyContext();
                var activities = await context.Activities.ToListAsync();
                Activities.Clear();
                foreach (var activity in activities)
                {
                    Activities.Add(activity);
                }
            }
            catch (System.Exception ex)
            {
                // Phase 1: Simple error handling
                MessageBox.Show($"Error loading activities: {ex.Message}");
            }
        }
    }
}

using System.Collections.ObjectModel;
using System.Windows;
using BusBuddy.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels.Student
{
    /// <summary>
    /// ViewModel for the assign-route dialog — manages AM/PM route selections and the dialog result.
    /// /// </summary>
    public partial class AssignRouteViewModel : ObservableObject
    {
        private readonly Window dialog;

        public ObservableCollection<Route> AvailableRoutes { get; }

        [ObservableProperty]
        private Route? selectedAmRoute;

        [ObservableProperty]
        private Route? selectedPmRoute;

        public bool DialogResult { get; private set; }

        public AssignRouteViewModel(Window dialog, ObservableCollection<Route> availableRoutes, Route? amRoute, Route? pmRoute)
        {
            this.dialog = dialog;
            AvailableRoutes = availableRoutes;
            SelectedAmRoute = amRoute;
            SelectedPmRoute = pmRoute;
        }

        [RelayCommand]
        private void Save()
        {
            DialogResult = true;
            dialog.DialogResult = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
            dialog.DialogResult = false;
        }
    }
}

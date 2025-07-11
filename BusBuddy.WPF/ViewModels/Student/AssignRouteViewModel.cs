using BusBuddy.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace BusBuddy.WPF.ViewModels.Student
{
    public partial class AssignRouteViewModel : ObservableObject
    {
        private readonly Window _dialog;

        public ObservableCollection<Route> AvailableRoutes { get; }

        [ObservableProperty]
        private Route? _selectedAmRoute;

        [ObservableProperty]
        private Route? _selectedPmRoute;

        public bool DialogResult { get; private set; }

        public AssignRouteViewModel(Window dialog, ObservableCollection<Route> availableRoutes, Route? amRoute, Route? pmRoute)
        {
            _dialog = dialog;
            AvailableRoutes = availableRoutes;
            SelectedAmRoute = amRoute;
            SelectedPmRoute = pmRoute;
        }

        [RelayCommand]
        private void Save()
        {
            DialogResult = true;
            _dialog.DialogResult = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
            _dialog.DialogResult = false;
        }
    }
}

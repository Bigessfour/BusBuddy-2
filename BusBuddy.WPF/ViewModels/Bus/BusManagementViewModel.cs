using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BusBuddy.WPF.ViewModels
{
    public partial class BusManagementViewModel : ObservableObject
    {
        private readonly IBusService _busService;

        [ObservableProperty]
        private ObservableCollection<Bus> _buses;

        [ObservableProperty]
        private Bus _selectedBus;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _pageSize = 20;

        [ObservableProperty]
        private int _totalPages;

        [ObservableProperty]
        private int _totalRecords;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _sortColumn = "BusNumber";

        [ObservableProperty]
        private bool _isSortAscending = true;

        public IAsyncRelayCommand LoadBusesCommand { get; }
        public IAsyncRelayCommand AddBusCommand { get; }
        public IAsyncRelayCommand UpdateBusCommand { get; }
        public IAsyncRelayCommand DeleteBusCommand { get; }
        public IAsyncRelayCommand NextPageCommand { get; }
        public IAsyncRelayCommand PreviousPageCommand { get; }
        public IAsyncRelayCommand FirstPageCommand { get; }
        public IAsyncRelayCommand LastPageCommand { get; }
        public IAsyncRelayCommand<string> SortCommand { get; }

        public BusManagementViewModel(IBusService busService)
        {
            _busService = busService;
            _buses = new ObservableCollection<Bus>();
            _selectedBus = new Bus();

            LoadBusesCommand = new AsyncRelayCommand(LoadBusesAsync);
            AddBusCommand = new AsyncRelayCommand(AddBusAsync);
            UpdateBusCommand = new AsyncRelayCommand(UpdateBusAsync, CanUpdateOrDelete);
            DeleteBusCommand = new AsyncRelayCommand(DeleteBusAsync, CanUpdateOrDelete);

            // Pagination commands
            NextPageCommand = new AsyncRelayCommand(NextPageAsync, CanGoToNextPage);
            PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync, CanGoToPreviousPage);
            FirstPageCommand = new AsyncRelayCommand(GoToFirstPageAsync, CanGoToPreviousPage);
            LastPageCommand = new AsyncRelayCommand(GoToLastPageAsync, CanGoToNextPage);

            // Sorting command
            SortCommand = new AsyncRelayCommand<string>(SortDataAsync);

            _ = LoadBusesAsync();
        }

        private async Task LoadBusesAsync()
        {
            try
            {
                IsBusy = true;

                // Use the paginated method
                var result = await _busService.GetBusesPaginatedAsync(
                    CurrentPage,
                    PageSize,
                    SortColumn,
                    IsSortAscending);

                // Update the collection
                Buses.Clear();
                foreach (var b in result.Buses)
                    Buses.Add(b);

                // Update pagination information
                TotalRecords = result.TotalCount;
                TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

                // Ensure currentPage is valid
                if (CurrentPage > TotalPages && TotalPages > 0)
                {
                    CurrentPage = TotalPages;
                    await LoadBusesAsync();
                }
            }
            catch (Exception)
            {
                // Handle any exceptions
                // Could display error message to user
            }
            finally
            {
                IsBusy = false;

                // Update command states
                (NextPageCommand as IRelayCommand)?.NotifyCanExecuteChanged();
                (PreviousPageCommand as IRelayCommand)?.NotifyCanExecuteChanged();
                (FirstPageCommand as IRelayCommand)?.NotifyCanExecuteChanged();
                (LastPageCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        private async Task SortDataAsync(string? column)
        {
            if (column == SortColumn)
            {
                // Toggle sort direction if same column
                IsSortAscending = !IsSortAscending;
            }
            else if (column != null)
            {
                // New column, default to ascending
                SortColumn = column;
                IsSortAscending = true;
            }

            await LoadBusesAsync();
        }

        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadBusesAsync();
            }
        }

        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadBusesAsync();
            }
        }

        private async Task GoToFirstPageAsync()
        {
            CurrentPage = 1;
            await LoadBusesAsync();
        }

        private async Task GoToLastPageAsync()
        {
            CurrentPage = TotalPages;
            await LoadBusesAsync();
        }

        private bool CanGoToNextPage() => CurrentPage < TotalPages && !IsBusy;

        private bool CanGoToPreviousPage() => CurrentPage > 1 && !IsBusy;

        private async Task AddBusAsync()
        {
            if (SelectedBus != null)
            {
                IsBusy = true;
                try
                {
                    await _busService.AddBusEntityAsync(SelectedBus);
                    await LoadBusesAsync();
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task UpdateBusAsync()
        {
            if (SelectedBus != null)
            {
                IsBusy = true;
                try
                {
                    await _busService.UpdateBusEntityAsync(SelectedBus);
                    await LoadBusesAsync();
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task DeleteBusAsync()
        {
            if (SelectedBus != null)
            {
                IsBusy = true;
                try
                {
                    await _busService.DeleteBusEntityAsync(SelectedBus.VehicleId);
                    await LoadBusesAsync();
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private bool CanUpdateOrDelete()
        {
            return SelectedBus != null && SelectedBus.VehicleId != 0 && !IsBusy;
        }

        partial void OnSelectedBusChanged(Bus value)
        {
            (UpdateBusCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (DeleteBusCommand as IRelayCommand)?.NotifyCanExecuteChanged();
        }

        partial void OnCurrentPageChanged(int value)
        {
            (NextPageCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (PreviousPageCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (FirstPageCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (LastPageCommand as IRelayCommand)?.NotifyCanExecuteChanged();
        }
    }
}

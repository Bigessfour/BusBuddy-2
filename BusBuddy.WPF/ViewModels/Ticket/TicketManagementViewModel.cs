using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for managing tickets in the Ticket Management module
    /// </summary>
    public partial class TicketManagementViewModel : BaseViewModel
    {
        private readonly ITicketService _ticketService;
        private readonly IStudentService _studentService;
        private readonly IRouteService _routeService;
        private readonly ILogger<TicketManagementViewModel> _logger;

        // Observable collections
        public ObservableCollection<Ticket> Tickets { get; } = new();
        public ObservableCollection<Student> Students { get; } = new();
        public ObservableCollection<Route> Routes { get; } = new();

        // Collection views for filtering
        private readonly CollectionViewSource _ticketsViewSource = new CollectionViewSource();
        public ICollectionView? TicketsView => _ticketsViewSource?.View;

        // Selected items
        [ObservableProperty]
        private Ticket? _selectedTicket;

        [ObservableProperty]
        private Student? _selectedStudent;

        [ObservableProperty]
        private Route? _selectedRoute;

        // Filter properties
        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedStatus = "All";

        [ObservableProperty]
        private string _selectedTicketType = "All";

        [ObservableProperty]
        private DateTime? _fromDate;

        [ObservableProperty]
        private DateTime? _toDate;

        // Statistics
        [ObservableProperty]
        private Dictionary<string, decimal> _revenueStats = new();

        [ObservableProperty]
        private Dictionary<string, int> _countStats = new();

        // New ticket properties
        [ObservableProperty]
        private Ticket _newTicket = new()
        {
            TravelDate = DateTime.Today,
            IssuedDate = DateTime.Now,
            TicketType = "Single",
            Status = "Valid",
            PaymentMethod = "Cash",
            Price = 2.50m
        };

        // Available ticket types and statuses for dropdowns
        public List<string> TicketTypes { get; } = new()
        {
            "Single",
            "Round Trip",
            "Daily Pass",
            "Weekly Pass",
            "Monthly Pass"
        };

        public List<string> TicketStatuses { get; } = new()
        {
            "Valid",
            "Used",
            "Expired",
            "Cancelled"
        };

        public List<string> PaymentMethods { get; } = new()
        {
            "Cash",
            "Card",
            "Mobile Pay",
            "Student Account"
        };

        public List<string> StatusFilters { get; } = new()
        {
            "All",
            "Valid",
            "Used",
            "Expired",
            "Cancelled"
        };

        public List<string> TicketTypeFilters { get; } = new()
        {
            "All",
            "Single",
            "Round Trip",
            "Daily Pass",
            "Weekly Pass",
            "Monthly Pass"
        };

        public TicketManagementViewModel(
            ITicketService ticketService,
            IStudentService studentService,
            IRouteService routeService,
            ILogger<TicketManagementViewModel> logger)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize collection view source
            _ticketsViewSource.Source = Tickets;

            if (_ticketsViewSource.View != null)
            {
                _ticketsViewSource.Filter += TicketsViewSource_Filter;
            }

            // Set dates for filter
            FromDate = DateTime.Today.AddDays(-30);
            ToDate = DateTime.Today;

            // We need to initialize data in the view, not in the constructor
            // to avoid DbContext concurrency issues
        }

        /// <summary>
        /// Filter logic for tickets collection view
        /// </summary>
        private void TicketsViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is not Ticket ticket) return;

            var passesTextFilter = string.IsNullOrWhiteSpace(SearchText) ||
                                   ticket.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   ticket.QRCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   (ticket.Student?.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false);

            var passesStatusFilter = SelectedStatus == "All" || ticket.Status == SelectedStatus;

            var passesTypeFilter = SelectedTicketType == "All" || ticket.TicketType == SelectedTicketType;

            var passesDateFilter = true;
            if (FromDate.HasValue)
                passesDateFilter = ticket.IssuedDate.Date >= FromDate.Value.Date;
            if (ToDate.HasValue && passesDateFilter)
                passesDateFilter = ticket.IssuedDate.Date <= ToDate.Value.Date;

            e.Accepted = passesTextFilter && passesStatusFilter && passesTypeFilter && passesDateFilter;
        }

        /// <summary>
        /// Loads all necessary data for the view
        /// </summary>
        public async Task LoadAllDataAsync()
        {
            await LoadDataAsync(async () =>
            {
                // Load sequentially to prevent DbContext concurrency issues
                await LoadTicketsAsync();
                await LoadStudentsAsync();
                await LoadRoutesAsync();
                await LoadStatisticsAsync();
            });
        }

        /// <summary>
        /// Loads all tickets from the service
        /// </summary>
        private async Task LoadTicketsAsync()
        {
            try
            {
                var tickets = await _ticketService.GetAllTicketsAsync();
                Tickets.Clear();
                foreach (var ticket in tickets)
                    Tickets.Add(ticket);

                // Refresh the view if it's not null
                _ticketsViewSource.View?.Refresh();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tickets");
            }
        }
        // Removed parameterless call to LoadDataAsync to fix compile error
        private async Task LoadStudentsAsync()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                Students.Clear();
                foreach (var student in students)
                    Students.Add(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading students");
            }
        }

        /// <summary>
        /// Loads all routes for dropdown selection
        /// </summary>
        private async Task LoadRoutesAsync()
        {
            try
            {
                var routes = await _routeService.GetAllActiveRoutesAsync();
                Routes.Clear();
                foreach (var route in routes)
                    Routes.Add(route);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading routes");
            }
        }

        /// <summary>
        /// Loads ticket statistics
        /// </summary>
        private async Task LoadStatisticsAsync()
        {
            try
            {
                RevenueStats = await _ticketService.GetTicketRevenueStatisticsAsync();
                CountStats = await _ticketService.GetTicketCountStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ticket statistics");
                // Initialize with empty dictionaries to prevent null reference exceptions
                RevenueStats = new Dictionary<string, decimal>();
                CountStats = new Dictionary<string, int>();
            }
        }

        #region Commands

        /// <summary>
        /// Command to refresh data
        /// </summary>
        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadAllDataAsync();
        }

        /// <summary>
        /// Command to create a new ticket
        /// </summary>
        [RelayCommand]
        private async Task CreateTicketAsync()
        {
            try
            {
                if (NewTicket.StudentId <= 0 || NewTicket.RouteId <= 0)
                {
                    _logger.LogWarning("Cannot create ticket: Student or Route not selected");
                    return;
                }

                var validationErrors = await _ticketService.ValidateTicketAsync(NewTicket);
                if (validationErrors.Any())
                {
                    _logger.LogWarning("Ticket validation failed: {Errors}", string.Join(", ", validationErrors));
                    return;
                }

                var createdTicket = await _ticketService.CreateTicketAsync(NewTicket);
                Tickets.Add(createdTicket);
                _ticketsViewSource.View?.Refresh();

                // Reset new ticket
                NewTicket = new Ticket
                {
                    TravelDate = DateTime.Today,
                    IssuedDate = DateTime.Now,
                    TicketType = "Single",
                    Status = "Valid",
                    PaymentMethod = "Cash",
                    Price = 2.50m
                };

                await LoadStatisticsAsync();

                _logger.LogInformation("Created new ticket {TicketId}", createdTicket.TicketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new ticket");
            }
        }

        /// <summary>
        /// Command to update an existing ticket
        /// </summary>
        [RelayCommand]
        private async Task UpdateTicketAsync()
        {
            if (SelectedTicket == null) return;

            try
            {
                var validationErrors = await _ticketService.ValidateTicketAsync(SelectedTicket);
                if (validationErrors.Any())
                {
                    _logger.LogWarning("Ticket validation failed: {Errors}", string.Join(", ", validationErrors));
                    return;
                }

                await _ticketService.UpdateTicketAsync(SelectedTicket);
                _ticketsViewSource.View?.Refresh();
                await LoadStatisticsAsync();

                _logger.LogInformation("Updated ticket {TicketId}", SelectedTicket.TicketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId}", SelectedTicket.TicketId);
            }
        }

        /// <summary>
        /// Command to delete a ticket
        /// </summary>
        [RelayCommand]
        private async Task DeleteTicketAsync()
        {
            if (SelectedTicket == null) return;

            try
            {
                await _ticketService.DeleteTicketAsync(SelectedTicket.TicketId);
                Tickets.Remove(SelectedTicket);
                _ticketsViewSource.View?.Refresh();
                await LoadStatisticsAsync();

                _logger.LogInformation("Deleted ticket {TicketId}", SelectedTicket.TicketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket {TicketId}", SelectedTicket.TicketId);
            }
        }

        /// <summary>
        /// Command to cancel a ticket
        /// </summary>
        [RelayCommand]
        private async Task CancelTicketAsync()
        {
            if (SelectedTicket == null) return;

            try
            {
                await _ticketService.CancelTicketAsync(SelectedTicket.TicketId, "User cancelled");
                await LoadTicketsAsync();
                await LoadStatisticsAsync();

                _logger.LogInformation("Cancelled ticket {TicketId}", SelectedTicket.TicketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket {TicketId}", SelectedTicket.TicketId);
            }
        }

        /// <summary>
        /// Command to mark a ticket as used
        /// </summary>
        [RelayCommand]
        private async Task UseTicketAsync()
        {
            if (SelectedTicket == null) return;

            try
            {
                await _ticketService.UseTicketAsync(SelectedTicket.TicketId);
                await LoadTicketsAsync();
                await LoadStatisticsAsync();

                _logger.LogInformation("Marked ticket {TicketId} as used", SelectedTicket.TicketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking ticket {TicketId} as used", SelectedTicket.TicketId);
            }
        }

        /// <summary>
        /// Command to apply filters
        /// </summary>
        [RelayCommand]
        private void ApplyFilters()
        {
            _ticketsViewSource.View?.Refresh();
        }

        /// <summary>
        /// Command to reset filters
        /// </summary>
        [RelayCommand]
        private void ResetFilters()
        {
            SearchText = string.Empty;
            SelectedStatus = "All";
            SelectedTicketType = "All";
            FromDate = DateTime.Today.AddDays(-30);
            ToDate = DateTime.Today;

            _ticketsViewSource.View?.Refresh();
        }

        /// <summary>
        /// Command to export tickets to CSV
        /// </summary>
        [RelayCommand]
        private async Task ExportToCsvAsync()
        {
            try
            {
                var csv = await _ticketService.ExportTicketsToCsvAsync(FromDate, ToDate);
                // In a real app, you would save this to a file or clipboard
                _logger.LogInformation("Exported {Length} bytes of CSV data", csv.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting tickets to CSV");
            }
        }

        /// <summary>
        /// Command to generate a QR code for a ticket
        /// </summary>
        [RelayCommand]
        private async Task GenerateQrCodeAsync()
        {
            if (SelectedTicket == null) return;

            try
            {
                var qrCode = await _ticketService.GenerateQRCodeAsync(SelectedTicket.TicketId);
                await LoadTicketsAsync();

                _logger.LogInformation("Generated QR code for ticket {TicketId}: {QRCode}", SelectedTicket.TicketId, qrCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for ticket {TicketId}", SelectedTicket.TicketId);
            }
        }

        #endregion
    }
}

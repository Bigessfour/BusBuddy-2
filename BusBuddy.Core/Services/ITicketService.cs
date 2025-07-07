using BusBuddy.Core.Models;

namespace BusBuddy.Core.Services;

/// <summary>
/// Service interface for managing bus ticket operations
/// Provides CRUD operations and business logic for ticket management
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// Gets all tickets from the database
    /// </summary>
    /// <returns>List of all tickets</returns>
    Task<List<Ticket>> GetAllTicketsAsync();

    /// <summary>
    /// Gets a specific ticket by ID
    /// </summary>
    /// <param name="ticketId">The ticket ID</param>
    /// <returns>Ticket if found, null otherwise</returns>
    Task<Ticket?> GetTicketByIdAsync(int ticketId);

    /// <summary>
    /// Gets tickets for a specific student
    /// </summary>
    /// <param name="studentId">Student ID to filter by</param>
    /// <returns>List of tickets for the specified student</returns>
    Task<List<Ticket>> GetTicketsByStudentIdAsync(int studentId);

    /// <summary>
    /// Gets tickets for a specific route
    /// </summary>
    /// <param name="routeId">Route ID to filter by</param>
    /// <returns>List of tickets for the specified route</returns>
    Task<List<Ticket>> GetTicketsByRouteIdAsync(int routeId);

    /// <summary>
    /// Gets tickets by travel date
    /// </summary>
    /// <param name="travelDate">Travel date to filter by</param>
    /// <returns>List of tickets for the specified date</returns>
    Task<List<Ticket>> GetTicketsByTravelDateAsync(DateTime travelDate);

    /// <summary>
    /// Gets tickets by status
    /// </summary>
    /// <param name="status">Ticket status to filter by</param>
    /// <returns>List of tickets with the specified status</returns>
    Task<List<Ticket>> GetTicketsByStatusAsync(string status);

    /// <summary>
    /// Gets tickets by ticket type
    /// </summary>
    /// <param name="ticketType">Ticket type to filter by</param>
    /// <returns>List of tickets of the specified type</returns>
    Task<List<Ticket>> GetTicketsByTypeAsync(string ticketType);

    /// <summary>
    /// Gets active/valid tickets only
    /// </summary>
    /// <returns>List of active tickets</returns>
    Task<List<Ticket>> GetActiveTicketsAsync();

    /// <summary>
    /// Gets tickets issued within a date range
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>List of tickets issued in the specified range</returns>
    Task<List<Ticket>> GetTicketsByDateRangeAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Searches tickets by student name or QR code
    /// </summary>
    /// <param name="searchTerm">Term to search for</param>
    /// <returns>List of matching tickets</returns>
    Task<List<Ticket>> SearchTicketsAsync(string searchTerm);

    /// <summary>
    /// Creates a new ticket
    /// </summary>
    /// <param name="ticket">Ticket to create</param>
    /// <returns>The created ticket with ID</returns>
    Task<Ticket> CreateTicketAsync(Ticket ticket);

    /// <summary>
    /// Updates an existing ticket
    /// </summary>
    /// <param name="ticket">Ticket to update</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UpdateTicketAsync(Ticket ticket);

    /// <summary>
    /// Deletes a ticket from the database
    /// </summary>
    /// <param name="ticketId">ID of the ticket to delete</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> DeleteTicketAsync(int ticketId);

    /// <summary>
    /// Cancels a ticket and processes refund if applicable
    /// </summary>
    /// <param name="ticketId">ID of the ticket to cancel</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> CancelTicketAsync(int ticketId, string reason);

    /// <summary>
    /// Marks a ticket as used
    /// </summary>
    /// <param name="ticketId">ID of the ticket to mark as used</param>
    /// <param name="driverName">Name of the driver who used the ticket</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UseTicketAsync(int ticketId, string? driverName = null);

    /// <summary>
    /// Validates ticket data before save
    /// </summary>
    /// <param name="ticket">Ticket to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    Task<List<string>> ValidateTicketAsync(Ticket ticket);

    /// <summary>
    /// Gets ticket revenue statistics
    /// </summary>
    /// <returns>Dictionary with revenue statistics</returns>
    Task<Dictionary<string, decimal>> GetTicketRevenueStatisticsAsync();

    /// <summary>
    /// Gets ticket count statistics
    /// </summary>
    /// <returns>Dictionary with count statistics</returns>
    Task<Dictionary<string, int>> GetTicketCountStatisticsAsync();

    /// <summary>
    /// Gets expired tickets that need attention
    /// </summary>
    /// <returns>List of expired tickets</returns>
    Task<List<Ticket>> GetExpiredTicketsAsync();

    /// <summary>
    /// Gets tickets expiring within specified days
    /// </summary>
    /// <param name="days">Number of days to look ahead</param>
    /// <returns>List of tickets expiring soon</returns>
    Task<List<Ticket>> GetTicketsExpiringWithinAsync(int days);

    /// <summary>
    /// Validates a ticket for use by QR code
    /// </summary>
    /// <param name="qrCode">QR code to validate</param>
    /// <returns>Ticket if valid, null otherwise</returns>
    Task<Ticket?> ValidateTicketByQRCodeAsync(string qrCode);

    /// <summary>
    /// Generates a QR code for a ticket
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <returns>Generated QR code string</returns>
    Task<string> GenerateQRCodeAsync(int ticketId);

    /// <summary>
    /// Exports ticket data to CSV format
    /// </summary>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <returns>CSV string containing ticket data</returns>
    Task<string> ExportTicketsToCsvAsync(DateTime? fromDate = null, DateTime? toDate = null);
}

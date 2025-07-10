using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BusBuddy.Core.Services;

/// <summary>
/// Service implementation for managing bus ticket operations
/// </summary>
public class TicketService : ITicketService
{
    private readonly IBusBuddyDbContextFactory _contextFactory;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IBusBuddyDbContextFactory contextFactory, ILogger<TicketService> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all tickets");
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tickets");
            throw;
        }
    }

    public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
    {
        try
        {
            _logger.LogInformation("Retrieving ticket {TicketId}", ticketId);
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<List<Ticket>> GetTicketsByStudentIdAsync(int studentId)
    {
        try
        {
            _logger.LogInformation("Retrieving tickets for student {StudentId}", studentId);
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.StudentId == studentId)
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets for student {StudentId}", studentId);
            throw;
        }
    }

    public async Task<List<Ticket>> GetTicketsByRouteIdAsync(int routeId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.RouteId == routeId)
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets for route {RouteId}", routeId);
            throw;
        }
    }

    public async Task<List<Ticket>> GetTicketsByTravelDateAsync(DateTime travelDate)
    {
        try
        {
            var date = travelDate.Date;
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.TravelDate.Date == date)
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets for travel date {TravelDate}", travelDate);
            throw;
        }
    }

    public async Task<List<Ticket>> GetTicketsByStatusAsync(string status)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets with status {Status}", status);
            throw;
        }
    }

    public async Task<List<Ticket>> GetTicketsByTypeAsync(string ticketType)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.TicketType == ticketType)
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets of type {TicketType}", ticketType);
            throw;
        }
    }

    public async Task<List<Ticket>> GetActiveTicketsAsync()
    {
        try
        {
            var today = DateTime.Today;
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.Status == "Valid" && (!t.ValidUntil.HasValue || t.ValidUntil.Value >= today))
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active tickets");
            throw;
        }
    }

    public async Task<List<Ticket>> GetTicketsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var from = fromDate.Date;
            var to = toDate.Date.AddDays(1); // Include end date
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.IssuedDate >= from && t.IssuedDate < to)
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets for date range {FromDate} to {ToDate}", fromDate, toDate);
            throw;
        }
    }

    public async Task<List<Ticket>> SearchTicketsAsync(string searchTerm)
    {
        try
        {
            var term = searchTerm.ToLower();
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.Student!.FullName.ToLower().Contains(term) ||
                           t.QRCode.ToLower().Contains(term) ||
                           t.TicketId.ToString().Contains(term))
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tickets with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        try
        {
            _logger.LogInformation("Creating ticket for student {StudentId}", ticket.StudentId);
            // Set default values
            ticket.IssuedDate = DateTime.Now;
            ticket.Status = "Valid";
            ticket.SetValidityPeriod();
            ticket.GenerateQRCode();
            using var context = _contextFactory.CreateWriteDbContext();
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
            _logger.LogInformation("Created ticket {TicketId} for student {StudentId}", ticket.TicketId, ticket.StudentId);
            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket for student {StudentId}", ticket.StudentId);
            throw;
        }
    }

    public async Task<bool> UpdateTicketAsync(Ticket ticket)
    {
        try
        {
            _logger.LogInformation("Updating ticket {TicketId}", ticket.TicketId);
            using var context = _contextFactory.CreateWriteDbContext();
            context.Tickets.Update(ticket);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket {TicketId}", ticket.TicketId);
            throw;
        }
    }

    public async Task<bool> DeleteTicketAsync(int ticketId)
    {
        try
        {
            _logger.LogInformation("Deleting ticket {TicketId}", ticketId);
            using var context = _contextFactory.CreateWriteDbContext();
            var ticket = await context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;
            context.Tickets.Remove(ticket);
            var result = await context.SaveChangesAsync();
            _logger.LogInformation("Deleted ticket {TicketId}", ticketId);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<bool> CancelTicketAsync(int ticketId, string reason)
    {
        try
        {
            using var context = _contextFactory.CreateWriteDbContext();
            var ticket = await context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.CancelTicket();
            ticket.Notes = $"Cancelled: {reason}";

            var result = await context.SaveChangesAsync();

            _logger.LogInformation("Cancelled ticket {TicketId} - {Reason}", ticketId, reason);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<bool> UseTicketAsync(int ticketId, string? driverName = null)
    {
        try
        {
            using var context = _contextFactory.CreateWriteDbContext();
            var ticket = await context.Tickets.FindAsync(ticketId);
            if (ticket == null || !ticket.CanBeUsed) return false;

            ticket.MarkAsUsed(driverName);

            var result = await context.SaveChangesAsync();

            _logger.LogInformation("Used ticket {TicketId} by driver {DriverName}", ticketId, driverName);
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<List<string>> ValidateTicketAsync(Ticket ticket)
    {
        var errors = new List<string>();

        try
        {
            // Basic validation
            if (ticket.StudentId <= 0)
                errors.Add("Student is required");

            if (ticket.RouteId <= 0)
                errors.Add("Route is required");

            if (ticket.TravelDate == default)
                errors.Add("Travel date is required");

            if (ticket.Price <= 0)
                errors.Add("Price must be greater than 0");

            if (string.IsNullOrWhiteSpace(ticket.TicketType))
                errors.Add("Ticket type is required");

            if (string.IsNullOrWhiteSpace(ticket.PaymentMethod))
                errors.Add("Payment method is required");

            using var context = _contextFactory.CreateDbContext();
            // Check if student exists
            if (ticket.StudentId > 0)
            {
                var studentExists = await context.Students.AnyAsync(s => s.StudentId == ticket.StudentId);
                if (!studentExists)
                    errors.Add("Student does not exist");
            }

            // Check if route exists
            if (ticket.RouteId > 0)
            {
                var routeExists = await context.Routes.AnyAsync(r => r.RouteId == ticket.RouteId);
                if (!routeExists)
                    errors.Add("Route does not exist");
            }

            // Check for duplicate tickets
            if (ticket.TicketId == 0) // New ticket
            {
                var duplicateExists = await context.Tickets.AnyAsync(t =>
                    t.StudentId == ticket.StudentId &&
                    t.RouteId == ticket.RouteId &&
                    t.TravelDate.Date == ticket.TravelDate.Date &&
                    t.Status != "Cancelled");

                if (duplicateExists)
                    errors.Add("A ticket already exists for this student, route, and date");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ticket");
            errors.Add("Error during validation");
        }

        return errors;
    }

    public async Task<Dictionary<string, decimal>> GetTicketRevenueStatisticsAsync()
    {
        try
        {
            var stats = new Dictionary<string, decimal>();

            using var context = _contextFactory.CreateDbContext();
            var tickets = await context.Tickets
                .Where(t => t.Status == "Valid" || t.Status == "Used")
                .ToListAsync();

            stats["TotalRevenue"] = tickets.Sum(t => t.Price);
            stats["TodayRevenue"] = tickets.Where(t => t.IssuedDate.Date == DateTime.Today).Sum(t => t.Price);
            stats["WeekRevenue"] = tickets.Where(t => t.IssuedDate >= DateTime.Today.AddDays(-7)).Sum(t => t.Price);
            stats["MonthRevenue"] = tickets.Where(t => t.IssuedDate >= DateTime.Today.AddDays(-30)).Sum(t => t.Price);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket revenue statistics");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetTicketCountStatisticsAsync()
    {
        try
        {
            var stats = new Dictionary<string, int>();

            using var context = _contextFactory.CreateDbContext();
            stats["TotalTickets"] = await context.Tickets.CountAsync();
            stats["ValidTickets"] = await context.Tickets.CountAsync(t => t.Status == "Valid");
            stats["UsedTickets"] = await context.Tickets.CountAsync(t => t.Status == "Used");
            stats["CancelledTickets"] = await context.Tickets.CountAsync(t => t.Status == "Cancelled");
            stats["ExpiredTickets"] = await context.Tickets.CountAsync(t => t.Status == "Expired");

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket count statistics");
            throw;
        }
    }

    public async Task<List<Ticket>> GetExpiredTicketsAsync()
    {
        try
        {
            var today = DateTime.Today;
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.ValidUntil.HasValue && t.ValidUntil.Value < today && t.Status == "Valid")
                .OrderBy(t => t.ValidUntil)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expired tickets");
            throw;
        }
    }

    public async Task<List<Ticket>> GetTicketsExpiringWithinAsync(int days)
    {
        try
        {
            var futureDate = DateTime.Today.AddDays(days);
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .Where(t => t.ValidUntil.HasValue &&
                           t.ValidUntil.Value >= DateTime.Today &&
                           t.ValidUntil.Value <= futureDate &&
                           t.Status == "Valid")
                .OrderBy(t => t.ValidUntil)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets expiring within {Days} days", days);
            throw;
        }
    }

    public async Task<Ticket?> ValidateTicketByQRCodeAsync(string qrCode)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.QRCode == qrCode && t.CanBeUsed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ticket by QR code {QRCode}", qrCode);
            throw;
        }
    }

    public async Task<string> GenerateQRCodeAsync(int ticketId)
    {
        try
        {
            using var context = _contextFactory.CreateWriteDbContext();
            var ticket = await context.Tickets.FindAsync(ticketId);
            if (ticket == null) throw new ArgumentException("Ticket not found");

            ticket.GenerateQRCode();
            await context.SaveChangesAsync();

            return ticket.QRCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<string> ExportTicketsToCsvAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Tickets
                .Include(t => t.Student)
                .Include(t => t.Route)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(t => t.IssuedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.IssuedDate <= toDate.Value);

            var tickets = await query.OrderByDescending(t => t.IssuedDate).ToListAsync();

            var csv = "TicketId,StudentName,RouteName,TravelDate,IssuedDate,TicketType,Price,Status,PaymentMethod,QRCode\n";

            foreach (var ticket in tickets)
            {
                csv += $"{ticket.TicketId}," +
                       $"\"{ticket.Student?.FullName ?? "Unknown"}\"," +
                       $"\"{ticket.Route?.RouteName ?? "Unknown"}\"," +
                       $"{ticket.TravelDate:yyyy-MM-dd}," +
                       $"{ticket.IssuedDate:yyyy-MM-dd HH:mm}," +
                       $"{ticket.TicketType}," +
                       $"{ticket.Price:F2}," +
                       $"{ticket.Status}," +
                       $"{ticket.PaymentMethod}," +
                       $"{ticket.QRCode}\n";
            }

            return csv;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting tickets to CSV");
            throw;
        }
    }
}

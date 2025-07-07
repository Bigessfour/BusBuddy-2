using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Core.Services;

/// <summary>
/// Maintenance service implementation using Entity Framework
/// </summary>
public class MaintenanceService : IMaintenanceService
{
    private readonly BusBuddyDbContext _context;

    public MaintenanceService(BusBuddyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Maintenance>> GetAllMaintenanceRecordsAsync()
    {
        return await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<Maintenance?> GetMaintenanceRecordByIdAsync(int id)
    {
        return await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .FirstOrDefaultAsync(m => m.MaintenanceId == id);
    }

    public async Task<Maintenance> CreateMaintenanceRecordAsync(Maintenance maintenance)
    {
        maintenance.CreatedDate = DateTime.UtcNow;
        _context.MaintenanceRecords.Add(maintenance);
        await _context.SaveChangesAsync();
        return maintenance;
    }

    public async Task<Maintenance> UpdateMaintenanceRecordAsync(Maintenance maintenance)
    {
        maintenance.UpdatedDate = DateTime.UtcNow;
        _context.MaintenanceRecords.Update(maintenance);
        await _context.SaveChangesAsync();
        return maintenance;
    }

    public async Task<bool> DeleteMaintenanceRecordAsync(int id)
    {
        var maintenance = await _context.MaintenanceRecords.FindAsync(id);
        if (maintenance == null)
            return false;

        _context.MaintenanceRecords.Remove(maintenance);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Maintenance>> GetMaintenanceRecordsByVehicleAsync(int vehicleId)
    {
        return await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Where(m => m.VehicleId == vehicleId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Maintenance>> GetMaintenanceRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Where(m => m.Date >= startDate && m.Date <= endDate)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Maintenance>> GetMaintenanceRecordsByPriorityAsync(string priority)
    {
        return await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Where(m => m.Priority == priority)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetMaintenanceCostTotalAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.MaintenanceRecords
            .Where(m => m.VehicleId == vehicleId && m.RepairCost > 0);

        if (startDate.HasValue)
            query = query.Where(m => m.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.Date <= endDate.Value);

        return await query.SumAsync(m => m.RepairCost);
    }
}

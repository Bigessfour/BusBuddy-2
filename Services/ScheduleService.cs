using Bus_Buddy.Data;
using Bus_Buddy.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bus_Buddy.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly BusBuddyDbContext _context;

        public ScheduleService(BusBuddyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Activity>> GetAllSchedulesAsync()
        {
            return await _context.Activities
                .Include(a => a.AssignedVehicle)
                .Include(a => a.Driver)
                .Include(a => a.Route)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.LeaveTime)
                .ToListAsync();
        }

        public async Task<Activity> GetScheduleByIdAsync(int id)
        {
            return await _context.Activities
                .Include(a => a.AssignedVehicle)
                .Include(a => a.Driver)
                .Include(a => a.Route)
                .FirstOrDefaultAsync(a => a.ActivityId == id) ?? throw new InvalidOperationException($"Activity with ID {id} not found.");
        }

        public async Task<List<Activity>> GetSchedulesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Activities
                .Include(a => a.AssignedVehicle)
                .Include(a => a.Driver)
                .Include(a => a.Route)
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.LeaveTime)
                .ToListAsync();
        }

        public async Task<Activity> AddScheduleAsync(Activity schedule)
        {
            // Validate schedule conflict before adding
            if (await ValidateScheduleConflictAsync(schedule))
            {
                throw new InvalidOperationException("Schedule conflict detected. The vehicle or driver is already assigned during this time.");
            }

            _context.Activities.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<Activity> UpdateScheduleAsync(Activity schedule)
        {
            // Validate schedule conflict before updating (excluding current schedule)
            var existingConflicts = await _context.Activities
                .Where(a => a.ActivityId != schedule.ActivityId &&
                           a.Date == schedule.Date &&
                           ((a.AssignedVehicleId == schedule.AssignedVehicleId) ||
                            (a.DriverId == schedule.DriverId)) &&
                           ((a.LeaveTime <= schedule.LeaveTime && a.EventTime >= schedule.LeaveTime) ||
                            (a.LeaveTime <= schedule.EventTime && a.EventTime >= schedule.EventTime) ||
                            (schedule.LeaveTime <= a.LeaveTime && schedule.EventTime >= a.EventTime)))
                .AnyAsync();

            if (existingConflicts)
            {
                throw new InvalidOperationException("Schedule conflict detected. The vehicle or driver is already assigned during this time.");
            }

            _context.Activities.Update(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            var schedule = await _context.Activities.FindAsync(id);
            if (schedule == null)
                return false;

            _context.Activities.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Activity>> GetSchedulesByVehicleAsync(string vehicleId)
        {
            if (int.TryParse(vehicleId, out int parsedVehicleId))
            {
                return await _context.Activities
                    .Include(a => a.AssignedVehicle)
                    .Include(a => a.Driver)
                    .Include(a => a.Route)
                    .Where(a => a.AssignedVehicleId == parsedVehicleId)
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.LeaveTime)
                    .ToListAsync();
            }
            return new List<Activity>();
        }

        public async Task<List<Activity>> GetSchedulesByDriverAsync(string driverName)
        {
            return await _context.Activities
                .Include(a => a.AssignedVehicle)
                .Include(a => a.Driver)
                .Include(a => a.Route)
                .Where(a => a.Driver != null && a.Driver.DriverName == driverName)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.LeaveTime)
                .ToListAsync();
        }

        public async Task<bool> ValidateScheduleConflictAsync(Activity schedule)
        {
            // Check for vehicle conflicts using mapped properties
            var vehicleConflict = await _context.Activities
                .Where(a => a.AssignedVehicleId == schedule.AssignedVehicleId &&
                           a.Date == schedule.Date &&
                           ((a.LeaveTime <= schedule.LeaveTime && a.EventTime >= schedule.LeaveTime) ||
                            (a.LeaveTime <= schedule.EventTime && a.EventTime >= schedule.EventTime) ||
                            (schedule.LeaveTime <= a.LeaveTime && schedule.EventTime >= a.EventTime)))
                .AnyAsync();

            if (vehicleConflict) return true;

            // Check for driver conflicts using mapped properties
            var driverConflict = await _context.Activities
                .Where(a => a.DriverId == schedule.DriverId &&
                           a.Date == schedule.Date &&
                           ((a.LeaveTime <= schedule.LeaveTime && a.EventTime >= schedule.LeaveTime) ||
                            (a.LeaveTime <= schedule.EventTime && a.EventTime >= schedule.EventTime) ||
                            (schedule.LeaveTime <= a.LeaveTime && schedule.EventTime >= a.EventTime)))
                .AnyAsync();

            if (driverConflict) return true;

            return false;
        }
    }
}

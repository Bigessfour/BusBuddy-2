using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Core.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly BusBuddyDbContext _context;

        public ScheduleService(BusBuddyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesAsync()
        {
            return await _context.Schedules
                .Include(s => s.Route)
                .Include(s => s.Bus)
                .Include(s => s.Driver)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Schedule?> GetScheduleByIdAsync(int id)
        {
            return await _context.Schedules
                .Include(s => s.Route)
                .Include(s => s.Bus)
                .Include(s => s.Driver)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ScheduleId == id);
        }

        public async Task AddScheduleAsync(Schedule schedule)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));
            if (schedule.DepartureTime >= schedule.ArrivalTime)
                throw new ArgumentException("Departure time must be before arrival time.");
            if (!await _context.Routes.AnyAsync(r => r.RouteId == schedule.RouteId))
                throw new ArgumentException("Invalid route ID.");
            if (!await _context.Vehicles.AnyAsync(b => b.VehicleId == schedule.BusId))
                throw new ArgumentException("Invalid bus ID.");
            if (!await _context.Drivers.AnyAsync(d => d.DriverId == schedule.DriverId))
                throw new ArgumentException("Invalid driver ID.");
            await _context.Schedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateScheduleAsync(Schedule schedule)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));
            var existing = await _context.Schedules.FindAsync(schedule.ScheduleId);
            if (existing == null)
                throw new InvalidOperationException("Schedule not found.");
            if (schedule.DepartureTime >= schedule.ArrivalTime)
                throw new ArgumentException("Departure time must be before arrival time.");
            if (!await _context.Routes.AnyAsync(r => r.RouteId == schedule.RouteId))
                throw new ArgumentException("Invalid route ID.");
            if (!await _context.Vehicles.AnyAsync(b => b.VehicleId == schedule.BusId))
                throw new ArgumentException("Invalid bus ID.");
            if (!await _context.Drivers.AnyAsync(d => d.DriverId == schedule.DriverId))
                throw new ArgumentException("Invalid driver ID.");
            _context.Entry(existing).CurrentValues.SetValues(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteScheduleAsync(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return;
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }
}

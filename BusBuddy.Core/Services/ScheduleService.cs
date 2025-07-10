using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Core.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IBusBuddyDbContextFactory _contextFactory;

        public ScheduleService(IBusBuddyDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesAsync()
        {
            var context = _contextFactory.CreateDbContext();
            try
            {
                return await context.Schedules
                    .Include(s => s.Route)
                    .Include(s => s.Bus)
                    .Include(s => s.Driver)
                    .AsNoTracking()
                    .ToListAsync();
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task<Schedule?> GetScheduleByIdAsync(int id)
        {
            var context = _contextFactory.CreateDbContext();
            try
            {
                return await context.Schedules
                    .Include(s => s.Route)
                    .Include(s => s.Bus)
                    .Include(s => s.Driver)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ScheduleId == id);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task AddScheduleAsync(Schedule schedule)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));
            if (schedule.DepartureTime >= schedule.ArrivalTime)
                throw new ArgumentException("Departure time must be before arrival time.");

            var context = _contextFactory.CreateWriteDbContext();
            try
            {
                if (!await context.Routes.AnyAsync(r => r.RouteId == schedule.RouteId))
                    throw new ArgumentException("Invalid route ID.");
                if (!await context.Vehicles.AnyAsync(b => b.VehicleId == schedule.BusId))
                    throw new ArgumentException("Invalid bus ID.");
                if (!await context.Drivers.AnyAsync(d => d.DriverId == schedule.DriverId))
                    throw new ArgumentException("Invalid driver ID.");

                await context.Schedules.AddAsync(schedule);
                await context.SaveChangesAsync();
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task UpdateScheduleAsync(Schedule schedule)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            var context = _contextFactory.CreateWriteDbContext();
            try
            {
                var existing = await context.Schedules.FindAsync(schedule.ScheduleId);
                if (existing == null)
                    throw new InvalidOperationException("Schedule not found.");
                if (schedule.DepartureTime >= schedule.ArrivalTime)
                    throw new ArgumentException("Departure time must be before arrival time.");
                if (!await context.Routes.AnyAsync(r => r.RouteId == schedule.RouteId))
                    throw new ArgumentException("Invalid route ID.");
                if (!await context.Vehicles.AnyAsync(b => b.VehicleId == schedule.BusId))
                    throw new ArgumentException("Invalid bus ID.");
                if (!await context.Drivers.AnyAsync(d => d.DriverId == schedule.DriverId))
                    throw new ArgumentException("Invalid driver ID.");

                context.Entry(existing).CurrentValues.SetValues(schedule);
                await context.SaveChangesAsync();
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        public async Task DeleteScheduleAsync(int id)
        {
            var context = _contextFactory.CreateWriteDbContext();
            try
            {
                var schedule = await context.Schedules.FindAsync(id);
                if (schedule == null)
                    return;

                context.Schedules.Remove(schedule);
                await context.SaveChangesAsync();
            }
            finally
            {
                await context.DisposeAsync();
            }
        }
    }
}

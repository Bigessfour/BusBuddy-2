using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Core.Services
{
    public class DriverService : IDriverService
    {
        private readonly BusBuddyDbContext _context;

        public DriverService(BusBuddyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Driver>> GetAllDriversAsync()
        {
            return await _context.Drivers.ToListAsync();
        }

        public async Task<Driver?> GetDriverByIdAsync(int driverId)
        {
            return await _context.Drivers.FindAsync(driverId);
        }

        public async Task<Driver> AddDriverAsync(Driver driver)
        {
            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();
            return driver;
        }

        public async Task<bool> UpdateDriverAsync(Driver driver)
        {
            _context.Entry(driver).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Drivers.Any(e => e.DriverId == driver.DriverId))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteDriverAsync(int driverId)
        {
            var driver = await _context.Drivers.FindAsync(driverId);
            if (driver == null)
            {
                return false;
            }

            _context.Drivers.Remove(driver);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Core.Services
{
    public class DriverService : IDriverService
    {
        private readonly IBusBuddyDbContextFactory _contextFactory;

        public DriverService(IBusBuddyDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Driver>> GetAllDriversAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Drivers
                .AsNoTracking()
                .Select(d => new Driver
                {
                    DriverId = d.DriverId,
                    DriverName = d.DriverName,
                    DriverPhone = d.DriverPhone,
                    DriverEmail = d.DriverEmail,
                    Address = d.Address,
                    City = d.City,
                    State = d.State,
                    Zip = d.Zip,
                    DriversLicenceType = d.DriversLicenceType,
                    TrainingComplete = d.TrainingComplete,
                    Status = d.Status,
                    LicenseNumber = d.LicenseNumber,
                    LicenseExpiryDate = d.LicenseExpiryDate,
                    HireDate = d.HireDate
                    // Only including fields likely needed for display in grids or lists
                })
                .ToListAsync();
        }

        public async Task<Driver?> GetDriverByIdAsync(int driverId)
        {
            using var context = _contextFactory.CreateDbContext();
            // For a single entity lookup, we need all properties since this 
            // is likely for detail views or editing
            return await context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DriverId == driverId);
        }

        public async Task<Driver> AddDriverAsync(Driver driver)
        {
            using var context = _contextFactory.CreateWriteDbContext();
            context.Drivers.Add(driver);
            await context.SaveChangesAsync();
            return driver;
        }

        public async Task<bool> UpdateDriverAsync(Driver driver)
        {
            using var context = _contextFactory.CreateWriteDbContext();
            context.Entry(driver).State = EntityState.Modified;
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!context.Drivers.Any(e => e.DriverId == driver.DriverId))
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
            using var context = _contextFactory.CreateWriteDbContext();
            var driver = await context.Drivers.FindAsync(driverId);
            if (driver == null)
            {
                return false;
            }

            context.Drivers.Remove(driver);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

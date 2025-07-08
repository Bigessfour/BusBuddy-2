using BusBuddy.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusBuddy.Core.Services
{
    public interface IDriverService
    {
        Task<List<Driver>> GetAllDriversAsync();
        Task<Driver?> GetDriverByIdAsync(int driverId);
        Task<Driver> AddDriverAsync(Driver driver);
        Task<bool> UpdateDriverAsync(Driver driver);
        Task<bool> DeleteDriverAsync(int driverId);
    }
}

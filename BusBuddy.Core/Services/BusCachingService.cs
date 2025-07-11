using BusBuddy.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BusBuddy.Core.Services
{
    /// <summary>
    /// Service to cache bus entity data and reduce redundant database queries
    /// </summary>
    public class BusCachingService : IBusCachingService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<BusCachingService> _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        // Cache keys
        private const string ALL_BUSES_KEY = "AllBuses";
        private const string BUS_ENTITY_PREFIX = "BusEntity_";

        // Cache durations
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _extendedCacheDuration = TimeSpan.FromMinutes(15);

        public BusCachingService(IMemoryCache cache, ILogger<BusCachingService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all buses from cache or using the provided factory method
        /// </summary>
        public async Task<List<Bus>> GetAllBusesAsync(Func<Task<List<Bus>>> factory)
        {
            if (_cache.TryGetValue(ALL_BUSES_KEY, out List<Bus>? buses))
            {
                _logger.LogInformation("Retrieved all buses from cache");
                return buses ?? new List<Bus>();
            }

            try
            {
                await _semaphore.WaitAsync();

                // Double-check after acquiring semaphore
                if (_cache.TryGetValue(ALL_BUSES_KEY, out buses) && buses != null)
                {
                    _logger.LogInformation("Retrieved all buses from cache after semaphore");
                    return buses;
                }

                try
                {
                    // Cache miss - get from database
                    buses = await factory();

                    // Ensure no null collections exist
                    buses ??= new List<Bus>();

                    // Validate the data before caching
                    foreach (var bus in buses)
                    {
                        // Ensure string properties are not null
                        bus.BusNumber ??= string.Empty;
                        bus.Make ??= string.Empty;
                        bus.Model ??= string.Empty;
                        bus.VINNumber ??= string.Empty;
                        bus.LicenseNumber ??= string.Empty;
                        bus.Status ??= "Active";
                    }

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_cacheDuration)
                        .SetPriority(CacheItemPriority.High);

                    _cache.Set(ALL_BUSES_KEY, buses, cacheOptions);
                    _logger.LogInformation("Added all buses to cache");
                }
                catch (System.Data.SqlTypes.SqlNullValueException ex)
                {
                    _logger.LogWarning(ex, "SQL NULL value error when retrieving buses. Returning empty list to avoid application failure.");
                    buses = new List<Bus>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving buses for caching");
                    throw;
                }

                return buses ?? new List<Bus>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Get bus by ID from cache or using the provided factory method
        /// </summary>
        public async Task<Bus?> GetBusByIdAsync(int busId, Func<int, Task<Bus?>> factory)
        {
            var cacheKey = $"{BUS_ENTITY_PREFIX}{busId}";

            if (_cache.TryGetValue(cacheKey, out Bus? bus))
            {
                _logger.LogInformation("Retrieved bus ID {BusId} from cache", busId);
                return bus;
            }

            try
            {
                await _semaphore.WaitAsync();

                // Double-check after acquiring semaphore
                if (_cache.TryGetValue(cacheKey, out bus) && bus != null)
                {
                    _logger.LogInformation("Retrieved bus ID {BusId} from cache after semaphore", busId);
                    return bus;
                }

                // Cache miss - get from database
                bus = await factory(busId);

                if (bus != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_extendedCacheDuration)
                        .SetPriority(CacheItemPriority.Normal);

                    _cache.Set(cacheKey, bus, cacheOptions);
                    _logger.LogInformation("Added bus ID {BusId} to cache", busId);
                }

                return bus;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Invalidate the cache for a specific bus and the all buses collection
        /// </summary>
        public void InvalidateBusCache(int busId)
        {
            _logger.LogInformation("Invalidating cache for bus ID {BusId}", busId);
            _cache.Remove($"{BUS_ENTITY_PREFIX}{busId}");
            _cache.Remove(ALL_BUSES_KEY);
        }

        /// <summary>
        /// Invalidate the entire bus cache
        /// </summary>
        public void InvalidateAllBusCache()
        {
            _logger.LogInformation("Invalidating all bus caches");
            _cache.Remove(ALL_BUSES_KEY);
        }
    }

    public interface IBusCachingService
    {
        Task<List<Bus>> GetAllBusesAsync(Func<Task<List<Bus>>> factory);
        Task<Bus?> GetBusByIdAsync(int busId, Func<int, Task<Bus?>> factory);
        void InvalidateBusCache(int busId);
        void InvalidateAllBusCache();
    }
}

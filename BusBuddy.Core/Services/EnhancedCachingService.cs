using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BusBuddy.Core.Services
{
    public interface IEnhancedCachingService
    {
        Task<IReadOnlyList<Bus>> GetAllBusesAsync(Func<Task<IEnumerable<Bus>>> fetchFunc);
        Task<IReadOnlyList<Driver>> GetAllDriversAsync(Func<Task<IEnumerable<Driver>>> fetchFunc);
        Task<IReadOnlyList<Route>> GetAllRoutesAsync(Func<Task<IEnumerable<Route>>> fetchFunc);
        Task<IReadOnlyList<Student>> GetAllStudentsAsync(Func<Task<IEnumerable<Student>>> fetchFunc);
        Task<IReadOnlyDictionary<string, int>> GetDashboardMetricsAsync(Func<Task<Dictionary<string, int>>> fetchFunc);
        Task<Dictionary<string, int>> GetCachedDashboardMetricsAsync();
        void SetDashboardMetricsDirectly(Dictionary<string, int> metrics);
        void InvalidateCache(string key);
        void InvalidateAllCaches();
    }

    public class EnhancedCachingService : IEnhancedCachingService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<EnhancedCachingService> _logger;
        private static readonly TimeSpan DefaultCacheTime = TimeSpan.FromMinutes(15);
        private static readonly HashSet<string> _allCacheKeys = new HashSet<string>();
        private static readonly object _cacheLock = new object();

        public EnhancedCachingService(IMemoryCache cache, ILogger<EnhancedCachingService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("EnhancedCachingService initialized");
        }

        public async Task<IReadOnlyList<Bus>> GetAllBusesAsync(Func<Task<IEnumerable<Bus>>> fetchFunc)
        {
            return await GetOrAddCacheAsync("AllBuses", fetchFunc);
        }

        public async Task<IReadOnlyList<Driver>> GetAllDriversAsync(Func<Task<IEnumerable<Driver>>> fetchFunc)
        {
            return await GetOrAddCacheAsync("AllDrivers", fetchFunc);
        }

        public async Task<IReadOnlyList<Route>> GetAllRoutesAsync(Func<Task<IEnumerable<Route>>> fetchFunc)
        {
            return await GetOrAddCacheAsync("AllRoutes", fetchFunc);
        }

        public async Task<IReadOnlyList<Student>> GetAllStudentsAsync(Func<Task<IEnumerable<Student>>> fetchFunc)
        {
            return await GetOrAddCacheAsync("AllStudents", fetchFunc);
        }

        public Task<Dictionary<string, int>> GetCachedDashboardMetricsAsync()
        {
            const string cacheKey = "DashboardMetrics";
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, int>? cachedResult) && cachedResult != null)
            {
                _logger.LogDebug("Cache hit for {Key}, returning cached dashboard metrics", cacheKey);
                return Task.FromResult(cachedResult);
            }

            _logger.LogDebug("No cached dashboard metrics found");
            return Task.FromResult(new Dictionary<string, int>());
        }

        public async Task<IReadOnlyDictionary<string, int>> GetDashboardMetricsAsync(Func<Task<Dictionary<string, int>>> fetchFunc)
        {
            const string cacheKey = "DashboardMetrics";
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, int>? cachedResult) && cachedResult != null)
            {
                _logger.LogDebug("Cache hit for {Key}, returned dashboard metrics", cacheKey);
                return cachedResult;
            }

            _logger.LogInformation("Cache miss for {Key}, fetching dashboard metrics", cacheKey);
            var stopwatch = Stopwatch.StartNew();
            var result = await fetchFunc();
            stopwatch.Stop();

            _logger.LogInformation("Fetched dashboard metrics in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            // Add to cache with a shorter expiration for metrics
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, result, cacheEntryOptions);

            // Track this key
            lock (_cacheLock)
            {
                _allCacheKeys.Add(cacheKey);
            }

            return result;
        }

        public void SetDashboardMetricsDirectly(Dictionary<string, int> metrics)
        {
            if (metrics == null || !metrics.Any())
            {
                _logger.LogWarning("Attempted to cache empty dashboard metrics");
                return;
            }

            const string cacheKey = "DashboardMetrics";
            _logger.LogInformation("Directly setting dashboard metrics cache with {Count} values", metrics.Count);

            // Add to cache with a shorter expiration for metrics
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, metrics, cacheEntryOptions);

            // Track this key
            lock (_cacheLock)
            {
                _allCacheKeys.Add(cacheKey);
            }

            _logger.LogDebug("Dashboard metrics cached: {Metrics}",
                string.Join(", ", metrics.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        public void InvalidateCache(string key)
        {
            _logger.LogInformation("Invalidating cache for key: {Key}", key);
            _cache.Remove(key);

            lock (_cacheLock)
            {
                _allCacheKeys.Remove(key);
            }
        }

        public void InvalidateAllCaches()
        {
            _logger.LogInformation("Invalidating all caches");

            lock (_cacheLock)
            {
                foreach (var key in _allCacheKeys)
                {
                    _cache.Remove(key);
                }
                _allCacheKeys.Clear();
            }
        }

        private async Task<IReadOnlyList<T>> GetOrAddCacheAsync<T>(string key, Func<Task<IEnumerable<T>>> fetchFunc)
        {
            if (_cache.TryGetValue(key, out IReadOnlyList<T>? cachedResult) && cachedResult != null)
            {
                _logger.LogDebug("Cache hit for {Key}, returned {Count} items", key, cachedResult.Count);
                return cachedResult;
            }

            _logger.LogInformation("Cache miss for {Key}, fetching data", key);
            var stopwatch = Stopwatch.StartNew();
            var result = (await fetchFunc()).ToList();
            stopwatch.Stop();

            _logger.LogInformation("Fetched {Count} items for {Key} in {ElapsedMs}ms",
                result.Count, key, stopwatch.ElapsedMilliseconds);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(DefaultCacheTime);

            _cache.Set(key, result, cacheEntryOptions);

            // Track this key
            lock (_cacheLock)
            {
                _allCacheKeys.Add(key);
            }

            return result;
        }
    }
}

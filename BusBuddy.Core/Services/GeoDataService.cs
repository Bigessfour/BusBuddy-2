using System.Net.Http;
using System.Net.Http.Headers;

namespace BusBuddy.Core.Services
{
    public class GeoDataService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _geeApiBaseUrl;
        private readonly string _geeAccessToken;
        private bool _disposed;

        public GeoDataService(string geeApiBaseUrl, string geeAccessToken)
        {
            _httpClient = new HttpClient();
            _geeApiBaseUrl = geeApiBaseUrl;
            _geeAccessToken = geeAccessToken;
        }

        public async Task<string> GetGeoJsonAsync(string assetId)
        {
            // Example GEE REST API call for a FeatureCollection asset
            var url = $"{_geeApiBaseUrl}/v1beta/projects/earthengine-public/assets/{assetId}:exportGeoJson";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _geeAccessToken);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var geoJson = await response.Content.ReadAsStringAsync();
            return geoJson;
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose method
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }

        // Add additional methods for imagery, tiles, etc. as needed
    }
}

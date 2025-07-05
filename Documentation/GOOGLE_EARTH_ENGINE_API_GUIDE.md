# Google Earth Engine API Integration Guide for Bus Buddy

## Overview
This guide provides comprehensive documentation for integrating Google Earth Engine (GEE) API with the Bus Buddy transportation management system for advanced route mapping and geospatial analysis.

## API References

### Primary Documentation
- **Google Earth Engine Code Editor**: https://code.earthengine.google.com/
- **Google Cloud Recommender API**: https://cloud.google.com/recommender/docs/quickstart-recommendation-hub?_gl=1*3nxflk*_ga*MTc0MDMzNDgyMC4xNzUxNjc2Mzcy*_ga_WH2QY8WWF5*czE3NTE2NzYzNzEkbzEkZzEkdDE3NTE2NzY3NTEkajQ4JGwwJGgw
- **GEE JavaScript API**: https://developers.google.com/earth-engine/apidocs
- **GEE Python API**: https://developers.google.com/earth-engine/guides/python_install
- **Google Cloud Platform Console**: https://console.cloud.google.com/

### Key API Endpoints
- **Authentication**: `https://earthengine.googleapis.com/v1alpha/authenticate`
- **Data Catalog**: `https://earthengine.googleapis.com/v1alpha/projects/{project}/assets`
- **Image Processing**: `https://earthengine.googleapis.com/v1alpha/projects/{project}/images`
- **Vector Processing**: `https://earthengine.googleapis.com/v1alpha/projects/{project}/tables`

## Authentication Setup

### 1. Google Cloud Project Setup
```bash
# Install Google Cloud SDK
# Download from: https://cloud.google.com/sdk/docs/install

# Initialize gcloud
gcloud init

# Set your project
gcloud config set project your-project-id

# Enable Earth Engine API
gcloud services enable earthengine.googleapis.com
```

### 2. Service Account Creation
```bash
# Create service account
gcloud iam service-accounts create bus-buddy-gee \
    --description="Bus Buddy Google Earth Engine Service Account" \
    --display-name="Bus Buddy GEE"

# Create and download key
gcloud iam service-accounts keys create bus-buddy-gee-key.json \
    --iam-account=bus-buddy-gee@your-project-id.iam.gserviceaccount.com

# Grant necessary permissions
gcloud projects add-iam-policy-binding your-project-id \
    --member="serviceAccount:bus-buddy-gee@your-project-id.iam.gserviceaccount.com" \
    --role="roles/earthengine.reader"
```

### 3. Environment Configuration
```json
// appsettings.json
{
  "GoogleEarthEngine": {
    "ProjectId": "your-project-id",
    "ServiceAccountKeyPath": "path/to/bus-buddy-gee-key.json",
    "BaseUrl": "https://earthengine.googleapis.com/v1alpha",
    "CodeEditorUrl": "https://code.earthengine.google.com/",
    "TimeoutSeconds": 30,
    "RetryAttempts": 3
  }
}
```

## C# Integration Examples

### 1. Authentication Service
```csharp
using Google.Apis.Auth.OAuth2;
using Google.Apis.EarthEngine.v1alpha;
using Google.Apis.Services;

public class GoogleEarthEngineAuthService
{
    private readonly IConfiguration _configuration;
    private GoogleCredential _credential;
    
    public async Task<GoogleCredential> GetCredentialAsync()
    {
        if (_credential == null)
        {
            var keyPath = _configuration["GoogleEarthEngine:ServiceAccountKeyPath"];
            _credential = GoogleCredential.FromFile(keyPath)
                .CreateScoped(EarthEngineService.Scope.CloudPlatform);
        }
        return _credential;
    }
    
    public async Task<EarthEngineService> GetServiceAsync()
    {
        var credential = await GetCredentialAsync();
        return new EarthEngineService(new BaseClientService.Initializer()
        {
            HttpClientCredential = credential,
            ApplicationName = "Bus Buddy Transportation System"
        });
    }
}
```

### 2. Route Optimization with GEE
```csharp
public class RouteOptimizationService
{
    private readonly GoogleEarthEngineService _geeService;
    
    public async Task<OptimizedRoute> OptimizeRouteAsync(List<GeoPoint> waypoints)
    {
        // Get terrain data
        var terrainData = await _geeService.GetTerrainDataAsync(waypoints);
        
        // Get traffic patterns
        var trafficData = await _geeService.GetTrafficPatternsAsync(waypoints);
        
        // Calculate optimal path
        var optimizedPath = CalculateOptimalPath(waypoints, terrainData, trafficData);
        
        return new OptimizedRoute
        {
            Path = optimizedPath,
            EstimatedTravelTime = CalculateTravelTime(optimizedPath, trafficData),
            FuelConsumption = CalculateFuelConsumption(optimizedPath, terrainData),
            SafetyScore = CalculateSafetyScore(optimizedPath)
        };
    }
}
```

### 3. Real-Time Satellite Imagery
```csharp
public class SatelliteImageryService
{
    public async Task<SatelliteImage> GetLatestImageryAsync(BoundingBox area)
    {
        var request = new ImageRequest
        {
            Area = area,
            Date = DateTime.UtcNow.AddDays(-7), // Latest imagery within 7 days
            Resolution = 10, // 10m resolution
            Bands = new[] { "B4", "B3", "B2" } // RGB bands
        };
        
        return await _geeService.GetImageryAsync(request);
    }
}
```

## JavaScript API Integration

### 1. Code Editor Scripts
```javascript
// Bus Route Analysis Script
// To be run in Google Earth Engine Code Editor

// Define study area (example: school district boundaries)
var studyArea = ee.Geometry.Rectangle([-122.5, 37.7, -122.3, 37.9]);

// Load road network data
var roads = ee.FeatureCollection("projects/google/road_network")
  .filterBounds(studyArea);

// Load elevation data
var elevation = ee.Image("USGS/SRTMGL1_003")
  .clip(studyArea);

// Load population density
var population = ee.ImageCollection("WorldPop/GP/100m/pop")
  .filterDate('2020-01-01', '2020-12-31')
  .first()
  .clip(studyArea);

// Analyze optimal bus stop locations
var optimalStops = analyzeOptimalBusStops(population, roads, elevation);

// Export results
Export.table.toDrive({
  collection: optimalStops,
  description: 'optimal_bus_stops',
  fileFormat: 'GeoJSON'
});

function analyzeOptimalBusStops(population, roads, elevation) {
  // Population density analysis
  var highDensityAreas = population.gt(1000);
  
  // Accessibility analysis (flat terrain preferred)
  var accessibleAreas = elevation.lt(50); // Less than 50m slope
  
  // Road proximity analysis
  var roadBuffer = roads.distance(100); // 100m buffer around roads
  
  // Combine criteria
  var suitability = highDensityAreas
    .and(accessibleAreas)
    .and(roadBuffer.lt(100));
    
  // Generate optimal points
  return suitability.selfMask()
    .sample({
      region: studyArea,
      scale: 100,
      numPixels: 50
    });
}
```

### 2. Weather Integration
```javascript
// Weather Analysis for Route Planning
var weatherData = ee.ImageCollection("NOAA/GFS0P25")
  .filterDate(ee.Date.fromYMD(2025, 1, 1), ee.Date.fromYMD(2025, 12, 31))
  .select(['temperature_2m_above_ground', 'precipitation_rate']);

// Analyze weather patterns affecting bus routes
var weatherImpact = weatherData.map(function(image) {
  var temp = image.select('temperature_2m_above_ground');
  var precip = image.select('precipitation_rate');
  
  // Define hazardous conditions
  var icyConditions = temp.lt(273.15); // Below 0°C
  var heavyRain = precip.gt(5); // >5mm/hr
  
  return icyConditions.or(heavyRain).rename('hazardous');
});

// Generate weather alerts for routes
var weatherAlerts = weatherImpact.reduce(ee.Reducer.frequencyHistogram());
```

## Route Analysis Algorithms

### 1. Multi-Criteria Route Optimization
```csharp
public class RouteAnalysisAlgorithms
{
    public struct RouteMetrics
    {
        public double Distance { get; set; }
        public double ElevationGain { get; set; }
        public double TrafficDensity { get; set; }
        public double SafetyScore { get; set; }
        public double WeatherRisk { get; set; }
    }
    
    public async Task<List<RouteOption>> GenerateRouteOptionsAsync(
        GeoPoint start, 
        GeoPoint end, 
        List<GeoPoint> requiredStops)
    {
        var routes = new List<RouteOption>();
        
        // Generate multiple route alternatives
        var directRoute = await CalculateDirectRoute(start, end, requiredStops);
        var scenicRoute = await CalculateScenicRoute(start, end, requiredStops);
        var efficientRoute = await CalculateEfficientRoute(start, end, requiredStops);
        
        routes.AddRange(new[] { directRoute, scenicRoute, efficientRoute });
        
        // Score each route
        foreach (var route in routes)
        {
            route.Score = await CalculateRouteScore(route);
        }
        
        return routes.OrderByDescending(r => r.Score).ToList();
    }
    
    private async Task<double> CalculateRouteScore(RouteOption route)
    {
        var metrics = await GetRouteMetrics(route);
        
        // Weighted scoring algorithm
        var score = 
            (1.0 / Math.Max(metrics.Distance, 1)) * 0.3 +           // Shorter is better
            (1.0 / Math.Max(metrics.ElevationGain, 1)) * 0.2 +      // Flatter is better
            (1.0 / Math.Max(metrics.TrafficDensity, 1)) * 0.2 +     // Less traffic is better
            metrics.SafetyScore * 0.2 +                             // Higher safety is better
            (1.0 / Math.Max(metrics.WeatherRisk, 1)) * 0.1;         // Lower weather risk is better
            
        return score;
    }
}
```

### 2. Real-Time Traffic Integration
```csharp
public class TrafficAnalysisService
{
    public async Task<TrafficData> GetCurrentTrafficAsync(List<GeoPoint> routePoints)
    {
        // Use GEE to analyze current traffic patterns
        var script = @"
            var trafficData = ee.ImageCollection('projects/google/traffic_data')
                .filterDate(ee.Date.fromYMD({year}, {month}, {day}))
                .filterBounds(ee.Geometry.LineString({coordinates}));
            
            var avgTraffic = trafficData.reduce(ee.Reducer.mean());
            return avgTraffic.getInfo();
        ";
        
        var result = await _geeService.ExecuteScriptAsync(script, new
        {
            year = DateTime.Now.Year,
            month = DateTime.Now.Month,
            day = DateTime.Now.Day,
            coordinates = routePoints.Select(p => new[] { p.Longitude, p.Latitude }).ToArray()
        });
        
        return ParseTrafficData(result);
    }
}
```

## Troubleshooting Guide

### Common Issues and Solutions

#### 1. Authentication Errors
**Problem**: `401 Unauthorized` or `403 Forbidden` errors
**Solutions**:
```bash
# Verify service account permissions
gcloud projects get-iam-policy your-project-id

# Re-authenticate
gcloud auth activate-service-account --key-file=bus-buddy-gee-key.json

# Check Earth Engine access
gcloud auth list
```

#### 2. Quota Exceeded Errors
**Problem**: `429 Too Many Requests` or quota exceeded messages
**Solutions**:
- Implement exponential backoff retry logic
- Cache frequently requested data
- Optimize query complexity
- Request quota increase from Google Cloud Console

```csharp
public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
{
    var retryAttempts = 3;
    var baseDelay = TimeSpan.FromSeconds(1);
    
    for (int i = 0; i < retryAttempts; i++)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            if (i == retryAttempts - 1) throw;
            
            var delay = TimeSpan.FromTicks(baseDelay.Ticks * (long)Math.Pow(2, i));
            await Task.Delay(delay);
        }
    }
    
    throw new InvalidOperationException("Max retry attempts exceeded");
}
```

#### 3. Memory and Performance Issues
**Problem**: Slow response times or memory errors
**Solutions**:
- Reduce image resolution for initial analysis
- Use `.limit()` on large collections
- Implement server-side filtering
- Use `.select()` to get only needed bands

```javascript
// Optimized data loading
var optimizedImagery = ee.ImageCollection("LANDSAT/LC08/C01/T1_SR")
  .filterDate('2023-01-01', '2023-12-31')
  .filterBounds(studyArea)
  .limit(10)  // Limit collection size
  .select(['B4', 'B3', 'B2'])  // Only RGB bands
  .map(function(image) {
    return image.reproject('EPSG:4326', null, 30);  // Consistent projection
  });
```

#### 4. Network Connectivity Issues
**Problem**: Connection timeouts or network errors
**Solutions**:
```csharp
public class ResilientGeeService
{
    private readonly HttpClient _httpClient;
    
    public ResilientGeeService()
    {
        _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
    }
    
    public async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        try
        {
            return await operation().WithCancellation(cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Google Earth Engine request timed out");
        }
    }
}
```

### Debugging Tools

#### 1. Enable Detailed Logging
```csharp
// In appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "GoogleEarthEngine": "Debug",
      "Bus_Buddy.Services.GoogleEarthEngineService": "Trace"
    }
  }
}
```

#### 2. API Response Monitoring
```csharp
public class GeeApiMonitor
{
    private readonly ILogger<GeeApiMonitor> _logger;
    
    public void LogApiCall(string operation, TimeSpan duration, bool success)
    {
        _logger.LogInformation(
            "GEE API Call: {Operation}, Duration: {Duration}ms, Success: {Success}",
            operation, duration.TotalMilliseconds, success);
    }
    
    public void LogApiError(string operation, Exception exception)
    {
        _logger.LogError(exception,
            "GEE API Error: {Operation}, Error: {Error}",
            operation, exception.Message);
    }
}
```

## Performance Optimization

### 1. Caching Strategy
```csharp
public class GeeDataCache
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromHours(1);
    
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
    {
        if (_cache.TryGetValue(key, out T cachedValue))
        {
            return cachedValue;
        }
        
        var value = await factory();
        _cache.Set(key, value, _defaultExpiry);
        return value;
    }
}
```

### 2. Batch Processing
```csharp
public async Task<List<RouteAnalysis>> AnalyzeMultipleRoutesAsync(List<Route> routes)
{
    const int batchSize = 5;
    var results = new List<RouteAnalysis>();
    
    for (int i = 0; i < routes.Count; i += batchSize)
    {
        var batch = routes.Skip(i).Take(batchSize);
        var batchTasks = batch.Select(AnalyzeSingleRouteAsync);
        var batchResults = await Task.WhenAll(batchTasks);
        results.AddRange(batchResults);
        
        // Rate limiting
        await Task.Delay(100);
    }
    
    return results;
}
```

## Data Export and Integration

### 1. Export to Common Formats
```javascript
// Export route analysis results
Export.table.toDrive({
  collection: routeAnalysis,
  description: 'bus_route_analysis',
  fileFormat: 'CSV',
  selectors: ['route_id', 'distance', 'elevation_gain', 'safety_score']
});

// Export imagery for offline use
Export.image.toDrive({
  image: satelliteImagery,
  description: 'route_satellite_imagery',
  scale: 30,
  region: studyArea,
  fileFormat: 'GeoTIFF'
});
```

### 2. Database Integration
```csharp
public class GeeDataImporter
{
    public async Task ImportRouteAnalysisAsync(string csvPath)
    {
        var records = await File.ReadAllLinesAsync(csvPath);
        var routeAnalyses = records.Skip(1).Select(ParseRouteAnalysisRecord);
        
        await _dbContext.RouteAnalyses.AddRangeAsync(routeAnalyses);
        await _dbContext.SaveChangesAsync();
    }
    
    private RouteAnalysis ParseRouteAnalysisRecord(string csvLine)
    {
        var fields = csvLine.Split(',');
        return new RouteAnalysis
        {
            RouteId = int.Parse(fields[0]),
            Distance = double.Parse(fields[1]),
            ElevationGain = double.Parse(fields[2]),
            SafetyScore = double.Parse(fields[3]),
            AnalysisDate = DateTime.UtcNow
        };
    }
}
```

## Security Best Practices

### 1. API Key Management
- Store service account keys securely (never in source control)
- Use environment variables or Azure Key Vault
- Rotate keys regularly
- Monitor API usage for anomalies

### 2. Access Control
```csharp
[Authorize(Roles = "Admin,RouteManager")]
public class RouteMapController : ControllerBase
{
    [HttpGet("satellite-imagery")]
    [RequirePermission("ViewSatelliteImagery")]
    public async Task<IActionResult> GetSatelliteImagery([FromQuery] BoundingBox area)
    {
        // Validate area bounds
        if (!IsValidArea(area))
        {
            return BadRequest("Invalid area bounds");
        }
        
        var imagery = await _geeService.GetImageryAsync(area);
        return Ok(imagery);
    }
}
```

## Monitoring and Alerting

### 1. Health Checks
```csharp
public class GeeHealthCheck : IHealthCheck
{
    private readonly GoogleEarthEngineService _geeService;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _geeService.TestConnectionAsync();
            return HealthCheckResult.Healthy("Google Earth Engine API is responsive");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Google Earth Engine API is not responsive", ex);
        }
    }
}
```

### 2. Performance Metrics
```csharp
public class GeeMetrics
{
    private static readonly Counter ApiCalls = Metrics
        .CreateCounter("gee_api_calls_total", "Total GEE API calls");
        
    private static readonly Histogram ApiDuration = Metrics
        .CreateHistogram("gee_api_duration_seconds", "GEE API call duration");
        
    public void RecordApiCall(string operation, TimeSpan duration)
    {
        ApiCalls.WithLabels(operation).Inc();
        ApiDuration.Observe(duration.TotalSeconds);
    }
}
```

## Support and Resources

### Documentation Links
- [Google Earth Engine Guides](https://developers.google.com/earth-engine/guides)
- [GEE JavaScript API](https://developers.google.com/earth-engine/apidocs)
- [Google Cloud Console](https://console.cloud.google.com/)
- [Earth Engine Data Catalog](https://developers.google.com/earth-engine/datasets)

### Community Support
- [GEE Developer Forum](https://developers.google.com/earth-engine/help)
- [Stack Overflow GEE Tag](https://stackoverflow.com/questions/tagged/google-earth-engine)
- [GEE GitHub Issues](https://github.com/google/earthengine-api/issues)

### Professional Support
- Google Cloud Support (for billing and quota issues)
- Google Earth Engine Premium Support (for enterprise customers)
- Bus Buddy Development Team: support@busbuddy.com

---

**Last Updated**: July 4, 2025
**Version**: 1.0
**Maintained By**: Bus Buddy Development Team

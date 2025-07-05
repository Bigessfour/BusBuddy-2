# Google Earth Engine Configuration Template

## Step-by-Step Setup Guide

### 1. Google Cloud Project Setup
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create new project or select existing project
3. Enable the following APIs:
   - Earth Engine API
   - Cloud Resource Manager API
   - Cloud Billing API (if using paid features)

### 2. Service Account Creation
```bash
# Set your project ID
export PROJECT_ID="your-bus-buddy-project"

# Create service account
gcloud iam service-accounts create bus-buddy-gee \
    --project=$PROJECT_ID \
    --description="Bus Buddy Google Earth Engine Service Account" \
    --display-name="Bus Buddy GEE"

# Create key file
gcloud iam service-accounts keys create ./keys/bus-buddy-gee-key.json \
    --iam-account=bus-buddy-gee@$PROJECT_ID.iam.gserviceaccount.com

# Grant Earth Engine permissions
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:bus-buddy-gee@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/earthengine.reader"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:bus-buddy-gee@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/earthengine.writer"
```

### 3. Update appsettings.json
```json
{
  "GoogleEarthEngine": {
    "ProjectId": "your-bus-buddy-project",
    "ServiceAccountKeyPath": "keys/bus-buddy-gee-key.json",
    "BaseUrl": "https://earthengine.googleapis.com/v1alpha",
    "CodeEditorUrl": "https://code.earthengine.google.com/",
    "TimeoutSeconds": 30,
    "RetryAttempts": 3,
    "CacheExpiryHours": 1,
    "MaxConcurrentRequests": 5
  },
  "RouteMapping": {
    "DefaultImageResolution": 30,
    "MaxRouteLength": 100000,
    "TerrainAnalysisEnabled": true,
    "TrafficAnalysisEnabled": true,
    "WeatherAnalysisEnabled": true,
    "SatelliteImageryEnabled": true
  }
}
```

### 4. Environment Variables (Alternative)
```bash
# For production deployment
export GEE_PROJECT_ID="your-bus-buddy-project"
export GEE_SERVICE_ACCOUNT_KEY_PATH="/secure/path/bus-buddy-gee-key.json"
export GEE_BASE_URL="https://earthengine.googleapis.com/v1alpha"
```

### 5. Docker Configuration (Optional)
```dockerfile
# Dockerfile additions for GEE
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install Google Cloud SDK
RUN apt-get update && apt-get install -y curl gnupg
RUN echo "deb [signed-by=/usr/share/keyrings/cloud.google.gpg] https://packages.cloud.google.com/apt cloud-sdk main" | tee -a /etc/apt/sources.list.d/google-cloud-sdk.list
RUN curl https://packages.cloud.google.com/apt/doc/apt-key.gpg | apt-key --keyring /usr/share/keyrings/cloud.google.gpg add -
RUN apt-get update && apt-get install -y google-cloud-sdk

# Copy service account key
COPY keys/bus-buddy-gee-key.json /app/keys/
```

### 6. Verification Script
```csharp
// Test script to verify GEE integration
public class GeeSetupVerification
{
    public async Task<bool> VerifySetupAsync()
    {
        var tests = new List<(string Name, Func<Task<bool>> Test)>
        {
            ("Authentication", TestAuthentication),
            ("API Access", TestApiAccess),
            ("Data Access", TestDataAccess),
            ("Image Processing", TestImageProcessing),
            ("Route Analysis", TestRouteAnalysis)
        };

        foreach (var (name, test) in tests)
        {
            try
            {
                var result = await test();
                Console.WriteLine($"✓ {name}: {(result ? "PASS" : "FAIL")}");
                if (!result) return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ {name}: ERROR - {ex.Message}");
                return false;
            }
        }

        Console.WriteLine("🎉 All GEE integration tests passed!");
        return true;
    }

    private async Task<bool> TestAuthentication()
    {
        var service = await _geeService.GetServiceAsync();
        return service != null;
    }

    private async Task<bool> TestApiAccess()
    {
        // Test basic API call
        return await _geeService.TestConnectionAsync();
    }

    private async Task<bool> TestDataAccess()
    {
        // Test data catalog access
        var datasets = await _geeService.GetAvailableDatasetsAsync();
        return datasets.Any();
    }

    private async Task<bool> TestImageProcessing()
    {
        // Test image processing capability
        var testArea = new BoundingBox(-122.5, 37.7, -122.3, 37.9);
        var imagery = await _geeService.GetImageryAsync(testArea);
        return imagery != null;
    }

    private async Task<bool> TestRouteAnalysis()
    {
        // Test route analysis functionality
        var testRoute = new List<GeoPoint>
        {
            new(-122.4, 37.8),
            new(-122.3, 37.9)
        };
        var analysis = await _routeAnalysisService.AnalyzeRouteAsync(testRoute);
        return analysis != null;
    }
}
```

## Quick Setup Checklist

- [ ] Google Cloud Project created
- [ ] Earth Engine API enabled
- [ ] Service account created with proper permissions
- [ ] Service account key downloaded and secured
- [ ] Configuration file updated
- [ ] Test authentication successful
- [ ] Sample data access working
- [ ] Route analysis functional
- [ ] Error handling implemented
- [ ] Logging configured
- [ ] Monitoring setup

## Security Checklist

- [ ] Service account key stored securely (not in source control)
- [ ] Minimum required permissions granted
- [ ] API usage monitored
- [ ] Rate limiting implemented
- [ ] Error logging configured
- [ ] Access logs reviewed regularly
- [ ] Key rotation schedule established

## Performance Optimization Checklist

- [ ] Caching implemented for frequent queries
- [ ] Request batching for multiple operations
- [ ] Connection pooling configured
- [ ] Retry logic with exponential backoff
- [ ] Timeout values optimized
- [ ] Monitoring metrics collected
- [ ] Performance benchmarks established

---

**Setup Support**: If you encounter issues during setup, contact support@busbuddy.com with your configuration details and error messages.

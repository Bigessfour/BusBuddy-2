# Google Earth Engine API - Quick Troubleshooting Reference

## Emergency Quick Fixes

### Authentication Issues
```bash
# Quick auth check
gcloud auth list
gcloud config list project

# Re-authenticate
gcloud auth activate-service-account --key-file=bus-buddy-gee-key.json
gcloud config set project your-project-id
```

### Common Error Codes
| Error Code | Meaning | Quick Fix |
|------------|---------|-----------|
| 401 | Unauthorized | Check service account key path |
| 403 | Forbidden | Verify Earth Engine API is enabled |
| 429 | Rate Limited | Add retry logic with exponential backoff |
| 500 | Server Error | Check GEE status page, retry later |

### API Status Check
- **GEE Status**: https://status.cloud.google.com/
- **Test Connection**: Run simple `.getInfo()` call in Code Editor

### Quick Performance Fixes
```javascript
// Reduce data size
.limit(10)
.select(['B4', 'B3', 'B2'])
.reproject('EPSG:4326', null, 30)

// Add filters early
.filterBounds(studyArea)
.filterDate(startDate, endDate)
```

### Emergency Contacts
- **Google Cloud Support**: https://cloud.google.com/support
- **GEE Forum**: https://developers.google.com/earth-engine/help
- **Bus Buddy Team**: support@busbuddy.com

## Code Snippets for Common Issues

### Connection Test
```csharp
public async Task<bool> TestGeeConnection()
{
    try
    {
        var service = await _geeService.GetServiceAsync();
        return true;
    }
    catch
    {
        return false;
    }
}
```

### Rate Limiting Handler
```csharp
public async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation)
{
    for (int i = 0; i < 3; i++)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
        }
    }
    throw new Exception("Max retries exceeded");
}
```

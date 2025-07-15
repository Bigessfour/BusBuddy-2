# BusBuddy Development Quick Reference

## 🚀 Quick Start (< 30 minutes)

### 1. **Environment Setup**
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj
```

### 2. **Database Setup**
```bash
# Update database with latest migrations
dotnet ef database update --project BusBuddy.Core --startup-project BusBuddy.WPF/BusBuddy.WPF.csproj

# Add new migration (if needed)
dotnet ef migrations add MigrationName --project BusBuddy.Core --startup-project BusBuddy.WPF/BusBuddy.WPF.csproj
```

## 🔧 Development Features

### **Environment Configuration**
- **Development Mode**: Set `"Environment": "Development"` in `appsettings.json`
- **Production Mode**: Set `"Environment": "Production"` in `appsettings.json`

### **Seed Data (Development Only)**
```csharp
// Available in DevelopmentModeService
await developmentService.SeedDataAsync();        // Create sample data
await developmentService.ClearSeedDataAsync();   // Clear sample data
```

### **Performance Monitoring**
- **Memory Usage**: Automatically tracked and logged
- **Query Performance**: Activity logs optimized with pagination
- **Thresholds**:
  - Development: 200MB memory warning
  - Production: 500MB memory warning

### **Logging Levels**
- **Development**: Debug level (verbose)
- **Production**: Information level (minimal)

## 🐛 Troubleshooting

### **Common Issues Fixed**
1. ✅ **DriversLicenceType SQL Error**: Fixed column name mismatch
2. ✅ **Slow Activity Logs**: Added database index on Timestamp
3. ✅ **Repeated Error Attempts**: Added session-based prevention

### **Empty Data Solutions**
- **No Activity Logs**: Use `SeedDataService.SeedActivityLogsAsync(100)`
- **No Drivers**: Use `SeedDataService.SeedDriversAsync(15)`
- **No Records**: Use `SeedDataService.SeedAllAsync()`

### **Performance Issues**
- **High Memory (>250MB)**: Check for memory leaks in Syncfusion components
- **Slow Queries**: Verify database indexes are applied
- **UI Lag**: Reduce page sizes or enable pagination

### **Database Issues**
- **Connection Errors**: Check `DATABASE_CONNECTION_STRING` environment variable
- **Schema Errors**: Run `dotnet ef database update`
- **Migration Errors**: Check EF Core model configuration

## 📊 Performance Optimization

### **Activity Logs**
```csharp
// Use pagination instead of loading all records
var logs = await activityService.GetLogsPagedAsync(pageNumber: 1, pageSize: 50);

// Or use the standard method with limits
var logs = await activityService.GetLogsAsync(count: 100);
```

### **Memory Management**
- Monitor memory usage in logs
- Current usage: `PerformanceMonitor.GetCurrentMemoryUsageMB()`
- Automatic warnings at threshold levels

### **Database Optimization**
- **Indexes**: ActivityLog.Timestamp (descending)
- **Pagination**: Default 50 records per page in development
- **Caching**: Entity caching enabled for frequently accessed data

## 🎯 Development Workflow

1. **Set Environment**: `"Environment": "Development"`
2. **Run Migrations**: `dotnet ef database update`
3. **Seed Data**: Call `SeedDataService.SeedAllAsync()` if needed
4. **Monitor Performance**: Check logs for memory/query warnings
5. **Test Features**: Use sample data for testing
6. **Clean Up**: Call `ClearSeedDataAsync()` when done

## 🔍 Log Analysis

### **Key Log Patterns**
- `[MEMORY_WARNING]`: High memory usage detected
- `[ACTIVITY_PERF]`: Slow activity log operations
- `[ERROR]`: Database or application errors
- `📊 Error:`: Aggregated error summaries

### **Log Retention**
- **General Logs**: 7 days
- **Error Logs**: 30 days
- **Location**: `logs/` directory

## 🚨 Production Checklist

- [ ] Set `"Environment": "Production"`
- [ ] Verify database connection string
- [ ] Apply all migrations
- [ ] Monitor memory usage
- [ ] Check log retention policies
- [ ] Disable seed data features
- [ ] Verify Syncfusion licensing

## 💡 Tips

- **Single User**: App runs locally, no server needed
- **Syncfusion**: Uses Office2019DarkGray theme by default
- **Azure SQL**: Configured for cloud database
- **Local Testing**: Use LocalConnection for development
- **Memory Monitoring**: Built-in tracking and warnings
- **Performance**: Optimized for <30 second startup

---

*For issues or questions, check the logs first, then refer to the troubleshooting section above.*

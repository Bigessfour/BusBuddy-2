# BusBuddy API Reference Documentation

**Date:** July 10, 2025  
**Version:** 1.0  
**Target:** Developers & Integration Teams

## 📋 Overview

This document provides comprehensive API reference for the BusBuddy school bus transportation management system, covering all service layers, data models, and integration points.

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   WPF Views     │    │   ViewModels    │    │   Services      │
│                 │◄───┤                 │◄───┤                 │
│ - Dashboard     │    │ - DashboardVM   │    │ - BusService    │
│ - BusManagement │    │ - BusManagementVM│    │ - DriverService │
│ - etc.          │    │ - etc.          │    │ - etc.          │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                        │
                                               ┌─────────────────┐
                                               │  Repositories   │
                                               │                 │
                                               │ - BusRepository │
                                               │ - DriverRepo    │
                                               │ - etc.          │
                                               └─────────────────┘
                                                        │
                                               ┌─────────────────┐
                                               │   Data Layer    │
                                               │                 │
                                               │ - DbContext     │
                                               │ - Entity Models │
                                               │ - SQL Server    │
                                               └─────────────────┘
```

## 🚌 Bus Management API

### IBusService Interface

**Namespace:** `BusBuddy.Core.Services.Interfaces`

#### Methods

##### GetAllBusesAsync()
```csharp
Task<IEnumerable<Bus>> GetAllBusesAsync()
```
**Description:** Retrieves all buses in the fleet  
**Returns:** Collection of Bus entities  
**Throws:** `DatabaseException` if database connection fails

**Usage Example:**
```csharp
var busService = serviceProvider.GetRequiredService<IBusService>();
var buses = await busService.GetAllBusesAsync();
```

##### GetBusByIdAsync(int id)
```csharp
Task<Bus?> GetBusByIdAsync(int id)
```
**Parameters:**
- `id` (int): Vehicle ID of the bus

**Returns:** Bus entity or null if not found  
**Throws:** `ArgumentException` if id is invalid

##### AddBusAsync(Bus bus)
```csharp
Task<Bus> AddBusAsync(Bus bus)
```
**Parameters:**
- `bus` (Bus): Bus entity to add

**Returns:** Added bus with generated ID  
**Throws:** 
- `ValidationException` if bus data is invalid
- `DuplicateException` if bus number already exists

**Validation Rules:**
- BusNumber: Required, max 20 characters, unique
- Make: Required, max 50 characters
- Model: Required, max 50 characters
- Year: 1900-2030
- SeatingCapacity: 1-90

##### UpdateBusAsync(Bus bus)
```csharp
Task<Bus> UpdateBusAsync(Bus bus)
```
**Parameters:**
- `bus` (Bus): Bus entity with updates

**Returns:** Updated bus entity  
**Throws:** 
- `NotFoundException` if bus doesn't exist
- `ValidationException` if data is invalid

##### DeleteBusAsync(int id)
```csharp
Task<bool> DeleteBusAsync(int id)
```
**Parameters:**
- `id` (int): Vehicle ID to delete

**Returns:** True if deleted successfully  
**Throws:** `NotFoundException` if bus doesn't exist

### Bus Entity Model

**Namespace:** `BusBuddy.Core.Models`

```csharp
public class Bus : INotifyPropertyChanged
{
    [Key]
    public int VehicleId { get; set; }
    
    [Required, StringLength(20)]
    public string BusNumber { get; set; }
    
    [Required, Range(1900, 2030)]
    public int Year { get; set; }
    
    [Required, StringLength(50)]
    public string Make { get; set; }
    
    [Required, StringLength(50)]
    public string Model { get; set; }
    
    [Required, Range(1, 90)]
    public int SeatingCapacity { get; set; }
    
    [Required, StringLength(17)]
    public string VINNumber { get; set; }
    
    [Required, StringLength(20)]
    public string LicenseNumber { get; set; }
    
    public DateTime? DateLastInspection { get; set; }
    public int? CurrentOdometer { get; set; }
    public string Status { get; set; } = "Active";
    
    // Navigation Properties
    public virtual ICollection<Route> AMRoutes { get; set; }
    public virtual ICollection<Route> PMRoutes { get; set; }
    public virtual ICollection<Fuel> FuelRecords { get; set; }
    public virtual ICollection<Maintenance> MaintenanceRecords { get; set; }
    
    // Computed Properties
    [NotMapped]
    public int Age => DateTime.Now.Year - Year;
    
    [NotMapped]
    public string FullDescription => $"{Year} {Make} {Model} (#{BusNumber})";
    
    [NotMapped]
    public string InspectionStatus { get; }
    
    [NotMapped]
    public bool IsAvailable => Status == "Active";
}
```

## 👨‍💼 Driver Management API

### IDriverService Interface

**Namespace:** `BusBuddy.Core.Services.Interfaces`

#### Methods

##### GetAllDriversAsync()
```csharp
Task<IEnumerable<Driver>> GetAllDriversAsync()
```
**Description:** Retrieves all drivers  
**Returns:** Collection of Driver entities

##### GetDriverByIdAsync(int id)
```csharp
Task<Driver?> GetDriverByIdAsync(int id)
```
**Parameters:**
- `id` (int): Driver ID

**Returns:** Driver entity or null

##### AddDriverAsync(Driver driver)
```csharp
Task<Driver> AddDriverAsync(Driver driver)
```
**Parameters:**
- `driver` (Driver): Driver entity to add

**Returns:** Added driver with generated ID

**Validation Rules:**
- FirstName: Required, max 50 characters
- LastName: Required, max 50 characters
- LicenseNumber: Required, unique, max 20 characters
- LicenseExpiry: Must be future date
- PhoneNumber: Valid phone format

### Driver Entity Model

```csharp
public class Driver : INotifyPropertyChanged
{
    [Key]
    public int DriverId { get; set; }
    
    [Required, StringLength(50)]
    public string FirstName { get; set; }
    
    [Required, StringLength(50)]
    public string LastName { get; set; }
    
    [Required, StringLength(20)]
    public string LicenseNumber { get; set; }
    
    public DateTime LicenseExpiry { get; set; }
    
    [StringLength(15)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(200)]
    public string? Address { get; set; }
    
    public DateTime HireDate { get; set; }
    public string Status { get; set; } = "Active";
    
    // Navigation Properties
    public virtual ICollection<Route> AMRoutes { get; set; }
    public virtual ICollection<Route> PMRoutes { get; set; }
    
    // Computed Properties
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
    
    [NotMapped]
    public string LicenseStatus { get; }
    
    [NotMapped]
    public bool IsAvailable => Status == "Active";
}
```

## 🛣️ Route Management API

### IRouteService Interface

#### Methods

##### GetAllActiveRoutesAsync()
```csharp
Task<IEnumerable<Route>> GetAllActiveRoutesAsync()
```
**Description:** Retrieves all active routes  
**Returns:** Collection of active Route entities

##### CreateRouteAsync(Route route)
```csharp
Task<Route> CreateRouteAsync(Route route)
```
**Parameters:**
- `route` (Route): Route entity to create

**Returns:** Created route with generated ID

**Validation Rules:**
- RouteName: Required, max 100 characters, unique
- RouteNumber: Required, max 20 characters, unique
- AMDriverId/PMDriverId: Must exist in Drivers table
- AMBusId/PMBusId: Must exist in Vehicles table

### Route Entity Model

```csharp
public class Route : INotifyPropertyChanged
{
    [Key]
    public int RouteId { get; set; }
    
    [Required, StringLength(100)]
    public string RouteName { get; set; }
    
    [Required, StringLength(20)]
    public string RouteNumber { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    // AM Route Assignment
    public int? AMDriverId { get; set; }
    public int? AMBusId { get; set; }
    public TimeOnly? AMStartTime { get; set; }
    public TimeOnly? AMEndTime { get; set; }
    
    // PM Route Assignment
    public int? PMDriverId { get; set; }
    public int? PMBusId { get; set; }
    public TimeOnly? PMStartTime { get; set; }
    public TimeOnly? PMEndTime { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Driver? AMDriver { get; set; }
    public virtual Bus? AMBus { get; set; }
    public virtual Driver? PMDriver { get; set; }
    public virtual Bus? PMBus { get; set; }
    public virtual ICollection<Student> Students { get; set; }
}
```

## ⛽ Fuel Management API

### IFuelService Interface

#### Methods

##### GetFuelRecordsAsync(int? busId = null)
```csharp
Task<IEnumerable<Fuel>> GetFuelRecordsAsync(int? busId = null)
```
**Parameters:**
- `busId` (int?, optional): Filter by specific bus

**Returns:** Collection of Fuel records

##### AddFuelRecordAsync(Fuel fuelRecord)
```csharp
Task<Fuel> AddFuelRecordAsync(Fuel fuelRecord)
```
**Parameters:**
- `fuelRecord` (Fuel): Fuel record to add

**Returns:** Added fuel record with generated ID

### Fuel Entity Model

```csharp
public class Fuel : INotifyPropertyChanged
{
    [Key]
    public int FuelId { get; set; }
    
    [Required]
    public int VehicleId { get; set; }
    
    [Required]
    public DateTime FuelDate { get; set; }
    
    [Required, Column(TypeName = "decimal(8,3)")]
    public decimal GallonsPurchased { get; set; }
    
    [Required, Column(TypeName = "decimal(8,2)")]
    public decimal PricePerGallon { get; set; }
    
    [Required, Column(TypeName = "decimal(10,2)")]
    public decimal TotalCost { get; set; }
    
    public int? CurrentOdometer { get; set; }
    
    [StringLength(100)]
    public string? FuelStation { get; set; }
    
    // Navigation Properties
    public virtual Bus Vehicle { get; set; }
    
    // Computed Properties
    [NotMapped]
    public decimal CostPerMile { get; }
}
```

## 📊 Dashboard Metrics API

### IDashboardMetricsService Interface

#### Methods

##### GetDashboardMetricsAsync()
```csharp
Task<Dictionary<string, int>> GetDashboardMetricsAsync()
```
**Description:** Gets aggregated metrics for dashboard display  
**Returns:** Dictionary with metric names and values

**Returned Metrics:**
- `BusCount`: Total number of buses
- `DriverCount`: Total number of drivers  
- `RouteCount`: Total number of active routes
- `StudentCount`: Total number of enrolled students

##### GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate)
```csharp
Task<PerformanceMetrics> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate)
```
**Parameters:**
- `fromDate` (DateTime): Start date for metrics
- `toDate` (DateTime): End date for metrics

**Returns:** Performance metrics for specified period

## 🔧 Configuration API

### Configuration Models

#### Database Configuration
```csharp
public class DatabaseConfiguration
{
    public string ConnectionString { get; set; }
    public int CommandTimeout { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public string Provider { get; set; } = "SqlServer";
}
```

#### Syncfusion Configuration
```csharp
public class SyncfusionConfiguration
{
    public string LicenseKey { get; set; }
    public string Theme { get; set; } = "MaterialLight";
    public bool EnableLogging { get; set; } = false;
}
```

## 🚨 Error Handling

### Exception Types

#### BusBuddyException (Base)
```csharp
public abstract class BusBuddyException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, object> Context { get; }
    
    protected BusBuddyException(string errorCode, string message, 
        Dictionary<string, object>? context = null) : base(message)
    {
        ErrorCode = errorCode;
        Context = context ?? new Dictionary<string, object>();
    }
}
```

#### ValidationException
```csharp
public class ValidationException : BusBuddyException
{
    public List<ValidationError> ValidationErrors { get; }
    
    public ValidationException(List<ValidationError> errors) 
        : base("VALIDATION_FAILED", "One or more validation errors occurred")
    {
        ValidationErrors = errors;
    }
}
```

#### DatabaseException
```csharp
public class DatabaseException : BusBuddyException
{
    public DatabaseException(string message, Exception innerException) 
        : base("DATABASE_ERROR", message)
    {
        // Inner exception details
    }
}
```

### Error Response Format

```csharp
public class ErrorResponse
{
    public string ErrorCode { get; set; }
    public string Message { get; set; }
    public Dictionary<string, object> Context { get; set; }
    public List<ValidationError>? ValidationErrors { get; set; }
    public DateTime Timestamp { get; set; }
    public string RequestId { get; set; }
}
```

## 🔄 Async Patterns

### Service Layer Guidelines

All service methods use async/await pattern:

```csharp
public async Task<Bus> AddBusAsync(Bus bus)
{
    try
    {
        // Validation
        ValidateBus(bus);
        
        // Database operation
        var result = await _busRepository.AddAsync(bus);
        
        // Post-processing
        await _auditService.LogAddAsync("Bus", result.VehicleId);
        
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding bus {BusNumber}", bus.BusNumber);
        throw;
    }
}
```

### Repository Pattern

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<int> SaveChangesAsync();
}
```

## 📈 Performance Guidelines

### Optimization Best Practices

1. **Use AsNoTracking() for read-only queries:**
```csharp
public async Task<IEnumerable<Bus>> GetAllBusesAsync()
{
    return await _context.Vehicles
        .AsNoTracking()
        .ToListAsync();
}
```

2. **Include related data efficiently:**
```csharp
public async Task<Route> GetRouteWithDetailsAsync(int id)
{
    return await _context.Routes
        .Include(r => r.AMDriver)
        .Include(r => r.PMDriver)
        .Include(r => r.AMBus)
        .Include(r => r.PMBus)
        .FirstOrDefaultAsync(r => r.RouteId == id);
}
```

3. **Use pagination for large datasets:**
```csharp
public async Task<PagedResult<Bus>> GetBusesPagedAsync(int page, int pageSize)
{
    var skip = (page - 1) * pageSize;
    
    var buses = await _context.Vehicles
        .Skip(skip)
        .Take(pageSize)
        .ToListAsync();
        
    var totalCount = await _context.Vehicles.CountAsync();
    
    return new PagedResult<Bus>(buses, totalCount, page, pageSize);
}
```

## 🔍 Integration Examples

### Service Registration (DI Container)

```csharp
// In App.xaml.cs or Startup.cs
services.AddScoped<IBusService, BusService>();
services.AddScoped<IDriverService, DriverService>();
services.AddScoped<IRouteService, RouteService>();
services.AddScoped<IFuelService, FuelService>();
services.AddScoped<IDashboardMetricsService, DashboardMetricsService>();
```

### ViewModel Integration

```csharp
public class BusManagementViewModel : BaseViewModel
{
    private readonly IBusService _busService;
    
    public BusManagementViewModel(IBusService busService)
    {
        _busService = busService;
    }
    
    public async Task LoadBusesAsync()
    {
        try
        {
            IsLoading = true;
            var buses = await _busService.GetAllBusesAsync();
            Buses = new ObservableCollection<Bus>(buses);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load buses: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

## 📚 Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [MVVM Pattern Guide](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm)
- [Dependency Injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)

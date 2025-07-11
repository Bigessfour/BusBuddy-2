# DEBUG Instrumentation Guide

**Last Updated:** July 10, 2025

## Overview

This document provides guidelines for implementing DEBUG statements throughout the Bus Buddy codebase. Proper debug instrumentation is critical for troubleshooting issues, understanding system behavior, and maintaining the application over time.

## Principles

1. **Be Comprehensive But Not Excessive**: Add DEBUG statements at key points that provide meaningful information without overwhelming the output.
2. **Be Consistent**: Follow standard patterns for similar operations across different modules.
3. **Be Informative**: Include relevant contextual data in DEBUG statements.
4. **Be Performance-Conscious**: Use conditional compilation to ensure DEBUG statements don't impact release builds.
5. **Be Organized**: Use categories and levels for DEBUG statements to allow filtering.

## Implementation Patterns

### Basic DEBUG Statement Structure

```csharp
#if DEBUG
    Debug.WriteLine($"[{GetType().Name}] {methodName}: {message}");
#endif
```

### Method Entry/Exit Pattern

```csharp
public async Task<bool> SomeMethod(int id, string name)
{
    #if DEBUG
        Debug.WriteLine($"[{GetType().Name}] ENTER SomeMethod: id={id}, name={name}");
    #endif
    
    // Method implementation
    
    #if DEBUG
        Debug.WriteLine($"[{GetType().Name}] EXIT SomeMethod: result={result}");
    #endif
    
    return result;
}
```

### Exception Reporting

```csharp
try
{
    // Code that might throw
}
catch (Exception ex)
{
    #if DEBUG
        Debug.WriteLine($"[{GetType().Name}] ERROR in {methodName}: {ex.Message}");
        Debug.WriteLine($"[{GetType().Name}] STACK: {ex.StackTrace}");
    #endif
    
    // Regular exception handling
    throw;
}
```

### Data State Changes

```csharp
#if DEBUG
    Debug.WriteLine($"[{GetType().Name}] DATA: {entityName} state changed from {oldState} to {newState}");
    if (detailedLogging)
    {
        Debug.WriteLine($"[{GetType().Name}] DATA DETAILS: {JsonSerializer.Serialize(entity)}");
    }
#endif
```

### Performance Tracking

```csharp
#if DEBUG
    var stopwatch = Stopwatch.StartNew();
#endif

// Performance-sensitive operation

#if DEBUG
    stopwatch.Stop();
    Debug.WriteLine($"[{GetType().Name}] PERF: {operationName} took {stopwatch.ElapsedMilliseconds}ms");
#endif
```

## Categorization

Use consistent prefixes to categorize DEBUG statements:

- `ENTER/EXIT`: Method entry and exit points
- `DATA`: Data state changes or important data values
- `UI`: User interface interactions and updates
- `API`: External API calls and responses
- `DB`: Database operations
- `PERF`: Performance measurements
- `ERROR`: Exception details and error states
- `CONFIG`: Configuration loading and values
- `AUTH`: Authentication and authorization operations
- `NAV`: Navigation changes

## Module-Specific Guidelines

### Student Management Module

- Log student record changes with before/after values
- Track route assignments and changes
- Record student search queries and result counts

```csharp
#if DEBUG
    Debug.WriteLine($"[StudentManagementViewModel] DATA: Student #{student.Id} ({student.LastName}, {student.FirstName}) updated");
    Debug.WriteLine($"[StudentManagementViewModel] DATA: Route assignment changed from {oldRouteId} to {newRouteId} for student #{student.Id}");
    Debug.WriteLine($"[StudentManagementViewModel] QUERY: Student search with criteria '{searchText}' returned {resultCount} results");
#endif
```

### Activity Trip Scheduling Module

- Log trip creation, modification, and deletion
- Track bus assignments and conflicts
- Record calendar view changes and filter applications

```csharp
#if DEBUG
    Debug.WriteLine($"[ScheduleViewModel] DATA: Activity trip #{trip.Id} ({trip.Title}) created for {trip.Date.ToShortDateString()}");
    Debug.WriteLine($"[ScheduleViewModel] CONFLICT: Bus #{busId} has overlapping assignments on {date.ToShortDateString()}");
    Debug.WriteLine($"[ScheduleViewModel] UI: Calendar view changed to {viewType} with date range {startDate.ToShortDateString()} - {endDate.ToShortDateString()}");
#endif
```

### Maintenance & Fuel Tracking Modules

- Log maintenance records and service entries
- Track fuel transactions and bulk station readings
- Record consumption calculations and anomalies

```csharp
#if DEBUG
    Debug.WriteLine($"[MaintenanceViewModel] DATA: Maintenance record #{record.Id} created for vehicle #{vehicle.Id} ({vehicle.Name})");
    Debug.WriteLine($"[FuelViewModel] DATA: Bulk station reading recorded: {reading.Date.ToShortDateString()} with value {reading.MeterReading}");
    Debug.WriteLine($"[FuelViewModel] ANOMALY: Fuel consumption for vehicle #{vehicle.Id} exceeds threshold by {excessAmount} gallons");
#endif
```

### Activity Logging Module

- Track log entry creation and viewing
- Record export operations
- Log filter applications and search queries

```csharp
#if DEBUG
    Debug.WriteLine($"[ActivityLogViewModel] DATA: {activityCount} log entries loaded for date range {startDate.ToShortDateString()} - {endDate.ToShortDateString()}");
    Debug.WriteLine($"[ActivityLogViewModel] EXPORT: Log exported to {filePath} in {format} format");
    Debug.WriteLine($"[ActivityLogViewModel] FILTER: Log filtered by user '{username}' and action type '{actionType}'");
#endif
```

## Debugging Dashboard

The Bus Buddy application includes a debugging dashboard that is only available in DEBUG builds. This dashboard provides a centralized view of system state, performance metrics, and runtime diagnostics.

To access the debugging dashboard:

1. Run the application in DEBUG mode
2. Press Ctrl+Shift+D or navigate to Help > Debug Dashboard
3. Use the tabs to explore different aspects of the system

The dashboard includes:

- **System State**: Current memory usage, thread count, and uptime
- **Database Metrics**: Connection pool status, query execution times, and entity counts
- **Performance Counters**: Real-time performance metrics for key operations
- **Log Viewer**: Filtered view of DEBUG output organized by category
- **Exception Monitor**: Recent exceptions with full details
- **Configuration Viewer**: Current application configuration settings

## Conditional Compilation

To ensure DEBUG statements don't impact release builds, always wrap them in conditional compilation directives:

```csharp
#if DEBUG
    // Debug-only code here
#endif
```

For more complex scenarios, consider using multiple debug levels:

```csharp
#if DEBUG_VERBOSE
    // Extremely detailed debug information
#elif DEBUG
    // Standard debug information
#endif
```

### DEBUG Configuration Management

Create a centralized debug configuration class for consistent control:

```csharp
#if DEBUG
public static class DebugConfig
{
    public static bool EnableVerboseLogging { get; set; } = true;
    public static bool EnablePerformanceTracking { get; set; } = true;
    public static bool EnableDataStateLogging { get; set; } = false; // Can be overwhelming
    public static bool EnableUITracking { get; set; } = false;
    
    public static void WriteIf(bool condition, string category, string message)
    {
        if (condition)
            Debug.WriteLine($"[{category}] {message}");
    }
}
#endif
```

Usage:
```csharp
#if DEBUG
    DebugConfig.WriteIf(DebugConfig.EnableDataStateLogging, GetType().Name, 
        $"DATA: Student #{student.Id} updated");
#endif
```

## Integration with Existing Logging Infrastructure

Bus Buddy already has a comprehensive Serilog configuration with multiple log files. DEBUG statements should complement, not duplicate, this infrastructure:

### Using Both DEBUG and Serilog
```csharp
#if DEBUG
    Debug.WriteLine($"[{GetType().Name}] ENTER {methodName}: id={id}");
#endif
_logger.LogDebug("Entering {MethodName} with parameters: {Parameters}", nameof(methodName), new { id, name });

// For critical issues, use both:
#if DEBUG
    Debug.WriteLine($"[{GetType().Name}] CRITICAL: {message}");
#endif
_logger.LogError("Critical error in {ClassName}: {Message}", GetType().Name, message);
```

### Leveraging Existing Log Categories
The application already logs to specialized files:
- `diagnostic-.log` for verbose debugging information
- `performance-.log` for performance metrics
- `busbuddy-.log` for general application events

Use appropriate logger levels:
```csharp
// For performance tracking - goes to performance-.log
_logger.LogInformation("Operation {Operation} completed in {Duration}ms", operationName, duration);

// For detailed diagnostics - goes to diagnostic-.log  
_logger.LogDebug("Data state changed: {Entity} from {OldState} to {NewState}", entityName, oldState, newState);
```

## Best Practices

1. **Keep It Clean**: DEBUG statements should not contain sensitive data (passwords, personal information, etc.)
2. **Be Selective**: Not every method needs entry/exit logging; focus on complex operations
3. **Stay Relevant**: Include only contextually relevant information in DEBUG output
4. **Use Hierarchical Naming**: Include class names and categories for easy filtering
5. **Check Performance Impact**: For very frequent operations, ensure DEBUG statements don't cause performance issues even in debug builds
6. **Standardize Formats**: Use consistent formatting for similar types of information
7. **Update Regularly**: Review and update DEBUG statements when code changes

## Conclusion

Effective DEBUG instrumentation is an investment in the maintainability and supportability of the Bus Buddy application. By following these guidelines, developers can create a comprehensive debugging infrastructure that facilitates troubleshooting and enhances understanding of the system's behavior.

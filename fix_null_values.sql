-- BusBuddy Database NULL Value Fix Script
-- This script fixes NULL values in required string columns that cause SqlNullValueException
-- Run this script against your BusBuddyDb database to resolve the issue immediately

USE BusBuddyDb;
GO

PRINT 'Starting BusBuddy Database NULL Value Fix...';
PRINT '============================================';

-- Fix Drivers table NULL values
PRINT 'Fixing Drivers table NULL values...';

UPDATE Drivers 
SET DriverName = COALESCE(NULLIF(LTRIM(RTRIM(DriverName)), ''), 'Driver-' + CAST(DriverId AS VARCHAR(10)))
WHERE DriverName IS NULL OR LTRIM(RTRIM(DriverName)) = '';

UPDATE Drivers 
SET DriversLicenceType = COALESCE(NULLIF(LTRIM(RTRIM(DriversLicenceType)), ''), 'Standard')
WHERE DriversLicenceType IS NULL OR LTRIM(RTRIM(DriversLicenceType)) = '';

UPDATE Drivers 
SET Status = COALESCE(NULLIF(LTRIM(RTRIM(Status)), ''), 'Active')
WHERE Status IS NULL OR LTRIM(RTRIM(Status)) = '';

-- Clean up nullable fields that should be NULL instead of empty strings
UPDATE Drivers 
SET FirstName = NULLIF(LTRIM(RTRIM(FirstName)), '')
WHERE FirstName = '';

UPDATE Drivers 
SET LastName = NULLIF(LTRIM(RTRIM(LastName)), '')
WHERE LastName = '';

UPDATE Drivers 
SET DriverPhone = NULLIF(LTRIM(RTRIM(DriverPhone)), '')
WHERE DriverPhone = '';

UPDATE Drivers 
SET DriverEmail = NULLIF(LTRIM(RTRIM(DriverEmail)), '')
WHERE DriverEmail = '';

UPDATE Drivers 
SET Address = NULLIF(LTRIM(RTRIM(Address)), '')
WHERE Address = '';

UPDATE Drivers 
SET City = NULLIF(LTRIM(RTRIM(City)), '')
WHERE City = '';

UPDATE Drivers 
SET State = NULLIF(LTRIM(RTRIM(State)), '')
WHERE State = '';

UPDATE Drivers 
SET Zip = NULLIF(LTRIM(RTRIM(Zip)), '')
WHERE Zip = '';

PRINT 'Drivers table fixed.';

-- Fix Vehicles table NULL values
PRINT 'Fixing Vehicles table NULL values...';

UPDATE Vehicles 
SET BusNumber = COALESCE(NULLIF(LTRIM(RTRIM(BusNumber)), ''), 'BUS-' + CAST(VehicleId AS VARCHAR(10)))
WHERE BusNumber IS NULL OR LTRIM(RTRIM(BusNumber)) = '';

UPDATE Vehicles 
SET Make = COALESCE(NULLIF(LTRIM(RTRIM(Make)), ''), 'Unknown')
WHERE Make IS NULL OR LTRIM(RTRIM(Make)) = '';

UPDATE Vehicles 
SET Model = COALESCE(NULLIF(LTRIM(RTRIM(Model)), ''), 'Unknown')
WHERE Model IS NULL OR LTRIM(RTRIM(Model)) = '';

UPDATE Vehicles 
SET Status = COALESCE(NULLIF(LTRIM(RTRIM(Status)), ''), 'Active')
WHERE Status IS NULL OR LTRIM(RTRIM(Status)) = '';

UPDATE Vehicles 
SET VINNumber = COALESCE(NULLIF(LTRIM(RTRIM(VINNumber)), ''), 'VIN' + CAST(VehicleId AS VARCHAR(10)) + '0000000000')
WHERE VINNumber IS NULL OR LTRIM(RTRIM(VINNumber)) = '';

UPDATE Vehicles 
SET LicenseNumber = COALESCE(NULLIF(LTRIM(RTRIM(LicenseNumber)), ''), 'LIC-' + CAST(VehicleId AS VARCHAR(10)))
WHERE LicenseNumber IS NULL OR LTRIM(RTRIM(LicenseNumber)) = '';

PRINT 'Vehicles table fixed.';

-- Fix Routes table NULL values
PRINT 'Fixing Routes table NULL values...';

UPDATE Routes 
SET RouteName = COALESCE(NULLIF(LTRIM(RTRIM(RouteName)), ''), 'Route-' + CAST(RouteId AS VARCHAR(10)))
WHERE RouteName IS NULL OR LTRIM(RTRIM(RouteName)) = '';

-- Clean up nullable fields
UPDATE Routes 
SET Description = NULLIF(LTRIM(RTRIM(Description)), '')
WHERE Description = '';

UPDATE Routes 
SET BusNumber = NULLIF(LTRIM(RTRIM(BusNumber)), '')
WHERE BusNumber = '';

UPDATE Routes 
SET DriverName = NULLIF(LTRIM(RTRIM(DriverName)), '')
WHERE DriverName = '';

PRINT 'Routes table fixed.';

-- Fix Activities table NULL values (if table exists)
IF OBJECT_ID('Activities', 'U') IS NOT NULL
BEGIN
    PRINT 'Fixing Activities table NULL values...';

    UPDATE Activities 
    SET ActivityType = COALESCE(NULLIF(LTRIM(RTRIM(ActivityType)), ''), 'General')
    WHERE ActivityType IS NULL OR LTRIM(RTRIM(ActivityType)) = '';

    UPDATE Activities 
    SET Destination = COALESCE(NULLIF(LTRIM(RTRIM(Destination)), ''), 'Unspecified')
    WHERE Destination IS NULL OR LTRIM(RTRIM(Destination)) = '';

    UPDATE Activities 
    SET RequestedBy = COALESCE(NULLIF(LTRIM(RTRIM(RequestedBy)), ''), 'System')
    WHERE RequestedBy IS NULL OR LTRIM(RTRIM(RequestedBy)) = '';

    UPDATE Activities 
    SET Status = COALESCE(NULLIF(LTRIM(RTRIM(Status)), ''), 'Scheduled')
    WHERE Status IS NULL OR LTRIM(RTRIM(Status)) = '';

    UPDATE Activities 
    SET Description = COALESCE(NULLIF(LTRIM(RTRIM(Description)), ''), 'Activity')
    WHERE Description IS NULL OR LTRIM(RTRIM(Description)) = '';

    PRINT 'Activities table fixed.';
END
ELSE
BEGIN
    PRINT 'Activities table not found, skipping...';
END

-- Show final counts
PRINT 'Final verification - checking for remaining NULL values...';

DECLARE @DriversNulls INT = (SELECT COUNT(*) FROM Drivers WHERE DriverName IS NULL OR DriversLicenceType IS NULL OR Status IS NULL);
DECLARE @VehiclesNulls INT = (SELECT COUNT(*) FROM Vehicles WHERE BusNumber IS NULL OR Make IS NULL OR Model IS NULL OR Status IS NULL OR VINNumber IS NULL OR LicenseNumber IS NULL);
DECLARE @RoutesNulls INT = (SELECT COUNT(*) FROM Routes WHERE RouteName IS NULL);

PRINT 'Remaining NULL values:';
PRINT '  Drivers: ' + CAST(@DriversNulls AS VARCHAR(10));
PRINT '  Vehicles: ' + CAST(@VehiclesNulls AS VARCHAR(10));
PRINT '  Routes: ' + CAST(@RoutesNulls AS VARCHAR(10));

IF (@DriversNulls = 0 AND @VehiclesNulls = 0 AND @RoutesNulls = 0)
    PRINT 'SUCCESS: All critical NULL values have been fixed!';
ELSE
    PRINT 'WARNING: Some NULL values remain. Please investigate.';

PRINT 'BusBuddy Database NULL Value Fix Complete!';
PRINT '===========================================';

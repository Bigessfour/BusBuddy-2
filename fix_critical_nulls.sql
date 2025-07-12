-- Critical BusBuddy Database NULL Value Fix (Minimal)
-- This script fixes only the most critical NULL values causing SqlNullValueException

USE BusBuddyDb;
GO

PRINT 'Starting Critical NULL Value Fix...';

-- Fix Drivers table - the main source of SqlNullValueException
UPDATE Drivers 
SET DriverName = COALESCE(NULLIF(LTRIM(RTRIM(DriverName)), ''), 'Driver-' + CAST(DriverId AS VARCHAR(10)))
WHERE DriverName IS NULL OR LTRIM(RTRIM(DriverName)) = '';

UPDATE Drivers 
SET DriversLicenceType = COALESCE(NULLIF(LTRIM(RTRIM(DriversLicenceType)), ''), 'Standard')
WHERE DriversLicenceType IS NULL OR LTRIM(RTRIM(DriversLicenceType)) = '';

UPDATE Drivers 
SET Status = COALESCE(NULLIF(LTRIM(RTRIM(Status)), ''), 'Active')
WHERE Status IS NULL OR LTRIM(RTRIM(Status)) = '';

-- Fix Vehicles table critical columns
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

-- Fix Routes table
UPDATE Routes 
SET RouteName = COALESCE(NULLIF(LTRIM(RTRIM(RouteName)), ''), 'Route-' + CAST(RouteId AS VARCHAR(10)))
WHERE RouteName IS NULL OR LTRIM(RTRIM(RouteName)) = '';

PRINT 'Critical NULL values fixed!';

-- Verify the fix
DECLARE @DriversNulls INT = (SELECT COUNT(*) FROM Drivers WHERE DriverName IS NULL OR DriversLicenceType IS NULL OR Status IS NULL);
DECLARE @VehiclesNulls INT = (SELECT COUNT(*) FROM Vehicles WHERE BusNumber IS NULL OR Make IS NULL OR Model IS NULL OR Status IS NULL OR VINNumber IS NULL OR LicenseNumber IS NULL);
DECLARE @RoutesNulls INT = (SELECT COUNT(*) FROM Routes WHERE RouteName IS NULL);

PRINT 'Remaining critical NULL values:';
PRINT '  Drivers: ' + CAST(@DriversNulls AS VARCHAR(10));
PRINT '  Vehicles: ' + CAST(@VehiclesNulls AS VARCHAR(10));
PRINT '  Routes: ' + CAST(@RoutesNulls AS VARCHAR(10));

IF (@DriversNulls = 0 AND @VehiclesNulls = 0 AND @RoutesNulls = 0)
    PRINT 'SUCCESS: All critical NULL values have been fixed!';
ELSE
    PRINT 'WARNING: Some NULL values remain.';

PRINT 'Critical NULL Value Fix Complete!';

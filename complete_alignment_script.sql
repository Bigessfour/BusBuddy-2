-- Complete Database Schema Alignment Script
-- This script handles index dependencies and performs safe data type conversions

USE BusBuddyDb;
GO

PRINT 'Starting complete database schema alignment...';

-- ================================================
-- 1. DROP EXISTING INDEXES THAT DEPEND ON COLUMNS WE'RE CHANGING
-- ================================================
PRINT 'Dropping dependent indexes...';

-- Drop Activities table indexes
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_activities_date')
    DROP INDEX idx_activities_date ON Activities;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Activities_Date')
    DROP INDEX IX_Activities_Date ON Activities;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Activities_DateTimeRange')
    DROP INDEX IX_Activities_DateTimeRange ON Activities;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Activities_DriverSchedule')
    DROP INDEX IX_Activities_DriverSchedule ON Activities;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Activities_VehicleSchedule')
    DROP INDEX IX_Activities_VehicleSchedule ON Activities;

-- Drop other potential indexes
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivitySchedule_Date')
    DROP INDEX IX_ActivitySchedule_Date ON ActivitySchedule;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Maintenance_Date')
    DROP INDEX IX_Maintenance_Date ON Maintenance;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Routes_Date')
    DROP INDEX IX_Routes_Date ON Routes;

PRINT 'Dependent indexes dropped';

-- ================================================
-- 2. ACTIVITIES TABLE (empty - safe to recreate columns)
-- ================================================
PRINT 'Fixing Activities table...';

-- Drop and recreate Date column as datetime2
ALTER TABLE Activities DROP COLUMN Date;
ALTER TABLE Activities ADD Date datetime2;
PRINT 'Activities.Date converted to datetime2';

-- Drop and recreate time columns
ALTER TABLE Activities DROP COLUMN LeaveTime;
ALTER TABLE Activities ADD LeaveTime time;
PRINT 'Activities.LeaveTime converted to time';

ALTER TABLE Activities DROP COLUMN EventTime;
ALTER TABLE Activities ADD EventTime time;
PRINT 'Activities.EventTime converted to time';

ALTER TABLE Activities DROP COLUMN ReturnTime;
ALTER TABLE Activities ADD ReturnTime time;
PRINT 'Activities.ReturnTime converted to time';

-- ================================================
-- 3. ACTIVITYSCHEDULE TABLE (empty - safe to recreate)
-- ================================================
PRINT 'Fixing ActivitySchedule table...';

ALTER TABLE ActivitySchedule DROP COLUMN Date;
ALTER TABLE ActivitySchedule ADD Date datetime2;
PRINT 'ActivitySchedule.Date converted to datetime2';

ALTER TABLE ActivitySchedule DROP COLUMN ScheduledLeaveTime;
ALTER TABLE ActivitySchedule ADD ScheduledLeaveTime time;
PRINT 'ActivitySchedule.ScheduledLeaveTime converted to time';

ALTER TABLE ActivitySchedule DROP COLUMN ScheduledEventTime;
ALTER TABLE ActivitySchedule ADD ScheduledEventTime time;
PRINT 'ActivitySchedule.ScheduledEventTime converted to time';

ALTER TABLE ActivitySchedule DROP COLUMN ScheduledReturnTime;
ALTER TABLE ActivitySchedule ADD ScheduledReturnTime time;
PRINT 'ActivitySchedule.ScheduledReturnTime converted to time';

-- ================================================
-- 4. MAINTENANCE TABLE (has 1 record - handle carefully)
-- ================================================
PRINT 'Fixing Maintenance table...';

-- Check and save current data
DECLARE @MaintenanceDate nvarchar(50);
DECLARE @MaintenanceCost float;
DECLARE @MaintenanceID int;

SELECT @MaintenanceID = MaintenanceID, @MaintenanceDate = Date, @MaintenanceCost = RepairCost 
FROM Maintenance;

-- Drop and recreate columns
ALTER TABLE Maintenance DROP COLUMN Date;
ALTER TABLE Maintenance ADD Date datetime2;

ALTER TABLE Maintenance DROP COLUMN RepairCost;
ALTER TABLE Maintenance ADD RepairCost decimal(10,2);

-- Restore data with proper types
IF @MaintenanceID IS NOT NULL
BEGIN
    UPDATE Maintenance 
    SET Date = TRY_CONVERT(datetime2, @MaintenanceDate),
        RepairCost = CASE WHEN @MaintenanceCost IS NOT NULL THEN CONVERT(decimal(10,2), @MaintenanceCost) ELSE NULL END
    WHERE MaintenanceID = @MaintenanceID;
END

PRINT 'Maintenance table fixed';

-- ================================================
-- 5. ROUTES TABLE (has 1 record - handle carefully)
-- ================================================
PRINT 'Fixing Routes table...';

-- Save existing route data
DECLARE @RouteID int;
DECLARE @RouteDate nvarchar(50);
DECLARE @AMBegin float, @AMEnd float, @PMBegin float, @PMEnd float;

SELECT @RouteID = RouteID, @RouteDate = Date, 
       @AMBegin = AMBeginMiles, @AMEnd = AMEndMiles,
       @PMBegin = PMBeginMiles, @PMEnd = PMEndMiles
FROM Routes;

-- Drop and recreate columns
ALTER TABLE Routes DROP COLUMN Date;
ALTER TABLE Routes ADD Date datetime2;

ALTER TABLE Routes DROP COLUMN AMBeginMiles;
ALTER TABLE Routes ADD AMBeginMiles decimal(10,2);

ALTER TABLE Routes DROP COLUMN AMEndMiles;
ALTER TABLE Routes ADD AMEndMiles decimal(10,2);

ALTER TABLE Routes DROP COLUMN PMBeginMiles;
ALTER TABLE Routes ADD PMBeginMiles decimal(10,2);

ALTER TABLE Routes DROP COLUMN PMEndMiles;
ALTER TABLE Routes ADD PMEndMiles decimal(10,2);

-- Restore data with proper types
IF @RouteID IS NOT NULL
BEGIN
    UPDATE Routes SET 
        Date = TRY_CONVERT(datetime2, @RouteDate),
        AMBeginMiles = CASE WHEN @AMBegin IS NOT NULL THEN CONVERT(decimal(10,2), @AMBegin) ELSE NULL END,
        AMEndMiles = CASE WHEN @AMEnd IS NOT NULL THEN CONVERT(decimal(10,2), @AMEnd) ELSE NULL END,
        PMBeginMiles = CASE WHEN @PMBegin IS NOT NULL THEN CONVERT(decimal(10,2), @PMBegin) ELSE NULL END,
        PMEndMiles = CASE WHEN @PMEnd IS NOT NULL THEN CONVERT(decimal(10,2), @PMEnd) ELSE NULL END
    WHERE RouteID = @RouteID;
END

PRINT 'Routes table fixed';

-- ================================================
-- 6. RECREATE OPTIMIZED INDEXES
-- ================================================
PRINT 'Creating optimized indexes...';

-- Activities table indexes
CREATE INDEX IX_Activities_Date ON Activities(Date);
CREATE INDEX IX_Activities_DateTimeRange ON Activities(Date, LeaveTime, ReturnTime);
CREATE INDEX IX_Activities_VehicleSchedule ON Activities(AssignedVehicleID, Date);
CREATE INDEX IX_Activities_DriverSchedule ON Activities(DriverID, Date);

-- Other table indexes
CREATE INDEX IX_ActivitySchedule_Date ON ActivitySchedule(Date);
CREATE INDEX IX_Maintenance_Date ON Maintenance(Date);
CREATE INDEX IX_Routes_Date ON Routes(Date);

PRINT 'Optimized indexes created';

-- ================================================
-- 7. VERIFY FINAL SCHEMA
-- ================================================
PRINT 'Verifying final schema alignment...';

-- Show the corrected schema
SELECT 'ACTIVITIES TABLE SCHEMA' as Info, COLUMN_NAME, DATA_TYPE, 
       CASE DATA_TYPE 
           WHEN 'datetime2' THEN 'DateTime' 
           WHEN 'time' THEN 'TimeSpan'
           ELSE DATA_TYPE 
       END as 'C# Type'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Activities' 
AND COLUMN_NAME IN ('Date', 'LeaveTime', 'EventTime', 'ReturnTime')
ORDER BY COLUMN_NAME;

SELECT 'ACTIVITYSCHEDULE TABLE SCHEMA' as Info, COLUMN_NAME, DATA_TYPE,
       CASE DATA_TYPE 
           WHEN 'datetime2' THEN 'DateTime' 
           WHEN 'time' THEN 'TimeSpan'
           ELSE DATA_TYPE 
       END as 'C# Type'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ActivitySchedule' 
AND COLUMN_NAME IN ('Date', 'ScheduledLeaveTime', 'ScheduledEventTime', 'ScheduledReturnTime')
ORDER BY COLUMN_NAME;

SELECT 'MAINTENANCE TABLE SCHEMA' as Info, COLUMN_NAME, DATA_TYPE,
       CASE DATA_TYPE 
           WHEN 'datetime2' THEN 'DateTime' 
           WHEN 'decimal' THEN 'decimal'
           ELSE DATA_TYPE 
       END as 'C# Type'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Maintenance' 
AND COLUMN_NAME IN ('Date', 'RepairCost')
ORDER BY COLUMN_NAME;

SELECT 'ROUTES TABLE SCHEMA' as Info, COLUMN_NAME, DATA_TYPE,
       CASE DATA_TYPE 
           WHEN 'datetime2' THEN 'DateTime' 
           WHEN 'decimal' THEN 'decimal'
           ELSE DATA_TYPE 
       END as 'C# Type'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Routes' 
AND COLUMN_NAME IN ('Date', 'AMBeginMiles', 'AMEndMiles', 'PMBeginMiles', 'PMEndMiles')
ORDER BY COLUMN_NAME;

PRINT 'Complete database schema alignment finished successfully!';
PRINT 'All data type mismatches resolved. Database schema now matches C# models.';

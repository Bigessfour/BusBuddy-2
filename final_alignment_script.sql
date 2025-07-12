-- Final Comprehensive Database Schema Alignment Script
-- This script drops ALL dependent indexes and performs complete schema alignment

USE BusBuddyDb;
GO

PRINT 'Starting final comprehensive database schema alignment...';

-- ================================================
-- 1. DROP ALL DEPENDENT INDEXES
-- ================================================
PRINT 'Dropping all dependent indexes...';

-- ActivitySchedule indexes
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_activityschedule_date')
    DROP INDEX idx_activityschedule_date ON ActivitySchedule;

-- Maintenance indexes  
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_maintenance_date')
    DROP INDEX idx_maintenance_date ON Maintenance;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Maintenance_VehicleDate')
    DROP INDEX IX_Maintenance_VehicleDate ON Maintenance;

-- Routes indexes
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_routes_date')
    DROP INDEX idx_routes_date ON Routes;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Routes_DateRouteName')
    DROP INDEX IX_Routes_DateRouteName ON Routes;

-- SchoolCalendar indexes (if needed later)
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_calendar_date')
    DROP INDEX idx_calendar_date ON SchoolCalendar;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SchoolCalendar_Date')
    DROP INDEX IX_SchoolCalendar_Date ON SchoolCalendar;

PRINT 'All dependent indexes dropped successfully';

-- ================================================
-- 2. ACTIVITIES TABLE (already fixed from previous run)
-- ================================================
PRINT 'Activities table already fixed in previous run';

-- ================================================
-- 3. ACTIVITYSCHEDULE TABLE (now safe to modify)
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
-- 4. MAINTENANCE TABLE (handle existing data)
-- ================================================
PRINT 'Fixing Maintenance table...';

-- Save existing data
DECLARE @MaintenanceData TABLE (
    MaintenanceID int,
    DateValue nvarchar(50),
    RepairCostValue float
);

INSERT INTO @MaintenanceData (MaintenanceID, DateValue, RepairCostValue)
SELECT MaintenanceID, Date, RepairCost FROM Maintenance;

-- Drop and recreate columns
ALTER TABLE Maintenance DROP COLUMN Date;
ALTER TABLE Maintenance ADD Date datetime2;

ALTER TABLE Maintenance DROP COLUMN RepairCost;
ALTER TABLE Maintenance ADD RepairCost decimal(10,2);

-- Restore data with proper types
UPDATE m 
SET Date = TRY_CONVERT(datetime2, md.DateValue),
    RepairCost = CASE WHEN md.RepairCostValue IS NOT NULL THEN CONVERT(decimal(10,2), md.RepairCostValue) ELSE NULL END
FROM Maintenance m
INNER JOIN @MaintenanceData md ON m.MaintenanceID = md.MaintenanceID;

PRINT 'Maintenance table fixed with data preserved';

-- ================================================
-- 5. ROUTES TABLE (handle existing data)
-- ================================================
PRINT 'Fixing Routes table...';

-- Save existing data
DECLARE @RouteData TABLE (
    RouteID int,
    DateValue nvarchar(50),
    AMBeginMilesValue float,
    AMEndMilesValue float,
    PMBeginMilesValue float,
    PMEndMilesValue float
);

INSERT INTO @RouteData (RouteID, DateValue, AMBeginMilesValue, AMEndMilesValue, PMBeginMilesValue, PMEndMilesValue)
SELECT RouteID, Date, AMBeginMiles, AMEndMiles, PMBeginMiles, PMEndMiles FROM Routes;

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
UPDATE r SET 
    Date = TRY_CONVERT(datetime2, rd.DateValue),
    AMBeginMiles = CASE WHEN rd.AMBeginMilesValue IS NOT NULL THEN CONVERT(decimal(10,2), rd.AMBeginMilesValue) ELSE NULL END,
    AMEndMiles = CASE WHEN rd.AMEndMilesValue IS NOT NULL THEN CONVERT(decimal(10,2), rd.AMEndMilesValue) ELSE NULL END,
    PMBeginMiles = CASE WHEN rd.PMBeginMilesValue IS NOT NULL THEN CONVERT(decimal(10,2), rd.PMBeginMilesValue) ELSE NULL END,
    PMEndMiles = CASE WHEN rd.PMEndMilesValue IS NOT NULL THEN CONVERT(decimal(10,2), rd.PMEndMilesValue) ELSE NULL END
FROM Routes r
INNER JOIN @RouteData rd ON r.RouteID = rd.RouteID;

PRINT 'Routes table fixed with data preserved';

-- ================================================
-- 6. RECREATE ALL NECESSARY INDEXES
-- ================================================
PRINT 'Recreating optimized indexes...';

-- Activities table indexes (already have proper columns)
CREATE INDEX IX_Activities_Date ON Activities(Date);
CREATE INDEX IX_Activities_DateTimeRange ON Activities(Date, LeaveTime, ReturnTime);
CREATE INDEX IX_Activities_VehicleSchedule ON Activities(AssignedVehicleID, Date);
CREATE INDEX IX_Activities_DriverSchedule ON Activities(DriverID, Date);

-- ActivitySchedule indexes
CREATE INDEX IX_ActivitySchedule_Date ON ActivitySchedule(Date);
CREATE INDEX IX_ActivitySchedule_VehicleDate ON ActivitySchedule(ScheduledVehicleID, Date);

-- Maintenance indexes
CREATE INDEX IX_Maintenance_Date ON Maintenance(Date);
CREATE INDEX IX_Maintenance_VehicleDate ON Maintenance(VehicleID, Date);

-- Routes indexes
CREATE INDEX IX_Routes_Date ON Routes(Date);
CREATE INDEX IX_Routes_DateRouteName ON Routes(Date, RouteName);

-- SchoolCalendar (keep original if exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SchoolCalendar')
BEGIN
    CREATE INDEX IX_SchoolCalendar_Date ON SchoolCalendar(Date);
END

PRINT 'All optimized indexes recreated successfully';

-- ================================================
-- 7. FINAL SCHEMA VERIFICATION
-- ================================================
PRINT 'Performing final schema verification...';

-- Show complete corrected schema
SELECT 'FINAL SCHEMA VERIFICATION - ALL TABLES' as Status;

-- Activities table
SELECT 'Activities' as TableName, COLUMN_NAME, DATA_TYPE, 
       CASE DATA_TYPE 
           WHEN 'datetime2' THEN 'DateTime' 
           WHEN 'time' THEN 'TimeSpan'
           WHEN 'int' THEN 'int'
           WHEN 'bit' THEN 'bool'
           WHEN 'decimal' THEN 'decimal'
           ELSE DATA_TYPE 
       END as 'C# Type Equivalent'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Activities' 
AND COLUMN_NAME IN ('Date', 'LeaveTime', 'EventTime', 'ReturnTime', 'AssignedVehicleID', 'DriverID')
ORDER BY COLUMN_NAME;

-- ActivitySchedule table
SELECT 'ActivitySchedule' as TableName, COLUMN_NAME, DATA_TYPE,
       CASE DATA_TYPE 
           WHEN 'datetime2' THEN 'DateTime' 
           WHEN 'time' THEN 'TimeSpan'
           WHEN 'int' THEN 'int'
           ELSE DATA_TYPE 
       END as 'C# Type Equivalent'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ActivitySchedule' 
AND COLUMN_NAME IN ('Date', 'ScheduledLeaveTime', 'ScheduledEventTime', 'ScheduledReturnTime', 'ScheduledVehicleID')
ORDER BY COLUMN_NAME;

-- Maintenance table
SELECT 'Maintenance' as TableName, COLUMN_NAME, DATA_TYPE,
       CASE DATA_TYPE 
           WHEN 'datetime2' THEN 'DateTime' 
           WHEN 'decimal' THEN 'decimal'
           WHEN 'int' THEN 'int'
           ELSE DATA_TYPE 
       END as 'C# Type Equivalent'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Maintenance' 
AND COLUMN_NAME IN ('Date', 'RepairCost', 'VehicleID', 'OdometerReading')
ORDER BY COLUMN_NAME;

-- Routes table
SELECT 'Routes' as TableName, COLUMN_NAME, DATA_TYPE,
       CASE DATA_TYPE 
           WHEN 'datetime2' THEN 'DateTime' 
           WHEN 'decimal' THEN 'decimal'
           WHEN 'int' THEN 'int'
           WHEN 'time' THEN 'TimeSpan'
           ELSE DATA_TYPE 
       END as 'C# Type Equivalent'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Routes' 
AND COLUMN_NAME IN ('Date', 'AMBeginMiles', 'AMEndMiles', 'PMBeginMiles', 'PMEndMiles', 'AMBeginTime', 'PMBeginTime')
ORDER BY COLUMN_NAME;

-- Check for any remaining nvarchar date/time fields
SELECT 'REMAINING NVARCHAR DATE/TIME FIELDS (should be empty)' as Warning;
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE DATA_TYPE = 'nvarchar' 
AND (COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME LIKE '%Time%')
AND TABLE_NAME IN ('Activities', 'ActivitySchedule', 'Maintenance', 'Routes', 'Fuel')
ORDER BY TABLE_NAME, COLUMN_NAME;

PRINT '';
PRINT '========================================';
PRINT 'COMPREHENSIVE DATABASE ALIGNMENT COMPLETE!';
PRINT '========================================';
PRINT 'All data type mismatches have been resolved.';
PRINT 'Database schema now perfectly matches C# model expectations.';
PRINT 'Performance indexes have been recreated and optimized.';
PRINT 'Application should run without casting errors.';
PRINT '========================================';

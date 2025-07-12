-- Safe Database Schema Alignment Script
-- This script safely converts data types for empty or nearly empty tables

USE BusBuddyDb;
GO

PRINT 'Starting safe database schema alignment...';

-- ================================================
-- 1. ACTIVITIES TABLE (empty - safe to recreate columns)
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
-- 2. ACTIVITYSCHEDULE TABLE (empty - safe to recreate)
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
-- 3. MAINTENANCE TABLE (has 1 record - handle carefully)
-- ================================================
PRINT 'Fixing Maintenance table...';

-- Check current data in Maintenance table
SELECT 'Current Maintenance Data:' as Info;
SELECT MaintenanceID, Date, RepairCost FROM Maintenance;

-- For the Date field, save existing data first
DECLARE @MaintenanceData TABLE (
    MaintenanceID int,
    DateValue nvarchar(50),
    RepairCostValue float
);

INSERT INTO @MaintenanceData (MaintenanceID, DateValue, RepairCostValue)
SELECT MaintenanceID, Date, RepairCost FROM Maintenance;

-- Drop and recreate Date column
ALTER TABLE Maintenance DROP COLUMN Date;
ALTER TABLE Maintenance ADD Date datetime2;

-- Update with converted values
UPDATE m 
SET Date = TRY_CONVERT(datetime2, md.DateValue)
FROM Maintenance m
INNER JOIN @MaintenanceData md ON m.MaintenanceID = md.MaintenanceID;

PRINT 'Maintenance.Date converted to datetime2';

-- Fix RepairCost: float -> decimal(10,2)
ALTER TABLE Maintenance DROP COLUMN RepairCost;
ALTER TABLE Maintenance ADD RepairCost decimal(10,2);

-- Update with converted values
UPDATE m 
SET RepairCost = CONVERT(decimal(10,2), md.RepairCostValue)
FROM Maintenance m
INNER JOIN @MaintenanceData md ON m.MaintenanceID = md.MaintenanceID
WHERE md.RepairCostValue IS NOT NULL;

PRINT 'Maintenance.RepairCost converted to decimal(10,2)';

-- ================================================
-- 4. ROUTES TABLE (has 1 record - handle carefully)
-- ================================================
PRINT 'Fixing Routes table...';

-- Check current data
SELECT 'Current Routes Data:' as Info;
SELECT RouteID, Date, AMBeginMiles, AMEndMiles, PMBeginMiles, PMEndMiles FROM Routes;

-- Save existing route data
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

-- Fix Date column
ALTER TABLE Routes DROP COLUMN Date;
ALTER TABLE Routes ADD Date datetime2;

UPDATE r 
SET Date = TRY_CONVERT(datetime2, rd.DateValue)
FROM Routes r
INNER JOIN @RouteData rd ON r.RouteID = rd.RouteID;

PRINT 'Routes.Date converted to datetime2';

-- Fix mileage columns
ALTER TABLE Routes DROP COLUMN AMBeginMiles;
ALTER TABLE Routes ADD AMBeginMiles decimal(10,2);

ALTER TABLE Routes DROP COLUMN AMEndMiles;
ALTER TABLE Routes ADD AMEndMiles decimal(10,2);

ALTER TABLE Routes DROP COLUMN PMBeginMiles;
ALTER TABLE Routes ADD PMBeginMiles decimal(10,2);

ALTER TABLE Routes DROP COLUMN PMEndMiles;
ALTER TABLE Routes ADD PMEndMiles decimal(10,2);

-- Update with converted values
UPDATE r SET 
    AMBeginMiles = CASE WHEN rd.AMBeginMilesValue IS NOT NULL THEN CONVERT(decimal(10,2), rd.AMBeginMilesValue) ELSE NULL END,
    AMEndMiles = CASE WHEN rd.AMEndMilesValue IS NOT NULL THEN CONVERT(decimal(10,2), rd.AMEndMilesValue) ELSE NULL END,
    PMBeginMiles = CASE WHEN rd.PMBeginMilesValue IS NOT NULL THEN CONVERT(decimal(10,2), rd.PMBeginMilesValue) ELSE NULL END,
    PMEndMiles = CASE WHEN rd.PMEndMilesValue IS NOT NULL THEN CONVERT(decimal(10,2), rd.PMEndMilesValue) ELSE NULL END
FROM Routes r
INNER JOIN @RouteData rd ON r.RouteID = rd.RouteID;

PRINT 'Routes mileage fields converted to decimal(10,2)';

-- ================================================
-- 5. RECREATE INDEXES
-- ================================================
PRINT 'Creating performance indexes...';

-- Create indexes for the newly typed columns
CREATE INDEX IX_Activities_Date ON Activities(Date);
CREATE INDEX IX_ActivitySchedule_Date ON ActivitySchedule(Date);
CREATE INDEX IX_Maintenance_Date ON Maintenance(Date);
CREATE INDEX IX_Routes_Date ON Routes(Date);

PRINT 'Performance indexes created';

-- ================================================
-- 6. VERIFY SCHEMA ALIGNMENT
-- ================================================
PRINT 'Verifying schema alignment...';

SELECT 'FINAL SCHEMA VERIFICATION' as Status;

SELECT 
    'Activities' as TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Activities' 
AND COLUMN_NAME IN ('Date', 'LeaveTime', 'EventTime', 'ReturnTime')
ORDER BY COLUMN_NAME;

SELECT 
    'ActivitySchedule' as TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ActivitySchedule' 
AND COLUMN_NAME IN ('Date', 'ScheduledLeaveTime', 'ScheduledEventTime', 'ScheduledReturnTime')
ORDER BY COLUMN_NAME;

SELECT 
    'Maintenance' as TableName,
    COLUMN_NAME,
    DATA_TYPE,
    NUMERIC_PRECISION,
    NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Maintenance' 
AND COLUMN_NAME IN ('Date', 'RepairCost')
ORDER BY COLUMN_NAME;

SELECT 
    'Routes' as TableName,
    COLUMN_NAME,
    DATA_TYPE,
    NUMERIC_PRECISION,
    NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Routes' 
AND COLUMN_NAME IN ('Date', 'AMBeginMiles', 'AMEndMiles', 'PMBeginMiles', 'PMEndMiles')
ORDER BY COLUMN_NAME;

PRINT 'Safe database schema alignment completed successfully!';
PRINT 'All temporal and numeric fields now have proper data types matching C# models.';

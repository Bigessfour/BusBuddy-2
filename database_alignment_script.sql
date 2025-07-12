-- Comprehensive Database Schema Alignment Script
-- This script fixes all data type mismatches between C# models and database schema

USE BusBuddyDb;
GO

PRINT 'Starting comprehensive database schema alignment...';

-- ================================================
-- 1. ACTIVITIES TABLE FIXES
-- ================================================
PRINT 'Fixing Activities table...';

-- Fix Date field: nvarchar(50) -> datetime2
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Activities' AND COLUMN_NAME = 'Date' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE Activities ADD Date_New datetime2;
    UPDATE Activities SET Date_New = TRY_CONVERT(datetime2, Date) WHERE Date IS NOT NULL;
    ALTER TABLE Activities DROP COLUMN Date;
    EXEC sp_rename 'Activities.Date_New', 'Date', 'COLUMN';
    PRINT 'Activities.Date converted from nvarchar to datetime2';
END

-- Fix LeaveTime: nvarchar(50) -> time
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Activities' AND COLUMN_NAME = 'LeaveTime' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE Activities ADD LeaveTime_New time;
    UPDATE Activities SET LeaveTime_New = TRY_CONVERT(time, LeaveTime) WHERE LeaveTime IS NOT NULL;
    ALTER TABLE Activities DROP COLUMN LeaveTime;
    EXEC sp_rename 'Activities.LeaveTime_New', 'LeaveTime', 'COLUMN';
    PRINT 'Activities.LeaveTime converted from nvarchar to time';
END

-- Fix EventTime: nvarchar(50) -> time
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Activities' AND COLUMN_NAME = 'EventTime' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE Activities ADD EventTime_New time;
    UPDATE Activities SET EventTime_New = TRY_CONVERT(time, EventTime) WHERE EventTime IS NOT NULL;
    ALTER TABLE Activities DROP COLUMN EventTime;
    EXEC sp_rename 'Activities.EventTime_New', 'EventTime', 'COLUMN';
    PRINT 'Activities.EventTime converted from nvarchar to time';
END

-- Fix ReturnTime: nvarchar(50) -> time
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Activities' AND COLUMN_NAME = 'ReturnTime' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE Activities ADD ReturnTime_New time;
    UPDATE Activities SET ReturnTime_New = TRY_CONVERT(time, ReturnTime) WHERE ReturnTime IS NOT NULL;
    ALTER TABLE Activities DROP COLUMN ReturnTime;
    EXEC sp_rename 'Activities.ReturnTime_New', 'ReturnTime', 'COLUMN';
    PRINT 'Activities.ReturnTime converted from nvarchar to time';
END

-- ================================================
-- 2. ACTIVITYSCHEDULE TABLE FIXES
-- ================================================
PRINT 'Fixing ActivitySchedule table...';

-- Fix Date field: nvarchar(50) -> datetime2
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ActivitySchedule' AND COLUMN_NAME = 'Date' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE ActivitySchedule ADD Date_New datetime2;
    UPDATE ActivitySchedule SET Date_New = TRY_CONVERT(datetime2, Date) WHERE Date IS NOT NULL;
    ALTER TABLE ActivitySchedule DROP COLUMN Date;
    EXEC sp_rename 'ActivitySchedule.Date_New', 'Date', 'COLUMN';
    PRINT 'ActivitySchedule.Date converted from nvarchar to datetime2';
END

-- Fix ScheduledLeaveTime: nvarchar(50) -> time
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ActivitySchedule' AND COLUMN_NAME = 'ScheduledLeaveTime' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE ActivitySchedule ADD ScheduledLeaveTime_New time;
    UPDATE ActivitySchedule SET ScheduledLeaveTime_New = TRY_CONVERT(time, ScheduledLeaveTime) WHERE ScheduledLeaveTime IS NOT NULL;
    ALTER TABLE ActivitySchedule DROP COLUMN ScheduledLeaveTime;
    EXEC sp_rename 'ActivitySchedule.ScheduledLeaveTime_New', 'ScheduledLeaveTime', 'COLUMN';
    PRINT 'ActivitySchedule.ScheduledLeaveTime converted from nvarchar to time';
END

-- Fix ScheduledEventTime: nvarchar(50) -> time
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ActivitySchedule' AND COLUMN_NAME = 'ScheduledEventTime' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE ActivitySchedule ADD ScheduledEventTime_New time;
    UPDATE ActivitySchedule SET ScheduledEventTime_New = TRY_CONVERT(time, ScheduledEventTime) WHERE ScheduledEventTime IS NOT NULL;
    ALTER TABLE ActivitySchedule DROP COLUMN ScheduledEventTime;
    EXEC sp_rename 'ActivitySchedule.ScheduledEventTime_New', 'ScheduledEventTime', 'COLUMN';
    PRINT 'ActivitySchedule.ScheduledEventTime converted from nvarchar to time';
END

-- Fix ScheduledReturnTime: nvarchar(50) -> time
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ActivitySchedule' AND COLUMN_NAME = 'ScheduledReturnTime' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE ActivitySchedule ADD ScheduledReturnTime_New time;
    UPDATE ActivitySchedule SET ScheduledReturnTime_New = TRY_CONVERT(time, ScheduledReturnTime) WHERE ScheduledReturnTime IS NOT NULL;
    ALTER TABLE ActivitySchedule DROP COLUMN ScheduledReturnTime;
    EXEC sp_rename 'ActivitySchedule.ScheduledReturnTime_New', 'ScheduledReturnTime', 'COLUMN';
    PRINT 'ActivitySchedule.ScheduledReturnTime converted from nvarchar to time';
END

-- ================================================
-- 3. DRIVERS TABLE FIXES
-- ================================================
PRINT 'Fixing Drivers table...';

-- Fix TrainingComplete: int -> bit (boolean)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Drivers' AND COLUMN_NAME = 'TrainingComplete' AND DATA_TYPE = 'int')
BEGIN
    ALTER TABLE Drivers ADD TrainingComplete_New bit;
    UPDATE Drivers SET TrainingComplete_New = CASE WHEN TrainingComplete = 1 THEN 1 ELSE 0 END;
    ALTER TABLE Drivers DROP COLUMN TrainingComplete;
    EXEC sp_rename 'Drivers.TrainingComplete_New', 'TrainingComplete', 'COLUMN';
    PRINT 'Drivers.TrainingComplete converted from int to bit';
END

-- ================================================
-- 4. FUEL TABLE FIXES (already partially done)
-- ================================================
PRINT 'Checking Fuel table...';

-- Fix FuelAmount: float -> decimal (if needed)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Fuel' AND COLUMN_NAME = 'FuelAmount' AND DATA_TYPE = 'float')
BEGIN
    ALTER TABLE Fuel ADD FuelAmount_New decimal(10,2);
    UPDATE Fuel SET FuelAmount_New = CONVERT(decimal(10,2), FuelAmount) WHERE FuelAmount IS NOT NULL;
    ALTER TABLE Fuel DROP COLUMN FuelAmount;
    EXEC sp_rename 'Fuel.FuelAmount_New', 'FuelAmount', 'COLUMN';
    PRINT 'Fuel.FuelAmount converted from float to decimal';
END

-- Fix FuelCost: float -> decimal (if needed)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Fuel' AND COLUMN_NAME = 'FuelCost' AND DATA_TYPE = 'float')
BEGIN
    ALTER TABLE Fuel ADD FuelCost_New decimal(10,2);
    UPDATE Fuel SET FuelCost_New = CONVERT(decimal(10,2), FuelCost) WHERE FuelCost IS NOT NULL;
    ALTER TABLE Fuel DROP COLUMN FuelCost;
    EXEC sp_rename 'Fuel.FuelCost_New', 'FuelCost', 'COLUMN';
    PRINT 'Fuel.FuelCost converted from float to decimal';
END

-- ================================================
-- 5. MAINTENANCE TABLE FIXES
-- ================================================
PRINT 'Fixing Maintenance table...';

-- Fix Date field: nvarchar(50) -> datetime2
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Maintenance' AND COLUMN_NAME = 'Date' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE Maintenance ADD Date_New datetime2;
    UPDATE Maintenance SET Date_New = TRY_CONVERT(datetime2, Date) WHERE Date IS NOT NULL;
    ALTER TABLE Maintenance DROP COLUMN Date;
    EXEC sp_rename 'Maintenance.Date_New', 'Date', 'COLUMN';
    PRINT 'Maintenance.Date converted from nvarchar to datetime2';
END

-- Fix RepairCost: float -> decimal
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Maintenance' AND COLUMN_NAME = 'RepairCost' AND DATA_TYPE = 'float')
BEGIN
    ALTER TABLE Maintenance ADD RepairCost_New decimal(10,2);
    UPDATE Maintenance SET RepairCost_New = CONVERT(decimal(10,2), RepairCost) WHERE RepairCost IS NOT NULL;
    ALTER TABLE Maintenance DROP COLUMN RepairCost;
    EXEC sp_rename 'Maintenance.RepairCost_New', 'RepairCost', 'COLUMN';
    PRINT 'Maintenance.RepairCost converted from float to decimal';
END

-- ================================================
-- 6. ROUTES TABLE FIXES
-- ================================================
PRINT 'Fixing Routes table...';

-- Fix Date field: nvarchar(50) -> datetime2
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Routes' AND COLUMN_NAME = 'Date' AND DATA_TYPE = 'nvarchar')
BEGIN
    ALTER TABLE Routes ADD Date_New datetime2;
    UPDATE Routes SET Date_New = TRY_CONVERT(datetime2, Date) WHERE Date IS NOT NULL;
    ALTER TABLE Routes DROP COLUMN Date;
    EXEC sp_rename 'Routes.Date_New', 'Date', 'COLUMN';
    PRINT 'Routes.Date converted from nvarchar to datetime2';
END

-- ================================================
-- 7. RECREATE INDEXES
-- ================================================
PRINT 'Recreating indexes...';

-- Recreate indexes that may have been dropped
CREATE INDEX IX_Activities_Date ON Activities(Date);
CREATE INDEX IX_Activities_AssignedVehicleID ON Activities(AssignedVehicleID);
CREATE INDEX IX_Activities_DriverID ON Activities(DriverID);
CREATE INDEX IX_ActivitySchedule_Date ON ActivitySchedule(Date);
CREATE INDEX IX_Maintenance_Date ON Maintenance(Date);
CREATE INDEX IX_Maintenance_VehicleID ON Maintenance(VehicleID);
CREATE INDEX IX_Routes_Date ON Routes(Date);

PRINT 'Database schema alignment completed successfully!';
PRINT 'All data type mismatches between C# models and database schema have been resolved.';

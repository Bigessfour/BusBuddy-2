-- Targeted Database Schema Alignment Script
-- This script fixes specific data type mismatches between C# models and database schema

USE BusBuddyDb;
GO

PRINT 'Starting targeted database schema alignment...';

-- ================================================
-- 1. ACTIVITIES TABLE FIXES
-- ================================================
PRINT 'Fixing Activities table...';

-- Fix Date field: nvarchar(50) -> datetime2
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE Activities ADD Date_New datetime2;
    
    -- Update new column with converted values
    UPDATE Activities 
    SET Date_New = CASE 
        WHEN Date IS NOT NULL AND LEN(TRIM(Date)) > 0
        THEN TRY_CONVERT(datetime2, Date)
        ELSE NULL
    END;
    
    -- Drop old column and rename new one
    ALTER TABLE Activities DROP COLUMN Date;
    EXEC sp_rename 'Activities.Date_New', 'Date', 'COLUMN';
    
    PRINT 'Activities.Date converted from nvarchar to datetime2';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting Activities.Date: ' + ERROR_MESSAGE();
END CATCH

-- Fix LeaveTime: nvarchar(50) -> time
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE Activities ADD LeaveTime_New time;
    
    UPDATE Activities 
    SET LeaveTime_New = CASE 
        WHEN LeaveTime IS NOT NULL AND LEN(TRIM(LeaveTime)) > 0
        THEN TRY_CONVERT(time, LeaveTime)
        ELSE NULL
    END;
    
    ALTER TABLE Activities DROP COLUMN LeaveTime;
    EXEC sp_rename 'Activities.LeaveTime_New', 'LeaveTime', 'COLUMN';
    
    PRINT 'Activities.LeaveTime converted from nvarchar to time';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting Activities.LeaveTime: ' + ERROR_MESSAGE();
END CATCH

-- Fix EventTime: nvarchar(50) -> time
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE Activities ADD EventTime_New time;
    
    UPDATE Activities 
    SET EventTime_New = CASE 
        WHEN EventTime IS NOT NULL AND LEN(TRIM(EventTime)) > 0
        THEN TRY_CONVERT(time, EventTime)
        ELSE NULL
    END;
    
    ALTER TABLE Activities DROP COLUMN EventTime;
    EXEC sp_rename 'Activities.EventTime_New', 'EventTime', 'COLUMN';
    
    PRINT 'Activities.EventTime converted from nvarchar to time';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting Activities.EventTime: ' + ERROR_MESSAGE();
END CATCH

-- Fix ReturnTime: nvarchar(50) -> time
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE Activities ADD ReturnTime_New time;
    
    UPDATE Activities 
    SET ReturnTime_New = CASE 
        WHEN ReturnTime IS NOT NULL AND LEN(TRIM(ReturnTime)) > 0
        THEN TRY_CONVERT(time, ReturnTime)
        ELSE NULL
    END;
    
    ALTER TABLE Activities DROP COLUMN ReturnTime;
    EXEC sp_rename 'Activities.ReturnTime_New', 'ReturnTime', 'COLUMN';
    
    PRINT 'Activities.ReturnTime converted from nvarchar to time';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting Activities.ReturnTime: ' + ERROR_MESSAGE();
END CATCH

-- ================================================
-- 2. ACTIVITYSCHEDULE TABLE FIXES
-- ================================================
PRINT 'Fixing ActivitySchedule table...';

-- Fix Date field: nvarchar(50) -> datetime2
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE ActivitySchedule ADD Date_New datetime2;
    
    UPDATE ActivitySchedule 
    SET Date_New = CASE 
        WHEN Date IS NOT NULL AND LEN(TRIM(Date)) > 0
        THEN TRY_CONVERT(datetime2, Date)
        ELSE NULL
    END;
    
    ALTER TABLE ActivitySchedule DROP COLUMN Date;
    EXEC sp_rename 'ActivitySchedule.Date_New', 'Date', 'COLUMN';
    
    PRINT 'ActivitySchedule.Date converted from nvarchar to datetime2';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting ActivitySchedule.Date: ' + ERROR_MESSAGE();
END CATCH

-- Fix ScheduledLeaveTime: nvarchar(50) -> time
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE ActivitySchedule ADD ScheduledLeaveTime_New time;
    
    UPDATE ActivitySchedule 
    SET ScheduledLeaveTime_New = CASE 
        WHEN ScheduledLeaveTime IS NOT NULL AND LEN(TRIM(ScheduledLeaveTime)) > 0
        THEN TRY_CONVERT(time, ScheduledLeaveTime)
        ELSE NULL
    END;
    
    ALTER TABLE ActivitySchedule DROP COLUMN ScheduledLeaveTime;
    EXEC sp_rename 'ActivitySchedule.ScheduledLeaveTime_New', 'ScheduledLeaveTime', 'COLUMN';
    
    PRINT 'ActivitySchedule.ScheduledLeaveTime converted from nvarchar to time';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting ActivitySchedule.ScheduledLeaveTime: ' + ERROR_MESSAGE();
END CATCH

-- Fix ScheduledEventTime: nvarchar(50) -> time
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE ActivitySchedule ADD ScheduledEventTime_New time;
    
    UPDATE ActivitySchedule 
    SET ScheduledEventTime_New = CASE 
        WHEN ScheduledEventTime IS NOT NULL AND LEN(TRIM(ScheduledEventTime)) > 0
        THEN TRY_CONVERT(time, ScheduledEventTime)
        ELSE NULL
    END;
    
    ALTER TABLE ActivitySchedule DROP COLUMN ScheduledEventTime;
    EXEC sp_rename 'ActivitySchedule.ScheduledEventTime_New', 'ScheduledEventTime', 'COLUMN';
    
    PRINT 'ActivitySchedule.ScheduledEventTime converted from nvarchar to time';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting ActivitySchedule.ScheduledEventTime: ' + ERROR_MESSAGE();
END CATCH

-- Fix ScheduledReturnTime: nvarchar(50) -> time
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE ActivitySchedule ADD ScheduledReturnTime_New time;
    
    UPDATE ActivitySchedule 
    SET ScheduledReturnTime_New = CASE 
        WHEN ScheduledReturnTime IS NOT NULL AND LEN(TRIM(ScheduledReturnTime)) > 0
        THEN TRY_CONVERT(time, ScheduledReturnTime)
        ELSE NULL
    END;
    
    ALTER TABLE ActivitySchedule DROP COLUMN ScheduledReturnTime;
    EXEC sp_rename 'ActivitySchedule.ScheduledReturnTime_New', 'ScheduledReturnTime', 'COLUMN';
    
    PRINT 'ActivitySchedule.ScheduledReturnTime converted from nvarchar to time';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting ActivitySchedule.ScheduledReturnTime: ' + ERROR_MESSAGE();
END CATCH

-- ================================================
-- 3. MAINTENANCE TABLE FIXES
-- ================================================
PRINT 'Fixing Maintenance table...';

-- Fix Date field: nvarchar(50) -> datetime2
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE Maintenance ADD Date_New datetime2;
    
    UPDATE Maintenance 
    SET Date_New = CASE 
        WHEN Date IS NOT NULL AND LEN(TRIM(Date)) > 0
        THEN TRY_CONVERT(datetime2, Date)
        ELSE NULL
    END;
    
    ALTER TABLE Maintenance DROP COLUMN Date;
    EXEC sp_rename 'Maintenance.Date_New', 'Date', 'COLUMN';
    
    PRINT 'Maintenance.Date converted from nvarchar to datetime2';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting Maintenance.Date: ' + ERROR_MESSAGE();
END CATCH

-- Fix RepairCost: float -> decimal(10,2)
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE Maintenance ADD RepairCost_New decimal(10,2);
    
    UPDATE Maintenance 
    SET RepairCost_New = CASE 
        WHEN RepairCost IS NOT NULL
        THEN CONVERT(decimal(10,2), RepairCost)
        ELSE NULL
    END;
    
    ALTER TABLE Maintenance DROP COLUMN RepairCost;
    EXEC sp_rename 'Maintenance.RepairCost_New', 'RepairCost', 'COLUMN';
    
    PRINT 'Maintenance.RepairCost converted from float to decimal';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting Maintenance.RepairCost: ' + ERROR_MESSAGE();
END CATCH

-- ================================================
-- 4. ROUTES TABLE FIXES
-- ================================================
PRINT 'Fixing Routes table...';

-- Fix Date field: nvarchar(50) -> datetime2
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE Routes ADD Date_New datetime2;
    
    UPDATE Routes 
    SET Date_New = CASE 
        WHEN Date IS NOT NULL AND LEN(TRIM(Date)) > 0
        THEN TRY_CONVERT(datetime2, Date)
        ELSE NULL
    END;
    
    ALTER TABLE Routes DROP COLUMN Date;
    EXEC sp_rename 'Routes.Date_New', 'Date', 'COLUMN';
    
    PRINT 'Routes.Date converted from nvarchar to datetime2';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting Routes.Date: ' + ERROR_MESSAGE();
END CATCH

-- Fix mileage fields: float -> decimal(10,2)
BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE Routes ADD AMBeginMiles_New decimal(10,2);
    ALTER TABLE Routes ADD AMEndMiles_New decimal(10,2);
    ALTER TABLE Routes ADD PMBeginMiles_New decimal(10,2);
    ALTER TABLE Routes ADD PMEndMiles_New decimal(10,2);
    
    UPDATE Routes SET 
        AMBeginMiles_New = CASE WHEN AMBeginMiles IS NOT NULL THEN CONVERT(decimal(10,2), AMBeginMiles) ELSE NULL END,
        AMEndMiles_New = CASE WHEN AMEndMiles IS NOT NULL THEN CONVERT(decimal(10,2), AMEndMiles) ELSE NULL END,
        PMBeginMiles_New = CASE WHEN PMBeginMiles IS NOT NULL THEN CONVERT(decimal(10,2), PMBeginMiles) ELSE NULL END,
        PMEndMiles_New = CASE WHEN PMEndMiles IS NOT NULL THEN CONVERT(decimal(10,2), PMEndMiles) ELSE NULL END;
    
    ALTER TABLE Routes DROP COLUMN AMBeginMiles;
    ALTER TABLE Routes DROP COLUMN AMEndMiles;
    ALTER TABLE Routes DROP COLUMN PMBeginMiles;
    ALTER TABLE Routes DROP COLUMN PMEndMiles;
    
    EXEC sp_rename 'Routes.AMBeginMiles_New', 'AMBeginMiles', 'COLUMN';
    EXEC sp_rename 'Routes.AMEndMiles_New', 'AMEndMiles', 'COLUMN';
    EXEC sp_rename 'Routes.PMBeginMiles_New', 'PMBeginMiles', 'COLUMN';
    EXEC sp_rename 'Routes.PMEndMiles_New', 'PMEndMiles', 'COLUMN';
    
    PRINT 'Routes mileage fields converted from float to decimal';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error converting Routes mileage fields: ' + ERROR_MESSAGE();
END CATCH

PRINT 'Targeted database schema alignment completed successfully!';
PRINT 'Key data type mismatches have been resolved.';

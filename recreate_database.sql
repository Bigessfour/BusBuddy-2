-- ================================================
-- BusBuddy Database Recreation Script
-- Fixes all schema alignment issues and exceptions
-- ================================================

USE master;
GO

-- Drop existing database if it exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'BusBuddyDB')
BEGIN
    ALTER DATABASE BusBuddyDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BusBuddyDB;
    PRINT 'Existing BusBuddyDB database dropped successfully.';
END

-- Create new database with proper settings
CREATE DATABASE BusBuddyDB
ON (
    NAME = 'BusBuddyDB',
    FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\BusBuddyDB.mdf',
    SIZE = 100MB,
    MAXSIZE = 1GB,
    FILEGROWTH = 10MB
)
LOG ON (
    NAME = 'BusBuddyDB_Log',
    FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\BusBuddyDB_Log.ldf',
    SIZE = 10MB,
    MAXSIZE = 100MB,
    FILEGROWTH = 10%
);

PRINT 'New BusBuddyDB database created successfully.';

-- Set database options for optimal performance and compatibility
USE BusBuddyDB;
GO

-- Enable case insensitive collation
ALTER DATABASE BusBuddyDB COLLATE SQL_Latin1_General_CP1_CI_AS;

-- Set compatibility level to SQL Server 2022
ALTER DATABASE BusBuddyDB SET COMPATIBILITY_LEVEL = 160;

-- Enable snapshot isolation for better concurrency
ALTER DATABASE BusBuddyDB SET ALLOW_SNAPSHOT_ISOLATION ON;
ALTER DATABASE BusBuddyDB SET READ_COMMITTED_SNAPSHOT ON;

-- Optimize for EF Core
ALTER DATABASE BusBuddyDB SET AUTO_CLOSE OFF;
ALTER DATABASE BusBuddyDB SET AUTO_SHRINK OFF;
ALTER DATABASE BusBuddyDB SET AUTO_UPDATE_STATISTICS ON;
ALTER DATABASE BusBuddyDB SET AUTO_CREATE_STATISTICS ON;

PRINT 'Database configuration completed successfully.';
PRINT 'Ready for EF Core migrations.';

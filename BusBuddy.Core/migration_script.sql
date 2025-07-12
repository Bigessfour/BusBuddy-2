IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE TABLE [Drivers] (
        [DriverId] int NOT NULL IDENTITY,
        [DriverName] nvarchar(100) NOT NULL,
        [DriverPhone] nvarchar(20) NULL,
        [DriverEmail] nvarchar(100) NULL,
        [Address] nvarchar(200) NULL,
        [City] nvarchar(50) NULL,
        [State] nvarchar(2) NULL,
        [Zip] nvarchar(10) NULL,
        [DriversLicenceType] nvarchar(20) NOT NULL,
        [TrainingComplete] bit NOT NULL,
        CONSTRAINT [PK_Drivers] PRIMARY KEY ([DriverId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE TABLE [Students] (
        [StudentId] int NOT NULL IDENTITY,
        [StudentName] nvarchar(100) NOT NULL,
        [StudentNumber] nvarchar(20) NULL,
        [Grade] nvarchar(20) NULL,
        [School] nvarchar(100) NULL,
        [HomeAddress] nvarchar(200) NULL,
        [City] nvarchar(50) NULL,
        [State] nvarchar(2) NULL,
        [Zip] nvarchar(10) NULL,
        [HomePhone] nvarchar(20) NULL,
        [ParentGuardian] nvarchar(100) NULL,
        [EmergencyPhone] nvarchar(20) NULL,
        [MedicalNotes] nvarchar(1000) NULL,
        [TransportationNotes] nvarchar(1000) NULL,
        [Active] bit NOT NULL,
        [EnrollmentDate] datetime2 NULL,
        [AMRoute] nvarchar(50) NULL,
        [PMRoute] nvarchar(50) NULL,
        [BusStop] nvarchar(50) NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([StudentId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE TABLE [Vehicles] (
        [VehicleId] int NOT NULL IDENTITY,
        [BusNumber] nvarchar(20) NOT NULL,
        [Year] int NOT NULL,
        [Make] nvarchar(50) NOT NULL,
        [Model] nvarchar(50) NOT NULL,
        [SeatingCapacity] int NOT NULL,
        [VINNumber] nvarchar(17) NOT NULL,
        [LicenseNumber] nvarchar(20) NOT NULL,
        [DateLastInspection] datetime2 NULL,
        [CurrentOdometer] int NULL,
        [Status] nvarchar(20) NOT NULL,
        [PurchaseDate] datetime2 NULL,
        [PurchasePrice] decimal(10,2) NULL,
        [InsurancePolicyNumber] nvarchar(100) NULL,
        [InsuranceExpiryDate] datetime2 NULL,
        CONSTRAINT [PK_Vehicles] PRIMARY KEY ([VehicleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE TABLE [Fuel] (
        [FuelId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [FuelDate] datetime2 NOT NULL,
        [Gallons] decimal(8,3) NOT NULL,
        [PricePerGallon] decimal(8,3) NOT NULL,
        [TotalCost] decimal(10,2) NOT NULL,
        [OdometerReading] int NULL,
        [FuelStation] nvarchar(50) NULL,
        [Location] nvarchar(50) NULL,
        [Notes] nvarchar(500) NULL,
        [MilesPerGallon] decimal(5,2) NULL,
        CONSTRAINT [PK_Fuel] PRIMARY KEY ([FuelId]),
        CONSTRAINT [FK_Fuel_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE TABLE [Maintenance] (
        [MaintenanceId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [MaintenanceDate] datetime2 NOT NULL,
        [MaintenanceType] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Cost] decimal(10,2) NULL,
        [OdometerReading] int NULL,
        [PerformedBy] nvarchar(100) NULL,
        [ShopVendor] nvarchar(100) NULL,
        [NextServiceDue] datetime2 NULL,
        [NextServiceOdometer] int NULL,
        [Status] nvarchar(20) NOT NULL,
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_Maintenance] PRIMARY KEY ([MaintenanceId]),
        CONSTRAINT [FK_Maintenance_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE TABLE [Routes] (
        [RouteId] int NOT NULL IDENTITY,
        [Date] datetime2 NOT NULL,
        [RouteName] nvarchar(50) NOT NULL,
        [AMVehicleId] int NULL,
        [AMBeginMiles] decimal(10,2) NULL,
        [AMEndMiles] decimal(10,2) NULL,
        [AMRiders] int NULL,
        [AMDriverId] int NULL,
        [PMVehicleId] int NULL,
        [PMBeginMiles] decimal(10,2) NULL,
        [PMEndMiles] decimal(10,2) NULL,
        [PMRiders] int NULL,
        [PMDriverId] int NULL,
        CONSTRAINT [PK_Routes] PRIMARY KEY ([RouteId]),
        CONSTRAINT [FK_Routes_Drivers_AMDriverId] FOREIGN KEY ([AMDriverId]) REFERENCES [Drivers] ([DriverId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Routes_Drivers_PMDriverId] FOREIGN KEY ([PMDriverId]) REFERENCES [Drivers] ([DriverId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Routes_Vehicles_AMVehicleId] FOREIGN KEY ([AMVehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Routes_Vehicles_PMVehicleId] FOREIGN KEY ([PMVehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE TABLE [Activity] (
        [ActivityId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [RouteId] int NOT NULL,
        [DriverId] int NOT NULL,
        [ActivityDate] datetime2 NOT NULL,
        [ActivityType] nvarchar(10) NOT NULL,
        [StartTime] time NULL,
        [EndTime] time NULL,
        [StartOdometer] int NULL,
        [EndOdometer] int NULL,
        [Notes] nvarchar(500) NULL,
        [StudentsCount] int NULL,
        CONSTRAINT [PK_Activity] PRIMARY KEY ([ActivityId]),
        CONSTRAINT [FK_Activity_Drivers_DriverId] FOREIGN KEY ([DriverId]) REFERENCES [Drivers] ([DriverId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Activity_Routes_RouteId] FOREIGN KEY ([RouteId]) REFERENCES [Routes] ([RouteId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Activity_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DriverId', N'Address', N'City', N'DriverEmail', N'DriverName', N'DriverPhone', N'DriversLicenceType', N'State', N'TrainingComplete', N'Zip') AND [object_id] = OBJECT_ID(N'[Drivers]'))
        SET IDENTITY_INSERT [Drivers] ON;
    EXEC(N'INSERT INTO [Drivers] ([DriverId], [Address], [City], [DriverEmail], [DriverName], [DriverPhone], [DriversLicenceType], [State], [TrainingComplete], [Zip])
    VALUES (1, NULL, NULL, N''john.smith@school.edu'', N''John Smith'', N''555-0123'', N''CDL'', NULL, CAST(1 AS bit), NULL),
    (2, NULL, NULL, N''mary.johnson@school.edu'', N''Mary Johnson'', N''555-0456'', N''CDL'', NULL, CAST(1 AS bit), NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DriverId', N'Address', N'City', N'DriverEmail', N'DriverName', N'DriverPhone', N'DriversLicenceType', N'State', N'TrainingComplete', N'Zip') AND [object_id] = OBJECT_ID(N'[Drivers]'))
        SET IDENTITY_INSERT [Drivers] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'VehicleId', N'BusNumber', N'CurrentOdometer', N'DateLastInspection', N'InsuranceExpiryDate', N'InsurancePolicyNumber', N'LicenseNumber', N'Make', N'Model', N'PurchaseDate', N'PurchasePrice', N'SeatingCapacity', N'Status', N'VINNumber', N'Year') AND [object_id] = OBJECT_ID(N'[Vehicles]'))
        SET IDENTITY_INSERT [Vehicles] ON;
    EXEC(N'INSERT INTO [Vehicles] ([VehicleId], [BusNumber], [CurrentOdometer], [DateLastInspection], [InsuranceExpiryDate], [InsurancePolicyNumber], [LicenseNumber], [Make], [Model], [PurchaseDate], [PurchasePrice], [SeatingCapacity], [Status], [VINNumber], [Year])
    VALUES (1, N''001'', NULL, NULL, NULL, NULL, N''TX123456'', N''Blue Bird'', N''Vision'', ''2020-08-15T00:00:00.0000000'', 85000.0, 72, N''Active'', N''1BAANKCL7LF123456'', 2020),
    (2, N''002'', NULL, NULL, NULL, NULL, N''TX654321'', N''Thomas Built'', N''Saf-T-Liner C2'', ''2019-07-10T00:00:00.0000000'', 82000.0, 66, N''Active'', N''4DRBTAAN7KB654321'', 2019)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'VehicleId', N'BusNumber', N'CurrentOdometer', N'DateLastInspection', N'InsuranceExpiryDate', N'InsurancePolicyNumber', N'LicenseNumber', N'Make', N'Model', N'PurchaseDate', N'PurchasePrice', N'SeatingCapacity', N'Status', N'VINNumber', N'Year') AND [object_id] = OBJECT_ID(N'[Vehicles]'))
        SET IDENTITY_INSERT [Vehicles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Activity_DriverId] ON [Activity] ([DriverId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Activity_RouteId] ON [Activity] ([RouteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Activity_VehicleId] ON [Activity] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Fuel_VehicleId] ON [Fuel] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Maintenance_VehicleId] ON [Maintenance] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Routes_AMDriverId] ON [Routes] ([AMDriverId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Routes_AMVehicleId] ON [Routes] ([AMVehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Routes_PMDriverId] ON [Routes] ([PMDriverId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Routes_PMVehicleId] ON [Routes] ([PMVehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Vehicles_BusNumber] ON [Vehicles] ([BusNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Vehicles_VINNumber] ON [Vehicles] ([VINNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703120139_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250703120139_InitialCreate', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activity] DROP CONSTRAINT [FK_Activity_Drivers_DriverId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activity] DROP CONSTRAINT [FK_Activity_Routes_RouteId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activity] DROP CONSTRAINT [FK_Activity_Vehicles_VehicleId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Fuel] DROP CONSTRAINT [FK_Fuel_Vehicles_VehicleId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] DROP CONSTRAINT [FK_Maintenance_Vehicles_VehicleId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Routes] DROP CONSTRAINT [FK_Routes_Drivers_AMDriverId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Routes] DROP CONSTRAINT [FK_Routes_Drivers_PMDriverId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Routes] DROP CONSTRAINT [FK_Routes_Vehicles_AMVehicleId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Routes] DROP CONSTRAINT [FK_Routes_Vehicles_PMVehicleId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DROP INDEX [IX_Fuel_VehicleId] ON [Fuel];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activity] DROP CONSTRAINT [PK_Activity];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DROP INDEX [IX_Activity_RouteId] ON [Activity];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Maintenance]') AND [c].[name] = N'MaintenanceType');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Maintenance] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Maintenance] DROP COLUMN [MaintenanceType];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Fuel]') AND [c].[name] = N'FuelStation');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Fuel] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Fuel] DROP COLUMN [FuelStation];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Fuel]') AND [c].[name] = N'Location');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Fuel] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Fuel] DROP COLUMN [Location];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Fuel]') AND [c].[name] = N'MilesPerGallon');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Fuel] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Fuel] DROP COLUMN [MilesPerGallon];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Fuel]') AND [c].[name] = N'OdometerReading');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Fuel] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Fuel] DROP COLUMN [OdometerReading];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Activity]') AND [c].[name] = N'EndOdometer');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Activity] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Activity] DROP COLUMN [EndOdometer];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Activity]') AND [c].[name] = N'EndTime');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Activity] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Activity] DROP COLUMN [EndTime];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Activity]') AND [c].[name] = N'RouteId');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Activity] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Activity] DROP COLUMN [RouteId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Activity]') AND [c].[name] = N'StartOdometer');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Activity] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Activity] DROP COLUMN [StartOdometer];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Activity]') AND [c].[name] = N'StartTime');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Activity] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [Activity] DROP COLUMN [StartTime];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC sp_rename N'[Activity]', N'Activities';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC sp_rename N'[Maintenance].[ShopVendor]', N'WorkOrderNumber', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC sp_rename N'[Maintenance].[MaintenanceDate]', N'Date', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC sp_rename N'[Maintenance].[Cost]', N'PartsCost', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC sp_rename N'[Fuel].[VehicleId]', N'VehicleOdometerReading', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC sp_rename N'[Activities].[VehicleId]', N'AssignedVehicleId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC sp_rename N'[Activities].[ActivityDate]', N'Date', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC sp_rename N'[Activities].[IX_Activity_VehicleId]', N'IX_Activities_VehicleId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC sp_rename N'[Activities].[IX_Activity_DriverId]', N'IX_Activities_DriverId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vehicles]') AND [c].[name] = N'Status');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Vehicles] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Vehicles] ADD DEFAULT N'Active' FOR [Status];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [CreatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [Department] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [FleetType] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [FuelCapacity] decimal(8,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [FuelType] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [GPSDeviceId] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [GPSTracking] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [LastServiceDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [MilesPerGallon] decimal(6,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [NextMaintenanceDue] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [NextMaintenanceMileage] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [Notes] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [SpecialEquipment] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [UpdatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Vehicles] ADD [UpdatedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [Allergies] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [AlternativeContact] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [AlternativePhone] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [CreatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [DateOfBirth] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [DoctorName] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [DoctorPhone] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [DropoffAddress] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [FieldTripPermission] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [Gender] nvarchar(10) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [Medications] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [PhotoPermission] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [PickupAddress] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [SpecialAccommodations] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [SpecialNeeds] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [UpdatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Students] ADD [UpdatedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Maintenance]') AND [c].[name] = N'OdometerReading');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Maintenance] DROP CONSTRAINT [' + @var11 + '];');
    EXEC(N'UPDATE [Maintenance] SET [OdometerReading] = 0 WHERE [OdometerReading] IS NULL');
    ALTER TABLE [Maintenance] ALTER COLUMN [OdometerReading] int NOT NULL;
    ALTER TABLE [Maintenance] ADD DEFAULT 0 FOR [OdometerReading];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Maintenance]') AND [c].[name] = N'Description');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Maintenance] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Maintenance] ALTER COLUMN [Description] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [CreatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [LaborCost] decimal(10,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [LaborHours] decimal(8,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [MaintenanceCompleted] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [PartsUsed] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [Priority] nvarchar(20) NOT NULL DEFAULT N'Normal';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [RepairCost] decimal(10,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [UpdatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [UpdatedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [Vendor] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [Warranty] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD [WarrantyExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Fuel]') AND [c].[name] = N'TotalCost');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Fuel] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [Fuel] ALTER COLUMN [TotalCost] decimal(10,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Fuel]') AND [c].[name] = N'PricePerGallon');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Fuel] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Fuel] ALTER COLUMN [PricePerGallon] decimal(8,3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Fuel]') AND [c].[name] = N'Gallons');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Fuel] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [Fuel] ALTER COLUMN [Gallons] decimal(8,3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Fuel] ADD [FuelLocation] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Fuel] ADD [FuelType] nvarchar(20) NOT NULL DEFAULT N'Gasoline';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Fuel] ADD [VehicleFueledId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Drivers]') AND [c].[name] = N'State');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Drivers] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [Drivers] ALTER COLUMN [State] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Drivers]') AND [c].[name] = N'LastName');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Drivers] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [Drivers] ALTER COLUMN [LastName] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Drivers]') AND [c].[name] = N'FirstName');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Drivers] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [Drivers] ALTER COLUMN [FirstName] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [BackgroundCheckDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [BackgroundCheckExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [CreatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [DrugTestDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [DrugTestExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [EmergencyContactName] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [EmergencyContactPhone] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [MedicalRestrictions] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [Notes] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [PhysicalExamDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [PhysicalExamExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [UpdatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Drivers] ADD [UpdatedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Activities]') AND [c].[name] = N'ActivityType');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Activities] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [Activities] ALTER COLUMN [ActivityType] nvarchar(50) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [ActivityCategory] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [ActualCost] decimal(10,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [ApprovalDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [ApprovalRequired] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [Approved] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [ApprovedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [CreatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [Destination] nvarchar(200) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [EstimatedCost] decimal(10,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [EventTime] time NOT NULL DEFAULT '00:00:00';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [LeaveTime] time NOT NULL DEFAULT '00:00:00';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [RequestedBy] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [Status] nvarchar(20) NOT NULL DEFAULT N'Scheduled';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [UpdatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD [UpdatedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD CONSTRAINT [PK_Activities] PRIMARY KEY ([ActivityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE TABLE [ActivitySchedule] (
        [ActivityScheduleId] int NOT NULL IDENTITY,
        [ScheduledDate] datetime2 NOT NULL,
        [TripType] nvarchar(50) NOT NULL,
        [ScheduledVehicleId] int NOT NULL,
        [ScheduledDestination] nvarchar(200) NOT NULL,
        [ScheduledLeaveTime] time NOT NULL,
        [ScheduledEventTime] time NOT NULL,
        [ScheduledRiders] int NULL,
        [ScheduledDriverId] int NOT NULL,
        [RequestedBy] nvarchar(100) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [CreatedBy] nvarchar(100) NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_ActivitySchedule] PRIMARY KEY ([ActivityScheduleId]),
        CONSTRAINT [FK_ActivitySchedule_Driver] FOREIGN KEY ([ScheduledDriverId]) REFERENCES [Drivers] ([DriverId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ActivitySchedule_Vehicle] FOREIGN KEY ([ScheduledVehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE TABLE [RouteStops] (
        [RouteStopId] int NOT NULL IDENTITY,
        [RouteId] int NOT NULL,
        [StopName] nvarchar(100) NOT NULL,
        [StopAddress] nvarchar(200) NULL,
        [Latitude] decimal(10,8) NULL,
        [Longitude] decimal(11,8) NULL,
        [StopOrder] int NOT NULL,
        [ScheduledArrival] time NOT NULL,
        [ScheduledDeparture] time NOT NULL,
        [StopDuration] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_RouteStops] PRIMARY KEY ([RouteStopId]),
        CONSTRAINT [FK_RouteStops_Route] FOREIGN KEY ([RouteId]) REFERENCES [Routes] ([RouteId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE TABLE [Schedules] (
        [ScheduleId] int NOT NULL IDENTITY,
        [BusId] int NOT NULL,
        [RouteId] int NOT NULL,
        [DriverId] int NOT NULL,
        [DepartureTime] datetime2 NOT NULL,
        [ArrivalTime] datetime2 NOT NULL,
        [ScheduleDate] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_Schedules] PRIMARY KEY ([ScheduleId]),
        CONSTRAINT [FK_Schedules_Bus] FOREIGN KEY ([BusId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Schedules_Driver] FOREIGN KEY ([DriverId]) REFERENCES [Drivers] ([DriverId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Schedules_Route] FOREIGN KEY ([RouteId]) REFERENCES [Routes] ([RouteId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE TABLE [SchoolCalendar] (
        [CalendarId] int NOT NULL IDENTITY,
        [Date] datetime2 NOT NULL,
        [EventType] nvarchar(50) NOT NULL,
        [EventName] nvarchar(100) NOT NULL,
        [SchoolYear] nvarchar(10) NOT NULL,
        [StartDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        [RoutesRequired] bit NOT NULL,
        [Description] nvarchar(200) NULL,
        [Notes] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_SchoolCalendar] PRIMARY KEY ([CalendarId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC(N'UPDATE [Drivers] SET [BackgroundCheckDate] = NULL, [BackgroundCheckExpiry] = NULL, [CreatedBy] = NULL, [CreatedDate] = ''2025-07-03T18:31:36.1061651Z'', [DrugTestDate] = NULL, [DrugTestExpiry] = NULL, [EmergencyContactName] = NULL, [EmergencyContactPhone] = NULL, [FirstName] = NULL, [LastName] = NULL, [MedicalRestrictions] = NULL, [Notes] = NULL, [PhysicalExamDate] = NULL, [PhysicalExamExpiry] = NULL, [UpdatedBy] = NULL, [UpdatedDate] = NULL
    WHERE [DriverId] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC(N'UPDATE [Drivers] SET [BackgroundCheckDate] = NULL, [BackgroundCheckExpiry] = NULL, [CreatedBy] = NULL, [CreatedDate] = ''2025-07-03T18:31:36.1061660Z'', [DrugTestDate] = NULL, [DrugTestExpiry] = NULL, [EmergencyContactName] = NULL, [EmergencyContactPhone] = NULL, [FirstName] = NULL, [LastName] = NULL, [MedicalRestrictions] = NULL, [Notes] = NULL, [PhysicalExamDate] = NULL, [PhysicalExamExpiry] = NULL, [UpdatedBy] = NULL, [UpdatedDate] = NULL
    WHERE [DriverId] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC(N'UPDATE [Vehicles] SET [CreatedBy] = NULL, [CreatedDate] = ''2025-07-03T18:31:36.1061559Z'', [Department] = NULL, [FleetType] = NULL, [FuelCapacity] = NULL, [FuelType] = NULL, [GPSDeviceId] = NULL, [GPSTracking] = CAST(0 AS bit), [LastServiceDate] = NULL, [MilesPerGallon] = NULL, [NextMaintenanceDue] = NULL, [NextMaintenanceMileage] = NULL, [Notes] = NULL, [SpecialEquipment] = NULL, [UpdatedBy] = NULL, [UpdatedDate] = NULL
    WHERE [VehicleId] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    EXEC(N'UPDATE [Vehicles] SET [CreatedBy] = NULL, [CreatedDate] = ''2025-07-03T18:31:36.1061602Z'', [Department] = NULL, [FleetType] = NULL, [FuelCapacity] = NULL, [FuelType] = NULL, [GPSDeviceId] = NULL, [GPSTracking] = CAST(0 AS bit), [LastServiceDate] = NULL, [MilesPerGallon] = NULL, [NextMaintenanceDue] = NULL, [NextMaintenanceMileage] = NULL, [Notes] = NULL, [SpecialEquipment] = NULL, [UpdatedBy] = NULL, [UpdatedDate] = NULL
    WHERE [VehicleId] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Vehicles_DateLastInspection] ON [Vehicles] ([DateLastInspection]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Vehicles_FleetType] ON [Vehicles] ([FleetType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Vehicles_InsuranceExpiryDate] ON [Vehicles] ([InsuranceExpiryDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Vehicles_LicenseNumber] ON [Vehicles] ([LicenseNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Vehicles_MakeModelYear] ON [Vehicles] ([Make], [Model], [Year]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Vehicles_Status] ON [Vehicles] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Students_Active] ON [Students] ([Active]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Students_Grade] ON [Students] ([Grade]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Students_Name] ON [Students] ([StudentName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Students_School] ON [Students] ([School]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Routes_Date] ON [Routes] ([Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Routes_DateRouteName] ON [Routes] ([Date], [RouteName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Routes_RouteName] ON [Routes] ([RouteName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Maintenance_Date] ON [Maintenance] ([Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Maintenance_Priority] ON [Maintenance] ([Priority]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Maintenance_Type] ON [Maintenance] ([MaintenanceCompleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Maintenance_VehicleDate] ON [Maintenance] ([VehicleId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Fuel_FuelDate] ON [Fuel] ([FuelDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Fuel_Location] ON [Fuel] ([FuelLocation]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Fuel_Type] ON [Fuel] ([FuelType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Fuel_VehicleDate] ON [Fuel] ([VehicleFueledId], [FuelDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Fuel_VehicleId] ON [Fuel] ([VehicleFueledId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Drivers_Email] ON [Drivers] ([DriverEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Drivers_LicenseExpiration] ON [Drivers] ([LicenseExpiryDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Drivers_LicenseType] ON [Drivers] ([DriversLicenceType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Drivers_Phone] ON [Drivers] ([DriverPhone]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Drivers_TrainingComplete] ON [Drivers] ([TrainingComplete]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Activities_ActivityType] ON [Activities] ([ActivityType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Activities_ApprovalRequired] ON [Activities] ([ApprovalRequired]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Activities_Date] ON [Activities] ([Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Activities_DateTimeRange] ON [Activities] ([Date], [LeaveTime], [EventTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Activities_DriverSchedule] ON [Activities] ([DriverId], [Date], [LeaveTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Activities_Status] ON [Activities] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Activities_VehicleSchedule] ON [Activities] ([AssignedVehicleId], [Date], [LeaveTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_ActivitySchedule_Date] ON [ActivitySchedule] ([ScheduledDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_ActivitySchedule_DriverId] ON [ActivitySchedule] ([ScheduledDriverId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_ActivitySchedule_TripType] ON [ActivitySchedule] ([TripType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_ActivitySchedule_VehicleId] ON [ActivitySchedule] ([ScheduledVehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_RouteStops_RouteId] ON [RouteStops] ([RouteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_RouteStops_RouteOrder] ON [RouteStops] ([RouteId], [StopOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Schedules_BusId] ON [Schedules] ([BusId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Schedules_Date] ON [Schedules] ([ScheduleDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Schedules_DriverId] ON [Schedules] ([DriverId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_Schedules_RouteId] ON [Schedules] ([RouteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_SchoolCalendar_Date] ON [SchoolCalendar] ([Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_SchoolCalendar_EventType] ON [SchoolCalendar] ([EventType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_SchoolCalendar_RoutesRequired] ON [SchoolCalendar] ([RoutesRequired]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    CREATE INDEX [IX_SchoolCalendar_SchoolYear] ON [SchoolCalendar] ([SchoolYear]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD CONSTRAINT [FK_Activities_Driver] FOREIGN KEY ([DriverId]) REFERENCES [Drivers] ([DriverId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Activities] ADD CONSTRAINT [FK_Activities_Vehicle] FOREIGN KEY ([AssignedVehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Fuel] ADD CONSTRAINT [FK_Fuel_Vehicle] FOREIGN KEY ([VehicleFueledId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Maintenance] ADD CONSTRAINT [FK_Maintenance_Vehicle] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Routes] ADD CONSTRAINT [FK_Routes_AMDriver] FOREIGN KEY ([AMDriverId]) REFERENCES [Drivers] ([DriverId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Routes] ADD CONSTRAINT [FK_Routes_AMVehicle] FOREIGN KEY ([AMVehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Routes] ADD CONSTRAINT [FK_Routes_PMDriver] FOREIGN KEY ([PMDriverId]) REFERENCES [Drivers] ([DriverId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    ALTER TABLE [Routes] ADD CONSTRAINT [FK_Routes_PMVehicle] FOREIGN KEY ([PMVehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703183136_InitialEnhancedSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250703183136_InitialEnhancedSchema', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    ALTER TABLE [Activities] ADD [RouteId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE TABLE [Tickets] (
        [TicketId] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [RouteId] int NOT NULL,
        [TravelDate] date NOT NULL,
        [IssuedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [TicketType] nvarchar(20) NOT NULL,
        [Price] decimal(10,2) NOT NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT N'Valid',
        [PaymentMethod] nvarchar(30) NOT NULL,
        [QRCode] nvarchar(50) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [ValidFrom] datetime2 NULL,
        [ValidUntil] datetime2 NULL,
        [IsRefundable] bit NOT NULL,
        [RefundAmount] decimal(10,2) NULL,
        [UsedDate] datetime2 NULL,
        [UsedByDriver] nvarchar(100) NULL,
        [Id] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedDate] datetime2 NULL,
        [CreatedBy] nvarchar(100) NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL,
        [CustomFields] nvarchar(max) NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Tickets] PRIMARY KEY ([TicketId]),
        CONSTRAINT [FK_Tickets_Route] FOREIGN KEY ([RouteId]) REFERENCES [Routes] ([RouteId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tickets_Student] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([StudentId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    EXEC(N'UPDATE [Drivers] SET [CreatedDate] = ''2025-07-03T23:46:42.6417208Z''
    WHERE [DriverId] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    EXEC(N'UPDATE [Drivers] SET [CreatedDate] = ''2025-07-03T23:46:42.6417216Z''
    WHERE [DriverId] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    EXEC(N'UPDATE [Vehicles] SET [CreatedDate] = ''2025-07-03T23:46:42.6417107Z''
    WHERE [VehicleId] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    EXEC(N'UPDATE [Vehicles] SET [CreatedDate] = ''2025-07-03T23:46:42.6417155Z''
    WHERE [VehicleId] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE INDEX [IX_Activities_RouteId] ON [Activities] ([RouteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE INDEX [IX_Tickets_IssuedDate] ON [Tickets] ([IssuedDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tickets_QRCode] ON [Tickets] ([QRCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE INDEX [IX_Tickets_RouteId] ON [Tickets] ([RouteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE INDEX [IX_Tickets_Status] ON [Tickets] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE INDEX [IX_Tickets_StudentId] ON [Tickets] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE INDEX [IX_Tickets_StudentRouteDate] ON [Tickets] ([StudentId], [RouteId], [TravelDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE INDEX [IX_Tickets_TicketType] ON [Tickets] ([TicketType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE INDEX [IX_Tickets_TravelDate] ON [Tickets] ([TravelDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    CREATE INDEX [IX_Tickets_TravelDateStatus] ON [Tickets] ([TravelDate], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    ALTER TABLE [Activities] ADD CONSTRAINT [FK_Activities_Route] FOREIGN KEY ([RouteId]) REFERENCES [Routes] ([RouteId]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250703234643_AddRouteIdToActivities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250703234643_AddRouteIdToActivities', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Routes] ADD [AMBeginTime] time NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Routes] ADD [BusNumber] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Routes] ADD [Distance] decimal(10,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Routes] ADD [DriverName] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Routes] ADD [EstimatedDuration] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Routes] ADD [PMBeginTime] time NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Routes] ADD [StopCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Routes] ADD [StudentCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Activities]') AND [c].[name] = N'DriverId');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Activities] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [Activities] ALTER COLUMN [DriverId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Activities] ADD [DestinationLatitude] decimal(10,8) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Activities] ADD [DestinationLongitude] decimal(11,8) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Activities] ADD [Directions] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Activities] ADD [DistanceMiles] decimal(8,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Activities] ADD [EstimatedTravelTime] time NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Activities] ADD [PickupLatitude] decimal(10,8) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Activities] ADD [PickupLocation] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    ALTER TABLE [Activities] ADD [PickupLongitude] decimal(11,8) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    EXEC(N'UPDATE [Drivers] SET [CreatedDate] = ''2025-07-05T22:29:38.1037895Z''
    WHERE [DriverId] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    EXEC(N'UPDATE [Drivers] SET [CreatedDate] = ''2025-07-05T22:29:38.1037907Z''
    WHERE [DriverId] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    EXEC(N'UPDATE [Vehicles] SET [CreatedDate] = ''2025-07-05T22:29:38.1037792Z''
    WHERE [VehicleId] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    EXEC(N'UPDATE [Vehicles] SET [CreatedDate] = ''2025-07-05T22:29:38.1037835Z''
    WHERE [VehicleId] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250705222938_MakeDriverIdOptional'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250705222938_MakeDriverIdOptional', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250708233756_AddActivityLogsTable'
)
BEGIN
    CREATE TABLE [ActivityLogs] (
        [Id] int NOT NULL IDENTITY,
        [Timestamp] datetime2 NOT NULL,
        [Action] nvarchar(200) NOT NULL,
        [User] nvarchar(100) NOT NULL,
        [Details] nvarchar(1000) NULL,
        CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250708233756_AddActivityLogsTable'
)
BEGIN
    EXEC(N'UPDATE [Drivers] SET [CreatedDate] = ''2025-07-08T23:37:55.5996046Z''
    WHERE [DriverId] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250708233756_AddActivityLogsTable'
)
BEGIN
    EXEC(N'UPDATE [Drivers] SET [CreatedDate] = ''2025-07-08T23:37:55.5996056Z''
    WHERE [DriverId] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250708233756_AddActivityLogsTable'
)
BEGIN
    EXEC(N'UPDATE [Vehicles] SET [CreatedDate] = ''2025-07-08T23:37:55.5995868Z''
    WHERE [VehicleId] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250708233756_AddActivityLogsTable'
)
BEGIN
    EXEC(N'UPDATE [Vehicles] SET [CreatedDate] = ''2025-07-08T23:37:55.5995904Z''
    WHERE [VehicleId] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250708233756_AddActivityLogsTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Schedules_RouteBusDeparture] ON [Schedules] ([RouteId], [BusId], [DepartureTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250708233756_AddActivityLogsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250708233756_AddActivityLogsTable', N'8.0.8');
END;
GO

COMMIT;
GO


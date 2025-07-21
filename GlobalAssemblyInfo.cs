// <copyright file="GlobalAssemblyInfo.cs" company="BusBuddy Transportation Solutions">
// Copyright (c) BusBuddy Transportation Solutions. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about the BusBuddy solution
[assembly: AssemblyCompany("BusBuddy Transportation Solutions")]
[assembly: AssemblyProduct("BusBuddy - School Transportation Management System")]
[assembly: AssemblyCopyright("Copyright © BusBuddy Transportation Solutions 2025")]
[assembly: AssemblyTrademark("BusBuddy™")]
[assembly: AssemblyConfiguration("Debug")]

// Version information for the BusBuddy solution
// Version format: Major.Minor.Build.Revision
// - Major: Significant changes, breaking changes
// - Minor: Feature additions, non-breaking changes
// - Build: Bug fixes, patches
// - Revision: Hotfixes, emergency patches
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0-beta")]

// Culture and localization
[assembly: NeutralResourcesLanguage("en-US")]

// Security and COM settings
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

// Assembly metadata for tooling and documentation
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/Bigessfour/BusBuddy-WPF")]
[assembly: AssemblyMetadata("ContactEmail", "support@busbuddy.com")]
[assembly: AssemblyMetadata("Documentation", "https://docs.busbuddy.com")]
[assembly: AssemblyMetadata("TargetFramework", ".NET 8.0")]
[assembly: AssemblyMetadata("UIFramework", "WPF")]
[assembly: AssemblyMetadata("ControlLibrary", "Syncfusion WPF 30.1.40")]
[assembly: AssemblyMetadata("Theme", "FluentDark/FluentLight")]
[assembly: AssemblyMetadata("Architecture", "MVVM")]
[assembly: AssemblyMetadata("DataAccess", "Entity Framework Core")]
[assembly: AssemblyMetadata("Database", "SQL Server")]
[assembly: AssemblyMetadata("Logging", "Serilog")]
[assembly: AssemblyMetadata("DependencyInjection", "Microsoft.Extensions.DependencyInjection")]
[assembly: AssemblyMetadata("BuildDate", "2025-07-20")]
[assembly: AssemblyMetadata("BuildEnvironment", "Development")]

// Development and debugging metadata
#if DEBUG
[assembly: AssemblyMetadata("BuildConfiguration", "Debug")]
[assembly: AssemblyMetadata("OptimizationLevel", "None")]
[assembly: AssemblyMetadata("DebugSymbols", "Full")]
#else
[assembly: AssemblyMetadata("BuildConfiguration", "Release")]
[assembly: AssemblyMetadata("OptimizationLevel", "Maximum")]
[assembly: AssemblyMetadata("DebugSymbols", "Portable")]
#endif

// Feature flags and capabilities
[assembly: AssemblyMetadata("Features.OfflineMode", "Enabled")]
[assembly: AssemblyMetadata("Features.RealtimeSync", "Enabled")]
[assembly: AssemblyMetadata("Features.MobileApp", "Planned")]
[assembly: AssemblyMetadata("Features.CloudBackup", "Enabled")]
[assembly: AssemblyMetadata("Features.Analytics", "Enabled")]
[assembly: AssemblyMetadata("Features.MultiTenant", "Planned")]

// Performance and scalability metadata
[assembly: AssemblyMetadata("Performance.MaxVehicles", "1000")]
[assembly: AssemblyMetadata("Performance.MaxDrivers", "500")]
[assembly: AssemblyMetadata("Performance.MaxStudents", "10000")]
[assembly: AssemblyMetadata("Performance.MaxRoutes", "200")]
[assembly: AssemblyMetadata("Scalability.DatabaseSharding", "Supported")]
[assembly: AssemblyMetadata("Scalability.LoadBalancing", "Planned")]

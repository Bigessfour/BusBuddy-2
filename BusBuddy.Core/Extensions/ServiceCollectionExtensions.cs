using BusBuddy.Core.Data;
using BusBuddy.Core.Data.Interfaces;
using BusBuddy.Core.Data.Repositories;
using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusBuddy.Core.Extensions
{
    /// <summary>
    /// Extension methods for registering data services with dependency injection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register all data services including DbContext, repositories, and Unit of Work
        /// </summary>
        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext with transient lifetime for thread safety
            services.AddTransient<BusBuddyDbContext>(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<BusBuddyDbContext>();
                optionsBuilder.UseInMemoryDatabase("BusBuddyDb");
                return new BusBuddyDbContext(optionsBuilder.Options);
            });

            // Register DbContext Factory for thread-safe context creation
            services.AddSingleton<IBusBuddyDbContextFactory, BusBuddyDbContextFactory>();

            // Register repositories - use fully qualified names to avoid ambiguity
            services.AddScoped<IVehicleRepository, BusBuddy.Core.Data.Repositories.VehicleRepository>();
            services.AddScoped<IActivityRepository, BusBuddy.Core.Data.Repositories.ActivityRepository>();
            services.AddScoped<IBusRepository, BusBuddy.Core.Data.Repositories.BusRepository>();
            services.AddScoped<IDriverRepository, BusBuddy.Core.Data.Repositories.DriverRepository>();
            services.AddScoped<IRouteRepository, BusBuddy.Core.Data.Repositories.RouteRepository>();
            services.AddScoped<IStudentRepository, BusBuddy.Core.Data.Repositories.StudentRepository>();
            services.AddScoped<IFuelRepository, BusBuddy.Core.Data.Repositories.FuelRepository>();
            services.AddScoped<IMaintenanceRepository, BusBuddy.Core.Data.Repositories.MaintenanceRepository>();
            services.AddScoped<IScheduleRepository, BusBuddy.Core.Data.Repositories.ScheduleRepository>();
            services.AddScoped<ISchoolCalendarRepository, BusBuddy.Core.Data.Repositories.SchoolCalendarRepository>();
            services.AddScoped<IActivityScheduleRepository, BusBuddy.Core.Data.Repositories.ActivityScheduleRepository>();

            // Register generic repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, BusBuddy.Core.Data.UnitOfWork.UnitOfWork>();

            // Register User Context Service
            services.AddScoped<IUserContextService, UserContextService>();

            // Register Business Services
            services.AddScoped<IBusService, BusService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IFuelService, FuelService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            // REMOVED: ITicketService - deprecated module

            // Register Address Validation Service
            services.AddScoped<IAddressValidationService, AddressValidationService>();

            // Register Activity Log Service
            services.AddScoped<IActivityLogService, ActivityLogService>();

            // Register Database NULL Fix Service
            services.AddScoped<BusBuddy.Core.Services.DatabaseNullFixService>();

            return services;
        }
    }
}

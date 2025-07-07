using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BusBuddy.Core.Data;
using BusBuddy.Core.Data.Repositories;
using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Services.Interfaces;

namespace BusBuddy.Core.Services
{
    public static class ServiceContainer
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BusBuddyDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IActivityService, ActivityService>();

            // Register other core services and repositories here
            // e.g., services.AddScoped<IBusRepository, BusRepository>();
        }
    }
}

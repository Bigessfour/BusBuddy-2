using BusBuddy.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Bus_Buddy;

/// <summary>
/// Design-time factory for Entity Framework migrations
/// </summary>
public class BusBuddyDbContextFactory : IDesignTimeDbContextFactory<BusBuddyDbContext>
{
    public BusBuddyDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BusBuddyDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);

        return new BusBuddyDbContext(optionsBuilder.Options);
    }
}

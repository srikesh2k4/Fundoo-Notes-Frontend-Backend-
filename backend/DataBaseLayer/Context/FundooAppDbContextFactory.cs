// ========================================
// FILE: DataBaseLayer/Context/FundooAppDbContextFactory.cs (FIXED)
// ========================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DataBaseLayer.Context
{
    /// <summary>
    /// Design-time factory for creating DbContext instances during migrations
    /// This class is used by EF Core tools (dotnet ef) at design time
    /// </summary>
    public class FundooAppDbContextFactory : IDesignTimeDbContextFactory<FundooAppDbContext>
    {
        public FundooAppDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("FundooConnection");

            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<FundooAppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FundooAppDbContext(optionsBuilder.Options);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MockApi.Runtime.Session;

namespace MockApi.Data
{
    public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Ścieżka do pliku appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // katalog projektu startowego
                .AddJsonFile("appsettings.json")
                .Build();

            // Odczyt connection stringa
            var connectionString = config.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString); // lub UseNpgsql jeśli masz PostgreSQL

            return new AppDbContext(optionsBuilder.Options, new NullAppSession());
        }
    }

    public class NullAppSession : IAppSession
    {
        public Guid? UserId => null;
    }
}

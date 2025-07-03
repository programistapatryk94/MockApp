using Microsoft.EntityFrameworkCore;
using MockApi.Models;

namespace MockApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected AppDbContext()
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Mock> Mocks => Set<Mock>();
    }
}

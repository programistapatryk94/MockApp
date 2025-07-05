using Microsoft.EntityFrameworkCore;
using MockApi.Models;
using System.Security.Claims;

namespace MockApi.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Guid CurrentUserId { get; private set; }

        public AppDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;

            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                CurrentUserId = userId;
            }
        }

        protected AppDbContext()
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Mock> Mocks => Set<Mock>();
        public DbSet<Project> Projects => Set<Project>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>().HasIndex(p => p.Secret).IsUnique();

            modelBuilder.Entity<Project>().HasQueryFilter(e => e.UserId == CurrentUserId);
            modelBuilder.Entity<Mock>().HasQueryFilter(e => e.UserId == CurrentUserId);
        }
    }
}

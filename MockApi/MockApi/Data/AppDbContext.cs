using Microsoft.EntityFrameworkCore;
using MockApi.Models;
using MockApi.Runtime.Session;

namespace MockApi.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IAppSession _appSession;

        public Guid? CurrentUserId => _appSession.UserId;

        public AppDbContext(DbContextOptions options, IAppSession appSession) : base(options)
        {
            _appSession = appSession;
        }

        protected AppDbContext()
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Mock> Mocks => Set<Mock>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>().HasIndex(p => p.Secret).IsUnique();

            modelBuilder.Entity<Project>().HasQueryFilter(e => CurrentUserId.HasValue && (e.UserId == CurrentUserId || e.ProjectMembers.Any(c => c.UserId == CurrentUserId)));
            modelBuilder.Entity<Mock>().HasQueryFilter(e => CurrentUserId.HasValue && (e.Project.UserId == CurrentUserId || e.Project.ProjectMembers.Any(c => c.UserId == CurrentUserId)));

            modelBuilder.Entity<ProjectMember>()
                .HasKey(pc => new { pc.ProjectId, pc.UserId });

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pc => pc.Project)
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(pc => pc.ProjectId);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pc => pc.User)
                .WithMany(u => u.ProjectMembers)
                .HasForeignKey(u => u.UserId);
        }
    }
}

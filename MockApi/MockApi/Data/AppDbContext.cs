using Microsoft.EntityFrameworkCore;
using MockApi.Models;
using MockApi.Runtime.Session;

namespace MockApi.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IAppSession _appSession;

        public Guid? CurrentUserId => _appSession.UserId;
        public bool ApplyOwnershipFilter { get; set; } = true;
        public bool ApplyCollaborationFilter { get; set; } = true;

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
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<SubscriptionHistory> SubscriptionHistories => Set<SubscriptionHistory>();
        public DbSet<FeatureSetting> FeatureSettings => Set<FeatureSetting>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>().HasIndex(p => p.Secret).IsUnique();

            modelBuilder.Entity<Project>().HasQueryFilter(e =>
            !ApplyOwnershipFilter && !ApplyCollaborationFilter
            || (
                (ApplyOwnershipFilter && e.UserId == CurrentUserId)
                || (ApplyCollaborationFilter && e.ProjectMembers.Any(c => c.UserId == CurrentUserId))
            ));
            modelBuilder.Entity<Mock>().HasQueryFilter(e =>
            !ApplyOwnershipFilter && !ApplyCollaborationFilter
            || (
                (ApplyOwnershipFilter && e.Project.UserId == CurrentUserId)
                || (ApplyCollaborationFilter && e.Project.ProjectMembers.Any(c => c.UserId == CurrentUserId))
            ));

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

            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.History)
                .WithOne(h => h.Subscription)
                .HasForeignKey(h => h.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

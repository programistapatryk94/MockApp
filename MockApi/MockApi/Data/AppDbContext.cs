using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using MockApi.Data.Utils;
using MockApi.Data.ValueConverters;
using MockApi.Extensions;
using MockApi.Helpers;
using MockApi.Models;
using MockApi.Runtime.DataModels;
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
        public DbSet<RequestLog> RequestLogs => Set<RequestLog>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<SubscriptionPlanPrice> SubscriptionPlanPrices => Set<SubscriptionPlanPrice>();
        public DbSet<CurrentSubscription> CurrentSubscriptions => Set<CurrentSubscription>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>().HasIndex(p => p.Secret).IsUnique();

            modelBuilder.Entity<Project>().HasQueryFilter(e =>
            !ApplyOwnershipFilter && !ApplyCollaborationFilter
            || (
                (ApplyOwnershipFilter && e.CreatorUserId == CurrentUserId)
                || (ApplyCollaborationFilter && e.CreatorUser.IsCollaborationEnabled && e.ProjectMembers.Any(c => c.UserId == CurrentUserId))
            ));
            modelBuilder.Entity<Mock>().HasQueryFilter(e =>
            !ApplyOwnershipFilter && !ApplyCollaborationFilter
            || (
                (ApplyOwnershipFilter && e.Project.CreatorUserId == CurrentUserId)
                || (ApplyCollaborationFilter && e.Project.CreatorUser.IsCollaborationEnabled && e.Project.ProjectMembers.Any(c => c.UserId == CurrentUserId))
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

            modelBuilder.Entity<User>()
                .HasOne(p => p.CurrentSubscription)
                .WithOne(p => p.User)
                .HasForeignKey<CurrentSubscription>(p => p.UserId);

            modelBuilder.Entity<Subscription>()
           .HasIndex(s => s.UserId)
           .IsUnique(); // <- ustawia unikalny indeks
        }

        public override int SaveChanges()
        {
            ApplyAuditConcepts();
            var result = base.SaveChanges();
            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditConcepts();
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }

        private void ApplyAuditConcepts()
        {
            var userId = GetAuditUserId();

            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                if (entry.State != EntityState.Modified && entry.CheckOwnedEntityChange())
                {
                    Entry(entry.Entity).State = EntityState.Modified;
                }

                ApplyAuditConcepts(entry, userId);
            }
        }

        private void ApplyAuditConcepts(EntityEntry entry, Guid? userId)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    ApplyAuditConceptsForAddedEntity(entry, userId);
                    break;
                case EntityState.Modified:
                    ApplyAuditConceptsForModifiedEntity(entry, userId);
                    break;
            }
        }

        private void ApplyAuditConceptsForAddedEntity(EntityEntry entry, Guid? userId)
        {
            CheckAndSetId(entry);
            SetCreationAuditProperties(entry.Entity, userId);
        }

        private void ApplyAuditConceptsForModifiedEntity(EntityEntry entry, Guid? userId)
        {
            SetModificationAuditProperties(entry.Entity, userId);
        }

        private void CheckAndSetId(EntityEntry entry)
        {
            var entity = entry.Entity as IEntity<Guid>;
            if (entity != null && entity.Id == Guid.Empty)
            {
                var idPropertyEntry = entry.Property(nameof(IEntity.Id));

                if (idPropertyEntry != null && idPropertyEntry.Metadata.ValueGenerated == Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never)
                {
                    entity.Id = Guid.NewGuid();
                }
            }
        }

        private void SetCreationAuditProperties(object entity, Guid? userId)
        {
            AuditingHelper.SetCreationAuditProperties(entity, userId);
        }

        private void SetModificationAuditProperties(object entity, Guid? userId)
        {
            AuditingHelper.SetModificationAuditProperties(entity, userId);
        }

        private Guid? GetAuditUserId()
        {
            return _appSession.UserId;
        }

        public void ConfigureGlobalValueConverter<TEntity>(ModelBuilder modelBuilder, IMutableEntityType entityType)
            where TEntity : class
        {
            if (entityType.BaseType == null &&
            //!typeof(TEntity).IsDefined(typeof(DisableDateTimeNormalizationAttribute), true) &&
            !typeof(TEntity).IsDefined(typeof(OwnedAttribute), true) &&
            !entityType.IsOwned())
            {
                var dateTimeValueConverter = new DateTimeValueConverter();
                var dateTimePropertyInfos = DateTimePropertyInfoHelper.GetDatePropertyInfos(typeof(TEntity));
                dateTimePropertyInfos.DateTimePropertyInfos.ForEach(property =>
                {
                    modelBuilder
                        .Entity<TEntity>()
                        .Property(property.Name)
                        .HasConversion(dateTimeValueConverter);
                });
            }
        }
    }
}

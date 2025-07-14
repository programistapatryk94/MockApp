
using System.ComponentModel.DataAnnotations.Schema;

namespace MockApi.Runtime.DataModels.Auditing
{
    [Serializable]
    public abstract class CreatedAuditedEntity : CreatedAuditedEntity<Guid>, IEntity
    {

    }

    [Serializable]
    public abstract class CreatedAuditedEntity<TPrimaryKey> : Entity<TPrimaryKey>, ICreatedAudited
    {
        public virtual Guid? CreatorUserId { get; set; }
        public virtual DateTime CreatedAt { get; set; }

        protected CreatedAuditedEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }

    public abstract class CreatedAuditedEntity<TPrimaryKey, TUser> : CreatedAuditedEntity<TPrimaryKey>, ICreatedAudited<TUser>
        where TUser : IEntity<Guid>
    {
        [ForeignKey("CreatorUserId")]
        public virtual TUser CreatorUser { get; set; }
    }
}

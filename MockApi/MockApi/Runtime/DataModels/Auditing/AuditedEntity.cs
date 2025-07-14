
using System.ComponentModel.DataAnnotations.Schema;

namespace MockApi.Runtime.DataModels.Auditing
{
    [Serializable]
    public abstract class AuditedEntity : AuditedEntity<Guid>, IEntity
    {

    }

    [Serializable]
    public abstract class AuditedEntity<TPrimaryKey> : CreatedAuditedEntity<TPrimaryKey>, IAudited
    {
        public virtual Guid? LastModifierUserId { get; set; }
        public virtual DateTime? ModifiedAt { get; set; }
    }

    [Serializable]
    public abstract class AuditedEntity<TPrimaryKey, TUser> : AuditedEntity<TPrimaryKey>, IAudited<TUser>
        where TUser : IEntity<Guid>
    {

        [ForeignKey("LastModifierUserId")]
        public virtual TUser LastModifierUser { get; set; }

        [ForeignKey("CreatorUserId")]
        public virtual TUser CreatorUser { get; set; }
    }
}

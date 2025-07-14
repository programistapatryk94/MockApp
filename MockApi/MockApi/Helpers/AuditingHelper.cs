using MockApi.Runtime.DataModels.Auditing;

namespace MockApi.Helpers
{
    public static class AuditingHelper
    {
        public static void SetCreationAuditProperties(object entity, Guid? userId)
        {
            var entityWithCreatedAt = entity as IHasCreatedAt;
            if (entityWithCreatedAt == null)
            {
                return;
            }

            if (entityWithCreatedAt.CreatedAt == default)
            {
                entityWithCreatedAt.CreatedAt = DateTime.UtcNow;
            }

            if (!userId.HasValue)
            {
                return;
            }

            if (entity is not ICreatedAudited entityWithCreatedAudited)
            {
                return;
            }

            if (entityWithCreatedAudited.CreatorUserId != null)
            {
                return;
            }

            entityWithCreatedAudited.CreatorUserId = userId;
        }

        public static void SetModificationAuditProperties(object entity, Guid? userId)
        {
            if (entity is IHasModifiedAt entityWithModifiedAt)
            {
                entityWithModifiedAt.ModifiedAt = DateTime.UtcNow;
            }

            if (!(entity is IModifiedAudited entityWithModifiedAudited))
            {
                return;
            }

            if (entityWithModifiedAudited.LastModifierUserId != null)
            {
                return;
            }

            entityWithModifiedAudited.LastModifierUserId = userId;
        }
    }
}
